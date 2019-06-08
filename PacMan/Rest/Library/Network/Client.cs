using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using Chat = System.Net;

namespace PacMan
{
    public class Client
    {
        public bool Running { get; private set; }
        Chat.Sockets.TcpClient tcpClient;
        StreamReader reader;
        StreamWriter writer;
        Thread thread;
        protected bool printAtStart = true;

        public Client() { }

        public bool Connect(IPAddress ip, int port)
        {
            this.tcpClient = new Chat.Sockets.TcpClient();
            try {
                this.tcpClient.Connect(ip, port);
                this.reader = new StreamReader(tcpClient.GetStream());
                this.writer = new StreamWriter(tcpClient.GetStream());
                return true;
            }
            catch (Exception e) {
                Console.WriteLine("***ERROR CONNECTING: " + e.ToString() + "***");
                return false;
            }
        }

        public void Start()
        {
            if (this.printAtStart) {
                Console.Clear();
                this.printWelcome();
                this.printHelp();
            }
            this.Running = true;
            this.thread = new Thread(new ThreadStart(this.readServer));
            this.thread.IsBackground = true;
            this.thread.Start();
            this.run();
        }

        public void Abort()
        {
            this.Running = false;
            this.thread.Abort();
            this.tcpClient.Close();
            this.reader.Dispose();
            this.writer.Dispose();
        }

        void printWelcome()
        {
            Console.WriteLine();
            Console.WriteLine(" ---------------------------");
            Console.WriteLine("  Matty's chat server test:");
            Console.WriteLine(" ---------------------------");
            Console.WriteLine();
        }

        void printHelp()
        {
            Console.WriteLine("Type 'quit' (or 'q') to quit.");
            Console.WriteLine("Type 'clear' (or 'c') to clear the screen.");
            Console.WriteLine("Type 'refresh' (or 'r') to refresh.");
            Console.WriteLine("Type 'help' (or 'h') for help.");
            Console.WriteLine();
        }

        protected virtual void run()
        {
            string line = "";
            while (this.Running) {
                // Get the typed text
                line = Console.ReadLine();

                // The commands
                if (!this.manageCommands(line)) {
                    // If the line was no command, send it as a chatline
                    this.SendMessage(line);
                }
            }
        }

        void readServer()
        {
            string line;
            while (this.Running) {
                try {
                    line = this.reader.ReadLine();
                }
                catch {
                    this.onError("Connection to the server interrupted.");
                    this.Abort();
                    return;
                }
                this.onMessage(line);
            }
        }

        bool manageCommands(string line)
        {
            line = line.ToLower();
            if (line == "q" || line == "quit") {
                this.Running = false;
                this.Abort();
            }
            else if (line == "c" || line == "clear") {
                Console.Clear();
                this.printWelcome();
            }
            else if (line == "h" || line == "help") {
                this.printHelp();
            }
            else {
                return false;
            }
            return true;
        }

        public virtual void SendMessage(string msg)
        {
            try {
                this.writer.WriteLine(msg);
                this.writer.Flush();
            }
            catch {
                this.onError("Connection to the server interrupted.");
                this.Abort();
            }
        }

        protected virtual void onMessage(string msg)
        {
            Console.WriteLine(msg);
        }

        protected virtual void onError(string error)
        {
            Console.WriteLine(String.Format("*** Error: {0} ***", error));
        }
    }
}
