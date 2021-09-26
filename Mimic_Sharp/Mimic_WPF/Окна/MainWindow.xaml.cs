using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.Sockets;

using Modbus.Device;
using Modbus.Data;

namespace Mimic_WPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public TcpSlave tcpSlave;
        public ConnectStatus tcpSlave_status = ConnectStatus.Offline;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            /*string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }*/

            //string address = "192.168.3.66"; // адрес сервера
            string address = "127.0.0.1"; // адрес сервера
            int port = 502; // порт сервера

            try
            {
                tcpSlave = new TcpSlave(address, port, 1);
                tcpSlave.Start();

                //AddLogMessage($"TCP-slave по адресу: {address}, порт {port} - Включён");
            }
            catch (Exception ex)
            {
                string message = $"Ошибка при подключении TCP-slave! {ex.Message}";
                //AddLogMessage("ERROR! " + message);
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Random rand = new Random();

            for (int i = 1; i < 65536; i++)
            {
                tcpSlave.Write(i, (ushort)rand.Next(1, 1000));
            }
        }
    }

    public enum ConnectStatus
    {
        Offline,
        Connect,
        Online
    }
}
