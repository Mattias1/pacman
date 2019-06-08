using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class MultiPlayerInfo
    {
        static readonly List<string> settingsExports = new List<string>() { "map", "speed" };

        public UserInfo Me;
        public List<UserInfo> Users;
        public PacManServer Server { get; private set; }
        public PacManClient Client { get; private set; }
        public bool Error
        {
            get { return this.Server == null && this.Client == null; }
            set
            {
                if (!value)
                    return;
                this.Abort();
                this.Server = null;
                this.Client = null;
                if (this.OnRefresh != null)
                    this.OnRefresh(this, new EventArgs());
            }
        }
        public bool Started { get; private set; }

        public event EventHandler OnRefresh;

        public MultiPlayerInfo(bool host)
        {
            this.Me = new UserInfo(Settings.Get.Name, 0);
            this.Users = new List<UserInfo>() { this.Me };
            this.Error = false;
            if (host)
                this.Server = new PacManServer(this);
            else
                this.Client = new PacManClient(this);
        }

        public void Abort()
        {
            if (this.IsHost() && this.Server != null)
                this.Server.Abort();
            else if (this.Client != null)
                this.Client.Abort();
        }

        public bool IsHost()
        {
            return this.Server != null;
        }

        public bool TakePosition(int id)
        {
            // Check if the spot is still free
            foreach (UserInfo user in this.Users)
                if (user.Id == id) {
                    if (this.IsHost())
                        user.Id = -1;
                    else
                        return false;
                }

            // Change spots
            if (this.IsHost())
                this.Command(this.Me.Name, "id", id.ToString());
            else
                this.Client.SendMessage("#id#" + id.ToString());
            return true;
        }
        public bool EmptyPosition(int id)
        {
            foreach (UserInfo user in this.Users) {
                if (user.Id == id) {
                    if (!this.IsHost() && user != this.Me)
                        return false;
                    if (this.IsHost()) {
                        this.Command(user.Name, "id", (-1).ToString());
                    }
                    else {
                        this.Client.SendMessage("#id#" + (-1).ToString());
                    }
                    return true;
                }
            }
            return true;
        }
        public bool ClosePosition(int id)
        {
            return false;
        }

        public void ChangeMap(string mapFile)
        {
            Settings.Get.Map = mapFile;
            this.informClients();
        }

        public bool StartGame()
        {
            // Tell the others we started.
            if (this.IsHost())
                this.Server.BroadCast("#start#", "#");
            // The host just told us we started :)
            else
                this.Started = true;
            return false;
        }

        public void AddUser(string name, int id = -1)
        {
            this.Users.Add(new UserInfo(name, id));
            this.informClients();
        }

        public void RemoveUser(string name)
        {
            for (int i = 0; i < this.Users.Count; i++)
                if (this.Users[i].Name == name) {
                    this.Users.RemoveAt(i);
                    this.informClients();
                    return;
                }
        }
        public void RemoveUser(int id)
        {
            for (int i = 0; i < this.Users.Count; i++)
                if (this.Users[i].Id == id) {
                    this.Users.RemoveAt(i);
                    this.informClients();
                    return;
                }
        }

        public void DoSwapUsers(int pacId, int ghostId, AI[] ais)
        {
            if (pacId == ghostId)
                return;
            foreach (UserInfo user in this.Users) {
                if (user.Id == pacId)
                    user.Id = ghostId;
                else if (user.Id == ghostId)
                    user.Id = pacId;
            }
            AI temp = ais[pacId];
            ais[pacId] = ais[ghostId];
            ais[ghostId] = temp;
        }
        public void SwapUsersCommand(int pacId, int ghostId, AI[] ais)
        {
            this.assertHost();

            this.DoSwapUsers(pacId, ghostId, ais);
            this.Server.BroadCast("#swap#", String.Format("{0},{1}", pacId, ghostId));
        }

        public string ToGameData()
        {
            string s = Settings.Get.ToString(MultiPlayerInfo.settingsExports, ';', "~");
            for (int i = 0; i < this.Users.Count; i++)
                s += "|" + this.Users[i].ToGameData();
            return s;
        }
        public void FromGameData(string data)
        {
            string[] s = data.Split(new char[] { '|' });
            Settings.Get.FromString(s[0], ';', "~");
            this.Users = new List<UserInfo>(s.Length);
            for (int i = 1; i < s.Length; i++) {
                this.Users.Add(UserInfo.FromGameData(s[i]));
                if (this.Users[i - 1].Name == this.Me.Name)
                    this.Me = this.Users[i - 1];
            }

            if (this.OnRefresh != null)
                this.OnRefresh(this, new EventArgs());
        }

        public void Command(string name, string c, string msg)
        {
            this.assertHost();
            UserInfo sender = this.assertUserExists(name);

            // Change spot
            if (c == "id") {
                int newId = int.Parse(msg);
                if (newId != -1)
                    foreach (UserInfo user in this.Users)
                        if (user.Id == newId)
                            return;
                sender.Id = newId;
                this.informClients();
            }

            if (this.OnRefresh != null)
                this.OnRefresh(this, new EventArgs());
        }

        private void informClients()
        {
            this.Server.BroadCast("#", this.ToGameData());
        }

        #region Asserts

        private void assertHost()
        {
            if (!this.IsHost())
                throw new Exception("Assumption violation: I should be host.");
        }
        private void assertClient()
        {
            if (this.IsHost())
                throw new Exception("Assumption violation: I should be client.");
        }
        private UserInfo assertUserExists(string name)
        {
            foreach (UserInfo user in this.Users)
                if (user.Name == name)
                    return user;
            throw new Exception(String.Format("Assumption violation: User '{0}' does not exist.", name));
        }
        private UserInfo assertUserExists(int id)
        {
            foreach (UserInfo user in this.Users)
                if (user.Id == id)
                    return user;
            throw new Exception(String.Format("Assumption violation: User with id '{0}' does not exist.", id));
        }

        #endregion
    }

    public class UserInfo
    {
        public string Name; // Nickname
        public int Id;      // The unit the player plays with (-1=observer, 0=pacman, 1=ms pacman, 2-5=ghosts)
        public int Score;   // The score the player has gathered so far

        private UserInfo() { }
        public UserInfo(string name, int id)
        {
            this.Name = name;
            this.Id = id;
            this.Score = 0;
        }

        public CameraGameObject GetPuppet(Level level)
        {
            return level.GetGameObject(this.Id);
        }

        public string ToGameData()
        {
            return String.Format("{0};{1};{2}", this.Name, this.Id.ToString(), this.Score.ToString());
        }
        public void FromgameData(string data)
        {
            string[] s = data.Split(new char[] { ';' });
            this.Name = s[0];
            this.Id = int.Parse(s[1]);
            this.Score = int.Parse(s[2]);
        }
        public static UserInfo FromGameData(string data)
        {
            UserInfo user = new UserInfo();
            user.FromgameData(data);
            return user;
        }
    }
}
