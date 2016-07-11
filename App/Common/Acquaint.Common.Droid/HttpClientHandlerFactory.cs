using System.Net.Http;
using Acquaint.Abstractions;

namespace Acquaint.Common.Droid
{
	public class HttpClientHandlerFactory : IHttpClientHandlerFactory
	{
		HttpClientHandler IHttpClientHandlerFactory.GetHttpClientHandler()
		{
			return null;
		}
	}
}

