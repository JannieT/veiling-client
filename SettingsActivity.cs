using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Preferences;

namespace Veiling
{
    [Activity(Label = "Settings")]          
    public class SettingsActivity : PreferenceActivity
    {
        public const string KEY_IP = "server_ip";
        public const string KEY_API_KEY = "api_key";
        public const string KEY_MAX = "max_number";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Display the fragment as the main content.
            FragmentManager.BeginTransaction()
                .Replace(Android.Resource.Id.Content, new PrefsFragment())
                .Commit();

        }
    }
}

