using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.AspNetCore.Auth.Mobile;
using Xamarin.Forms;

namespace MobileSample
{
	// Learn more about making custom code visible in the Xamarin.Forms previewer
	// by visiting https://aka.ms/xamarinforms-previewer
	[DesignTimeVisible(false)]
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
		}

		private async void Facebook_Clicked(object sender, EventArgs e)
			=> await AuthenticateAsync("Facebook");

		private async void Apple_Clicked(object sender, EventArgs e)
			=> await AuthenticateAsync("Apple");

		async Task AuthenticateAsync (string scheme)
		{
			AuthResult result = null;

			try
			{
				result = await Authentication.AuthenticateAsync(
					new Uri("myapp://"),
					new Uri("https://myapp.com"),
					scheme);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			HandleResult(result);
		}

		void HandleResult(AuthResult authResult)
		{
			Console.WriteLine(authResult?.AccessToken ?? "No Access Token");
		}
	}
}
