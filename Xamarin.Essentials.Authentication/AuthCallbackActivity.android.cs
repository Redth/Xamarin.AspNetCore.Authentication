using Android.App;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Essentials.Authentication
{
	public abstract class AuthCallbackActivity : Activity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			Platform.OnResume(Intent);

			Finish();
		}
	}
}
