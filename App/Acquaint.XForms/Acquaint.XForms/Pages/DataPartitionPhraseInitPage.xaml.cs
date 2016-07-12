using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acquaint.Util;
using Xamarin.Forms;

namespace Acquaint.XForms
{
	public partial class DataPartitionPhraseInitPage : ContentPage
	{
		public DataPartitionPhraseInitPage()
		{
			BindingContext = this;

			InitializeComponent();
		}

		Command _ContinuCommand;

		public Command ContinueCommand => _ContinuCommand ?? (_ContinuCommand = new Command(async () => await ExecuteContinueCommand()));

		async Task ExecuteContinueCommand()
		{
			if (!string.IsNullOrWhiteSpace(DataPartitionPhraseEntry.Text))
			{
				Settings.DataSeedPhrase = DataPartitionPhraseEntry.Text;

				await Navigation.PopModalAsync();
			}
		}
	}
}

