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
using System.Reflection;
using System.Globalization;

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
            string path = Directory.GetCurrentDirectory() + @"\";
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

        public void peopleReflection(StreamWriter ostream, string verb, string url)
        {
            string path = Directory.GetCurrentDirectory() + @"\";

            // Loading .dll file
            var assembly = Assembly.LoadFile(path + @"ServerController.dll");
            if (assembly == null) throw new Exception("No se pudo encontrar la ServerController.dll en el directorio.");

            // Getting Type of Controller Interface
            var controllerType = assembly.GetType("ServerController.IController");
            if (controllerType == null) throw new Exception("No se pudo encontrar una interface.");
            
            // Name of controller implemantation
            string controllerNameType = "TestController";

            // Getting Verb Type
            Type attrType = assembly.GetType("ServerController.Attr" + verb);

            // Getting Controller Type by Verifying if compatible with Interface
            var controller = assembly.GetTypes().FirstOrDefault(
                t => controllerType.IsAssignableFrom(t) &&
                t.Name == controllerNameType);
            if (controller == null) throw new Exception("No se pudo encontrar el Controller.");

            // Getting Method by Custom Attribute Type
            var method = controller.GetMethods().FirstOrDefault(
                m => m.GetCustomAttributes(attrType, false).Length > 0);
            if (method == null) throw new Exception("No se pudo encontrar el metodo.");

            Console.WriteLine("HttpVerb: " + verb);
            Console.WriteLine("URL: http://localhost:3000" + url);
            Console.WriteLine("Metodo invocado: {0}.{1}", controller.FullName, method.Name);
            Console.WriteLine();

            // Invoking Controller Method
            object classInstance = Activator.CreateInstance(controller, null);
            object[] parametersArray = {ostream, path};
            method.Invoke(classInstance, parametersArray);

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

            
            String username = null;
            String password = null;

            if(headerHash.ContainsKey("Authorization"))
            {
                // Decoding Base 64 Username and Password header hashtable
                byte[] convertedAuth = Convert.FromBase64String(headerHash["Authorization"].ToString().Split(' ')[1]);
                username = Encoding.UTF8.GetString(convertedAuth).Split(':')[0];
                password = Encoding.UTF8.GetString(convertedAuth).Split(':')[1];
            }

            if (url.Equals("/people"))
            {
                server.peopleReflection(oStream, verb, url);
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
