using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;
using System.Reflection;
using Framework;

namespace WebServer
{
    class Server
    {
        int port;
        string rootpath;
        const int defaultPortNumber = 8081;
        const string backslash = @"\";
        TcpListener server;
        TcpClient client;

        public Server()
        {
            port = defaultPortNumber;
            rootpath = Directory.GetCurrentDirectory();
            notifyInitReport();
        }

        public Server(int port, string path)
        {
            if (isPortAvailable(port))
            {
                this.port = port;
            }
            else
            {
                Console.WriteLine("ERROR: Port Not Available.");
                Console.WriteLine($"Default Port Number {defaultPortNumber} will be used instead.");
                Console.WriteLine();
                this.port = defaultPortNumber;
            }
            assignRootPath(path);
            notifyInitReport();
        }

        public Server(string port, string path)
        {
            int portNum;
            if (int.TryParse(port, out portNum))
            {
                if (isPortAvailable(portNum))
                {
                    this.port = portNum;
                }
                else
                {
                    Console.WriteLine("ERROR: Port Not Available.");
                    Console.WriteLine($"Default Port Number {defaultPortNumber} will be used instead.");
                    Console.WriteLine();
                    this.port = defaultPortNumber;
                }
            }
            else
            {
                if (port != null)
                {
                    Console.WriteLine($"ERROR: Port parameter provided is not a number.");
                    Console.WriteLine($"Default Port Number {defaultPortNumber} will be used instead.");
                    Console.WriteLine();
                }   
                this.port = defaultPortNumber;
            }
            assignRootPath(path);
            notifyInitReport();
        }

        private bool isPortAvailable(int port)
        {
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    return false;
                }
            }
            return true;
        }

        private void assignRootPath(string path)
        {
            if (path != null)
            {
                rootpath = path;
            }
            else
            {  
                rootpath = Directory.GetCurrentDirectory();
            }
        }

        public void notifyInitReport()
        {
            Console.WriteLine("Server initialization Complete!");
            Console.WriteLine($"Using PORT : {port}");
            Console.WriteLine($"Using PATH : {rootpath}");
            Console.WriteLine();
        }

        public void listen()
        {
            server = new TcpListener(IPAddress.Loopback, port); // Port number in localhost
            server.Start();
            Console.WriteLine($"Listening on port {port}...");
            Console.WriteLine();

            // Loop for listening clients

            while (true)
            {
                // Accepting connection with new client
                client = server.AcceptTcpClient();
                ClientServerManager manager = new ClientServerManager(this, client);
                // Handling request from client in a separate thread
                Thread thread = new Thread(new ThreadStart(manager.getRequest));
                thread.Start();
                Thread.Sleep(100);
            }
        }

        /// ------------------------------------------------------------------------------------------------------------

        // Receiving the verb to print on console
        public void sendVerb(string verb, string url)
        {
            Console.WriteLine("HttpVerb: " + verb);
            Console.WriteLine("URL: http://localhost:" + this.port + url);
        }

        // Receiving the username and password to print on console
        public void sendAuth(string username, string password)
        {
            if (username != null && password != null)
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
            string path = rootpath + backslash; ;
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
                    //if (!username.Equals("agente") || !password.Equals("secreto"))
                    if (!username.Equals("12345"))
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
                {
                    CopyStream(filestream, ostream.BaseStream);    
                }

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

        /// ------------------------------------------------------------------------------------------------------------

        public void processRequest(StreamWriter oStream, string url, string verb)
        {
            Response response = new Response();
            string appName = null;
            string controllerName = null;
            string usingpath = null;

            Console.WriteLine();

            // Parsing url
            if (url.Split('/')[1] != null && url.Split('/')[1] != "")
            {
                appName = url.Split('/')[1];
            }
            if (url.Split('/')[2] != null && url.Split('/')[2] != "")
            {
                controllerName = url.Split('/')[2];
            }

            // Checking if provided app exists
            if (!File.Exists($"{rootpath}{backslash}{appName}.dll"))
            {
                usingpath = Directory.GetCurrentDirectory();
                Console.WriteLine("App not found in custom directory. Using default directory instead.");
                if (!File.Exists($"{usingpath}{backslash}{appName}.dll"))
                {
                    response.respondToClient(oStream, 404, "Could not find your app.");
                    return;
                }
            }

            // Loading application file
            if (usingpath == null) { usingpath = rootpath; }
            var assembly = Assembly.LoadFile($"{usingpath}{backslash}{appName}.dll");
            if (assembly == null)
            {
                response.respondToClient(oStream, 404, "Could not find your app.");
                return;       
            }
            
            // Checking if is a valid httpVerb
            string[] validVerbs = { "GET", "POST", "PUT", "DELETE" };
            if (!validVerbs.Contains(verb))
            {
                response.respondToClient(oStream, 404, "Invalid HTTP verb.");
                return;
            }
            
            var controllerType = typeof(Controller);

            // Getting the Controller type
            Type tController = assembly.GetTypes().FirstOrDefault(
                t => t.FullName == $"{appName}.{controllerName}Controller" &&
                controllerType.IsAssignableFrom(t));
            if (tController == null)
            {
                response.respondToClient(oStream, 404, "Could not find your Controller.");
                return;
            }
            Console.WriteLine($"Your Controller Type: {tController}");

            // Get HTTP verb type
            Type tVerb = assembly.GetType($"{appName}.Attributes.Attr{verb}");
            Console.WriteLine($"Your Verb Type: {tVerb}");

            // Getting Controller method by verb attribute
            var method = tController.GetMethods()
                .FirstOrDefault(m => m.GetCustomAttributes(tVerb, false).Length > 0);
            if (method == null)
            {
                response.respondToClient(oStream, 404, "Could not find a method for your controller and verb.");
                return;
            }
            Console.WriteLine($"Your Method: {method}");


            Console.WriteLine($"Invoking Method...");

            // Creating an instance for the controller
            object classInstance = Activator.CreateInstance(tController, null);
            ParameterInfo[] parameters = method.GetParameters();
            object[] parametersArray = { usingpath+backslash };
            string filepath = (string)method.Invoke(classInstance, parametersArray);

            Console.WriteLine($"Returning file: {filepath}");
            response.respondToClient(oStream, filepath);
        }

        public long CopyStream(Stream source, Stream target)
        {
            const int bufSize = 0x1000;
            byte[] buf = new byte[bufSize];

            long totalBytes = 0;
            int bytesRead = 0;

            while ((bytesRead = source.Read(buf, 0, bufSize)) > 0)
            {
                target.Write(buf, 0, bytesRead);
                totalBytes += bytesRead;
            }
            return totalBytes;
        }
    }
}
