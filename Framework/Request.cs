using System;
using System.Text;
using System.Net.Sockets;
using System.Collections;
using System.IO;
using System.Threading;

namespace Framework
{
    public class Request
    {
        TcpClient client;
        Hashtable header;
        string verb;
        string url;
        string username;
        string password;

        public Request()
        { }

        public Request(TcpClient client)
        {
            initWithTcpClient(client);
        }

        public void initWithTcpClient(TcpClient client)
        {
            this.client = client;
            using (Stream stream = new BufferedStream(client.GetStream()))
            {
                setVerbAndURL(stream);
                setHeader(stream);
            }
            setCredentials();
        }

        public void setVerbAndURL(Stream stream)
        {
            String request = getStringFromStream(stream);
            verb = request.Split(' ')[0];
            url = request.Split(' ')[1];
        }

        public void setHeader(Stream stream)
        {
            //// Header into Hashtable
            header = new Hashtable();
            String key, entry, request;
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
                header[key] = entry;
            }
            //// Header Hashtable completed
        }

        public void setCredentials()
        {
            username = null;
            password = null;

            if (header.ContainsKey("Authorization"))
            {
                // Decoding Base 64 Username and Password header hashtable
                byte[] convertedAuth = Convert.FromBase64String(header["Authorization"].ToString().Split(' ')[1]);
                username = Encoding.UTF8.GetString(convertedAuth).Split(':')[0];
                password = Encoding.UTF8.GetString(convertedAuth).Split(':')[1];
            }
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


        public bool isFaviconRequest()
        {
            if (url.Equals("/favicon.ico"))
            {
                return true;
            }
            return false;
        }


        public TcpClient getClient()
        { return client; }

        public string getURL()
        { return url; }

        public string getVerb()
        { return verb; }

        public string getUsername()
        { return username; }
        
        public string getPassword()
        { return password; }


        public void closeClient()
        { client.Close(); }
    }
}
