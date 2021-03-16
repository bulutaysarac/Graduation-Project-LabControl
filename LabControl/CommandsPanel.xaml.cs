using LabControl.Libs;
using LabControl.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LabControl
{
    /// <summary>
    /// Interaction logic for CommandsPanel.xaml
    /// </summary>
    public partial class CommandsPanel : UserControl
    {
        public CommandsPanel()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if(this.Tag.Equals("single"))
                DataClass.commandPanels.Add(this);

            if (this.Tag.Equals("all"))
                this.btnViewScreen.IsEnabled = false;

            this.SetToolTips();
        }

        /// <summary>
        /// Setting tooltips of components.
        /// </summary>
        private void SetToolTips()
        {
            if (this.Tag.Equals("all"))
            {
                this.btnLock.ToolTip = "Lock all running computers";
                this.btnRestart.ToolTip = "Restart all running computers";
                this.btnSendFile.ToolTip = "Send files to all running computers";
                this.btnShutDown.ToolTip = "Shut down all running computers";
                ToolTipService.SetShowOnDisabled(this.btnViewScreen, true);
                this.btnViewScreen.ToolTip = "Remote-Control is disabled for multiple computers";
            }
            else if (this.Tag.Equals("single"))
            {
                this.btnLock.ToolTip = "Lock selected computer";
                this.btnRestart.ToolTip = "Restart selected computer";
                this.btnSendFile.ToolTip = "Send files to selected computer";
                this.btnShutDown.ToolTip = "Shut down selected computer";
                this.btnViewScreen.ToolTip = "Remote-Control selected computer";
            }
        }

        /// <summary>
        /// Beginning of file transfer process.
        /// </summary>
        private void BtnSendFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Files (*.*)|*.*";
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog().Equals(true))
                DataClass.fileNames = openFileDialog.FileNames;
            else
                return;

            if (this.Tag.Equals("single"))
            {
                FileTransferWindow fileTransferWindow = new FileTransferWindow() { Tag = DataClass.selectedComputer, Owner = DataClass.mainWindow };
                fileTransferWindow.ShowDialog();
            }
            else if (this.Tag.Equals("all"))
            {
                FileTransferListWindow fileTransferListWindow = new FileTransferListWindow() { Owner = DataClass.mainWindow };
                fileTransferListWindow.ShowDialog();
            }
        }

        /// <summary>
        /// Sending "lock" string to the client.
        /// </summary>
        private void BtnLock_Click(object sender, RoutedEventArgs e)
        {
            if (this.Tag.Equals("single"))
                DataSender.SendData("lock", DataClass.selectedComputer);
            else if (this.Tag.Equals("all"))
                DataSender.SendData("lock", Computer.Computers.Where(c => c.IsRunning == true).ToList());
        }

        /// <summary>
        /// Beginning of the screenshare process.
        /// </summary>
        private void BtnViewScreen_Click(object sender, RoutedEventArgs e)
        {
            ScreenShareWindow screenShareWindow = new ScreenShareWindow() { Tag = this };
            screenShareWindow.Show();
            Thread.Sleep(100);
            DataSender.SendData("share_screen:" + Computer.ThisComputer.IPAddress, DataClass.selectedComputer);
        }

        /// <summary>
        /// Sending "stop_screen_share" string to the client for stopping stream.
        /// </summary>
        public void StopScreenSharing()
        {
            DataSender.SendData("stop_screen_share", DataClass.selectedComputer);
        }

        /// <summary>
        /// Sending "shut_down" string to the client.
        /// </summary>
        private void BtnTurnOff_Click(object sender, RoutedEventArgs e)
        {
            if (this.Tag.Equals("single"))
                DataSender.SendData("shut_down", DataClass.selectedComputer);
            else if (this.Tag.Equals("all"))
                DataSender.SendData("shut_down", Computer.Computers.Where(c => c.IsRunning == true).ToList());
        }

        /// <summary>
        /// Sending "restart" string to the client.
        /// </summary>
        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            if(this.Tag.Equals("single"))
                DataSender.SendData("restart", DataClass.selectedComputer);
            else if(this.Tag.Equals("all"))
                DataSender.SendData("restart", Computer.Computers.Where(c => c.IsRunning == true).ToList());

        }

        /// <summary>
        /// Sending "kill_client" command to the Client for killing client program.
        /// </summary>
        private void btnKillClient_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Are you sure to kill client program? Your connection is not going to be alive anymore.", "Killing Client Program", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (this.Tag.Equals("single"))
                    DataSender.SendData("kill_client", DataClass.selectedComputer);
                else if (this.Tag.Equals("all"))
                    DataSender.SendData("kill_client", Computer.Computers.Where(c => c.IsRunning == true).ToList());
            }
        }

        private void btnRestartClient_Click(object sender, RoutedEventArgs e)
        {
            if (this.Tag.Equals("single"))
                DataSender.SendData("reset_client", DataClass.selectedComputer);
            else if (this.Tag.Equals("all"))
                DataSender.SendData("reset_client", Computer.Computers.Where(c => c.IsRunning == true).ToList());
        }
    }
}
