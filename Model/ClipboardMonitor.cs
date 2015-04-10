using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace YouTubeThumbnailGrabber.Model
{
    class ClipboardMonitor
    {
        private Window window;
        private IntPtr hWndNextViewer;
        private HwndSource hWndSource;

        private const int WM_DRAWCLIPBOARD = 0x0308;
        private const int WM_CHANGECBCHAIN = 0x030D;

        public bool IsMonitoringClipboard = false;
        /// <summary>
        /// Initializes a new instance of the ClipboardMonitor class to allow clipboard updates for the specified window
        /// </summary>
        /// <param name="win"></param>
        public ClipboardMonitor(Window win)
        {
            window = win;
        }
        /// <summary>
        /// Starts monitoring the clipboard for changes
        /// </summary>
        public void InitCBViewer()
        {
            WindowInteropHelper wih = new WindowInteropHelper(window);
            wih.EnsureHandle();
            hWndSource = HwndSource.FromHwnd(wih.Handle);

            hWndSource.AddHook(this.WndProc);
            hWndNextViewer = SetClipboardViewer(hWndSource.Handle);
            IsMonitoringClipboard = true;
        }
        /// <summary>
        /// Stops monitoring the clipboard for changes
        /// </summary>
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
                    else if (Clipboard.ContainsImage())
                        ClipboardImageToBitmap();
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

        private void ClipboardImageToBitmap()
        {
            MemoryStream ms = new MemoryStream();
            BmpBitmapEncoder enc = new BmpBitmapEncoder();
            enc.Frames.Add(BitmapFrame.Create(Clipboard.GetImage()));
            enc.Save(ms);
            ms.Seek(0, SeekOrigin.Begin);

            BmpBitmapDecoder dec = new BmpBitmapDecoder(ms,
                BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            OnClipboardImageChanged(dec.Frames[0]);
        }
        /// <summary>
        /// Notifies when text in the clipboard has changed
        /// </summary>
        public event EventHandler<ClipboardEventArgs> ClipboardTextChanged;
        private void OnClipboardTextChanged(string clipboardText)
        {
            EventHandler<ClipboardEventArgs> clipboardTextChanged = ClipboardTextChanged;
            if (clipboardTextChanged != null)
                ClipboardTextChanged(this, new ClipboardEventArgs(clipboardText));
        }
        /// <summary>
        /// Notifies when the image in the clipboard has changed
        /// </summary>
        public event EventHandler<ClipboardEventArgs> ClipboardImageChanged;
        private void OnClipboardImageChanged(BitmapFrame clipboardImage)
        {
            EventHandler<ClipboardEventArgs> clipboardImageChanged = ClipboardImageChanged;
            if (clipboardImageChanged != null)
                ClipboardImageChanged(this, new ClipboardEventArgs(clipboardImage));
        }

        [DllImport("User32.dll")]
        private static extern IntPtr SetClipboardViewer(IntPtr hWndNewViewer);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern bool ChangeClipboardChain(IntPtr hWndRemove, IntPtr hWndNewNext);
        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
    }
}
