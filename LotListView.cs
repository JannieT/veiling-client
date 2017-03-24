using System;
using Android.Content;
using Android.Views;
using Android.Util;
using Android.Widget;

namespace Veiling
{
    class LotListView : ListView
    {

        public GestureDetector FlingDetector { get; set; }

        public LotListView (Context context, IAttributeSet attrs, int defStyle) :
        base (context, attrs, defStyle)
        {
        }

        public LotListView (Context context, IAttributeSet attrs) :
        base (context, attrs)
        {
        }

        public LotListView (System.IntPtr handle, Android.Runtime.JniHandleOwnership owner) :
        base (handle, owner)
        {
        }


        public override bool OnTouchEvent(MotionEvent e)
        {
            bool handled = FlingDetector.OnTouchEvent(e);
            if (handled)
                return true;

            return base.OnTouchEvent(e);
        }
    }
}

