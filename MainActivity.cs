using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using Debug = System.Diagnostics.Debug;
using Environment = Android.OS.Environment;
using File = Java.IO.File;
using Toolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;


namespace Veiling
{
    [Activity(Label = "Strand-Noord Veiling", MainLauncher = true, ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity
    {
        const int CaptureImageActivityRequestCode = 100;

        List<VeilingItem> items = new List<VeilingItem>();
        BackendService backend;
        File imagesFolder;

        /* UI */
        LotListView listItems;
        FlipNumbers.FlipNumbersView flipper;
        SeekBar slider;
        TextView labelNaam, labelTotaal, txtTotaal;
        LinearLayout panelResults, panelRegister, panelHome;
        AppCompatButton btnCamera;
        Button btnPhoto, btnPing;

        SwitchCompat btnMode;
        IMenuItem btnFetch, btnSend;

        #region lifecycle

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            Debug.WriteLine("Inflated");

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolbar);

            btnMode = FindViewById<SwitchCompat>(Resource.Id.btnMode);
            btnMode.CheckedChange += HandleModeSwitch;

            // home panel
            btnPing = FindViewById<Button>(Resource.Id.btnPing);
            btnPing.Click += HandlePingTap;

            // registrasie panel
            btnCamera = FindViewById<AppCompatButton>(Resource.Id.btnCamera);
            btnCamera.Click += HandleCameraTap;

            btnPhoto = FindViewById<Button>(Resource.Id.btnPhoto);
            btnPhoto.Click += HandlePhotoClick;


            // betaalpunt
			labelNaam = FindViewById<TextView>(Resource.Id.lblNaam);

            labelTotaal = FindViewById<TextView>(Resource.Id.lblTotaal);
            labelTotaal.Clickable = true;
            labelTotaal.Click += HandleLabelTotaalClick;

            txtTotaal = FindViewById<TextView>(Resource.Id.txtTotaal);
            txtTotaal.Clickable = true;
            txtTotaal.Click += HandleTotaalClick;


            panelResults = FindViewById<LinearLayout>(Resource.Id.panelResult);
            panelRegister = FindViewById<LinearLayout>(Resource.Id.panelRegister);
            panelHome = FindViewById<LinearLayout>(Resource.Id.panelHome);

            Button btnMinus = FindViewById<Button>(Resource.Id.btnMinus);
            btnMinus.Click += HandleMinusClick;

            Button btnPlus = FindViewById<Button>(Resource.Id.btnPlus);
            btnPlus.Click += HandlePlusClick;

            slider = FindViewById<SeekBar>(Resource.Id.barNommer);
            slider.ProgressChanged += HandleProgressChanged;

            flipper = FindViewById<FlipNumbers.FlipNumbersView>(Resource.Id.flipNumbersView);


            listItems = FindViewById<LotListView>(Resource.Id.listItems);
            var gestureListener = new ListListener(this, (float)Resources.DisplayMetrics.DensityDpi);
            listItems.FlingDetector = gestureListener.Detector;

            // Set the initial UI state

            flipper.Value = 0;
            slider.Progress = 0;
            ClearResults();
            SetPanels();
        }


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            Console.WriteLine("OnCreateOptionsMenu");
            MenuInflater.Inflate(Resource.Menu.home, menu);
            btnSend = menu.GetItem(0);
            btnFetch = menu.GetItem(1);

            // now that the buttons are inflated, I can set them:
            isBetaalpunt = isBetaalpunt;

//            btnAction.SetVisible(false);
            return base.OnCreateOptionsMenu(menu);
        }

        protected override void OnResume()
        {
            base.OnResume();

            // load the settings here when the settings activity vanished
            var prefs = Android.Preferences.PreferenceManager.GetDefaultSharedPreferences(this);
            int max = int.TryParse(prefs.GetString(SettingsActivity.KEY_MAX, "75"), out max) ? max : 75;
            slider.Max = max;

            string server_ip = prefs.GetString(SettingsActivity.KEY_IP, Resources.GetString(Resource.String.default_ip));
            string api_key = prefs.GetString(SettingsActivity.KEY_API_KEY, Resources.GetString(Resource.String.default_api_key));
            backend = new BackendService(server_ip, api_key);
        }

        #endregion lifecycle

        #region navigation

        public override bool OnOptionsItemSelected(IMenuItem item)
        {   
            switch (item.ItemId)
            {
                case Resource.Id.menu_settings:
                    slider.Progress = 0;
                    var settings = new Intent(this, typeof(SettingsActivity));
                    StartActivity(settings);
                    return true;
                case Resource.Id.menu_send:
                    if (flipper.Value == 0)
                        return true;
                    
                    SendDataAsync();
                    return true;
                case Resource.Id.menu_fetch:
                    Debug.WriteLine("Fetch!");
                    if (flipper.Value == 0)
                    {
                        PingServerAsync();
                        return true;
                    }
                    FetchDataAsync();
                    return true;
                default:
                    break;
            }

            return false;
        }


        void HandlePlusClick(object sender, EventArgs e)
        {
            if (flipper.Value >= slider.Max)
            {
                Flash("Gebruik settings om meer beërs toe te laat.");
                return;
            }

            slider.Progress = flipper.Value + 1; 
        }

        void HandleMinusClick(object sender, EventArgs e)
        {
            if (flipper.Value < 1)
                return;

            slider.Progress = flipper.Value - 1; 
        }

        void HandleProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
			if (txtTotaal.Text != string.Empty) 
			{
				ClearResults ();
			}
			flipper.SetValue(e.Progress, false);
            SetPanels();
        }

        void HandleModeSwitch (object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            isBetaalpunt = e.IsChecked;
            SetPanels();
        }

        bool isBetaalpunt { 
            get { return btnMode.Checked; } 
            set { 
                Debug.Write("Modus ");
                if (value)
                {
                    Debug.WriteLine ("betaalpunt");
                    btnSend.SetVisible(false);
                    btnFetch.SetVisible(true);
                    SupportActionBar.SetTitle(Resource.String.mode_betaal);
                }
                else
                {
                    Debug.WriteLine ("registrasie");
                    btnSend.SetVisible(true);
                    btnFetch.SetVisible(false);
                    SupportActionBar.SetTitle(Resource.String.mode_registrasie);
                }
            }
        }


        void SetPanels()
        {

            if (flipper.Value == 0)
            {
                panelResults.Visibility = ViewStates.Invisible;
                panelRegister.Visibility = ViewStates.Invisible;
                panelHome.Visibility = ViewStates.Visible;
                return;
            }

            if (!isBetaalpunt)
            {
                SetPhotoButton();
                if (panelRegister.Visibility == ViewStates.Visible)
                    return;

                panelResults.Visibility = ViewStates.Invisible;
                panelRegister.Visibility = ViewStates.Visible;
                panelHome.Visibility = ViewStates.Invisible;
                return;                
            }

            if (panelResults.Visibility == ViewStates.Visible)
                return;

            // betaalpunt
            if (labelTotaal.Text != string.Empty)
            {
                ClearResults();
            }

            panelResults.Visibility = ViewStates.Visible;
            panelRegister.Visibility = ViewStates.Invisible;
            panelHome.Visibility = ViewStates.Invisible;

        }


        #endregion navigation

        #region ping

        async void HandlePingTap (object sender, EventArgs e)
        {
            btnPing.Enabled = false;
            await PingServerAsync();
            btnPing.Enabled = true;
        }

        async Task PingServerAsync()
        {
            try
            {
                await backend.PingAsync();
            }
            catch (FeedbackException fx)
            {
                Flash(fx.Feedback);
                return;
            }
            catch (Exception ex)
            {
                Flash(ex.Message);
                return;
            }

            Flash("Lyk of dit werk. Die server gesels.");
        }
        #endregion

        #region registration

        void HandlePhotoClick (object sender, EventArgs e)
        {
            string path = GetFileFor(flipper.Value).AbsolutePath;
            Intent intent = new Intent();
            intent.SetAction(Intent.ActionView);
            intent.SetDataAndType(Uri.Parse("file://" + path), "image/*");
            StartActivity(intent);
        }

        void SetPhotoButton()
        {
            if (!NumberHasImage())
            {
                Debug.WriteLine("Show photo empty");
                btnPhoto.Enabled = false;
            }
            else
            {
                Debug.WriteLine("Show photo in");
                btnPhoto.Enabled = true;
            }
            btnPhoto.Invalidate();
            Debug.WriteLine($"enabled: {btnPhoto.Enabled}  selected: {btnPhoto.Selected}");
        }


        void HandleCameraTap (object sender, EventArgs e)
        {
            Intent intent = new Intent(Android.Provider.MediaStore.ActionImageCapture);
            Uri fileUri = Uri.FromFile(GetFileFor(flipper.Value));
            intent.PutExtra(Android.Provider.MediaStore.ExtraOutput, fileUri);

            StartActivityForResult(intent, CaptureImageActivityRequestCode);
        }


        File GetFileFor(int number)
        {
            if (imagesFolder == null)
            {
                imagesFolder = new File(Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures), Const.ImageFolder);
                imagesFolder.Mkdirs();
            }

            File image = new File(imagesFolder, $"{number}.jpg");
            Debug.WriteLine($"file: {image.AbsolutePath}  size: {image.Length()}");
            return image;
        }

        async void SendDataAsync()
        {
            string imageFile = GetFileFor(flipper.Value).AbsolutePath;
            if (!NumberHasImage())
            {
                Flash("Neem eers 'n foto");
                return;
            }

            ShowBusy("Daar gaan hy.");
            try
            {
                await backend.UploadImageAsync(flipper.Value, imageFile);
                Debug.WriteLine("done!");
            }
            catch (FeedbackException fx)
            {
                Debug.WriteLine(fx.Feedback);
                Toast.MakeText(this, fx.Feedback, ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
            ShowBusy("");
        }

        #endregion

        #region betaalpunt
        void ClearResults()
        {
			labelNaam.Text = string.Empty;
            labelTotaal.Text = string.Empty;
            txtTotaal.Text = string.Empty;
            items.Clear();
            listItems.Adapter = new VeilingListAdapter(this, items);
        }

        void HandleLabelTotaalClick(object sender, EventArgs e)
        {
            if (!HasPaidItems())
                return;

            if (labelTotaal.Text == Const.LblTotaal)
            {
                labelTotaal.Text = Const.LblUitstaande;
            }
            else
            {
                labelTotaal.Text = Const.LblTotaal;
            }

            SetTotal(labelTotaal.Text);
        }

        bool HasPaidItems()
        {
            bool hasPaidItems = false;
            foreach (var item in items)
            {
                if (item.betaal)
                {
                    hasPaidItems = true;
                }
            }
            return hasPaidItems;
        }

        void SetTotal(string label)
        {
            float total = 0;
            float uitstaande = 0;

            foreach (VeilingItem item in items)
            {
                total += item.bedrag;
                uitstaande += item.betaal ? 0 : item.bedrag;
            }

            float display = (label == Const.LblUitstaande) ? uitstaande : total;

            RunOnUiThread(() =>
            {
                txtTotaal.Text = string.Format("R {0}", display.ToString("#,##0.00"));
                labelTotaal.Text = label;
            });
        }

		void SetBieerInfo(string naam)
		{
			RunOnUiThread(() =>
				{
					labelNaam.Text = naam;
				});
		}

        public void WipeItemAt(int posX, int posY)
        {
            try
            {
                int index = listItems.PointToPosition(posX, posY);
                items.RemoveAt(index);
                SetTotal(Const.LblTotaal);
                listItems.Adapter = new VeilingListAdapter(this, items);
            }
            catch (Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();             
            }
        }


        async Task FetchDataAsync()
        {
            ShowBusy("Byt vas. Ek kry hom gou ...");
            SetBieerInfo (String.Empty);

            try
            {
                BieerItem bieer = await backend.PullBieerInfoAsync(flipper.Value);
                SetBieerInfo(bieer.naam);
                items = await backend.PullItemsAsync(flipper.Value);
                SetTotal(Const.LblTotaal);
                listItems.Adapter = new VeilingListAdapter(this, items);
            }
            catch (FeedbackException fx)
            {
                Toast.MakeText(this, fx.Feedback, ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                items.Clear();
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                Debug.WriteLine($"Item fetch error: {ex.Message}");
            }

            ShowBusy("");
        }

		async void HandleBetaalOpsie(object sender, Android.Widget.PopupMenu.MenuItemClickEventArgs e)
		{
			string soort;

			switch (e.Item.ToString()) {
			case "Sommer niks":
				return;

			case "EFT":
				soort = "eft";
				break;

			case "Begin oor":
				soort = "herstel";
				break;

			default:
				soort = "cash";
				break;
			}


			List<int> nommers = new List<int>();
			foreach (var item in items)
			{
				nommers.Add(Int32.Parse(item.nommer));
			}

			ShowBusy("Ons laat hulle net gou weet.");

			try
			{
				await backend.MarkPaid(nommers, soort);
				foreach (var item in items)
				{
					item.betaal = (soort != "herstel");
				}
				listItems.Adapter = new VeilingListAdapter(this, items);
				SetTotal(Const.LblUitstaande);
			}
            catch (FeedbackException fx)
            {
                Toast.MakeText(this, fx.Feedback, ToastLength.Long).Show();
            }
			catch (Exception ex)
			{
				Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
				Console.WriteLine("Betaal update error: {0}", ex.Message);
			}

			ShowBusy("");

		}

        void HandleTotaalClick(object sender, EventArgs e)
        {
            if (items.Count < 1)
                return;

			var menu = new Android.Widget.PopupMenu(this, labelTotaal);
			menu.Inflate(Resource.Menu.betaal);
			menu.MenuItemClick += HandleBetaalOpsie;
			menu.Show();
        }

        #endregion betaalpunt

        #region util

        void Flash(string message)
        {
            AndHUD.Shared.ShowToast(this, message, MaskType.Clear, TimeSpan.FromSeconds(3));
        }

        bool NumberHasImage()
        {
            string imageFile = GetFileFor(flipper.Value).AbsolutePath;
            return System.IO.File.Exists(imageFile);
        }

        void ShowBusy(string message)
        {
            if (message == string.Empty)
            {
                AndHUD.Shared.Dismiss(this);
            }
            else
            {
                AndHUD.Shared.Show(this, message);
            }
        }


        #endregion
       
    }
}


