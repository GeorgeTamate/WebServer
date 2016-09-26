using System;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections;
using Framework;
using System.Linq;
using System.Reflection;

namespace WebServer
{
    class ClientServerManager
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

            server.processRequest(oStream);
            client.Close();
            return;

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

            if (headerHash.ContainsKey("Authorization"))
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
            var typ = typeof(IApp);
            Console.WriteLine(typ.FullName);


            // Clears request input stream
            stream = null;
            oStream.Flush();

            client.Close();
        }

        



        
    }
}
