using System;
using Microsoft.WindowsAzure.MobileServices;
using MvvmHelpers;
using Newtonsoft.Json;

namespace Acquaint.Data
{
	public class ObservableEntityData : ObservableObject
	{
		public ObservableEntityData()
		{
			Id = Guid.NewGuid().ToString();
		}

		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }

		[CreatedAt]
		public DateTimeOffset CreatedAt { get; set; }

		[UpdatedAt]
		public DateTimeOffset UpdatedAt { get; set; }

		[Version]
		public string AzureVersion { get; set; }
	}
}

