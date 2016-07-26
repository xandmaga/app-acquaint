using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace Acquaint.Data
{
	class AcquaintanceSyncHandler : IMobileServiceSyncHandler
	{
	    readonly IMobileServiceClient MobileServiceClient;

		public AcquaintanceSyncHandler(IMobileServiceClient client)
		{
			MobileServiceClient = client;
		}

		public async Task<JObject> ExecuteTableOperationAsync(IMobileServiceTableOperation operation)
		{
			JObject result = null;
			MobileServicePreconditionFailedException conflictError = null;
			do
			{
				try
				{
					result = await operation.ExecuteAsync();
				}
				catch (MobileServicePreconditionFailedException e)
				{
					conflictError = e;
				}

				if (conflictError != null)
				{
					// TODO: implement sync error handling
                    System.Diagnostics.Debug.WriteLine(conflictError.Message);
				}
			} while (conflictError != null);

			return result;
		}

		public Task OnPushCompleteAsync(MobileServicePushCompletionResult result)
		{
			return Task.FromResult(0);
		}
	}
}

