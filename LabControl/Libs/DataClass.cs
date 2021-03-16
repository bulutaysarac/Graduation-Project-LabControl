using LabControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LabControl
{
    class DataClass
    {
        public static List<CommandsPanel> commandPanels = new List<CommandsPanel>();
        public static MainWindow mainWindow = null;
        public static Computer selectedComputer = null;
        public static string[] fileNames = null;
    }
}
