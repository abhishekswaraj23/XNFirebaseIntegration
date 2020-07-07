
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using Android.App;
using Android.Content;
using Android.Gms.Common;
using Android.Gms.Extensions;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Firebase.Iid;
using XNFireBasePOC.Droid.Services;

namespace XNFireBasePOC.Droid.Activities
{
    [Activity(Label = "HomeActivity")]
    public class HomeActivity : Activity
    {

        FireBaseManager fireBaseManager;
        TextView mTvWelcome;
        Button mBtnSignOut;
        ImageView mIvProfile;
        Android.Net.Uri profile;
        TextView mPSMsg;

        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.HomePage);
            fireBaseManager = FireBaseManager.GetInstance();
            mTvWelcome = FindViewById<TextView>(Resource.Id.txtWelcome);

            var user = fireBaseManager.mAuth?.CurrentUser;

            if (user !=null)
            {
                mTvWelcome.Text = $"Welcome {user.DisplayName}";
                profile = user.PhotoUrl;
            }

            mBtnSignOut = FindViewById<Button>(Resource.Id.btnSignOut);
            mBtnSignOut.Click += MBtnSignOut_Click;

            mIvProfile = FindViewById<ImageView>(Resource.Id.ivProfile);


            //mIvProfile.SetImageURI(profile);
            if (profile != null)
            {
                var bit = GetImageBitmapFromUrl(profile?.ToString());
                mIvProfile.SetImageBitmap(bit);
            }
            // PushNotificatiob

            mPSMsg = FindViewById<TextView>(Resource.Id.playServiceMsg);

            if (Intent.Extras != null)
            {
                foreach (var key in Intent.Extras.KeySet())
                {
                    var value = Intent.Extras.GetString(key);
                    Log.Debug("Dev PushNot", "Key: {0} Value: {1}", key, value);
                }
            }
            IsPlayServicesAvailable();
            CreateNotificationChannel();
            var logTokenButton = FindViewById<Button>(Resource.Id.logTokenButton);
            logTokenButton.Click +=  delegate {
                //var instanceIdResult = await FirebaseInstanceId.Instance.GetInstanceId().AsAsync<IInstanceIdResult>();
                //var token = instanceIdResult.Token;
                Log.Debug("Dev PushNot", "InstanceID token: " + FirebaseInstanceId.Instance?.Token);
            };
        }

        private void MBtnSignOut_Click(object sender, EventArgs e)
        {
            Toast.MakeText(this, $"Signing out User - {fireBaseManager.mAuth?.CurrentUser}", ToastLength.Short).Show();
            fireBaseManager.SignOutUser();
            if(fireBaseManager.mAuth?.CurrentUser == null)
            {
                Toast.MakeText(this, $"Signing out Successfull.", ToastLength.Short).Show();
                Finish();
            }
            
        }
        /// <summary>
        /// download image byte and convert to bitmap
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private Bitmap GetImageBitmapFromUrl(string url)
        {
            Bitmap imageBitmap = null;

            using (var webClient = new WebClient())
            {
                var imageBytes = webClient.DownloadData(url);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }

            return imageBitmap;
        }

        /// <summary>
        /// Check Play Service available -- Push Notification
        /// </summary>
        /// <returns></returns>
        public bool IsPlayServicesAvailable()
        {
            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                if (GoogleApiAvailability.Instance.IsUserResolvableError(resultCode))
                    mPSMsg.Text = GoogleApiAvailability.Instance.GetErrorString(resultCode);
                else
                {
                    mPSMsg.Text = "This device is not supported for play service.";
                    //Finish();
                }
                return false;
            }
            else
            {
                mPSMsg.Text = "Google Play Services is available.";
                return true;
            }
        }
        /// <summary>
        /// Create Notification channel - PushNotification
        /// </summary>
        void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                // Notification channels are new in API 26 (and not a part of the
                // support library). There is no need to create a notification
                // channel on older versions of Android.
                return;
            }

            var channel = new NotificationChannel(CHANNEL_ID,
                                                  "FCM Notifications",
                                                  NotificationImportance.Default)
            {

                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}
