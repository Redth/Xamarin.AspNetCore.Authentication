using System;
using System.Collections.Generic;
using System.Text;
using Windows.ApplicationModel.Activation;

namespace Xamarin.Essentials.Authentication
{
	public partial class Platform
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
