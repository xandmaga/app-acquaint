using System;
using System.Collections.Generic;
using FormsToolkit;
using Xamarin.Forms;

namespace Acquaint.XForms
{
	public partial class SettingsPage : ContentPage
	{
		protected SettingsViewModel ViewModel => BindingContext as SettingsViewModel;

		public SettingsPage()
		{
			InitializeComponent();

			MessagingService.Current.Subscribe(MessageKeys.DataPartitionPhraseValidation, (service) => {
				DataPartitionPhraseEntry.PlaceholderColor = Color.Red;
				DataPartitionPhraseEntry.Focus();
			});

			BackendServiceUrlEntry.Focused += (o, e) => {
				if (BackendServiceUrlEntry.Text.EndsWith(InvalidUrlFormatMessage))
				{
					BackendServiceUrlEntry.Text = BackendServiceUrlEntry.Text.Replace(InvalidUrlFormatMessage, string.Empty);
				}
			};
		}

		const string InvalidUrlFormatMessage = " (Invalid URL format!)";

		void BackendServiceUrlEntry_Focused(object sender, FocusEventArgs e)
		{
			
		}
	}
}

