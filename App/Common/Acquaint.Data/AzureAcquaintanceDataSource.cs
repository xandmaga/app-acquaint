using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Acquaint.Util;
using Microsoft.Practices.ServiceLocation;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;

namespace Acquaint.Data
{
	public class AzureAcquaintanceSource : IDataSource<Acquaintance>
	{
		static readonly string _ServiceUrl = Settings.AzureAppServiceUrl;

		readonly string _DataPartitionId = Settings.DataPartitionId;

		public MobileServiceClient MobileService { get; set; }

		bool _IsInitialized;

		public async Task<bool> Initialize()
		{
			if (_IsInitialized)
				return true;

			// MobileServiceClient handles communication with our backend, auth, and more for us.
			MobileService = new MobileServiceClient(_ServiceUrl, GetHttpClientHandler());

			// Configure online/offline sync.
			var store = new MobileServiceSQLiteStore("app.db");

			store.DefineTable<Acquaintance>();

			await MobileService.SyncContext.InitializeAsync(store).ConfigureAwait(false);

			_IsInitialized = true;

			return _IsInitialized;
		}

		#region Data Access

		public async Task<Acquaintance> GetItem(string id)
		{
			return await Execute<Acquaintance>(async () => 
			{ 
				await SyncItems().ConfigureAwait(false);
				return await MobileService.GetSyncTable<Acquaintance>().LookupAsync(id).ConfigureAwait(false);
			}, null).ConfigureAwait(false);
		}

		public async Task<IEnumerable<Acquaintance>> GetItems()
		{
			return await Execute<IEnumerable<Acquaintance>>(async () => 
			{
				await SyncItems().ConfigureAwait(false);
				return await MobileService.GetSyncTable<Acquaintance>().Where(x => x.DataPartitionId == _DataPartitionId).OrderBy(x => x.LastName).ToEnumerableAsync().ConfigureAwait(false);
			}, new List<Acquaintance>()).ConfigureAwait(false);
		}

		public async Task<bool> AddItem(Acquaintance item)
		{
			return await Execute<bool>(async () => 
			{ 
				await Initialize().ConfigureAwait(false);
				await MobileService.GetSyncTable<Acquaintance>().InsertAsync(item).ConfigureAwait(false);
				await SyncItems().ConfigureAwait(false);
				return true;
			}, false).ConfigureAwait(false);
		}

		public async Task<bool> UpdateItem(Acquaintance item)
		{
			return await Execute<bool>(async () => 
			{ 
				await Initialize().ConfigureAwait(false);
				await MobileService.GetSyncTable<Acquaintance>().UpdateAsync(item).ConfigureAwait(false);
				await SyncItems().ConfigureAwait(false);
				return true;
			}, false).ConfigureAwait(false);
		}

		public async Task<bool> RemoveItem(Acquaintance item)
		{
			return await Execute<bool>(async () => 
			{
				await Initialize().ConfigureAwait(false);
				await MobileService.GetSyncTable<Acquaintance>().DeleteAsync(item).ConfigureAwait(false);
				await SyncItems().ConfigureAwait(false);
				return true;
			}, false).ConfigureAwait(false);
		}

		public async Task<bool> SyncItems()
		{
			return await Execute(async () => 
			{
				await Initialize().ConfigureAwait(false);

				try
				{
					await EnsureDataIsSeededForPartitionIdAsync(_DataPartitionId).ConfigureAwait(false);
					await MobileService.SyncContext.PushAsync().ConfigureAwait(false);
					await MobileService.GetSyncTable<Acquaintance>().PullAsync($"all{typeof(Acquaintance).Name}", MobileService.GetSyncTable<Acquaintance>().CreateQuery()).ConfigureAwait(false);
					return true;
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Error during Sync occurred: {ex.Message}");
					return false;
				}
			}, false);
		}
		#endregion

		#region some nifty helpers

		/// <summary>
		/// This method is intended for encapsulating the catching of exceptions related to the Azure MobileServiceClient.
		/// </summary>
		/// <param name="execute">A Func that contains the async work you'd like to do.</param>
		static async Task Execute(Func<Task> execute)
		{
			try
			{
				await execute().ConfigureAwait(false);
			}
			// isolate mobile service errors
			catch (MobileServiceInvalidOperationException ex)
			{
				System.Diagnostics.Debug.WriteLine(@"MOBILE SERVICE ERROR {0}", ex.Message);
			}
			// catch all other errors
			catch (Exception ex2)
			{
				System.Diagnostics.Debug.WriteLine(@"ERROR {0}", ex2.Message);
			}
		}

		/// <summary>
		/// This method is intended for encapsulating the catching of exceptions related to the Azure MobileServiceClient.
		/// </summary>
		/// <param name="execute">A Func that contains the async work you'd like to do, and will return some value.</param>
		/// <param name="defaultReturnObject">A default return object, which will be returned in the event that an operation in the Func throws an exception.</param>
		/// <typeparam name="T">The type of the return value that the Func will returns, and also the type of the default return object. </typeparam>
		static async Task<T> Execute<T>(Func<Task<T>> execute, T defaultReturnObject)
		{
			try
			{
				return await execute().ConfigureAwait(false);
			}
			catch (MobileServiceInvalidOperationException ex) // isolate mobile service errors
			{
				System.Diagnostics.Debug.WriteLine(@"MOBILE SERVICE ERROR {0}", ex.Message);
			}
			catch (Exception ex2) // catch all other errors
			{
				System.Diagnostics.Debug.WriteLine(@"ERROR {0}", ex2.Message);
			}
			return defaultReturnObject;
		}

		#endregion

		async Task EnsureDataIsSeededForPartitionIdAsync(string dataPartitionId)
		{
			if (Settings.DataIsSeeded)
				return;

			await MobileService.GetSyncTable<Acquaintance>().PullAsync($"all{typeof(Acquaintance).Name}", MobileService.GetSyncTable<Acquaintance>().CreateQuery()).ConfigureAwait(false);
			var any = (await MobileService.GetSyncTable<Acquaintance>().Where(x => x.DataPartitionId == _DataPartitionId).OrderBy(x => x.LastName).ToEnumerableAsync().ConfigureAwait(false)).Any();

			if (any)
				Settings.DataIsSeeded = true;


			if (!Settings.DataIsSeeded)
			{
				var newItems = SeedData.Get(_DataPartitionId);

				var insertTasks = new List<Task>();

				foreach (var i in newItems)
				{
					insertTasks.Add(MobileService.GetSyncTable<Acquaintance>().InsertAsync(i));
				}

				await Task.WhenAll(insertTasks).ConfigureAwait(false);
				Settings.DataIsSeeded = true;
			}
		}

		HttpClientHandler GetHttpClientHandler()
		{
			return ServiceLocator.Current.GetInstance<IHttpClientHandlerFactory>().GetHttpClientHandler();

		}
	}
}

