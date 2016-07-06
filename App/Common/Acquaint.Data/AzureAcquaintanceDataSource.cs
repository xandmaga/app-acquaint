using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Acquaint.Data
{
	public class AzureAcquaintanceDataSource : IDataSource<Acquaintance>
	{
		MobileServiceClient _MobileService;

		// The base URL of the Azure App Service instance.
		// Replace with your own, if you've decided to host your own instance.
		// Otherwise, feel free to use the provided service instance while you evaluate the app.
		static readonly string _ServiceUrl = "https://app-acquaint.azurewebsites.net";

		// Specify a GUID value for the data partition id. 
		// This makes your data in the service isolated from everyone else's.
		// Prevents data from being seen across different groups who are using this app for evaluation.
		readonly string _DataPartitionId = "01d676fd-789a-4488-b519-1840e080936e";

		IMobileServiceSyncTable<Acquaintance> _AcquaintanceTable;

		bool _IsInitialized;

		public async Task Init()
		{
			if (_IsInitialized)
				return;

			_MobileService = GetMobileServiceInstance();

			var store = new MobileServiceSQLiteStore("acquaintances.db");

			store.DefineTable<Acquaintance>();

			try
			{
				await _MobileService.SyncContext.InitializeAsync(store).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(@"Failed to initialize sync context: {0}", ex.Message);
			}

			_AcquaintanceTable = _MobileService.GetSyncTable<Acquaintance>();

			await Fullsync().ConfigureAwait(false);

			_IsInitialized = true;
		}

		/// <summary>
		/// Gets a MobileServiceClient instance. This particular method is for developer convenience, 
		/// because it implements a special handler for iOS, enabling the sniffing of outbound HTTP traffic 
		/// with tools such as Charles Deugging Proxy (in debug builds only).
		/// </summary>
		/// <returns>The MobileServiceClient instance.</returns>
		MobileServiceClient GetMobileServiceInstance()
		{
			MobileServiceClient client;

#if DEBUG
			// using a special handler on iOS so that we can use Charles debugging proxy to inspect outbound HTTP traffic from the app
			var handlerFactory = ServiceLocator.Current.GetInstance<IHttpClientHandlerFactory>();

			if (handlerFactory != null)
			{
				client = new MobileServiceClient(_ServiceUrl, handlerFactory.GetHttpClientHandler()); // { SerializerSettings = new MobileServiceJsonSerializerSettings() { CamelCasePropertyNames = true } };
			}
			else
				client = new MobileServiceClient(_ServiceUrl); // { SerializerSettings = new MobileServiceJsonSerializerSettings() { CamelCasePropertyNames = true } };
#else
			client = new MobileServiceClient(_ServiceUrl); // { SerializerSettings = new MobileServiceJsonSerializerSettings() { CamelCasePropertyNames = true } };
#endif

			return client;
		}

		async Task Fullsync()
		{
			await Execute(async () => {
				await _AcquaintanceTable.PullAsync("fullSyncAcquaintances", _AcquaintanceTable.Where(x => x.DataPartitionId == _DataPartitionId)).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		async Task DeltaSync()
		{
			await Execute(async () => {
				if (!_IsInitialized)
				{
					await Init().ConfigureAwait(false);
					return;
				}

				await Execute(async () => {
					await _AcquaintanceTable.PullAsync("deltaSyncAcquaintances", _AcquaintanceTable.Where(x => x.DataPartitionId == _DataPartitionId)).ConfigureAwait(false);
				});
			}).ConfigureAwait(false);
		}

		#region IDataSource implementation

		public async Task SaveItem(Acquaintance item)
		{
			await Execute(async () => {
				await DeltaSync().ConfigureAwait(false);
				if (item.Id == null)
					await _AcquaintanceTable.InsertAsync(item).ConfigureAwait(false);
				else
					await _AcquaintanceTable.UpdateAsync(item).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		public async Task DeleteItem(string id)
		{
			await Execute(async () => {
				await DeltaSync().ConfigureAwait(false);
				var item = await GetItem(id).ConfigureAwait(false);
				if (item != null)
					await _AcquaintanceTable.DeleteAsync(item).ConfigureAwait(false);
			}).ConfigureAwait(false);
		}

		public async Task<Acquaintance> GetItem(string id)
		{
			return await Execute(async () => {
				await DeltaSync().ConfigureAwait(false);
				return (await _AcquaintanceTable.Where(acquaintance => acquaintance.Id == id).ToEnumerableAsync().ConfigureAwait(false)).SingleOrDefault();
			}, null).ConfigureAwait(false);


		}

		public async Task<ICollection<Acquaintance>> GetItems(int start = 0, int count = 100, string query = "")
		{
			return await Execute<ICollection<Acquaintance>>(async () => {
				await DeltaSync().ConfigureAwait(false);
				return (await _AcquaintanceTable.Where(acquaintance => acquaintance.DataPartitionId == _DataPartitionId).OrderBy(b => b.Company).ToCollectionAsync().ConfigureAwait(false));
			}, new Collection<Acquaintance>()).ConfigureAwait(false);
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
	}
}

