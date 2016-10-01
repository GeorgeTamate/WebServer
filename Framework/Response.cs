using System;
using System.IO;

namespace Framework
{
    public class Response
    {
        public Response()
        { }

        public void respondToClient(StreamWriter oStream, string filepath)
        {
            if (File.Exists(filepath))
            {
                using (Stream filestream = File.Open(filepath, FileMode.Open))
                {
                    transferFile(filestream, oStream.BaseStream);
                }
            }
            else { response404(oStream, "File not found."); }
        }

        public void respondToClient(StreamWriter oStream, int code, string msg)
        {
            if (code == 404)
                response404(oStream, msg);
            else if (code == 401)
                response401(oStream, msg);
            else
                response200(oStream);
        }

        private void response404(StreamWriter oStream, string msg)
        {
            oStream.Write("HTTP/1.0 404 Not Found");
            oStream.Write(Environment.NewLine);
            oStream.Write(Environment.NewLine);
            if(msg != null)
            {
                oStream.Write($"ERROR 404: {msg}");
            }
        }

        private void response401(StreamWriter oStream, string msg)
        {
            oStream.Write("HTTP/1.0 401 Unauthorized");
            oStream.Write(Environment.NewLine);
            oStream.Write(Environment.NewLine);
            if (msg != null)
            {
                oStream.Write($"ERROR 401: {msg}");
            }
        }

        private void response200(StreamWriter oStream)
        {
            oStream.Write("HTTP/1.0 200 OK");
            oStream.Write(Environment.NewLine);
            oStream.Write(Environment.NewLine);
        }

        public static long transferFile(Stream source, Stream target)
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
