using System;
using Android.Content;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace MyFriends
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        FriendRepository db = new FriendRepository();
        private ListView lvFriends;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            lvFriends = FindViewById<ListView>(Resource.Id.lvFriends);
            ResetList();
            lvFriends.ItemClick += lvFriends_ItemClick;

        }

        private void lvFriends_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            int friendId = (int)e.Id;
            Android.Content.Intent editIntent = new Intent(this, typeof(DetailActivity));
            editIntent.PutExtra("FriendID", friendId);
            StartActivity(editIntent);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ListViewMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.actionNew:
                    {
                        StartActivity(typeof(DetailActivity));
                        break;
                    }
                case Resource.Id.actionRefresh:
                    {
                        ResetList();
                        break;
                    }


            }
            return base.OnOptionsItemSelected(item);
        }
        protected override void OnResume()
        {
            ResetList();
            base.OnResume();
        }
        void ResetList()
        {
            lvFriends.Adapter = new FriendAdapter(this, db.GetAllFriend());
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}