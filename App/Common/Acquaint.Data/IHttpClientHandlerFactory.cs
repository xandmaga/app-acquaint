using System.Net.Http;

namespace Acquaint.Data
{
	public interface IHttpClientHandlerFactory
	{
		HttpClientHandler GetHttpClientHandler();
	}
}

