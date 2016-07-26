using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Acquaint.Util;
using Xamarin.Forms;

namespace Acquaint.XForms
{
    public partial class SetupPage : ContentPage
    {
        public SetupPage()
        {
            BindingContext = this; // No need for all the ceremony of a viewmodel in this case. Just bind to ourself.

            InitializeComponent();
        }

        Command _ContinueCommand;

        public Command ContinueCommand => _ContinueCommand ?? (_ContinueCommand = new Command(async () => await ExecuteContinueCommand()));

        async Task ExecuteContinueCommand()
        {
            if (string.IsNullOrWhiteSpace(DataPartitionPhraseEntry.Text))
            {
                DataPartitionPhraseEntry.PlaceholderColor = Color.Red;

                DataPartitionPhraseEntry.Focus();

                return;
            }

            Settings.DataPartitionPhrase = DataPartitionPhraseEntry.Text;

            // The navigation logic startup needs to diverge per platform in order to meet the UX design requirements
            if (Device.OS != TargetPlatform.Android)
            {
                await Navigation.PopModalAsync();

                var navPage = new NavigationPage(
                    new AcquaintanceListPage()
                    {
                        Title = "Acquaintances",
                        BindingContext = new AcquaintanceListViewModel()
                    })
                {
                    BarBackgroundColor = Color.FromHex("547799")
                };

                navPage.BarTextColor = Color.White;

                // on the main UI thread, set the MainPage to the navPage
                Device.BeginInvokeOnMainThread(() =>
                    Application.Current.MainPage = navPage );
            }
            else
            {
                await Navigation.PopModalAsync();

                await Navigation.PushAsync(new AcquaintanceListPage() { Title = "Acquaintances", BindingContext = new AcquaintanceListViewModel() });
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // disable back button, so that the user is forced to enter a DataPartitionPhrase
            return true;
        }
    }
}

