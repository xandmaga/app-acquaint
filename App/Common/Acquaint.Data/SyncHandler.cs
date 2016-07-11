using System.Threading.Tasks;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Newtonsoft.Json.Linq;

namespace Acquaint.Data
{
	class MySyncHandler : IMobileServiceSyncHandler
	{
		IMobileServiceClient MobileServiceClient;

		public MySyncHandler(IMobileServiceClient client)
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
					// There was a conflict on the server. Let’s “fix” it by
					// forcing the client entity
					JObject serverItem = conflictError.Value;

					// In most cases, the server will return the server item in the request body
					// when a Precondition Failed is returned, but it’s not guaranteed for all
					// backend types.
					if (serverItem == null)
					{
						// If this is the case, let’s retrieve it. First get a table instance
						var table = MobileServiceClient.GetTable(operation.Table.TableName);

						// Force it to request the __version property
						//table.SystemProperties = MobileServiceSystemProperties.Version;

						serverItem = (JObject)(await table.LookupAsync((string)operation.Item[MobileServiceSystemColumns.Id]));
					}

					// Now update the local item with the server version
					operation.Item[MobileServiceSystemColumns.Version] = serverItem[MobileServiceSystemColumns.Version];
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

