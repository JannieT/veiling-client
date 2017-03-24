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

namespace Veiling
{
    public class ListListener : Java.Lang.Object, GestureDetector.IOnGestureListener
    {

        private static int SWIPE_MAX_OFF_PATH = 250;
        private static int SWIPE_MIN_DISTANCE = 100;
        private static int SWIPE_THRESHOLD_VELOCITY = 500;

        Context context;
        public GestureDetector Detector { get; private set; }

        public ListListener(Context context, float density)
        {
            this.context = context;
            this.Detector = new GestureDetector(context, this);

            SWIPE_MAX_OFF_PATH = (int)(250.0f * density / 160.0f + 0.5);
            SWIPE_MIN_DISTANCE = (int)(120.0f * density / 160.0f + 0.5);
            SWIPE_THRESHOLD_VELOCITY = (int)(200.0f * density / 160.0f + 0.5);
        }

        void HandleLeft(MotionEvent e)
        {
            var a = (MainActivity)context;
            a.WipeItemAt((int)e.GetX(), (int)e.GetY());
        }

        public bool OnFling( MotionEvent e1, MotionEvent e2, float velocityX, float velocityY )
        {
            // return true when you are happy you've detected a sufficient fling

            if ( Math.Abs( e1.GetY() - e2.GetY() ) > SWIPE_MAX_OFF_PATH )
            {
                return false;
            }
            if ( e1.GetX() - e2.GetX() > SWIPE_MIN_DISTANCE && Math.Abs( velocityX ) > SWIPE_THRESHOLD_VELOCITY )
            {
                HandleLeft(e1);
                return true;
            }
            if ( e2.GetX() - e1.GetX() > SWIPE_MIN_DISTANCE && Math.Abs( velocityX ) > SWIPE_THRESHOLD_VELOCITY )
            {
                Console.WriteLine("Right swipe");            
                return true;
            }

            return false;

        }

        public bool OnDown( MotionEvent e )
        {
            return true;  // this must return true for the OnFling to trigger
        }

        public void OnLongPress( MotionEvent e )
        {
        }

        public bool OnScroll( MotionEvent e1, MotionEvent e2, float distanceX, float distanceY )
        {
            return false;
        }

        public void OnShowPress( MotionEvent e )
        {

        }

        public bool OnSingleTapUp( MotionEvent e )
        {
            return false;
        }

    }
}

