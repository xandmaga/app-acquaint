using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Acquaint.Abstractions;
using Acquaint.Util;
using Microsoft.Practices.ServiceLocation;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using PCLStorage;

namespace Acquaint.Data
{
	public class AzureAcquaintanceSource : IDataSource<Acquaintance>
	{
		string _ServiceUrl => Settings.AzureAppServiceUrl;

	    string _DataPartitionId => GuidUtility.Create(Settings.DataPartitionPhrase).ToString().ToUpper();

	    public MobileServiceClient MobileService { get; set; }

		IMobileServiceSyncTable<Acquaintance> _AcquaintanceTable;

		bool _IsInitialized;

		const string _LocalDbName = "acquaintances.db";

		public async Task<bool> Initialize()
		{
			return await Execute<bool>(async () => 
			{
				if (_IsInitialized)
					return true;

				// We're passing in a handler here for the sole purpose of inspecting outbound HTTP requests with Charles Web Debugging Proxy on OS X. Only in debug builds.
				MobileService = new MobileServiceClient(_ServiceUrl, GetHttpClientHandler());

				var store = new MobileServiceSQLiteStore(_LocalDbName) ;

				store.DefineTable<Acquaintance>();

				_AcquaintanceTable = MobileService.GetSyncTable<Acquaintance>();

				await MobileService.SyncContext.InitializeAsync(store).ConfigureAwait(false);

				_IsInitialized = true;

				return _IsInitialized;
			}, false).ConfigureAwait(false);
		}

		#region Data Access

		public async Task<Acquaintance> GetItem(string id)
		{
			return await Execute<Acquaintance>(async () => 
			{ 
				await SyncItems().ConfigureAwait(false);
				return await _AcquaintanceTable.LookupAsync(id).ConfigureAwait(false);
			}, null).ConfigureAwait(false);
		}

		public async Task<IEnumerable<Acquaintance>> GetItems()
		{
			return await Execute<IEnumerable<Acquaintance>>(async () => 
			{
				await SyncItems().ConfigureAwait(false);
				return await _AcquaintanceTable.Where(x => x.DataPartitionId == _DataPartitionId).OrderBy(x => x.LastName).ToEnumerableAsync().ConfigureAwait(false);
			}, new List<Acquaintance>()).ConfigureAwait(false);
		}

		public async Task<bool> AddItem(Acquaintance item)
		{
			return await Execute<bool>(async () => 
			{
				item.DataPartitionId = _DataPartitionId;

				await Initialize().ConfigureAwait(false);
				await _AcquaintanceTable.InsertAsync(item).ConfigureAwait(false);
				await SyncItems().ConfigureAwait(false);
				return true;
			}, false).ConfigureAwait(false);
		}

		public async Task<bool> UpdateItem(Acquaintance item)
		{
			return await Execute<bool>(async () => 
			{ 
				await Initialize().ConfigureAwait(false);
				await _AcquaintanceTable.UpdateAsync(item).ConfigureAwait(false);
				await SyncItems().ConfigureAwait(false);
				return true;
			}, false).ConfigureAwait(false);
		}

		public async Task<bool> RemoveItem(Acquaintance item, bool softDelete = true)
		{
			return await Execute<bool>(async () => 
			{
				await Initialize().ConfigureAwait(false);
				await _AcquaintanceTable.DeleteAsync(item).ConfigureAwait(false);
				await SyncItems().ConfigureAwait(false);
				return true;
			}, false).ConfigureAwait(false);
		}

		public async Task<bool> SyncItems()
		{
			return await Execute(async () => 
			{
				if (Settings.LocalDataResetIsRequested)
					await ResetLocalStoreAsync();

				await Initialize().ConfigureAwait(false);
				await EnsureDataIsSeededAsync(_DataPartitionId).ConfigureAwait(false);
				await MobileService.SyncContext.PushAsync().ConfigureAwait(false);
				await _AcquaintanceTable.PullAsync($"getAll{typeof(Acquaintance).Name}", _AcquaintanceTable.Where(x => x.DataPartitionId == _DataPartitionId)).ConfigureAwait(false);
				return true;
			}, false);
		}

		async Task ResetLocalStoreAsync()
		{
			_AcquaintanceTable = null;
			await DeleteOldLocalDatabase();
			_IsInitialized = false;
			Settings.LocalDataResetIsRequested = false;
			Settings.DataIsSeeded = false;
		}

		/// <summary>
		/// Deletes the old local database.
		/// </summary>
		/// <returns>The old local database.</returns>
		async Task DeleteOldLocalDatabase()
		{
			var databaseFolder = await FileSystem.Current.GetFolderFromPathAsync(ServiceLocator.Current.GetInstance<IDatastoreFolderPathProvider>().GetPath());
			var dbFile = await databaseFolder.GetFileAsync(_LocalDbName, CancellationToken.None);

			if (dbFile != null)
				await dbFile.DeleteAsync();
		}

		#endregion

		#region some nifty exception helpers

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
			catch (MobileServiceInvalidOperationException ex) // Isolate mobile service errors. This is the base exception type of the Azure client.
			{
				// TODO: report with HockeyApp
				System.Diagnostics.Debug.WriteLine(@"MOBILE SERVICE ERROR {0}", ex.Message);
			}
			// catch all other errors
			catch (Exception ex2)
			{
				// TODO: report with HockeyApp
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
			catch (MobileServiceInvalidOperationException ex) // Isolate mobile service errors. This is the base exception type of the Azure client.
			{
				// TODO: report with HockeyApp
				System.Diagnostics.Debug.WriteLine(@"MOBILE SERVICE ERROR {0}", ex.Message);
			}
			catch (Exception ex2) // catch all other errors
			{
				// TODO: report with HockeyApp
				System.Diagnostics.Debug.WriteLine(@"ERROR {0}", ex2.Message);
			}
			return defaultReturnObject;
		}

		#endregion

		async Task EnsureDataIsSeededAsync(string dataPartitionId)
		{
			if (Settings.DataIsSeeded)
				return;

			await _AcquaintanceTable.PullAsync($"getAll{typeof(Acquaintance).Name}", _AcquaintanceTable.Where(x => x.DataPartitionId == _DataPartitionId)).ConfigureAwait(false);

			var any = (await _AcquaintanceTable.Where(x => x.DataPartitionId == _DataPartitionId).OrderBy(x => x.LastName).ToEnumerableAsync().ConfigureAwait(false)).Any();

			if (any)
				Settings.DataIsSeeded = true;


			if (!Settings.DataIsSeeded)
			{
				var newItems = SeedData.Get(_DataPartitionId);

				foreach (var i in newItems)
				{
					await _AcquaintanceTable.InsertAsync(i);
				}

				Settings.DataIsSeeded = true;
			}
		}

		/// <summary>
		/// Gets an HttpClentHandler. The main purpose of which in this case is to 
		/// be able to inspect outbound HTTP traffic from the iOS simulator with
		/// Charles Web Debugging Proxy on OS X. Android and UWP will return a null handler.
		/// </summary>
		/// <returns>An HttpClentHandler</returns>
		HttpClientHandler GetHttpClientHandler()
		{
			return ServiceLocator.Current.GetInstance<IHttpClientHandlerFactory>().GetHttpClientHandler();
		}
	}
}

