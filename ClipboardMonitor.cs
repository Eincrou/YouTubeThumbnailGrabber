using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace YouTubeThumbnailGrabber
{
    class ClipboardMonitor
    {
        private MainWindow mainWindow;
        private IntPtr hWndNextViewer;
        private HwndSource hWndSource;

        private const int WM_DRAWCLIPBOARD = 0x0308;
        private const int WM_CHANGECBCHAIN = 0x030D;

        public bool IsMonitoringClipboard = false;

        public ClipboardMonitor(MainWindow mw)
        {
            mainWindow = mw;
        }
        public void InitCBViewer()
        {
            WindowInteropHelper wih = new WindowInteropHelper(mainWindow);
            wih.EnsureHandle();
            hWndSource = HwndSource.FromHwnd(wih.Handle);

            hWndSource.AddHook(this.WndProc);
            hWndNextViewer = SetClipboardViewer(hWndSource.Handle);
            IsMonitoringClipboard = true;
        }
        public void CloseCBViewer()
        {
            ChangeClipboardChain(hWndSource.Handle, hWndNextViewer);

            hWndNextViewer = IntPtr.Zero;
            hWndSource.RemoveHook(this.WndProc);
            IsMonitoringClipboard = false;
        }
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DRAWCLIPBOARD:
                    if (Clipboard.ContainsText())
                        OnClipboardTextChanged(Clipboard.GetText());
                    SendMessage(hWndNextViewer, msg, wParam, lParam);
                    break;
                case WM_CHANGECBCHAIN:
                    if (wParam == hWndNextViewer)
                        hWndNextViewer = lParam;
                    else
                        SendMessage(hWndNextViewer, msg, wParam, lParam);
                    break;
            }
            return IntPtr.Zero;
        }

        public event EventHandler<ClipboardEventArgs> ClipboardTextChanged;
        private void OnClipboardTextChanged(string clipboardText)
        {
            EventHandler<ClipboardEventArgs> clipboardTextChanged = ClipboardTextChanged;
            if (clipboardTextChanged != null)
                ClipboardTextChanged(this, new ClipboardEventArgs(clipboardText));
        }

        [DllImport("User32.dll")]
        protected static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }
}
