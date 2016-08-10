using System.Threading.Tasks;
using Acquaint.Abstractions;
using Acquaint.Models;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace Acquaint.Data
{
	public class AcquaintanceSyncHandler : IMobileServiceSyncHandler
	{
		public async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
		{
			MobileServicePreconditionFailedException acquaintanceSyncException = null;
			JObject result = null;

			do
			{
				acquaintanceSyncException = null;
				try
				{
					result = await operation.ExecuteAsync();
				}
				catch (MobileServicePreconditionFailedException ex)
				{
					acquaintanceSyncException = ex;
				}

				// there is a conflict between the local version and the server version of the item
				if (acquaintanceSyncException != null)
				{
					var localItem = operation.Item.ToObject<Acquaintance>();
					var serverItem = acquaintanceSyncException.Value.ToObject<Acquaintance>();

					RaiseDataSyncErrorEvent(new DataSyncErrorEventArgs<Acquaintance>(localItem, serverItem));

					operation.AbortPush();

					return result;

					//// Update the version of the pending item so that it won't have another
					//// conflict when the operation is retried.
					//operation.Item[MobileServiceSystemColumns.Version] = serverItem[MobileServiceSystemColumns.Version];

					//// One shall decide on a default action to be done in a conflict scenario. Server values override or client values overrides
					//// The client can be shown with some sort of UI to let the end user decide on what has to be done with the conflicting record!
					//// Finally, one has to update the final value to be updated in the server and re-do the ExecuteAsync operation.
					////operation.Item["notes"] = MergeNotes(thisNotes, serverNotes);
				}
			} 
			while (acquaintanceSyncException != null);

			return result;
		}

		public Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
		{
			return Task.FromResult(0);
		}

		/// <summary>
		/// An event that is fired when a data sync error occurs.
		/// </summary>
		public event DataSyncErrorEventHandler<Acquaintance> OnDataSyncError;

		/// <summary>
		/// Raises the data sync error event.
		/// </summary>
		/// <param name="e">A DataSyncErrorEventArgs or type T.</param>
		protected virtual void RaiseDataSyncErrorEvent(DataSyncErrorEventArgs<Acquaintance> e)
		{
			DataSyncErrorEventHandler<Acquaintance> handler = OnDataSyncError;

			if (handler != null)
				handler(this, e);
		}
	}
}

