using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Util;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace MyFriends
{
   public class Friend
    {
        [PrimaryKey,AutoIncrement]
        public int FriendID { get; set; }
        [NotNull]
        [MaxLength(200)]
        public string FullName { get; set; }
        [NotNull]
        [MaxLength(200)]
        public string Email { get; set; }
        [NotNull]
        [MaxLength(200)]
        public string Phone { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class FriendRepository
    {
        private string dbPath;
        private string dbName = "MyFriendsDb.db";
        private SQLiteConnection db;

        public FriendRepository()
        {
            dbPath = System.Environment.GetFolderPath(
                System.Environment.SpecialFolder.LocalApplicationData
            );

            string fullPath = System.IO.Path.Combine(dbPath, dbName);

            db = new SQLiteConnection(fullPath);   // ← مهم
            db.CreateTable<Friend>();
        }

        public List<Friend> GetAllFriend()
        {
            return db.Table<Friend>().ToList();
        }

        public Friend GetFriendById(int friendId)
        {
            return db.Table<Friend>()
                     .SingleOrDefault(f => f.FriendID == friendId);
        }

        public void InsertFriend(Friend friend)
        {
            db.Insert(friend);
        }

        public void UpdateFriend(Friend friend)
        {
            db.Update(friend);
        }

        public void DeleteFriend(Friend friend)
        {
            db.Delete(friend);
        }
        public static string ImagePath(int friendId)
        {
            string path = System.IO.Path.Combine(
                Application.Context.GetExternalFilesDir(null).AbsolutePath,
                "FriendImages"
            );

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            return System.IO.Path.Combine(path, $"Friend_{friendId}.jpg");
        }
        public static Bitmap GetImageFriend(int friendId)
        {
            string path = ImagePath (friendId);
            if (File.Exists(path))
                return BitmapFactory.DecodeFile(path);
            return null;
           
        }


    }

    public class FriendAdapter : BaseAdapter<Friend>
    {
        private Activity _context;
        private List<Friend> _list;
        public FriendAdapter(Activity context, List<Friend> list)
        {
            _context = context;
            _list = list;
        }

        

        
        public override long GetItemId(int position)
        {
            return _list[position].FriendID;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView;
            if (view == null)
                view = _context.LayoutInflater.Inflate(Resource.Layout.ListItem, null);
            Friend friend = _list[position];
            view.FindViewById<TextView>(Resource.Id.lblName).Text = friend.FullName;
            view.FindViewById<TextView>(Resource.Id.lblEmail).Text = friend.Email;
            view.FindViewById<TextView>(Resource.Id.lblPhone).Text = friend.Phone;
            view.FindViewById<ImageView>(Resource.Id.img).SetImageBitmap(FriendRepository.GetImageFriend(friend.FriendID));

            return view;
        }
        public override int Count
        {
            get { return _list.Count; }

        }
        public override Friend this[int position]
        {
            get { return _list[position]; }
        }
    }
}