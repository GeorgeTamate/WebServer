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
            Console.WriteLine("HttpVerb: " + verb);
        }

        public void sendAuth(string username, string password)
        {
            Console.WriteLine("Username: " + username);
            Console.WriteLine("Password: " + password);
            Console.WriteLine();
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

        public String getStringFromStream(Stream s)
        {
            String str = "";
            int nextpos;
            // Parsing http request stream into a string
            while (true)
            {
                nextpos = s.ReadByte();
                if (nextpos == '\n')
                    break;
                if (nextpos == '\r')
                    continue;
                if (nextpos == -1) 
                { 
                    Thread.Sleep(1); 
                    continue; 
                };
                str += Convert.ToChar(nextpos);
            }
            return str;
        }
        
        public void getRequest()
        {
            // Gets the http request stream from client
            Stream stream = new BufferedStream(client.GetStream());
            // Parses http request stream to a string
            String request = getStringFromStream(stream);            
            String verb = request.Split(' ')[0];

            //// Header into Hashtable
            Hashtable headerHash = new Hashtable();
            String key, entry;
            int index;
            // Assigning keys and entries to the header hashtable
            while ((request = getStringFromStream(stream)) != null)
            {
                if (request == "")
                    break;
                index = request.IndexOf(':');
                key = request.Substring(0, index++);
                // Getting position where hash entry starts
                while ((index < request.Length) && (request[index] == ' '))
                    index++;
                entry = request.Substring(index, request.Length - index);
                headerHash[key] = entry;
            }
            //// Header Hashtable completed

            // Decoding Base 64 Username and Password header hashtable
            byte[] convertedAuth = Convert.FromBase64String(headerHash["Authorization"].ToString().Split(' ')[1]);
            String username = Encoding.UTF8.GetString(convertedAuth).Split(':')[0];
            String password = Encoding.UTF8.GetString(convertedAuth).Split(':')[1];

            //// Sends the http verb to server
            server.sendVerb(verb);
            //// Sends the Basic Authentication username and password
            server.sendAuth(username, password);
            // Clears request input stream
            stream = null;
            // Closes client connection
            client.Close();
        }
    }
}
