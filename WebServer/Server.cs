using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
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
        
        List<People> peopleDB;

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
            peopleDB = new List<People>();
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
            Response response = new Response();
            string path = rootpath + backslash; ;
            bool fileProtected = isProtected(filename);
            filename.TrimStart('/');

            if (File.Exists(path + filename))
            {
                if (fileProtected)
                {
                    if (username == null || password == null)
                    {
                        response.respondToClient(ostream, 401, "File is Protected. Username and Password must be provided.");
                        Console.WriteLine();
                        return;
                    }
                    //if (!username.Equals("agente") || !password.Equals("secreto"))
                    if (!username.Equals("12345"))
                    {
                        response.respondToClient(ostream, 401, "Wrong Username of Password.");
                        Console.WriteLine();
                        return;
                    }
                }
                response.respondToClient(ostream, path + filename);
            }
            else
            {
                response.respondToClient(ostream, 404, "File Not Found.");
            }
            Console.WriteLine();
        }

        /// ------------------------------------------------------------------------------------------------------------

        public void processRequest(StreamWriter oStream, string url, string verb)
        {
            Response response = new Response();
            string appName = null;
            string controllerName = null;
            string param = null;
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
                int qcount = controllerName.Count(q => q == '?');
                if (qcount > 0)
                {
                    param = controllerName.Split('?')[1];
                    controllerName = controllerName.Split('?')[0];
                }
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

            // Checking if app has contract with framework
            var appType = typeof(IApp);
            Type tApp = assembly.GetTypes().FirstOrDefault(t => appType.IsAssignableFrom(t));
            if (tApp == null)
            {
                response.respondToClient(oStream, 404, "Your app is not compatible with Server Framework.");
                return;
            }

            // Getting the Controller type
            var controllerType = typeof(Controller);
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
            object[] parametersArray = { usingpath + backslash };
            string filepath = (string)method.Invoke(classInstance, parametersArray);

            if (param == null || verb == "GET")
            {
                Console.WriteLine($"Returning file: {filepath}");
                response.respondToClient(oStream, filepath);
            }
            else
            {
                string param1 = param.Split('&')[0];
                string param2 = param.Split('&')[1];
                string param3 = param.Split('&')[2];
                string firstname = param1.Split('=')[1];
                string lastname = param2.Split('=')[1];
                string age = param3.Split('=')[1];

                if (verb == "POST" || verb == "PUT")
                {
                    response.respondToClient(oStream, 200, pPeople(firstname, lastname, age));
                }
                else if (verb == "DELETE")
                {
                    int dCode = getDeleteCode(firstname, lastname);
                    response.respondToClient(oStream, dCode, dPeople(dCode, firstname, lastname));
                }
            }

        }

        public string pPeople(string firstname, string lastname, string age)
        {
            peopleDB.Add(new People(firstname, lastname, age));
            string result = "";
            foreach (People p in peopleDB)
            {
                result += p.firstname + " " + p.lastname + " (" + p.age + ")\r\n";
            }
            return result;
        }

        public string dPeople(int code, string firstname, string lastname)
        {
            string uuid = "UNKNOWN CODE ERROR.";
            if (code == 200)
            {
                People person = peopleDB.FirstOrDefault(p => p.firstname.Equals(firstname) && p.lastname.Equals(lastname));
                uuid = "Deleted Registry UUID: " + person.id;
                peopleDB.Remove(person);
            }
            else if (code == 404)
            {
                uuid = "No registry found with provided Name and LastName.";
            }
            return uuid;
        }

        public int getDeleteCode(string firstname, string lastname)
        {
            if (peopleDB.Exists(p => p.firstname.Equals(firstname) && p.lastname.Equals(lastname)))
            {
                return 200;
            }
            return 404;
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
