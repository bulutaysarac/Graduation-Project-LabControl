using LabControl.Models;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace LabControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region variables

        private XmlSerializer serializer = new XmlSerializer(typeof(List<Computer>));
        private TcpClient testClient;
        private Thread threadTestConnection;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataClass.mainWindow = this;
            this.gridMain.IsEnabled = false;
            this.Cursor = Cursors.AppStarting;

            this.FillDataGrid();
            this.threadTestConnection = new Thread(TestComputers);
            this.threadTestConnection.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Testing all of the IPs to see which ones are online.
        /// </summary>
        private void TestComputers()
        {
            foreach (Computer currentComputer in Computer.Computers)
            {
                try
                {
                    currentComputer.IsRunning = this.TryToConnect(currentComputer);
                }
                catch (Exception ex) 
                {
                    MessageBox.Show("Error while testing connections " + ex.Source + " MainWindow TryToConnect()", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
                {
                    this.dataGridList.Items.Refresh();
                    this.txtCountOfAllComputers.Text = Computer.Computers.Count.ToString();
                    this.txtCountOfComputersNotRunning.Text = Computer.Computers.Where(c => c.IsRunning == false).ToList().Count.ToString();
                    this.txtCountOfComputersRunning.Text = Computer.Computers.Where(c => c.IsRunning == true).ToList().Count.ToString();
                }));
            }

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                this.gridMain.IsEnabled = true;
                this.Cursor = Cursors.Arrow;
            }));
        }

        /// <summary>
        /// Filling the datagrid with datas in the computers.xml file.
        /// </summary>
        private void FillDataGrid()
        {
            using (FileStream fs = new FileStream(Environment.CurrentDirectory + "\\computers.xml", FileMode.Open, FileAccess.Read))
            {
                Computer.Computers = this.serializer.Deserialize(fs) as List<Computer>;
            }
            this.dataGridList.ItemsSource = Computer.Computers;
            this.dataGridList.Items.Refresh();
        }

        /// <summary>
        /// Trying to connect the Computer. If it is success, returns true.
        /// </summary>
        private bool TryToConnect(Computer computer)
        {
            this.testClient = new TcpClient();
            IAsyncResult result = this.testClient.BeginConnect(IPAddress.Parse(computer.IPAddress), 1717, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(100, true);
            this.testClient.Close();
            return success;
        }

        /// <summary>
        /// Refreshing the datagrid.
        /// </summary>
        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.gridMain.IsEnabled = false;
            this.Cursor = Cursors.AppStarting;
            if (this.threadTestConnection.IsAlive)
                this.threadTestConnection.Abort();
            this.threadTestConnection = new Thread(TestComputers);
            this.threadTestConnection.Start();
        }

        /// <summary>
        /// Setting the selectedComputer static field of DataClass via "SelectionChanged" event.
        /// </summary>
        private void DgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.dataGridList.SelectedIndex != -1)
                DataClass.selectedComputer = this.dataGridList.SelectedItem as Computer;
        }

        /// <summary>
        /// Filtering the datagrid.
        /// </summary>
        private void BtnComputersRunning_Click(object sender, RoutedEventArgs e)
        {
            this.dataGridList.ItemsSource = Computer.Computers.Where(c => c.IsRunning == true).ToList();
            this.dataGridList.Items.Refresh();
        }

        /// <summary>
        /// Filtering the datagrid.
        /// </summary>
        private void BtnComputersNotRunning_Click(object sender, RoutedEventArgs e)
        {
            this.dataGridList.ItemsSource = Computer.Computers.Where(c => c.IsRunning == false).ToList();
            this.dataGridList.Items.Refresh();
        }

        /// <summary>
        /// Removing filters from the datagrid.
        /// </summary>
        private void BtnAllComputers_Click(object sender, RoutedEventArgs e)
        {
            this.dataGridList.ItemsSource = Computer.Computers;
            this.dataGridList.Items.Refresh();
        }

        /// <summary>
        /// Opening the ComputerManagementWindow.
        /// </summary>
        private void BtnMenuComputers_Click(object sender, RoutedEventArgs e)
        {
            ComputerManagementWindow computerManagementWindow = new ComputerManagementWindow();
            computerManagementWindow.ShowDialog();
        }

        private void rctRunning_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.dataGridList.ItemsSource = Computer.Computers.Where(c => c.IsRunning == true).ToList();
            this.dataGridList.Items.Refresh();
        }

        private void rctNotRunning_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.dataGridList.ItemsSource = Computer.Computers.Where(c => c.IsRunning == false).ToList();
            this.dataGridList.Items.Refresh();
        }

        private void txtTotal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.dataGridList.ItemsSource = Computer.Computers;
            this.dataGridList.Items.Refresh();
        }
    }
}
