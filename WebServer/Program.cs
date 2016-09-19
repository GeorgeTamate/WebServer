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

using ServerController;

namespace WebServer
{
    
    // MAIN CLASS
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

    
    
    
    
    
    
    
    // SERVER CLASS
    public class Server
    { 
        public Server()
        {}

        public void listen()
        {
            TcpListener server = new TcpListener(IPAddress.Loopback, 3000); // Port number in localhost
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
                Thread.Sleep(100);
            }
        }
        
        // Receiving the verb to print on console
        public void sendVerb(string verb, string url)
        {
            Console.WriteLine("HttpVerb: " + verb);
            Console.WriteLine("URL: http://localhost:3000" + url);
        }

        // Receiving the username and password to print on console
        public void sendAuth(string username, string password)
        {
            if(username != null && password != null)
            {
                Console.WriteLine("Username: " + username);
                Console.WriteLine("Password: " + password);
            }
        }

        public bool isProtected(string filename)
        {
            if (filename.Equals("/secret.txt"))
                return true;
            return false;
        }

        public void outputFileToClient(StreamWriter ostream, string filename, string username, string password)
        {
            string path = @"c:\Users\GeorgeTamate\Desktop\WebServer\content\";
            bool fileProtected = isProtected(filename);
            filename.TrimStart('/');

            if (File.Exists(path + filename))
            {
                if (fileProtected)
                {
                    if (username == null || password == null)
                    {
                        ostream.Write("HTTP/1.0 401 Unauthorized");
                        ostream.Write(Environment.NewLine);
                        ostream.Write(Environment.NewLine);

                        ostream.Write("ERROR 401: El archivo esta protegido. Debe proveer nombre de usuario y clave.");
                        Console.WriteLine();
                        return;
                    }
                    if (!username.Equals("agente") || !password.Equals("secreto"))
                    {
                        ostream.Write("HTTP/1.0 401 Unauthorized");
                        ostream.Write(Environment.NewLine);
                        ostream.Write(Environment.NewLine);

                        ostream.Write("ERROR 401: Nombre de usuario incorrecto o clave incorrecta.");
                        Console.WriteLine();
                        return;
                    }
                }
                
                using (Stream filestream = File.Open(path + filename, FileMode.Open))
                    filestream.CopyTo(ostream.BaseStream);
            }
            else
            {
                ostream.Write("HTTP/1.0 404 Not Found");
                ostream.Write(Environment.NewLine);
                ostream.Write(Environment.NewLine);

                ostream.Write("ERROR 404: El archivo solicitado NO existe.");
            }
            Console.WriteLine();

            //image/jpeg
            //text/plain; charset=UTF-8
            //text/html

            /*
            ostream.Write("HTTP/1.0 200 OK");
            ostream.Write(Environment.NewLine);
            ostream.Write("Content-Type: text/plain; charset=UTF-8");
            ostream.Write(Environment.NewLine);
            ostream.Write("Content-Length: " + filestream.Length);
            ostream.Write(Environment.NewLine);
            */
        }

        public void peopleReflection(StreamWriter ostream, string verb)
        {
            TestController tc = new TestController();

            /*
            System.Type testType = typeof(TestController.people());

            // Using reflection.
            System.Attribute[] attrs = System.Attribute.GetCustomAttributes(tc);  // Reflection.

            foreach (System.Attribute attr in attrs)
            {
                
            }
             */
            



        }
    }

    
    
    
    
    
    
    
    
    
    // SERVER MANAGER CLASS
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
            StreamWriter oStream = new StreamWriter(client.GetStream());

            // Parses http request stream to a string
            String request = getStringFromStream(stream);            
            String verb = request.Split(' ')[0];
            String url = request.Split(' ')[1];

            if (url.Equals("/favicon.ico"))
            {
                stream.Flush();
                oStream.Flush();
                return;
            }

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

            String username = null;
            String password = null;

            if(headerHash.ContainsKey("Authorization"))
            {
                byte[] convertedAuth = Convert.FromBase64String(headerHash["Authorization"].ToString().Split(' ')[1]);
                username = Encoding.UTF8.GetString(convertedAuth).Split(':')[0];
                password = Encoding.UTF8.GetString(convertedAuth).Split(':')[1];
            }

            if (url.Equals("people"))
            {

            }
            else 
            {
                //// Sends the http verb to server
                server.sendVerb(verb, url);
                //// Sends the Basic Authentication username and password
                server.sendAuth(username, password);
                //// Return a file to the client based on the URL
                server.outputFileToClient(oStream, url, username, password);
            }
            

            // Clears request input stream
            stream = null;
            oStream.Flush();

            client.Close();
        }
    }
}
