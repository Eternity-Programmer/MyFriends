using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX.AppCompat.App;
using Android.Provider;
using Android.Content.PM;
using Java.IO;
using Android.Locations;


namespace MyFriends
{
    [Activity(Label = "Add / Edit", Theme = "@style/AppTheme", MainLauncher = false)]
    public class DetailActivity : AppCompatActivity,ILocationListener
    {
        private EditText txtName;
        private EditText txtEmail;
        private EditText txtPhone;
        private EditText txtLat;
        private EditText txtLong;
        private ImageButton btnPhoto;
        private ImageButton btnLocation;
        private ImageButton btnMap;


        private ImageView imgFriend; 
        private Friend friend;
        FriendRepository db = new FriendRepository();
        private LocationManager _location;
        private ProgressDialog _progress;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Detaillayout);

            _location = GetSystemService(LocationService) as LocationManager;

            txtEmail = FindViewById<EditText>(Resource.Id.txtEmail);
            txtLat = FindViewById<EditText>(Resource.Id.txtLat);
            txtLong = FindViewById<EditText>(Resource.Id.txtLong);
            txtName = FindViewById<EditText>(Resource.Id.txtName);
            txtPhone = FindViewById<EditText>(Resource.Id.txtPhone);
            imgFriend = FindViewById<ImageView>(Resource.Id.imgFriend);
            btnPhoto = FindViewById<ImageButton>(Resource.Id.btnPhoto);
            btnLocation = FindViewById<ImageButton>(Resource.Id.btnLocation);
            btnMap = FindViewById<ImageButton>(Resource.Id.btnMap);


            btnPhoto.Click += btnPhoto_Click;
            btnLocation.Click += BtnLocation_Click;
            btnMap.Click += BtnMap_Click;

            if (Intent.HasExtra("FriendID"))
            {
                int friendId = Intent.GetIntExtra("FriendID", 0);
                friend = db.GetFriendById(friendId);
                txtEmail.Text = friend.Email;
                txtPhone.Text = friend.Phone;
                txtLat.Text = friend.Latitude.ToString();
                txtLong.Text = friend.Longitude.ToString();
                txtName.Text = friend.FullName;
                imgFriend.SetImageBitmap(FriendRepository.GetImageFriend(friendId));


            }
            else
            {
                friend = new Friend();
                btnPhoto.Visibility = ViewStates.Gone;
            }
        }

        private void BtnMap_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtLat.Text) &&
                !string.IsNullOrWhiteSpace(txtLong.Text))
            {
                string lat = txtLat.Text.Trim();
                string lng = txtLong.Text.Trim();

                // URI مخصوص Map
                var gmmIntentUri = Android.Net.Uri.Parse(
                    $"geo:0,0?q={lat},{lng}"
                );

                var mapIntent = new Intent(Intent.ActionView, gmmIntentUri);

                // فقط Google Maps
                mapIntent.SetPackage("com.google.android.apps.maps");

                try
                {
                    StartActivity(mapIntent);
                }
                catch (ActivityNotFoundException)
                {
                    Toast.MakeText(this, "Google Maps نصب نیست", ToastLength.Long).Show();
                }
            }
        }




        private void BtnLocation_Click(object sender, EventArgs e)
        {
            _progress = ProgressDialog.Show(this,"","Please Wait ...");
            Criteria criteria = new Criteria();
            criteria.Accuracy = Accuracy.NoRequirement;
            criteria.PowerRequirement = Power.NoRequirement;

            _location.RequestSingleUpdate(criteria, this, null);
        }

        private void btnPhoto_Click(object sender, EventArgs e)
        {
            var cameraIntent = new Intent(MediaStore.ActionImageCapture);

            if (cameraIntent.ResolveActivity(PackageManager) != null)
            {
                Java.IO.File imageFile = new Java.IO.File(
                    FriendRepository.ImagePath(friend.FriendID)
                );

                var imageUri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                    this,
                    Application.Context.PackageName + ".fileprovider",
                    imageFile
                );

                cameraIntent.PutExtra(MediaStore.ExtraOutput, imageUri);

                cameraIntent.AddFlags(ActivityFlags.GrantWriteUriPermission);
                cameraIntent.AddFlags(ActivityFlags.GrantReadUriPermission);

                StartActivityForResult(cameraIntent, 0);
            }
            else
            {
                Toast.MakeText(this, "Camera not found", ToastLength.Long).Show();
            }
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if(resultCode == Result.Ok)
            {
                imgFriend.SetImageBitmap(FriendRepository.GetImageFriend(friend.FriendID));
            }
            else
            {
                Toast.MakeText(this, "No Image", ToastLength.Long).Show();
            }
                base.OnActivityResult(requestCode, resultCode, data);
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.DetailMenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.actionSave:
                    {
                        if (ValidateInputs())
                        {
                            friend.FullName = txtName.Text;
                            friend.Email = txtEmail.Text;
                            friend.Phone = txtPhone.Text;
                            if (txtLat.Text != "" && txtLong.Text != "")
                            {
                                friend.Latitude = double.Parse(txtLat.Text);
                                friend.Longitude = double.Parse(txtLong.Text);
                            }

                            if (friend.FriendID == 0)
                            {
                                db.InsertFriend(friend);
                            }
                            else
                            {
                                db.UpdateFriend(friend);
                            }


                            Toast.MakeText(this, "Saved ...", ToastLength.Long).Show();
                            Finish();
                        }
                        break;
                    }
                case Resource.Id.actionDelete:
                    {
                        if (friend.FriendID != 0)
                        {
                            var alert = new Android.App.AlertDialog.Builder(this);
                            alert.SetTitle("توجه !");
                            alert.SetMessage("آیا از حذف مطمئن هستید ؟");
                            alert.SetPositiveButton("بله", yesClick);
                            alert.SetNegativeButton("خیر", delegate { });
                            alert.Show();
                        }
                        break;
                    }
            }
            return base.OnOptionsItemSelected(item);
        }

        private void yesClick(object sender, DialogClickEventArgs e)
        {
            db.DeleteFriend(friend);
            Finish();
        }

        bool ValidateInputs()
        {
            bool isValid = true;

            if (string.IsNullOrEmpty(txtEmail.Text))
            {
                txtEmail.Error = "Please Enter Email";
                isValid = false;
            }
            else
            {
                txtEmail.Error = null;

            }
            if (string.IsNullOrEmpty(txtPhone.Text))
            {
                txtPhone.Error = "Please Enter Phone";
                isValid = false;
            }
            else
            {
                txtPhone.Error = null;

            }
            if (string.IsNullOrEmpty(txtName.Text))
            {
                txtName.Error = "Please Enter Name";
                isValid = false;
            }
            else
            {
                txtName.Error = null;

            }

            return isValid;
        }

        public void OnLocationChanged(Location location)
        {
            _progress.Cancel();
            txtLat.Text = location.Latitude.ToString();
            txtLong.Text = location.Longitude.ToString();
        }

        public void OnProviderDisabled(string provider)
        {
            throw new NotImplementedException();
        }

        public void OnProviderEnabled(string provider)
        {
            throw new NotImplementedException();
        }

        public void OnStatusChanged(string provider, [GeneratedEnum] Availability status, Bundle extras)
        {
            throw new NotImplementedException();
        }
    }
}

