using System.Net.Http;
using Acquaint.Data;
using Acquaint.XForms.iOS;

[assembly: Xamarin.Forms.Dependency(typeof(HttpClientHandlerFactory))]

namespace Acquaint.XForms.iOS
{
	/// <summary>
	/// Http client handler factory for iOS. Allows the simulator to use the host operating systems's (OS X) proxy settings in order to allow debugging of HTTP requests with tools such as Charles.
	/// Only used for debugging, not production.
	/// </summary>
	public class HttpClientHandlerFactory : IHttpClientHandlerFactory
	{

		HttpClientHandler IHttpClientHandlerFactory.GetHttpClientHandler()
		{
			return new System.Net.Http.HttpClientHandler
			{
				Proxy = CoreFoundation.CFNetwork.GetDefaultProxy(),
				UseProxy = true
			};
		}
	}
}

