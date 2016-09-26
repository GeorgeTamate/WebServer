using System;
using System.Threading;

namespace WebServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Initializing Server... ");

            string port = null;
            string path = null;
            if (args.Length > 0) { port = args[0]; }
            if (args.Length > 1) { path = args[1]; }

            Server server = new Server(port, path);

            Console.WriteLine("Starting Thread... ");

            Thread thread = new Thread(new ThreadStart(server.listen));
            thread.Start();
        }
    }
}
