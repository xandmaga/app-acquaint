using System.Net.Http;

namespace Acquaint.Abstractions
{
	public interface IHttpClientHandlerFactory
	{
		HttpClientHandler GetHttpClientHandler();
	}
}

