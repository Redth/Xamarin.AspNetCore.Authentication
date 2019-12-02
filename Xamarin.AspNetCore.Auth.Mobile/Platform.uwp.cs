using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Activation;

namespace Xamarin.AspNetCore.Auth.Mobile
{
	public class Platform
	{
		public static void OnActivated(IActivatedEventArgs args)
		{
			if (args.Kind == ActivationKind.Protocol)
			{
				var protocolArgs = args as ProtocolActivatedEventArgs;

				WebAuthenticator.Callback(protocolArgs.Uri);
			}
		}
	}
}
