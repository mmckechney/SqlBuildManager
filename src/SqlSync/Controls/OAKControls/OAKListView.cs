/*************************************************************************
OAKListView 1.0
10/02/2004
Developer: Carlos Carvalho
carvalho@flag.com.br
+55 31 99440862 / +55 31 32616977
 
 http://www.codeproject.com/KB/list/OAKListView.aspx
**************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SqlSync.Controls.OAKControls
{
    /// <summary>
    /// Summary description for OAKListView.
    /// </summary>
    public class OAKListView : ListView
    {
        [StructLayoutAttribute(LayoutKind.Sequential)]
        public struct LV_ITEM
        {
            public UInt32 mask;
            public Int32 iItem;
            public Int32 iSubItem;
            public UInt32 state;
            public UInt32 stateMask;
            public String pszText;
            public Int32 cchTextMax;
            public Int32 iImage;
            public IntPtr lParam;
        }

        public const Int32 LVM_FIRST = 0x1000;
        public const Int32 LVM_GETITEM = LVM_FIRST + 5;
        public const Int32 LVM_SETITEM = LVM_FIRST + 6;
        public const Int32 LVIF_TEXT = 0x0001;
        public const Int32 LVIF_IMAGE = 0x0002;

        public const int LVW_FIRST = 0x1000;
        public const int LVM_GETEXTENDEDLISTVIEWSTYLE = LVW_FIRST + 54;

        public const int LVS_EX_GRIDLINES = 0x00000001;
        public const int LVS_EX_SUBITEMIMAGES = 0x00000002;
        public const int LVS_EX_CHECKBOXES = 0x00000004;
        public const int LVS_EX_TRACKSELECT = 0x00000008;
        public const int LVS_EX_HEADERDRAGDROP = 0x00000010;
        public const int LVS_EX_FULLROWSELECT = 0x00000020; // applies to report mode only
        public const int LVS_EX_ONECLICKACTIVATE = 0x00000040;

        /// <summary>
        /// Changing the style of listview to accept image on subitems
        /// </summary>
        public OAKListView()
        {
            HandleCreated += new EventHandler(OAKListView_HandleCreated);

            //// Change the style of listview to accept image on subitems
            //System.Windows.Forms.Message m = new Message();
            //m.HWnd = this.Handle;
            //m.Msg = LVM_GETEXTENDEDLISTVIEWSTYLE;
            //m.LParam = (IntPtr)(LVS_EX_GRIDLINES | 
            //                    LVS_EX_FULLROWSELECT | 
            //                    LVS_EX_SUBITEMIMAGES | 
            //                    LVS_EX_CHECKBOXES | 
            //                    LVS_EX_TRACKSELECT);
            //m.WParam = IntPtr.Zero;
            //this.WndProc(ref m);
        }

        void OAKListView_HandleCreated(object sender, EventArgs e)
        {
            System.Windows.Forms.Message m = new Message();
            m.HWnd = Handle;
            m.Msg = LVM_GETEXTENDEDLISTVIEWSTYLE;
            m.LParam = (IntPtr)(LVS_EX_GRIDLINES |
                LVS_EX_FULLROWSELECT |
                LVS_EX_SUBITEMIMAGES |
                LVS_EX_CHECKBOXES |
                LVS_EX_TRACKSELECT);
            m.WParam = IntPtr.Zero;
            WndProc(ref m);
        }


        [DllImport("user32.dll")]
        public static extern int SendMessage(
            IntPtr hWnd,    // handle to destination window 
            int Msg,        // message 
            IntPtr wParam,  // first message parameter 
            IntPtr lParam   // second message parameter 
            );

        [DllImport("user32.dll")]
        public static extern bool SendMessage(
            IntPtr hWnd,        // handle to destination window 
            Int32 msg,          // message 
            Int32 wParam,
            ref LV_ITEM lParam);// pointer to struct of LV_ITEM
    }
}
