﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

namespace Messenger
{
    public class Client
    {
        private string _userName;
        private readonly Socket _handler;
        private readonly Thread _userThread;

        public Client(Socket socket)
        {
            _handler = socket;
            _userThread = new Thread(Listener)
            {
                IsBackground = true
            };
            _userThread.Start();
        }

        public string UserName
        {
            get { return _userName; }
        }

        private void Listener()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRec = _handler.Receive(buffer);
                    string data = Encoding.UTF8.GetString(buffer, 0, bytesRec);
                    HandleCommand(data);
                } catch
                {
                    Server.EndClient(this);
                    return;
                }
            }
        }

        public void End()
        {
            try
            {
                _handler.Close();
                try
                {
                    _userThread.Abort();
                }
                catch {}
            } catch (Exception exp)
            {
                Console.WriteLine("Error with End(): {0}", exp.Message);
            }
        }

        private void HandleCommand(string data)
        {
            if (data.Contains("#setname"))
            {
                _userName = data.Split('&')[1];
                UpdateChat();
                return;
            }
            if (data.Contains("#newmsg"))
            {
                string message = data.Split('&')[1];
                ChatController.AddMessage(_userName, message);
                return;
            }
        }

        public void UpdateChat()
        {
            SendCommand(ChatController.GetChat());
        }

        public void SendCommand(string command)
        {
            try
            {
                int bytesSent = _handler.Send(Encoding.UTF8.GetBytes(command));
                if (bytesSent > 0)
                    Console.WriteLine("Success");
            } catch (Exception exp)
            {
                Console.WriteLine("Error with SendCommand(): {0}", exp.Message);
                Server.EndClient(this);
            }
        }
    }
}
