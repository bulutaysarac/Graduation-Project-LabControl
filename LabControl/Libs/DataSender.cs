using LabControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LabControl.Libs
{
    //VERİ İLETİMİ
    public static class DataSender
    {
        public static void SendData(string stringData, List<Computer> Computers)
        {
            try
            {
                byte[] data = Encoding.Default.GetBytes(stringData);
                foreach (Computer currentComputer in Computers)
                {
                    TcpClient client = new TcpClient();
                    IAsyncResult result = client.BeginConnect(IPAddress.Parse(currentComputer.IPAddress), 1717, null, null);
                    bool success = result.AsyncWaitHandle.WaitOne(1000, true);

                    if (!success)
                    {
                        client.Close();
                        return;
                    }

                    NetworkStream commandNetworkStream = client.GetStream();
                    commandNetworkStream.Write(data, 0, data.Length);
                    client.Close();
                }
            }
            catch {}
        }

        public static void SendData(string stringData, Computer Computer)
        {
            try
            {
                byte[] data = Encoding.Default.GetBytes(stringData);
                TcpClient client = new TcpClient();
                IAsyncResult result = client.BeginConnect(IPAddress.Parse(Computer.IPAddress), 1717, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(1000, true);

                if (!success)
                {
                    client.Close();
                    return;
                }

                NetworkStream commandNetworkStream = client.GetStream();
                commandNetworkStream.Write(data, 0, data.Length);
                client.Close();
            }
            catch {}
        }
    }
}
