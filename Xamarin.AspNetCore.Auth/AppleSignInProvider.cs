using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.AspNetCore.Auth
{
	public class AppleSignInOptions
	{
		public string KeyId { get; set; }

		public string TeamId { get; set; }

		public string ServerId { get; set; }

		public string P8Key { get; set; }
	}
}
