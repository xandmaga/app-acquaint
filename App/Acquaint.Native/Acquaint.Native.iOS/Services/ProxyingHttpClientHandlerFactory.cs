using System.Net.Http;
using Acquaint.Data;

namespace Acquaint.Native.iOS.Services
{
	/// <summary>
	/// Http client handler factory for iOS. Allows the simulator to use the host operating systems's (OS X) proxy settings in order to allow debugging of HTTP requests with tools such as Charles.
	/// Only used for debugging, not production.
	/// </summary>
	public class ProxyingHttpClientHandlerFactory : IHttpClientHandlerFactory
	{

		HttpClientHandler IHttpClientHandlerFactory.GetHttpClientHandler()
		{
			return new HttpClientHandler
			{
				Proxy = CoreFoundation.CFNetwork.GetDefaultProxy(),
				UseProxy = true
			};
		}
	}
}

