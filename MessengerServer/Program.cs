using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Configuration;

namespace Messenger
{
    class Program
    {
        private readonly static List<string> Commands = new List<string>() { "/help", "/getusers", "/killserver" };
        private readonly static string _serverHost = "localhost";
        // для теста в локальной сети
        // private readonly static string _serverHost = "192.168.1.1";
        private readonly static int _serverPort = 9933;
        private static Thread _serverThread;

        static void Main(string[] args)
        {
            _serverThread = new Thread(StartServer)
            {
                IsBackground = true
            };
            _serverThread.Start();

            while (true)
            {
                HandlerCommands(Console.ReadLine());
            }
        }

        private static void HandlerCommands(string cmd)
        {
            cmd = cmd.ToLower();
            if (cmd.Contains("/help"))
            {
                Console.WriteLine("List of available commands:");
                foreach (var command in Commands)
                {
                    Console.WriteLine(command);
                }
            }

            if (cmd.Contains("/getusers"))
            {
                int countUsers = Server.Clients.Count;

                if (countUsers == 0)
                    Console.WriteLine("No one is in the chat");
                else
                {
                    for (int i = 0; i < countUsers; i++)
                    {
                        Console.WriteLine("{0}. {1}", i + 1, Server.Clients[i].UserName);
                    }
                }
            }

            if (cmd.Contains("/killserver"))
            {
                Console.WriteLine("Server successfully killed");
                Environment.Exit(1);
            }
        }

        private static void StartServer()
        {
            IPHostEntry ipHost = Dns.GetHostEntry(_serverHost);
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, _serverPort);
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(ipEndPoint);
            socket.Listen(1000);
            Console.WriteLine("Server has been started on IP: {0}", ipEndPoint);
            Console.WriteLine("Use /help to display a list of all commands\n");

            while (true)
            {
                try
                {
                    Socket user = socket.Accept();
                    Server.NewClient(user);
                } catch (Exception exp)
                {
                    Console.WriteLine("Error with StartServer(): {0}", exp.Message);
                }
            }
        }
    }
}
