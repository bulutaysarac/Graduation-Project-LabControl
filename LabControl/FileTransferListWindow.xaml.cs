using LabControl.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for FileTransferListWindow.xaml
    /// </summary>
    public partial class FileTransferListWindow : Window
    {
        #region variables

        #endregion

        public FileTransferListWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ComputerState.FillComputerStateList();
            this.dgComputerList.ItemsSource = ComputerState.ComputerStateList.Where(c => c.IsRunning == true);
        }

        /// <summary>
        /// Starting file transfer process.
        /// </summary>
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            this.btnStart.IsEnabled = false;
            foreach (ComputerState currentComputerState in this.dgComputerList.Items)
            {
                FileTransferWindow fileTransferWindow = new FileTransferWindow() { Tag = currentComputerState, Owner = this };
                fileTransferWindow.ShowDialog();
                currentComputerState.Sent = true;
                this.dgComputerList.Items.Refresh();
            }

            MessageBox.Show("All transfers are completed", "Transfer Completed", MessageBoxButton.OK, MessageBoxImage.Information);

            Thread.Sleep(1000);

            this.Close();
        }
    }
}
