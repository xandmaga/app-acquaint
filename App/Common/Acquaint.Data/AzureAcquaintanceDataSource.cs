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

			await MobileService.SyncContext.InitializeAsync(store);

			_IsInitialized = true;

			return _IsInitialized;
		}

		#region Data Access

		public async Task<Acquaintance> GetItem(string id)
		{
			await SyncItems();
			return await MobileService.GetSyncTable<Acquaintance>().LookupAsync(id);
		}

		public async Task<IEnumerable<Acquaintance>> GetItems()
		{
			await SyncItems();
			return await MobileService.GetSyncTable<Acquaintance>().Where(x => x.DataPartitionId == _DataPartitionId).OrderBy(x => x.LastName).ToEnumerableAsync();
		}

		public async Task<bool> AddItem(Acquaintance item)
		{
			await Initialize();
			await MobileService.GetSyncTable<Acquaintance>().InsertAsync(item);
			await SyncItems();
			return true;
		}

		public async Task<bool> UpdateItem(Acquaintance item)
		{
			await Initialize();
			await MobileService.GetSyncTable<Acquaintance>().UpdateAsync(item);
			await SyncItems();
			return true;
		}

		public async Task<bool> RemoveItem(Acquaintance item)
		{
			await Initialize();
			await MobileService.GetSyncTable<Acquaintance>().DeleteAsync(item);
			await SyncItems();
			return true;
		}

		public async Task<bool> SyncItems()
		{
			await Initialize();

			try
			{
				await EnsureDataIsSeededForPartitionId(_DataPartitionId);
				await MobileService.SyncContext.PushAsync();
				await MobileService.GetSyncTable<Acquaintance>().PullAsync($"all{typeof(Acquaintance).Name}", MobileService.GetSyncTable<Acquaintance>().CreateQuery());
				return true;
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Error during Sync occurred: {ex.Message}");
				return false;
			}
		}
		#endregion

		async Task EnsureDataIsSeededForPartitionId(string dataPartitionId)
		{
			if (Settings.DataIsSeeded)
				return;

			await MobileService.GetSyncTable<Acquaintance>().PullAsync($"all{typeof(Acquaintance).Name}", MobileService.GetSyncTable<Acquaintance>().CreateQuery());
			var any = (await MobileService.GetSyncTable<Acquaintance>().Where(x => x.DataPartitionId == _DataPartitionId).OrderBy(x => x.LastName).ToEnumerableAsync()).Any();

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

