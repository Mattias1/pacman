using System;
using System.Threading;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class AIServer : AIPlayer
    {
        PacManServer Server;
        MultiPlayerInfo multiplayerInfo;
        volatile bool locked;
        string[] gameDatas;

        public AIServer(PacManServer server, MultiPlayerInfo mpInfo)
        {
            this.Server = server;
            this.Server.SetAI(this);
            this.multiplayerInfo = mpInfo;
            this.locked = false;
            this.gameDatas = new string[Level.MAXPLAYERCOUNT];
        }

        public override void UpdateBeforeMove(UpdateEventArgs e)
        {
            this.locked = true;

            // Update the level's information
            this.Level.UpdateSimAIs(this.gameDatas);

            this.gameDatas = new string[Level.MAXPLAYERCOUNT];
            this.locked = false;
        }

        public override void UpdateAfterMove(UpdateEventArgs e, Vector2 direction)
        {
            this.Server.BroadCast("#", this.Level.ToGameData());
        }

        public void MessageReceived(string nickname, string msg)
        {
            while (this.locked)
                Thread.Sleep(1);
            this.locked = true;
            foreach (UserInfo user in this.multiplayerInfo.Users)
                if (user.Name == nickname) {
                    this.gameDatas[user.Id] = msg;
                    break;
                }
            this.locked = false;
        }
    }

    public class PacManServer : Server
    {
        public MultiPlayerInfo MultiPlayerInfo { get; private set; }
        AIServer ai;

        public PacManServer(MultiPlayerInfo mpInfo)
            : base(Settings.Get.IP, Settings.Get.Port)
        {
            this.MultiPlayerInfo = mpInfo;
        }

        public bool SetAI(AIServer ai)
        {
            if (this.GameStarted())
                return false;
            this.ai = ai;
            this.StopAcceptingClients();
            return true;
        }

        public bool GameStarted()
        {
            return this.ai != null;
        }

        public override void BroadCast(string nick, string msg)
        {
            base.BroadCast(nick, msg);
        }

        public override void OnReceiveMessage(string nickName, string msg)
        {
            if (msg == null)
                return;

            // Handle unespected messages
            if (msg.Length < 1 || msg[0] != '#') {
                base.OnReceiveMessage(nickName, msg);
                return;
            }

            // The game is started, so delegate to the AI
            if (this.GameStarted()) {
                this.ai.MessageReceived(nickName, msg.Substring(1));
                return;
            }

            // Manage lobby stuff
            int indexOf = msg.IndexOf('#', 1);
            if (indexOf == -1)
                throw new Exception("Error: Message starts with a hash char (#), but is not an official message.");
            string id = msg.Substring(1, indexOf - 1);
            string rest = msg.Substring(indexOf + 1);
            this.MultiPlayerInfo.Command(nickName, id, rest);
        }

        public override void OnClientAdded(string nickName)
        {
            this.MultiPlayerInfo.AddUser(nickName);
            base.OnClientAdded(nickName);
        }

        public override void OnClientRemoved(string nickName)
        {
            if (this.GameStarted()) {
                #region Yeah, right...

                Thread beep = new Thread(new ThreadStart(() => {
                    Console.Beep(658, 125); Console.Beep(1320, 500); Console.Beep(990, 250); Console.Beep(1056, 250);
                    Console.Beep(1188, 250); Console.Beep(1320, 125); Console.Beep(1188, 125); Console.Beep(1056, 250);
                    Console.Beep(990, 250); Console.Beep(880, 500); Console.Beep(880, 250); Console.Beep(1056, 250);
                    Console.Beep(1320, 500); Console.Beep(1188, 250); Console.Beep(1056, 250); Console.Beep(990, 750);
                    Console.Beep(1056, 250); Console.Beep(1188, 500); Console.Beep(1320, 500); Console.Beep(1056, 500);
                    Console.Beep(880, 500); Console.Beep(880, 500); Thread.Sleep(250);
                    Console.Beep(1188, 500); Console.Beep(1408, 250); Console.Beep(1760, 500); Console.Beep(1584, 250);
                    Console.Beep(1408, 250); Console.Beep(1320, 750); Console.Beep(1056, 250); Console.Beep(1320, 500);
                    Console.Beep(1188, 250); Console.Beep(1056, 250); Console.Beep(990, 500); Console.Beep(990, 250);
                    Console.Beep(1056, 250); Console.Beep(1188, 500); Console.Beep(1320, 500); Console.Beep(1056, 500);
                    Console.Beep(880, 500); Console.Beep(880, 500); Thread.Sleep(500);
                    Console.Beep(1320, 500); Console.Beep(990, 250); Console.Beep(1056, 250); Console.Beep(1188, 250);
                    Console.Beep(1320, 125); Console.Beep(1188, 125); Console.Beep(1056, 250); Console.Beep(990, 250);
                    Console.Beep(880, 500); Console.Beep(880, 250); Console.Beep(1056, 250); Console.Beep(1320, 500);
                    Console.Beep(1188, 250); Console.Beep(1056, 250); Console.Beep(990, 750); Console.Beep(1056, 250);
                    Console.Beep(1188, 500); Console.Beep(1320, 500); Console.Beep(1056, 500); Console.Beep(880, 500);
                    Console.Beep(880, 500); Thread.Sleep(250);
                    Console.Beep(1188, 500); Console.Beep(1408, 250); Console.Beep(1760, 500); Console.Beep(1584, 250);
                    Console.Beep(1408, 250); Console.Beep(1320, 750); Console.Beep(1056, 250); Console.Beep(1320, 500);
                    Console.Beep(1188, 250); Console.Beep(1056, 250); Console.Beep(990, 500); Console.Beep(990, 250);
                    Console.Beep(1056, 250); Console.Beep(1188, 500); Console.Beep(1320, 500); Console.Beep(1056, 500);
                    Console.Beep(880, 500); Console.Beep(880, 500); Thread.Sleep(500);
                    Console.Beep(660, 1000); Console.Beep(528, 1000); Console.Beep(594, 1000); Console.Beep(495, 1000);
                    Console.Beep(528, 1000); Console.Beep(440, 1000); Console.Beep(419, 1000); Console.Beep(495, 1000);
                    Console.Beep(660, 1000); Console.Beep(528, 1000); Console.Beep(594, 1000); Console.Beep(495, 1000);
                    Console.Beep(528, 500); Console.Beep(660, 500); Console.Beep(880, 1000); Console.Beep(838, 2000);
                    Console.Beep(660, 1000); Console.Beep(528, 1000); Console.Beep(594, 1000); Console.Beep(495, 1000);
                    Console.Beep(528, 1000); Console.Beep(440, 1000); Console.Beep(419, 1000); Console.Beep(495, 1000);
                    Console.Beep(660, 1000); Console.Beep(528, 1000); Console.Beep(594, 1000); Console.Beep(495, 1000);
                    Console.Beep(528, 500); Console.Beep(660, 500); Console.Beep(880, 1000); Console.Beep(838, 2000);
                }));
                beep.IsBackground = true;
                beep.Start();

                #endregion
            }
            this.MultiPlayerInfo.RemoveUser(nickName);
            base.OnClientRemoved(nickName);
        }

        public override bool NickNameInvalid(string nickname)
        {
            // Do the base checks
            if (base.NickNameInvalid(nickname))
                return true;

            // Check if the nickname doesn't contain any of the following
            string[] invalids = new string[] { "|", "*", "#", "\n", ":" };
            foreach (string s in invalids)
                if (nickname.Contains(s))
                    return true;

            // It's ok, it is not invalid
            return false;
        }
    }
}
