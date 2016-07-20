using System;
using System.Collections.Generic;
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

	    private MobileServiceSQLiteStore _MobileServiceSQLiteStore;

        bool _IsInitialized;

		const string _LocalDbName = "acquaintances.db";

		#region Data Access

		public async Task<IEnumerable<Acquaintance>> GetItems()
		{
			return await Execute<IEnumerable<Acquaintance>>(async () => {
				await SyncItemsAsync().ConfigureAwait(false);
				return await _AcquaintanceTable.Where(x => x.DataPartitionId == _DataPartitionId).OrderBy(x => x.LastName).ToEnumerableAsync().ConfigureAwait(false);
			}, new List<Acquaintance>()).ConfigureAwait(false);
		}

		public async Task<Acquaintance> GetItem(string id)
		{
			return await Execute<Acquaintance>(async () => {
				await SyncItemsAsync().ConfigureAwait(false);
				return await _AcquaintanceTable.LookupAsync(id).ConfigureAwait(false);
			}, null).ConfigureAwait(false);
		}

		public async Task<bool> AddItem(Acquaintance item)
		{
			return await Execute<bool>(async () => {
				item.DataPartitionId = _DataPartitionId;

				await Initialize().ConfigureAwait(false);
				await _AcquaintanceTable.InsertAsync(item).ConfigureAwait(false);
				await SyncItemsAsync().ConfigureAwait(false);
				return true;
			}, false).ConfigureAwait(false);
		}

		public async Task<bool> UpdateItem(Acquaintance item)
		{
			return await Execute<bool>(async () => {
				await Initialize().ConfigureAwait(false);
				await _AcquaintanceTable.UpdateAsync(item).ConfigureAwait(false);
				await SyncItemsAsync().ConfigureAwait(false);
				return true;
			}, false).ConfigureAwait(false);
		}

		public async Task<bool> RemoveItem(Acquaintance item)
		{
			return await Execute<bool>(async () => {
				await Initialize().ConfigureAwait(false);
				await _AcquaintanceTable.DeleteAsync(item).ConfigureAwait(false);
				await SyncItemsAsync().ConfigureAwait(false);
				return true;
			}, false).ConfigureAwait(false);
		}

		#endregion


		#region helper methods for dealing with the state of the local store

		/// <summary>
		/// Initialize this instance.
		/// </summary>
		async Task<bool> Initialize()
		{
			return await Execute<bool>(async () => {
				if (_IsInitialized)
					return true;

				// We're passing in a handler here for the sole purpose of inspecting outbound HTTP requests with Charles Web Debugging Proxy on OS X. Only in debug builds.
				MobileService = new MobileServiceClient(_ServiceUrl, GetHttpClientHandler());

                _MobileServiceSQLiteStore = new MobileServiceSQLiteStore(_LocalDbName);

                _MobileServiceSQLiteStore.DefineTable<Acquaintance>();

				_AcquaintanceTable = MobileService.GetSyncTable<Acquaintance>();

				await MobileService.SyncContext.InitializeAsync(_MobileServiceSQLiteStore).ConfigureAwait(false);

				_IsInitialized = true;

				return _IsInitialized;
			}, false).ConfigureAwait(false);
		}

		async Task<bool> SyncItemsAsync()
		{
			return await Execute(async () => {
				if (Settings.LocalDataResetIsRequested)
					await ResetLocalStoreAsync().ConfigureAwait(false);

				await Initialize().ConfigureAwait(false);
				await EnsureDataIsSeededAsync(_DataPartitionId).ConfigureAwait(false);
				// PushAsync() has been omitted here because the MobileService.SyncContext automatically calls PushAsync() before PullAsync() if it sees pending changes in the queue.
				await _AcquaintanceTable.PullAsync($"getAll{typeof(Acquaintance).Name}", _AcquaintanceTable.Where(x => x.DataPartitionId == _DataPartitionId)).ConfigureAwait(false);
				return true;
			}, false);
		}

		/// <summary>
		/// Ensures the data is seeded.
		/// </summary>
		/// <returns>The data is seeded.</returns>
		/// <param name="dataPartitionId">Data partition identifier.</param>
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
		/// Resets the local store.
		/// </summary>
		/// <returns>The local store.</returns>
		async Task ResetLocalStoreAsync()
		{
			_AcquaintanceTable = null;
		    _MobileServiceSQLiteStore?.Dispose(); // it's necessary on UWP to Dispose() the SQLite store, otherwise the database deletion will fail because of an open file handle.
		    await DeleteOldLocalDatabase().ConfigureAwait(false);
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
			var databaseFolder = await FileSystem.Current.GetFolderFromPathAsync(ServiceLocator.Current.GetInstance<IDatastoreFolderPathProvider>().GetPath()).ConfigureAwait(false);
			var dbFile = await databaseFolder.GetFileAsync(_LocalDbName, CancellationToken.None).ConfigureAwait(false);

            if (dbFile != null)
		        await dbFile.DeleteAsync().ConfigureAwait(true);
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
			catch (Exception ex)
			{
				HandleExceptions(ex);
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
			catch (Exception ex)
			{
				HandleExceptions(ex);
			}
			return defaultReturnObject;
		}

		/// <summary>
		/// Handles the exceptions.
		/// </summary>
		/// <returns>The exceptions.</returns>
		/// <param name="ex">Ex.</param>
		static void HandleExceptions(Exception ex)
		{
			if (ex is MobileServiceInvalidOperationException)
			{
				// TODO: report with HockeyApp
			    System.Diagnostics.Debug.WriteLine($"MOBILE SERVICE ERROR {ex.Message}");
				return;
			}

			if (ex is MobileServicePushFailedException)
			{
				var pushResult = ((MobileServicePushFailedException) ex).PushResult;

				foreach (var e in pushResult.Errors)
				{
				    System.Diagnostics.Debug.WriteLine($"ERROR {pushResult.Status}: {e.RawResult}");
				}
			}

			else
			{
				// TODO: report with HockeyApp
			    System.Diagnostics.Debug.WriteLine($"ERROR {ex.Message}");
			}
		}

		#endregion


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

