using LabControl.Models;
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
using System.Xml.Serialization;

namespace LabControl
{
    /// <summary>
    /// Interaction logic for AddEditComputerWindow.xaml
    /// </summary>
    public partial class AddEditComputerWindow : Window
    {
        #region variables

        private Computer currentComputer;
        private bool IsEdit = false;

        #endregion

        public AddEditComputerWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Tag != null)
            {
                this.currentComputer = (Computer)this.Tag;
                string[] octals = this.currentComputer.IPAddress.Split('.');
                this.txtFirstOctal.Text = octals[0];
                this.txtSecondOctal.Text = octals[1];
                this.txtThirdOctal.Text = octals[2];
                this.txtFourthOctal.Text = octals[3];
                this.txtComputerName.Text = this.currentComputer.Name;
                this.btnAddEdit.Content = "Save";
                this.IsEdit = true;
            }
        }

        /// <summary>
        /// Adding a new computer to the computers.xml file.
        /// </summary>
        private void BtnAddEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!this.IsValid(this.txtComputerName.Text))
            {
                MessageBox.Show("ComputerName can not be empty.", "Data is not valid!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (txtFirstOctal.Text == string.Empty || txtSecondOctal.Text == string.Empty || txtThirdOctal.Text == string.Empty || txtFourthOctal.Text == string.Empty)
            {
                MessageBox.Show("IP Octals can not be empty.", "Data is not valid!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string ipAddress = this.txtFirstOctal.Text + "." + this.txtSecondOctal.Text + "." + this.txtThirdOctal.Text + "." + this.txtFourthOctal.Text;
            if (Computer.Computers.Find(c => c.IPAddress == ipAddress) != null && !this.IsEdit)
            {
                MessageBox.Show("There is already a computer with this IPAddress", "Data is not valid!", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!this.IsEdit)
            {
                Computer.Computers.Add(new Computer()
                {
                    IsRunning = false,
                    Name = txtComputerName.Text,
                    IPAddress = ipAddress
                });
            }
            else
            {
                Computer editingComputer = Computer.Computers.Find(c => c.IPAddress == currentComputer.IPAddress);
                editingComputer.IPAddress = ipAddress;
                editingComputer.Name = txtComputerName.Text;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(List<Computer>));
            using (FileStream fs = new FileStream(Environment.CurrentDirectory + "\\computers.xml", FileMode.Create, FileAccess.Write))
            {
                serializer.Serialize(fs, Computer.Computers);
            }

            Thread.Sleep(700);
            this.Close();
        }

        /// <summary>
        /// Checking if text chars are only numbers and below 255.
        /// </summary>
        private void TxtOctals_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox currentTextBox = (TextBox)sender;
            int? index = IsNumber(currentTextBox.Text);
            if (index != null)
                currentTextBox.Text = currentTextBox.Text.Remove((int)index, 1);
            else
            {
                if (currentTextBox.Text != string.Empty)
                    if (int.Parse(currentTextBox.Text) > 255)
                        currentTextBox.Text = "255";
            }
        }

        /// <summary>
        /// Checking if parameter chars are only numbers.
        /// </summary>
        private int? IsNumber(string Text)
        {
            for (int i = 0; i < Text.Length; i++)
                if (("0123456789").IndexOf(Text[i]) == -1)
                    return i;
            return null;
        }

        /// <summary>
        /// Checking if parameter is a valid name.
        /// </summary>
        private bool IsValid(string Text)
        {
            for (int i = 0; i < Text.Length; i++)
                if (Text[i] != ' ')
                    return true;
            return false;
        }
    }
}
