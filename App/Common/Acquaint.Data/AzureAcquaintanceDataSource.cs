using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Acquaint.Data
{
	public class AzureAcquaintanceDataSource : IDataSource<Acquaintance>
	{
		public MobileServiceClient _MobileService { get; set; }

		IMobileServiceSyncTable<Acquaintance> _AcquaintanceTable;

		readonly string _ServiceUrl = "https://app-acquaint.azurewebsites.net";

		readonly string _DataPartitionId = "01d676fd-789a-4488-b519-1840e080936e";

		bool _IsInitialized;

		public async Task Init()
		{
			if (_IsInitialized)
				return;

			_MobileService = new MobileServiceClient(_ServiceUrl, null);

			var store = new MobileServiceSQLiteStore("acquaintances.db");

			store.DefineTable<Acquaintance>();

			try
			{
				await _MobileService.SyncContext.InitializeAsync(store);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine(@"Failed to initialize sync context: {0}", ex.Message);
			}

			_AcquaintanceTable = _MobileService.GetSyncTable<Acquaintance>();

			await Fullsync();

			_IsInitialized = true;
		}

		async Task Fullsync()
		{
			await Execute(async () => {
				await _MobileService.SyncContext.PushAsync();
				await _AcquaintanceTable.PullAsync(null, _AcquaintanceTable.Where(x => x.DataPartitionId == _DataPartitionId));
			});
		}

		async Task DeltaSync()
		{
			await Execute(async () => {
				if (!_IsInitialized)
				{
					await Init();
					return;
				}

				await Execute(async () => {
					await _MobileService.SyncContext.PushAsync();
					await _AcquaintanceTable.PullAsync("deltaSyncAcquaintances", _AcquaintanceTable.Where(x => x.DataPartitionId == _DataPartitionId));
				});
			});
		}

		#region IDataSource implementation

		public async Task SaveItem(Acquaintance item)
		{
			await Execute(async () => { 
				await DeltaSync();
				if (item.Id == null)
					await _AcquaintanceTable.InsertAsync(item);
				else
					await _AcquaintanceTable.UpdateAsync(item);
			});
		}

		public async Task DeleteItem(string id)
		{
			await Execute(async () => { 
				await DeltaSync();
				var item = await GetItem(id);
				if (item != null)
					await _AcquaintanceTable.DeleteAsync(item);
			});
		}

		public async Task<Acquaintance> GetItem(string id)
		{
			return await Execute(async () => { 
				await DeltaSync();
				return (await _AcquaintanceTable.Where(acquaintance => acquaintance.Id == id).ToEnumerableAsync()).SingleOrDefault();
			}, null);


		}

		public async Task<ICollection<Acquaintance>> GetItems(int start = 0, int count = 100, string query = "")
		{
			return await Execute<ICollection<Acquaintance>>(async () => { 
				await DeltaSync();
				return (await _AcquaintanceTable.Where(acquaintance => acquaintance.DataPartitionId == _DataPartitionId).OrderBy(b => b.Company).ToCollectionAsync());
			}, new Collection<Acquaintance>());
		}

		#endregion


		#region some nifty helpers

		static async Task Execute(Func<Task> execute)
		{
			try
			{
				await execute();
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

		static async Task<T> Execute<T>(Func<Task<T>> execute, T defaultReturnObject)
		{
			try
			{
				return await execute();
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

