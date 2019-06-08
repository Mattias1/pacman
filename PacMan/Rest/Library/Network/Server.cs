using System;
using System.IO;
using System.Net;
using System.Threading;
using Chat = System.Net;
using System.Collections.Generic;

namespace PacMan
{
    public class Server
    {
        public static string Nickname = "Choose a nickname:";
        public static string NicknameRetry = "This nickmane already exists, please try a new one:";

        System.Net.Sockets.TcpListener tcpListener;
        public Dictionary<string, ClientManager> NickNames;
        public Dictionary<System.Net.Sockets.TcpClient, ClientManager> NickNameByConnect;
        public string Log { get; private set; }
        private Thread thread;
        private volatile bool running;

        public Server(IPAddress ip, int port)
        {
            this.NickNames = new Dictionary<string, ClientManager>(Level.MAXPLAYERCOUNT);
            this.NickNameByConnect = new Dictionary<Chat.Sockets.TcpClient, ClientManager>(Level.MAXPLAYERCOUNT);
            this.tcpListener = new System.Net.Sockets.TcpListener(ip, port);
            this.Log = "";
        }

        public void Start()
        {
            this.running = true;
            this.thread = new Thread(new ThreadStart(this.run));
            this.thread.IsBackground = true;
            this.thread.Start();
        }

        public void Abort()
        {
            this.running = false;
            this.SystemBroadCast("The server is aborted.");
            foreach (ClientManager client in this.NickNames.Values)
                client.Abort();
            this.thread.Abort();
            this.tcpListener.Stop();
        }

        public void StopAcceptingClients()
        {
            this.thread.Abort();
        }

        void run()
        {
            while (this.running) {
                // Accept clients
                tcpListener.Start();                     // TODO: Why is this inside the loop?
                if (tcpListener.Pending()) {
                    Chat.Sockets.TcpClient chatConnection = tcpListener.AcceptTcpClient();
                    ClientManager clientManager = new ClientManager(this, chatConnection);
                    clientManager.Start();
                }
            }
        }

        public virtual void BroadCast(string nick, string msg)
        {
            StreamWriter writer;
            Chat.Sockets.TcpClient[] tcpClient = new Chat.Sockets.TcpClient[this.NickNames.Count];
            int i = 0;
            foreach (ClientManager client in this.NickNames.Values)
                tcpClient[i++] = client.TcpClient;

            for (i = 0; i < tcpClient.Length; i++) {
                try {
                    // Loop through and write any messages to the window
                    if (msg.Trim() == "" || tcpClient[i] == null)
                        continue;

                    // Write the message
                    writer = new StreamWriter(tcpClient[i].GetStream());
                    writer.WriteLine(nick + ": " + msg);
                    writer.Flush();

                    // Dispose of the writer object until it's needed again
                    writer = null;
                }
                catch {
                    // If something goes wrong here, the client is considered disconnected
                    string str = this.NickNameByConnect[tcpClient[i]].NickName;
                    if (str == null)
                        continue;
                    this.NickNames.Remove(str);
                    this.NickNameByConnect.Remove(tcpClient[i]);
                    foreach (ClientManager client in this.NickNames.Values)
                        if (client.NickName == str) {
                            client.Abort();
                            break;
                        }
                    this.OnClientRemoved(str);
                }
            }
        }

        public virtual void OnReceiveMessage(string nickName, string msg)
        {
            this.BroadCast(nickName, msg);
        }

        public virtual void OnClientAdded(string nickName)
        {
            this.SystemBroadCast(nickName + " has joined the room.");
        }

        public virtual void OnClientRemoved(string nickName)
        {
            this.SystemBroadCast(nickName + " has Left The Room.");
        }

        public virtual void SystemBroadCast(string msg)
        {
            this.BroadCast("*** Server", msg + " ***");
        }

        public virtual bool NickNameInvalid(string nickname)
        {
            if (nickname.ToLower() == "server")
                return true;
            return false;
        }

        public class ClientManager
        {
            public Server Server { get; private set; }
            public string NickName { get; private set; }
            public Chat.Sockets.TcpClient TcpClient { get; private set; }
            StreamReader reader;
            StreamWriter writer;
            Thread thread;
            volatile bool running;

            public ClientManager(Server server, Chat.Sockets.TcpClient client)
            {
                this.Server = server;
                this.TcpClient = client;
                this.reader = new StreamReader(client.GetStream());
                this.writer = new StreamWriter(client.GetStream());
                this.NickName = "Server";
            }

            public void Start()
            {
                this.thread = new Thread(new ThreadStart(() => {
                    if (!this.getNickname()) {
                        this.TcpClient.Close();
                        this.reader.Dispose();
                        this.writer.Dispose();
                    }
                    else {
                        this.running = true;
                        this.run();
                    }
                }));
                this.thread.IsBackground = true;
                this.thread.Start();
            }

            public void Abort()
            {
                this.running = false;
                this.TcpClient.Close();
                if (this.thread != null)
                    this.thread.Abort();
                this.reader.Dispose();
                this.writer.Dispose();
            }

            private bool getNickname()
            {
                // Ask for a nickname (that doesn't exist yet)
                int retry = 0;
                while (this.Server.NickNameInvalid(this.NickName) || this.Server.NickNames.ContainsKey(NickName)) {
                    if (retry++ > 0)
                        this.writer.WriteLine(Server.NicknameRetry);
                    else
                        this.writer.WriteLine(Server.Nickname);
                    this.writer.Flush();
                    try {
                        this.NickName = this.reader.ReadLine();
                    }
                    catch {
                        return false;
                    }
                    if (this.NickName == null || retry > 5)
                        return false;

                    // Nescessary to auto kick users that left and want to rejoin
                    if (this.Server.NickNames.ContainsKey(this.NickName))
                        this.Server.SystemBroadCast("Duplicate nickname: " + this.NickName);
                }

                // Update the server hash tables
                this.Server.NickNames.Add(this.NickName, this);
                this.Server.NickNameByConnect.Add(this.TcpClient, this);

                // Celebrate the new user
                this.Server.OnClientAdded(this.NickName);
                return true;
            }

            private void run()
            {
                string line;
                while (this.running) {
                    try {
                        line = this.reader.ReadLine();
                    }
                    catch {
                        // I'm not sure what to do here, all kinds of errors happen when someone leaves.
                        // It'll be caught at the next broadcast, so its ok.
                        this.Server.BroadCast(this.NickName, String.Format("{0} has connection problems.", this.NickName));
                        continue;
                    }
                    this.Server.OnReceiveMessage(this.NickName, line);
                }
            }
        }
    }
}
