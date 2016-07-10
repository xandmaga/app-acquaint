using System;
using System.Net.Http;
using Acquaint.Data;
using Acquaint.XForms.Droid;

[assembly: Xamarin.Forms.Dependency(typeof(NullHttpClientHandlerFactory))]

namespace Acquaint.XForms.Droid
{
	public class NullHttpClientHandlerFactory : IHttpClientHandlerFactory
	{
		HttpClientHandler IHttpClientHandlerFactory.GetHttpClientHandler()
		{
			return null;
		}
	}
}

