using System.Collections.Generic;
using System.Threading.Tasks;

namespace Acquaint.Abstractions
{
	public interface IDataSource<T> where T : IObservableEntityData
	{
		Task<IEnumerable<T>> GetItems();
		Task<T> GetItem(string id);
		Task<bool> AddItem(T item);
		Task<bool> UpdateItem(T item);
		Task<bool> RemoveItem(T item, bool softDelete = true);
	}
}

