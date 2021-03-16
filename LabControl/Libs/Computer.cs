using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace LabControl.Models
{
    public class Computer
    {
        public bool IsRunning { get; set; }
        public string Name { get; set; }
        public string IPAddress { get; set; }

        public static List<Computer> Computers = new List<Computer>();

        public static Computer ThisComputer = new Computer
        {
            Name = Environment.MachineName,
            IPAddress = Computer.GetLocalIPAddress()
        };

        public static string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();
            return null;
        }
    }

    public class ComputerState : Computer
    {
        public bool Sent { get; set; }

        public static List<ComputerState> ComputerStateList = new List<ComputerState>();

        public static void FillComputerStateList()
        {
            ComputerState.ComputerStateList.Clear();
            foreach (Computer computer in Computer.Computers)
            {
                ComputerState.ComputerStateList.Add(new ComputerState()
                {
                    IsRunning = computer.IsRunning,
                    Name = computer.Name,
                    IPAddress = computer.IPAddress,
                    Sent = false
                });
            }
        }
    }
}
