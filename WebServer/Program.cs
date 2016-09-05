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

        // Receiving the verb
        public void sendVerb(string verb)
        {
            Console.WriteLine(verb);
        }

        public void listen()
        {
            TcpListener server = new TcpListener(3000); // Port number in localhost
            server.Start();
            
            // Loop for listening clients

            while (true)
            {
                // Accepting connection with new client
                TcpClient client = server.AcceptTcpClient();
                ClientServerManager manager = new ClientServerManager(this, client);
                // Handling request from client in a separate thread
                Thread thread = new Thread(new ThreadStart(manager.getRequest));
                thread.Start();
                Thread.Sleep(1);
            }
        }
    }

    public class ClientServerManager
    {
        // Client and Server instances from Server class
        TcpClient client;
        Server server;

        public ClientServerManager(Server server, TcpClient client)
        {
            this.client = client;
            this.server = server;
        }
        
        public void getRequest()
        {
            // Gets the request input stream from client
            Stream stream = new BufferedStream(client.GetStream());
            String request = "";
            // Parses the request
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
            // Sends the http verb to server
            server.sendVerb(tokens[0].ToUpper(););
            //clears request input stream
            stream = null;
            // Closes client connection
            client.Close();
        }
    }
}
