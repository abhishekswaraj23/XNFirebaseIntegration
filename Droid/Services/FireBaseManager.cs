using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;
using Firebase;
using Firebase.Auth;
using Xamarin.Facebook;

namespace XNFireBasePOC.Droid.Services
{
    public class FireBaseManager
    {
        public FirebaseAuth mAuth;
        private static FireBaseManager instance;

        Context mContext;

        private FireBaseManager()
        {

        }

        public static FireBaseManager GetInstance()
        {
            if (instance == null)
            {
                instance = new FireBaseManager();
            }
            return instance;
        }

        //public async Task<string> AuthenticateUsingGoogle()
        //{
        //    FirebaseAuth.Instance.sig
        //}

        //public async Task<string> AuthenticateUsingFacebook()
        //{

        //}

        //sign in with email
        public async Task<string> AuthenticateUsingEmail(string email1, string password1)
        {
            try
            {
                string email = "abhishekswaraj23@gmail.com";
                string password = "Igotit123";

                var user = await mAuth.SignInWithEmailAndPasswordAsync(email, password);
                var token = await user.User.GetIdTokenAsync(false);
                return token.Token;
            }
            catch (FirebaseAuthInvalidUserException e)
            {
                e.PrintStackTrace();
                return string.Empty;
            }
            catch (FirebaseAuthInvalidCredentialsException e)
            {
                e.PrintStackTrace();
                return string.Empty;

            }
        }

        public void SignOutUser()
        {
            mAuth.SignOut();
        }

        //public async Task<string> AuthenticateUsingCredential(AuthCredential cred)
        //{
        //    try
        //    {

        //        var user = mAuth.SignInWithCredential(cred);
        //        //var token = await user.User.GetIdTokenAsync(false);
        //        return token.Token;
        //    }
        //    catch (FirebaseAuthInvalidUserException e)
        //    {
        //        e.PrintStackTrace();
        //        return string.Empty;
        //    }
        //    catch (FirebaseAuthInvalidCredentialsException e)
        //    {
        //        e.PrintStackTrace();
        //        return string.Empty;

        //    }
        //}

        public void InitializeFireBase(Context context)
        {
            var app = FirebaseApp.InitializeApp(context);
            mContext = context;

            if (app == null)
            {
                var options = new FirebaseOptions.Builder()
                    .SetApplicationId("testproject-26b97")
                    .SetApiKey("AIzaSyCqsCzTUJb4x3esTr40iJx7rZr5flvZzxg")
                    //.SetDatabaseUrl("")
                    //.SetStorageBucket("")
                    .Build();
                app = FirebaseApp.InitializeApp(context, options);
                mAuth = FirebaseAuth.GetInstance(app);
            }
            else
            {
                mAuth = FirebaseAuth.Instance;
                if (mAuth == null)
                {
                    mAuth = new FirebaseAuth(app);
                }
            }
        }


    }
}
