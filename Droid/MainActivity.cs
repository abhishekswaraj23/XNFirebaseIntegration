using Android.App;
using Android.Widget;
using Android.OS;
using XNFireBasePOC.Droid.Activities;
using XNFireBasePOC.Droid.Services;
using System.Threading.Tasks;
using Xamarin.Facebook;
using Java.Lang;
using Xamarin.Facebook.Login;
using Firebase.Auth;
using Android.Gms.Tasks;
using Xamarin.Facebook.Login.Widget;
using Android.Runtime;
using Android.Content;
using Android.Gms.Auth.Api.SignIn;
using Android.Gms.Common.Apis;
using Android.Gms.Auth.Api;
using Android.Gms.Common;
using Android.Support.V7.App;
using System;

namespace XNFireBasePOC.Droid
{
    [Activity(Label = "XNFireBasePOC", MainLauncher = true, Icon = "@mipmap/icon", Theme = "@style/Theme.AppCompat")]
    [IntentFilter(new[] { Intent.ActionView },
        DataScheme = "@string/fb_login_protocol_scheme"),]
    public class MainActivity : AppCompatActivity, IFacebookCallback, IOnCompleteListener, GoogleApiClient.IConnectionCallbacks, GoogleApiClient.IOnConnectionFailedListener
    {
        FireBaseManager fireBaseManager;
        private ICallbackManager mCallbackManager;

        const int RC_SIGN_IN = 9001;
        const int SIGN_OUT = 9005;

        GoogleApiClient mGoogleApiClient;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            fireBaseManager = FireBaseManager.GetInstance();
            fireBaseManager.InitializeFireBase(Application.Context);





            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button btnEmail = FindViewById<Button>(Resource.Id.btnEmail);
            Button btnGoogle = FindViewById<Button>(Resource.Id.btnGoogle);

            btnEmail.Click += BtnEmail_Click;
            btnGoogle.Click += BtnGoogle_Click;

            ConfigureGoogleSignIn();
        }

        protected override void OnStart()
        {
            base.OnStart();
            ConfigureFacebookSignIn();
        }

        protected override void OnPause()
        {
            base.OnPause();
            mGoogleApiClient.StopAutoManage(this);
            mGoogleApiClient.Disconnect();

        }

        private void ConfigureFacebookSignIn()
        {
            FacebookSdk.SdkInitialize(this.ApplicationContext);

            LoginButton fblogin = FindViewById<LoginButton>(Resource.Id.btnFBLogin);
            fblogin.SetReadPermissions("email", "public_profile");

            mCallbackManager = CallbackManagerFactory.Create();
            fblogin.RegisterCallback(mCallbackManager, this);
        }

        private void ConfigureGoogleSignIn()
        {
            GoogleSignInOptions gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestIdToken("402739675097-9m67mrteg0df8qm3nas2aa61il5gril9.apps.googleusercontent.com")
                .RequestEmail()
                .Build();

            mGoogleApiClient = new GoogleApiClient.Builder(this)
                .EnableAutoManage(this, this)
                .AddApi(Auth.GOOGLE_SIGN_IN_API, gso)
                .Build();



        }

        private void BtnGoogle_Click(object sender, System.EventArgs e)
        {
            var intent = Auth.GoogleSignInApi.GetSignInIntent(mGoogleApiClient);
            StartActivityForResult(intent, RC_SIGN_IN);
            //await doAuth;
            //StartActivity(typeof(HomeActivity));
        }

        private async void BtnEmail_Click(object sender, System.EventArgs e)
        {
            await fireBaseManager.AuthenticateUsingEmail("", "");
            StartActivity(typeof(HomeActivity));
        }


        //facebook on cancel
        public void OnCancel()
        {
            Toast.MakeText(this, "Authentication Cancelled.", ToastLength.Short).Show();

        }
        //facebook on error
        public void OnError(FacebookException error)
        {
            Toast.MakeText(this, "Authentication Error. - " + error.ToString(), ToastLength.Short).Show();
        }

        //facebook on success
        public void OnSuccess(Java.Lang.Object result)
        {
            Toast.MakeText(this, "Authentication Successful.", ToastLength.Short).Show();
            LoginResult loginResult = result as LoginResult;


            handleFacebookAccessToken(loginResult.AccessToken);
            //throw new System.NotImplementedException();
        }

        /// <summary>
        /// handle facebook token
        /// </summary>
        /// <param name="accessToken"></param>
        private void handleFacebookAccessToken(AccessToken accessToken)
        {
            AuthCredential credential = FacebookAuthProvider.GetCredential(accessToken.Token);

            fireBaseManager.mAuth.SignInWithCredential(credential).AddOnCompleteListener(this, this);

        }

        /// <summary>
        /// Ioncompltion interface listner firebase
        /// </summary>
        /// <param name="task"></param>
        public void OnComplete(Android.Gms.Tasks.Task task)
        {
            if (task.IsSuccessful)
            {
                FirebaseUser user = fireBaseManager.mAuth.CurrentUser;
                Toast.MakeText(this, "Authentication Successfull. -" + user.DisplayName, ToastLength.Short).Show();
                StartActivityForResult(typeof(HomeActivity), SIGN_OUT);
            }
            else
            {
                Toast.MakeText(this, "Authentication failed. -" + task.Exception.ToString(), ToastLength.Short).Show();
                System.Console.WriteLine(task.Exception.ToString());

            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (requestCode == RC_SIGN_IN)//google login
            {

                var result = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);

                HandleGoogleSignInResult(result);

            }
            else if (requestCode == SIGN_OUT)
            {
                //signout
                AccessToken fbToken = AccessToken.CurrentAccessToken;
                bool isLoggedIn = fbToken != null && !fbToken.IsExpired;
                if (isLoggedIn)
                {
                    LoginManager.Instance.LogOut();
                }
                if (mGoogleApiClient.IsConnected)
                {
                    mGoogleApiClient.Disconnect();
                }
            }
            else //facebook login
            {
                var resultCodeNum = 0;
                switch (resultCode)
                {
                    case Result.Ok:
                        resultCodeNum = -1;
                        break;

                    case Result.Canceled:
                        resultCodeNum = 0;
                        break;

                    case Result.FirstUser:
                        resultCodeNum = 1;
                        break;
                }
                mCallbackManager.OnActivityResult(requestCode, resultCodeNum, data);
            }
        }

        /// <summary>
        /// Handle google result
        /// </summary>
        /// <param name="result"></param>
        private void HandleGoogleSignInResult(GoogleSignInResult result)
        {
            if (result.IsSuccess)
            {
                var accountDetails = result.SignInAccount;
                var gCred = GoogleAuthProvider.GetCredential(accountDetails.IdToken, null);
                fireBaseManager.mAuth.SignInWithCredential(gCred).AddOnCompleteListener(this, this);

                Toast.MakeText(this, "Google Authentication Success. -" + accountDetails.DisplayName, ToastLength.Short).Show();
                //StartActivity(typeof(HomeActivity));
            }
            else
            {
                Toast.MakeText(this, "Google Authentication failed. -" + result.Status, ToastLength.Short).Show();
                System.Console.WriteLine(result.Status.ToString());
            }
        }


        //google
        public void OnConnected(Bundle connectionHint)
        {
            //throw new System.NotImplementedException();
        }
        //google
        public void OnConnectionSuspended(int cause)
        {
            //throw new System.NotImplementedException();
        }
        //google
        public void OnConnectionFailed(ConnectionResult result)
        {
            //throw new System.NotImplementedException();
        }
    }
}

