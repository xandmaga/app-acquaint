using System;
using System.Net.Http;
using Acquaint.Data;

namespace Acquaint.Native.Droid
{
	public class NullHttpClientHandlerFactory : IHttpClientHandlerFactory
	{
		HttpClientHandler IHttpClientHandlerFactory.GetHttpClientHandler()
		{
			return null;
		}
	}
}

