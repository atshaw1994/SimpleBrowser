
// Copyright © 2013 The CefSharp Authors. All rights reserved.
//
// Use of this source code is governed by a BSD-style license that can be found in the LICENSE file.

using CefSharp;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SimpleBrowser
{
    public class DownloadHandler : IDownloadHandler
    {
        public event EventHandler<DownloadItem>? OnBeforeDownloadFired;

        public event EventHandler<DownloadItem>? OnDownloadUpdatedFired;

        private Border progressBar = new();
        private double max = 0.0;
        private double current = 0.0;
        private double omniBarWidth = 0.0;

        public DownloadHandler(Border progressBar, double omniBarWidth)
        {
            this.progressBar = progressBar;
            this.omniBarWidth = omniBarWidth;
        }

        public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
        {
            OnBeforeDownloadFired?.Invoke(this, downloadItem);

            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    callback.Continue(downloadItem.SuggestedFileName, showDialog: true);
                }
            }
        }

        public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
        {
            OnDownloadUpdatedFired?.Invoke(this, downloadItem);
            if (downloadItem.IsComplete)
            {
                // Hide ProgressBar
            }
            else
            {
                max = downloadItem.TotalBytes;
                current = downloadItem.ReceivedBytes;
                Trace.WriteLine($"T: {max} R: {current}");
                Trace.WriteLine($"obar Width: {omniBarWidth}");
                Trace.WriteLine($"pBar Width: { omniBarWidth * (current / max)}");
                //Application.Current.MainWindow.Dispatcher.Invoke(() =>
                //{
                //    progressBar.Width = omniBarWidth * (current / max);
                //});
            }
        }
    }
}
