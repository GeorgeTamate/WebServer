using System;
using System.IO;

namespace Framework
{
    public class Response
    {
        const string textMIME = "text/plain; charset=UTF-8";
        const string jpegMIME = "image/jpeg";
        const string htmlMIME = "text/html";

        public Response()
        { }

        public void respondToClient(StreamWriter oStream, string filepath)
        {
            if (File.Exists(filepath))
            {
                using (Stream filestream = File.Open(filepath, FileMode.Open))
                {
                    //response200(oStream, filepath.Substring(filepath.LastIndexOf('.')));
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
                response200(oStream, ".txt", msg);
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

        private void response200(StreamWriter oStream, string extension, string msg)
        {
            string mimetype;
            if (extension == ".txt") { mimetype = textMIME; }
            else if (extension == ".jpg") { mimetype = jpegMIME; }
            else if (extension == ".html" || extension == ".shtml") { mimetype = htmlMIME; }
            else { mimetype = textMIME; }

            oStream.Write("HTTP/1.0 200 OK");
            oStream.Write(Environment.NewLine);
            oStream.Write("Content-Type: " + mimetype);
            oStream.Write(Environment.NewLine);
            oStream.Write(Environment.NewLine);
            if (msg != null)
            {
                oStream.Write(msg);
            }
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
}
