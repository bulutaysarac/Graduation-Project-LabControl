using file_transfer;
using LabControl.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace LabControl
{
    /// <summary>
    /// Interaction logic for FileTransferWindow.xaml
    /// </summary>
    public partial class FileTransferWindow : Window
    {
        #region variables

        private List<TransferQueue> transfers = new List<TransferQueue>();
        private TransferClient fileTransferClient;
        private System.Windows.Forms.Timer overallProgressTimer;
        private Computer selectedComputer;

        #endregion

        public FileTransferWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.dgList.ItemsSource = this.transfers;
            this.overallProgressTimer = new System.Windows.Forms.Timer() { Interval = 100 };
            this.overallProgressTimer.Tick += this.OverallProgressTimer_Tick;
            this.selectedComputer = this.Tag as Computer;
            this.txtCurrentComputerIP.Text = this.selectedComputer.IPAddress;

            if (this.fileTransferClient == null)
            {
                this.fileTransferClient = new TransferClient();
                this.fileTransferClient.Connect(this.selectedComputer.IPAddress, 1818, this.ConnectCallback);
                this.IsEnabled = false;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.fileTransferClient.Close();
            this.fileTransferClient = null;
            this.DeregisterEvents();
        }

        /// <summary>
        /// Setting value of the progressBarOverall.
        /// </summary>
        private void OverallProgressTimer_Tick(object sender, EventArgs e)
        {
            if (this.fileTransferClient == null)
                return;

            this.progressBarOverall.Value = this.fileTransferClient.GetOverallProgress();
            byte valueByte = (byte)(this.progressBarOverall.Value * 255 / 100);
            this.progressBarOverall.Foreground = new SolidColorBrush(Color.FromArgb(255, (byte)(255 - valueByte), (byte)(valueByte * 0.65), 0));

            this.dgList.Items.Refresh();
        }

        /// <summary>
        /// Registering the events.
        /// </summary>
        private void RegisterEvents()
        {
            this.fileTransferClient.Complete += this.TransferClient_Complete;
            this.fileTransferClient.Disconnected += this.TransferClient_Disconnected;
            this.fileTransferClient.ProgressChanged += this.TransferClient_ProgressChanged;
            this.fileTransferClient.Queued += this.TransferClient_Queued;
            this.fileTransferClient.Stopped += this.TransferClient_Stopped;
        }

        /// <summary>
        /// Deregistering the events.
        /// </summary>
        private void DeregisterEvents()
        {
            if (this.fileTransferClient == null)
                return;

            this.fileTransferClient.Complete -= this.TransferClient_Complete;
            this.fileTransferClient.Disconnected -= this.TransferClient_Disconnected;
            this.fileTransferClient.ProgressChanged -= this.TransferClient_ProgressChanged;
            this.fileTransferClient.Queued -= this.TransferClient_Queued;
            this.fileTransferClient.Stopped -= this.TransferClient_Stopped;
        }

        /// <summary>
        /// Sending file to the client.
        /// </summary>
        private void SendFile()
        {
            if (this.fileTransferClient == null)
                return;

            this.overallProgressTimer.Start();

            foreach (string file in DataClass.fileNames)
                this.fileTransferClient.QueueTransfer(file);
        }

        /// <summary>
        /// Pausing the running transfers.
        /// </summary>
        private void BtnPauseTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (this.fileTransferClient == null)
                return;

            //HAFIZA YÖNETİMİ İÇİN DEĞİŞİKLİKLER OLACAK.
            foreach (TransferQueue transferQueue in this.transfers)
                if (transferQueue.Client != null)
                    transferQueue.Client.PauseTransfer(transferQueue);

            this.btnResumeTransfer.IsEnabled = true;
            this.btnPauseTransfer.IsEnabled = false;
        }

        /// <summary>
        /// Resuming the paused transfers.
        /// </summary>
        private void BtnResumeTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (this.fileTransferClient == null)
                return;

            //HAFIZA YÖNETİMİ İÇİN DEĞİŞİKLİKLER OLACAK.
            foreach (TransferQueue transferQueue in this.transfers)
                if(transferQueue.Client != null)
                    transferQueue.Client.PauseTransfer(transferQueue);

            this.btnResumeTransfer.IsEnabled = false;
            this.btnPauseTransfer.IsEnabled = true;
        }

        /// <summary>
        /// Stoping the transfers. (Completely)
        /// </summary>
        private void BtnStopTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (this.fileTransferClient == null)
                return;

            //HAFIZA YÖNETİMİ İÇİN DEĞİŞİKLİKLER OLACAK.
            foreach (TransferQueue transferQueue in this.transfers)
                if(transferQueue.Client != null)
                    transferQueue.Client.StopTransfer(transferQueue);

            this.transfers.Clear();
            this.progressBarOverall.Value = 0;

            Thread.Sleep(500);

            this.Close();
        }

        //private void btnClearComplete_Click(object sender, EventArgs e)
        //{
        //    foreach (TransferQueue transferQueue in transfers)
        //        if (transferQueue.Progress == 100 || !transferQueue.Running)
        //            transfers.Remove(transferQueue);
        //}

        /// <summary>
        /// Gets triggered when connection returns a result.
        /// </summary>
        private void ConnectCallback(object sender, string error)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new ConnectCallback(this.ConnectCallback), sender, error);
                return;
            }

            this.IsEnabled = true;

            if (error != null)
            {
                this.fileTransferClient.Close();
                this.fileTransferClient = null;
                MessageBox.Show(error, "Connection Error");
                return;
            }

            this.RegisterEvents();

            this.fileTransferClient.Run();

            this.SendFile();
        }

        /// <summary>
        /// Gets triggered when a file added into the queue.
        /// </summary>
        private void TransferClient_Queued(object sender, TransferQueue queue)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new TransferEventHandler(this.TransferClient_Queued), sender, queue);
                return;
            }

            this.transfers.Add(queue);

            if (queue.Type == QueueType.Download)
                this.fileTransferClient.StartTransfer(queue);
        }

        //Gets triggered when progress of the transferQueue changes.
        private void TransferClient_ProgressChanged(object sender, TransferQueue queue)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new TransferEventHandler(this.TransferClient_ProgressChanged), sender, queue);
                return;
            }

            foreach (var clientQueue in this.fileTransferClient.Transfers)
                if (clientQueue.Value.Progress < 100)
                    return;

            Thread.Sleep(500);

            this.Close();
        }

        /// <summary>
        /// Playing a system sound after the transfer finishes.
        /// </summary>
        private void TransferClient_Complete(object sender, TransferQueue queue)
        {
            System.Media.SystemSounds.Asterisk.Play();
        }

        /// <summary>
        /// Gets triggered when connection gets lost.
        /// </summary>
        private void TransferClient_Disconnected(object sender, EventArgs e)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new EventHandler(this.TransferClient_Disconnected), sender, e);
                return;
            }

            this.DeregisterEvents();

            foreach (TransferQueue transferQueue in this.transfers)
                if (transferQueue != null)
                    transferQueue.Close();

            this.transfers.Clear();

            this.progressBarOverall.Value = 0;

            this.fileTransferClient = null;
        }

        /// <summary>
        /// Gets triggered when client stops.
        /// </summary>
        private void TransferClient_Stopped(object sender, TransferQueue queue)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(new TransferEventHandler(this.TransferClient_Stopped), sender, queue);
                return;
            }

            this.transfers.Remove(queue);
        }
    }
}
