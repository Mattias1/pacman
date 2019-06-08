using System;
using System.Threading;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class AIClient : AIPlayer
    {
        PacManClient client;
        MultiPlayerInfo multiplayerInfo;
        volatile bool locked;
        string gameData;
        Queue<string> gameCommands;

        public AIClient(PacManClient client, MultiPlayerInfo mpInfo)
        {
            this.client = client;
            this.client.SetAI(this);
            this.multiplayerInfo = mpInfo;
            this.locked = false;
            this.gameData = "";
            this.gameCommands = new Queue<string>();
        }

        public override void UpdateBeforeMove(UpdateEventArgs e)
        {
            this.locked = true;

            // Execute all commands
            while (this.gameCommands.Count > 0) {
                string msgcmd = this.gameCommands.Dequeue();

                // Swap two players
                if (PacManClient.IsMsgCmd(msgcmd, "swap")) {
                    string[] data = PacManClient.TrimMsg(msgcmd).Split(new char[] { ',' });
                    this.multiplayerInfo.DoSwapUsers(int.Parse(data[0]), int.Parse(data[1]), this.Level.Ais);
                    this.Level.ResetPositions();
                }
            }

            // Update the level's information
            if (this.gameData != "") {
                this.Level.FromGameData(PacManClient.TrimMsg(this.gameData));
                this.gameData = "";
            }

            this.locked = false;
        }

        public override void UpdateAfterMove(UpdateEventArgs e, Vector2 direction)
        {
            this.client.SendMessage("#" + this.Puppet.UnitVecToString(direction).ToString());
        }

        public void OnMessageReceived(string msg)
        {
            while (this.locked)
                Thread.Sleep(1);
            this.locked = true;
            if (PacManClient.IsMsgCmd(msg))
                this.gameData = msg;
            else
                this.gameCommands.Enqueue(msg);
            this.locked = false;
        }
    }

    public class PacManClient : Client
    {
        public MultiPlayerInfo MultiPlayerInfo { get; private set; }
        AIClient ai;

        public PacManClient(MultiPlayerInfo mpInfo)
            : base()
        {
            this.MultiPlayerInfo = mpInfo;
            this.printAtStart = false;
        }

        public void SetAI(AIClient ai)
        {
            this.ai = ai;
        }

        public bool GameStarted()
        {
            return this.ai != null;
        }

        protected override void onMessage(string msg)
        {
            if (msg == null || msg == "")
                return;

            // The game is started, so delegate to the AI
            if (this.GameStarted() && msg[0] == '#') {
                this.ai.OnMessageReceived(msg);
                return;
            }

            // Some obvious cases
            if (msg == Server.Nickname || msg == Server.NicknameRetry) {
                this.SendMessage(this.MultiPlayerInfo.Me.Name);
                return;
            }

            // Manage lobby stuff
            if (PacManClient.IsMsgCmd(msg)) {
                this.MultiPlayerInfo.FromGameData(msg.Substring(3));
                return;
            }
            if (PacManClient.IsMsgCmd(msg, "start")) {
                this.MultiPlayerInfo.StartGame();
                return;
            }

            // Some unexpected cases
            base.onMessage(msg);
        }

        public static bool IsMsgCmd(string msg, string cmd = null)
        {
            cmd = cmd == null ? "#: " : "#" + cmd + "#: ";
            return msg.Length > cmd.Length && msg.Substring(0, cmd.Length) == cmd;
        }

        public static string TrimMsg(string msg)
        {
            int i = msg.IndexOf(": ");
            if (i == -1)
                return null;
            return msg.Substring(i + 1);
        }

        protected override void onError(string error)
        {
            this.MultiPlayerInfo.Error = true;
            base.onError(error);
        }

        protected override void run() { }
    }
}
