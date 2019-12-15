using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;

namespace Xamarin.Essentials.Authentication
{
	public static partial class Platform
	{
		static ActivityLifecycleContextListener lifecycleListener;

		internal static Context AppContext =>
			Application.Context;


		public static Activity GetCurrentActivity()
			=> GetCurrentActivity(false);

		public static Context GetContext()
			=> GetCurrentActivity(false) ?? Application.Context;

		internal static Activity GetCurrentActivity(bool throwOnNull)
		{
			var activity = lifecycleListener?.Activity;
			if (throwOnNull && activity == null)
				throw new NullReferenceException("The current Activity can not be detected. Ensure that you have called Init in your Activity or Application class.");

			return activity;
		}

		public static void Init(Application application)
		{
			lifecycleListener = new ActivityLifecycleContextListener();
			application.RegisterActivityLifecycleCallbacks(lifecycleListener);
		}

		public static void Init(Activity activity, Bundle bundle)
		{
			Init(activity.Application);
			lifecycleListener.Activity = activity;
		}

		public static bool OnResume(Intent intent)
			=> WebAuthenticator.Callback(intent);
	}

	class ActivityLifecycleContextListener : Java.Lang.Object, Application.IActivityLifecycleCallbacks
	{
		readonly WeakReference<Activity> currentActivity = new WeakReference<Activity>(null);

		internal Context Context =>
			Activity ?? Application.Context;

		internal Activity Activity
		{
			get => currentActivity.TryGetTarget(out var a) ? a : null;
			set => currentActivity.SetTarget(value);
		}

		void Application.IActivityLifecycleCallbacks.OnActivityCreated(Activity activity, Bundle savedInstanceState) =>
			Activity = activity;

		void Application.IActivityLifecycleCallbacks.OnActivityDestroyed(Activity activity)
		{
		}

		void Application.IActivityLifecycleCallbacks.OnActivityPaused(Activity activity) =>
			Activity = activity;

		void Application.IActivityLifecycleCallbacks.OnActivityResumed(Activity activity) =>
			Activity = activity;

		void Application.IActivityLifecycleCallbacks.OnActivitySaveInstanceState(Activity activity, Bundle outState)
		{
		}

		void Application.IActivityLifecycleCallbacks.OnActivityStarted(Activity activity)
		{
		}

		void Application.IActivityLifecycleCallbacks.OnActivityStopped(Activity activity)
		{
		}
	}
}