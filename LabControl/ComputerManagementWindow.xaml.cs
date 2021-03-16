using LabControl.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Serialization;

namespace LabControl
{
    /// <summary>
    /// Interaction logic for ComputerManagementWindow.xaml
    /// </summary>
    public partial class ComputerManagementWindow : Window
    {
        XmlSerializer serializer = new XmlSerializer(typeof(List<Computer>));
        
        public ComputerManagementWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.dgManagement.ItemsSource = Computer.Computers;
        }

        /// <summary>
        /// Removing the selected computer from computers.xml file.
        /// </summary>
        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            Computer.Computers.Remove((Computer)dgManagement.SelectedItem);

            this.ListToXML();

            this.Refresh();
        }

        /// <summary>
        /// Refreshing datagrid.
        /// </summary>
        private void Refresh()
        {
            this.dgManagement.ItemsSource = Computer.Computers;
            this.dgManagement.Items.Refresh();
        }

        /// <summary>
        /// Writing data of List to the computer.xml file.
        /// </summary>
        private void ListToXML()
        {
            this.serializer = new XmlSerializer(typeof(List<Computer>));
            using (FileStream fs = new FileStream(Environment.CurrentDirectory + "\\computers.xml", FileMode.Create, FileAccess.Write))
            {
                this.serializer.Serialize(fs, Computer.Computers);
            }
        }

        /// <summary>
        /// Opening a new edit AddEditComputerWindow.
        /// </summary>
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            AddEditComputerWindow addEditComputerWindow = new AddEditComputerWindow() { Tag = (Computer)this.dgManagement.SelectedItem };
            addEditComputerWindow.Closing += AddEditComputerWindow_Closing;
            addEditComputerWindow.ShowDialog();
        }

        /// <summary>
        /// Opening a new edit AddEditComputerWindow.
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            AddEditComputerWindow addEditComputerWindow = new AddEditComputerWindow() { Tag = null };
            addEditComputerWindow.Closing += AddEditComputerWindow_Closing;
            addEditComputerWindow.ShowDialog();
        }

        /// <summary>
        /// Refreshing datagrid after an edit or add process.
        /// </summary>
        private void AddEditComputerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Refresh();
        }

        private void DgManagement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.dgManagement.SelectedIndex != -1)
                this.btnEdit.IsEnabled = this.btnRemove.IsEnabled = true;
            else
                this.btnEdit.IsEnabled = this.btnRemove.IsEnabled = false;
        }
    }
}
