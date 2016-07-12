using System.Net.Http;
using Acquaint.Abstractions;

namespace Acquaint.Common.iOS
{
	/// <summary>
	/// Http client handler factory for iOS. Allows the simulator to use the host operating systems's (OS X) proxy settings in order to allow debugging of HTTP requests with tools such as Charles.
	/// Only used for debugging, not production.
	/// </summary>
	public class HttpClientHandlerFactory : IHttpClientHandlerFactory
	{

		public HttpClientHandler GetHttpClientHandler()
		{
#if DEBUG
			return new HttpClientHandler
			{
				Proxy = CoreFoundation.CFNetwork.GetDefaultProxy(),
				UseProxy = true
			};
#else
			return null;
#endif
		}
	}
}

