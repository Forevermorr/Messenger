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
using System.Threading;
using System.Windows.Interop;

namespace MessengerClient
{
    public partial class MainWindow : Window
    {
        private delegate void printer(string data);
        private delegate void cleaner();
        readonly printer Printer;
        readonly cleaner Cleaner;
        private Socket _serverSocket;
        private readonly Thread _clientThread;
        private readonly static string _serverHost = "localhost";
        private readonly static int _serverPort = 9933;

        public MainWindow()
        {
            InitializeComponent();
            Printer = new printer(Print);
            Cleaner = new cleaner(ClearChat);
            Connect();
            _clientThread = new Thread(Listener)
            {
                IsBackground = true
            };
            _clientThread.Start();
        }

        private void Listener()
        {
            while (_serverSocket.Connected)
            {
                byte[] buffer = new byte[8196];
                int bytesRec = _serverSocket.Receive(buffer);
                string data = Encoding.UTF8.GetString(buffer, 0, bytesRec);
                if (data.Contains("#updatechat"))
                {
                    UpdateChat(data);
                    continue;
                }
            }
        }

        private void Connect()
        {
            try
            {
                IPHostEntry ipHost = Dns.GetHostEntry(_serverHost);
                IPAddress ipAddress = ipHost.AddressList[0];
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, _serverPort);
                _serverSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Connect(ipEndPoint);
            } catch
            {
                Print("Сервер недоступен!");
            }
        }

        private void ClearChat()
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(Cleaner);
                return;
            }
            tbchatBox.Clear();
        }

        private void UpdateChat(string data)
        {
            // #updatechat&userName~data|username-data
            ClearChat();
            string[] messages = data.Split('&')[1].Split('|');
            int countMessages = messages.Length;
            if (countMessages <= 0)
                return;
            for (int i = 0; i < countMessages; i++)
            {
                try
                {
                    if (string.IsNullOrEmpty(messages[i]))
                        continue;
                    Print(String.Format("[{0}]: {1}.", messages[i].Split('~')[0], messages[i].Split('~')[1]));
                } catch
                {
                    continue;
                }
            }
        }

        private void Send(string data)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(data);
                int bytesSent = _serverSocket.Send(buffer);

            } catch
            {
                Print("Связь с сервером прервана...");
            }
        }

        private void Print(string msg)
        {
            if (!this.Dispatcher.CheckAccess())
            {
                this.Dispatcher.Invoke(Printer, msg);
                return;
            }
            if (tbchatBox.Text.Length == 0)
                tbchatBox.AppendText(msg);
            else
                tbchatBox.AppendText(Environment.NewLine + msg);
        }

        private void enterChat_Click(object sender, RoutedEventArgs e)
        {
            string Name = tbuserName.Text;
            if (string.IsNullOrEmpty(Name))
                return;
            Send("#setname&" + Name);
            tbchatBox.IsEnabled = true;
            tbchatMsg.IsEnabled = true;
            btnchatSend.IsEnabled = true;
            tbuserName.IsEnabled = false;
            btnenterChat.IsEnabled = false;
        }

        private void btnchatSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            try
            {
                string data = tbchatMsg.Text;
                if (string.IsNullOrEmpty(data))
                    return;
                Send("#newmsg&" + data);
                tbchatMsg.Text = string.Empty;
            } catch
            {
                MessageBox.Show("Ошибка при отправке сообщения!");
            }
        }

        private void chatBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendMessage();
        }

        private void tbchatMsg_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendMessage();
        }
    }
}
