using System.Collections.Generic;
using Android.App;
using Android.Widget;

namespace Veiling
{
    public class VeilingListAdapter : BaseAdapter<VeilingItem>
    {
        Activity context = null;
        IList<VeilingItem> items = new List<VeilingItem>();

        public VeilingListAdapter(Activity context, IList<VeilingItem> items) : base()
        {
            this.context = context;
            this.items = items;
        }

        public override VeilingItem this [int position]
        {
            get { return items[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override Android.Views.View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            // Get our object for position
            var item = items[position]; 

            //Try to reuse convertView if it's not  null, otherwise inflate it from our item layout
            // gives us some performance gains by not always inflating a new view
            var view = (convertView ??
                context.LayoutInflater.Inflate(
                    Resource.Layout.ItemRy,
                    parent, 
                    false)) as LinearLayout;

            // Find references to each subview in the list item's view
            var txtNommer = view.FindViewById<TextView>(Resource.Id.itemNommer);
            var txtBeskrywing = view.FindViewById<TextView>(Resource.Id.itemBeskrywing);
            var txtBedrag = view.FindViewById<TextView>(Resource.Id.itemBedrag);

            // Assign item's values to the various subviews
            txtNommer.SetText(item.nommer, TextView.BufferType.Normal);
            txtBeskrywing.SetText(item.beskrywing, TextView.BufferType.Normal);
            txtBedrag.SetText(item.bedrag.ToString("#,##0.00"), TextView.BufferType.Normal);
            txtBedrag.Selected = item.betaal;
            txtBeskrywing.Selected = item.betaal;

            return view;
        }

    }
}

