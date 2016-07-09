using System.Threading.Tasks;
using System.Collections.Generic;

namespace Acquaint.Data
{
	public interface IDataSource<T> where T : ObservableEntityData
	{
		Task<bool> Initialize();
		Task<IEnumerable<T>> GetItems();
		Task<T> GetItem(string id);
		Task<bool> AddItem(T item);
		Task<bool> UpdateItem(T item);
		Task<bool> RemoveItem(T item);
		Task<bool> SyncItems();
	}
}

