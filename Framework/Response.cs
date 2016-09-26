using System;
using System.IO;
using System.Net.Sockets;

namespace Framework
{
    public class Response
    {
        TcpClient client;
        StreamWriter oStream;

        public Response()
        { }

        public Response(StreamWriter oStream)
        { this.oStream = oStream; }

        public Response(TcpClient client)
        {
            this.client = client;
            oStream = new StreamWriter(this.client.GetStream());
        }

        public void initWithTcpClient(TcpClient client)
        { oStream = new StreamWriter(client.GetStream()); }

        public void respondToClient(Stream filestream)
        {
            if (filestream != null)
                filestream.CopyTo(oStream.BaseStream);
            else
                response404("File not found.");
        }

        public void respondToClient(string filepath, StreamWriter ostream)
        {
            using (Stream filestream = File.Open(filepath, FileMode.Open))
                filestream.CopyTo(ostream.BaseStream);
        }

        public void respondToClient(Stream filestream, StreamWriter ostream)
        {
            if (filestream != null)
                filestream.CopyTo(ostream.BaseStream);
            else
                response404("File not found.");
        }

        public void respondToClient(int code, string msg)
        {
            if (code == 404)
                response404(msg);
            else if (code == 401)
                response401(msg);
            else
                response200();
        }

        private void response404(string msg)
        {
            oStream.Write("HTTP/1.0 404 Not Found");
            oStream.Write(Environment.NewLine);
            oStream.Write(Environment.NewLine);
            if(msg != null)
            {
                oStream.Write($"ERROR 404: {msg}");
            }
        }

        private void response401(string msg)
        {
            oStream.Write("HTTP/1.0 401 Unauthorized");
            oStream.Write(Environment.NewLine);
            oStream.Write(Environment.NewLine);
            if (msg != null)
            {
                oStream.Write($"ERROR 401: {msg}");
            }
        }

        private void response200()
        {
            oStream.Write("HTTP/1.0 200 OK");
            oStream.Write(Environment.NewLine);
            oStream.Write(Environment.NewLine);
        }
    }
}
