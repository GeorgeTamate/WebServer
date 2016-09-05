using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections;

namespace WebServer
{
    public class Program
    {
        // Port 3000

        static void Main(string[] args)
        {
            Server server = new Server();
            Thread thread = new Thread(new ThreadStart(server.listen));
            thread.Start();
        }
    }

    public class Server
    { 
        public Server()
        {}

        public void sendVerb(string verb)
        {
            Console.WriteLine(verb);
        }

        public void listen()
        {
            TcpListener server = new TcpListener(3000); // Port number in localhost
            server.Start();
            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                ClientServerManager manager = new ClientServerManager(this, client);
                Thread thread = new Thread(new ThreadStart(manager.getRequest));
                thread.Start();
                Thread.Sleep(1);
            }
        }
    }

    public class ClientServerManager
    {
        TcpClient client;
        Server server;
        String verb;
        Stream stream;

        public ClientServerManager(Server server, TcpClient client)
        {
            this.client = client;
            this.server = server;
        }
        
        public void getRequest()
        {
            stream = new BufferedStream(client.GetStream());
            String request = "";
            int next;
            while (true)
            {
                next = stream.ReadByte();
                if (next == '\n') 
                { 
                    break;
                }
                if (next == '\r') 
                { 
                    continue; 
                }
                request += Convert.ToChar(next);
            }
            string[] tokens = request.Split(' ');
            verb = tokens[0].ToUpper();
            server.sendVerb(verb);
            stream = null;
            client.Close();
        }
    }
}
