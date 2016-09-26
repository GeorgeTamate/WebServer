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
        const int defaultPortNumber = 3000;
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
            object[] parametersArray = { ostream, path };
            method.Invoke(classInstance, parametersArray);

        }


        /// ------------------------------------------------------------------------------------------------------------

        public void processRequest(StreamWriter oStream)
        {
            //Getting request elements
            Request request = new Request(client);
            Response response = new Response(oStream);
            if (request.isFaviconRequest()) { return; }

            string url = request.getURL();
            string appName = url.Split('/')[1];
            string controllerName = url.Split('/')[2];
            string methodName = url.Split('/')[3];

            Console.WriteLine(request.getVerb());
            Console.WriteLine(controllerName);
            Console.WriteLine(methodName);

            string defpath = null;
            var assembly = Assembly.LoadFile($"{rootpath}{backslash}{appName}.dll");
            if (assembly == null)
            {
                defpath = Directory.GetCurrentDirectory();
                assembly = Assembly.LoadFile($"{defpath}{backslash}{appName}.dll");
                if (assembly == null)
                {
                    response.respondToClient(404, "Could not find your app.");
                    throw new Exception("Could not find your app.");
                }        
            }

            
            // Checking if is a valid httpVerb
            string[] validVerbs = { "GET", "POST", "PUT", "DELETE" };
            if (!validVerbs.Contains(request.getVerb())) throw new Exception("Invalid HTTP verb!");

            // Getting app interface type from Framework
            var appType = typeof(IApp);

            // Getting the App class type
            Type tApp = assembly.GetTypes().FirstOrDefault(
                t => t.FullName == $"{appName}.App" && 
                appType.IsAssignableFrom(t));
            if (tApp == null) throw new Exception("Could not find app class.");

            // Get method (the only method in App class)
            var appMethod = tApp.GetMethods().FirstOrDefault();
            if (appMethod == null) throw new Exception("Could not find method.");
            Console.WriteLine(appMethod);

            var controllerType = typeof(Controller);

            // Getting the Controller type
            Type tController = assembly.GetTypes().FirstOrDefault(
                t => t.FullName == $"{appName}.{controllerName}Controller" &&
                controllerType.IsAssignableFrom(t));
            if (tController == null) throw new Exception("Could not find method.");
            Console.WriteLine(tController);

            // Get HTTP verb type
            Type tVerb = assembly.GetType($"{appName}.Attributes.Attr{request.getVerb()}");
            Console.WriteLine(tVerb);

            // Getting Controller method
            var method = tController.GetMethods()
                .FirstOrDefault(m => m.GetCustomAttributes(tVerb, false).Length > 0 &&
                m.Name.Contains(methodName));
            if (method == null) throw new Exception("Could not find method.");
            Console.WriteLine(method);



            Console.WriteLine($"Invoking App getResponse Method...");

            object classInstance = Activator.CreateInstance(tApp, null);
            ParameterInfo[] parameters = appMethod.GetParameters();

            if (defpath == null) { defpath = rootpath; }
            object[] parametersArray = { defpath+backslash, request.getVerb(), controllerName, methodName };
            Response r = new Response(oStream);
            r.respondToClient((string)appMethod.Invoke(classInstance, parametersArray), oStream);







        }
    }
}
