using System;
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Acquaint.Util
{
	public static class Settings
	{
		private static ISettings AppSettings
		{
			get { return CrossSettings.Current; }
		}

		private const string DataIsSeededKey = "firstSyncIsComplete_key";
		private static readonly bool DataIsSeededDefault = false;

		private const string AzureAppServiceUrlKey = "AzureAppServiceUrl_key";
		private static readonly string AzureAppServiceUrlDefault = "http://app-acquaint.azurewebsites.net";

		private const string DataPartitionIdKey = "DataPartitionId_key";
		private static readonly string DataPartitionIdDefault = "01D676FD-789A-4488-B519-1840E080936E";

		private const string HockeyAppIdKey = "HockeyAppId_key";
		private static readonly string HovkeyAppIdDefault = "11111111222222223333333344444444"; // This is just a placeholder value. Replace with your real HockeyApp App ID.

		public static bool DataIsSeeded
		{
			get { return AppSettings.GetValueOrDefault<bool>(DataIsSeededKey, DataIsSeededDefault); }
			set { AppSettings.AddOrUpdateValue<bool>(DataIsSeededKey, value); }
		}

		public static string AzureAppServiceUrl
		{
			get { return AppSettings.GetValueOrDefault<string>(AzureAppServiceUrlKey, AzureAppServiceUrlDefault); }
			set { AppSettings.AddOrUpdateValue<string>(AzureAppServiceUrlKey, value); }
		}

		public static string DataPartitionId
		{
			get { return AppSettings.GetValueOrDefault<string>(DataPartitionIdKey, DataPartitionIdDefault); }
			set { AppSettings.AddOrUpdateValue<string>(DataPartitionIdKey, value); }
		}

		public static string HockeyAppId
		{
			get { return AppSettings.GetValueOrDefault<string>(HockeyAppIdKey, HovkeyAppIdDefault); }
			set { AppSettings.AddOrUpdateValue<string>(HockeyAppIdKey, value); }
		}
	}
}

