using LabControl.Libs;
using LabControl.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LabControl
{
    /// <summary>
    /// Interaction logic for ScreenShareWindow.xaml
    /// </summary>
    public partial class ScreenShareWindow : Window
    {
        #region variables

        private System.Windows.Forms.Timer printScreenTimer = new System.Windows.Forms.Timer() { Interval = 120 };
        private Computer currentComputer;
        private double _Width
        {
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.Width = value + 16;
                });
            }
        }

        private double _Height
        {
            set
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.Height = value + 39;
                });
            }
        }

        private bool widthHeightState = false;
        private TcpClient screenClient;
        private TcpListener screenServer;
        private NetworkStream mainStream;

        private Thread screenListeningThread;
        private Thread screenGetImageThread;
        private CommandsPanel ownerCommandPanel;

        #endregion

        #region win32_Dll_API_Cursor_Info

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        #endregion

        public ScreenShareWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (CommandsPanel currentCommandsPanel in DataClass.commandPanels)
                currentCommandsPanel.btnViewScreen.IsEnabled = false;

            DataClass.mainWindow.contextMenuDG.Visibility = Visibility.Hidden;
            DataClass.mainWindow.btnRefresh.IsEnabled = false;
            DataClass.mainWindow.contextMenuDG.IsEnabled = false;
            DataClass.mainWindow.rctNotRunning.IsEnabled = false;
            DataClass.mainWindow.rctRunning.IsEnabled = false;
            DataClass.mainWindow.txtTotal.IsEnabled = false;

            this.currentComputer = new Computer() { IPAddress = DataClass.selectedComputer.IPAddress, IsRunning = DataClass.selectedComputer.IsRunning, Name = DataClass.selectedComputer.Name };
            this.printScreenTimer.Tick += this.PrintScreenTimer_Tick;
            this.screenClient = new TcpClient();
            this.screenListeningThread = new Thread(this.StartListening);
            this.screenGetImageThread = new Thread(this.ReceiveImage);
            this.ownerCommandPanel = (CommandsPanel)this.Tag;
            this.screenServer = new TcpListener(IPAddress.Any, 1919);
            this.screenListeningThread.Start();
            this.printScreenTimer.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.printScreenTimer.Stop();
            this.printScreenTimer = null;
            this.ownerCommandPanel.StopScreenSharing();
            this.StopListening();

            foreach (CommandsPanel currentCommandsPanel in DataClass.commandPanels)
                currentCommandsPanel.btnViewScreen.IsEnabled = true;

            DataClass.mainWindow.contextMenuDG.Visibility = Visibility.Visible;
            DataClass.mainWindow.btnRefresh.IsEnabled = true;
            DataClass.mainWindow.contextMenuDG.IsEnabled = true;
            DataClass.mainWindow.rctNotRunning.IsEnabled = true;
            DataClass.mainWindow.rctRunning.IsEnabled = true;
            DataClass.mainWindow.txtTotal.IsEnabled = true;
        }

        /// <summary>
        /// Sending cursor point to the client.
        /// </summary>
        private void PrintScreenTimer_Tick(object sender, EventArgs e)
        {
            System.Windows.Point CursorPoint = this.GetMousePosition();
            if(CursorPoint.X >= 0 && CursorPoint.Y >= 0)
                DataSender.SendData("mouse_point:" + CursorPoint.X + ":" + CursorPoint.Y, this.currentComputer);
        }

        /// <summary>
        /// Starting the server for listening screenshots from client.
        /// </summary>
        private void StartListening()
        {
            try
            {
                while (!this.screenClient.Connected)
                {
                    this.screenServer.Start();
                    this.screenClient = this.screenServer.AcceptTcpClient();
                }
                this.screenGetImageThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error wile listening for ScreenShare " + ex.Source + " ScreenShareWindow StartListening()", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            }
        }

        /// <summary>
        /// Stopping the server.
        /// </summary>
        private void StopListening()
        {
            this.screenServer.Stop();
            this.screenClient = null;

            if (this.screenListeningThread.IsAlive)
                this.screenListeningThread.Abort();

            if (this.screenGetImageThread.IsAlive)
                this.screenGetImageThread.Abort();
        }

        /// <summary>
        /// Receiving screenshots from client.
        /// </summary>
        private void ReceiveImage()
        {
            BinaryFormatter imageBinaryFormatter = new BinaryFormatter();
            try
            {
                while (this.screenClient.Connected)
                {
                    this.mainStream = this.screenClient.GetStream();
                    System.Drawing.Image imgGelen = (System.Drawing.Image)imageBinaryFormatter.Deserialize(this.mainStream);
                    Bitmap img = (Bitmap)imgGelen;
                    ImageSource imgSrc = this.ImageSourceFromBitmap(img);
                    if (!this.widthHeightState)
                    {
                        this._Width = imgSrc.Width;
                        this._Height = imgSrc.Height;
                        this.widthHeightState = true;
                    }
                    this.imgScreen.Dispatcher.Invoke(() =>
                    {
                        this.imgScreen.Source = this.ImageSourceFromBitmap(img);
                    });
                }
            }
            catch
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.Close();
                });
            }
        }

        /// <summary>
        /// Converting Bitmap to ImageSource for using on WPF Applications.
        /// </summary>
        private ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            IntPtr handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        /// <summary>
        /// Getting the cursor position.
        /// </summary>
        private System.Windows.Point GetMousePosition()
        {
            return Mouse.GetPosition(this.imgScreen);
        }

        /// <summary>
        /// Overriding cursor for eliminate cursor conflicts.
        /// </summary>
        private void ImgScreen_MouseEnter(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.None;
        }

        /// <summary>
        /// Removing the override of the cursor when "MouseLeave" event gets triggered.
        /// </summary>
        private void ImgScreen_MouseLeave(object sender, MouseEventArgs e)
        {
            Mouse.OverrideCursor = null;
        }

        /// <summary>
        /// Sending the "LeftButtonDown" Mouse Action to the client.
        /// </summary>
        private void ImgScreen_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataSender.SendData("mouse_left_down", this.currentComputer);
        }

        /// <summary>
        /// Sending the "LeftButtonUp" Mouse Action to the client.
        /// </summary>
        private void ImgScreen_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataSender.SendData("mouse_left_up", this.currentComputer);
        }

        /// <summary>
        /// Sending the "RightButtonDown" Mouse Action to the client.
        /// </summary>
        private void ImgScreen_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataSender.SendData("mouse_right_down", this.currentComputer);
        }

        /// <summary>
        /// Sending the "RightButtonUp" Mouse Action to the client.
        /// </summary>
        private void ImgScreen_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataSender.SendData("mouse_right_up", this.currentComputer);
        }

        /// <summary>
        /// Sending the "KeyDown" Keyboard Actions to the client.
        /// </summary>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            //Burada Türkçe harfler yok. Eklenecek.
            Key[] shiftCombines = { Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J, Key.K, Key.L, Key.M, Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T, Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z, Key.Left, Key.Right, Key.Up, Key.Down };
            int[] specialKeys = { 0xA0, 0xA2, 0xA3, 0x00, 0x09 };
            int keyCode = KeyInterop.VirtualKeyFromKey(e.Key);
            if (!specialKeys.Contains(keyCode) && !((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) && !((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) && !((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift))
                DataSender.SendData("key:" + keyCode.ToString(), this.currentComputer);

            if (e.Key == Key.A && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                DataSender.SendData("combin:" + KeyInterop.VirtualKeyFromKey(Key.LeftCtrl) + "+" + KeyInterop.VirtualKeyFromKey(Key.A), this.currentComputer);
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                DataSender.SendData("combin:" + KeyInterop.VirtualKeyFromKey(Key.LeftCtrl) + "+" + KeyInterop.VirtualKeyFromKey(Key.C), this.currentComputer);
            else if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                DataSender.SendData("combin:" + KeyInterop.VirtualKeyFromKey(Key.LeftCtrl) + "+" + KeyInterop.VirtualKeyFromKey(Key.V), this.currentComputer);
            else if (e.Key == Key.X && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                DataSender.SendData("combin:" + KeyInterop.VirtualKeyFromKey(Key.LeftCtrl) + "+" + KeyInterop.VirtualKeyFromKey(Key.X), this.currentComputer);
            else if (e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                DataSender.SendData("combin:" + KeyInterop.VirtualKeyFromKey(Key.LeftCtrl) + "+" + KeyInterop.VirtualKeyFromKey(Key.W), this.currentComputer);
            else if (shiftCombines.Contains(e.Key) && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                DataSender.SendData("combin:" + KeyInterop.VirtualKeyFromKey(Key.LeftShift) + "+" + KeyInterop.VirtualKeyFromKey(e.Key), this.currentComputer);
        }

        /// <summary>
        /// Sending the "MouseWheel" Mouse Action to the client with delta value.
        /// </summary>
        private void imgScreen_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            DataSender.SendData("mouse_wheel:" + e.Delta.ToString(), this.currentComputer);
        }

        /// <summary>
        /// Preventing from ALT+F4.
        /// </summary>
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                e.Handled = true;
                DataSender.SendData("combin:" + KeyInterop.VirtualKeyFromKey(Key.LeftAlt) + "+" + KeyInterop.VirtualKeyFromKey(Key.F4), this.currentComputer);
            }
        }

        /// <summary>
        /// Drag-Drop for sending file.
        /// </summary>
        private void imgScreen_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                DataClass.fileNames = e.Data.GetData(DataFormats.FileDrop, true) as string[];
                FileTransferWindow fileTransferWindow = new FileTransferWindow() { Tag = this.currentComputer, Owner = DataClass.mainWindow };
                fileTransferWindow.ShowDialog();
            }
        }
    }
}
