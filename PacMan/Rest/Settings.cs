using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class Settings
    {
        public const string VersionString = "0.0";

        #region Settings internals

        private const string path = "pacman.ini";

        // Singleton code
        /// <summary>
        /// The settings instance
        /// </summary>
        private static Settings instance;
        /// <summary>
        /// The instance of the settings singleton
        /// </summary>
        public static Settings Get
        {
            get { return Settings.instance == null ? Settings.instance = new Settings() : Settings.instance; }
        }

        // The list with all settings
        /// <summary>
        /// The list with all settings. All keys are lowercase.
        /// </summary>
        private Dictionary<string, string> hashList;

        // String
        private string get(string key, string defaultValue)
        {
            if (!this.hashList.ContainsKey(key))
                this.set(key, defaultValue);
            return this.hashList[key];
        }
        private void set(string key, string value)
        {
            this.hashList[key] = value;
        }
        // Bool
        private bool get(string key, bool defaultValue)
        {
            return bool.Parse(this.get(key, defaultValue.ToString()));
        }
        private void set(string key, bool value)
        {
            this.set(key, value.ToString());
        }
        // Int
        private int get(string key, int defaultValue)
        {
            return int.Parse(this.get(key, defaultValue.ToString()));
        }
        private void set(string key, int value)
        {
            this.set(key, value.ToString());
        }
        // Float
        private float get(string key, float defaultValue)
        {
            return float.Parse(this.get(key, defaultValue.ToString()));
        }
        private void set(string key, float value)
        {
            this.set(key, value.ToString());
        }
        // Vector2 (assuming integer values)
        private Vector2 get(string key, Vector2 defaultValue)
        {
            return Str2Vec(this.get(key, Vec2Str(defaultValue)));
        }
        private void set(string key, Vector2 value)
        {
            this.set(key, Vec2Str(value));
        }
        // Int[]
        private int[] get(string key, int[] defaultValue, string separator = ",")
        {
            string[] values = this.get(key, String.Join<int>(separator, defaultValue)).Split(new string[] { separator }, StringSplitOptions.RemoveEmptyEntries);
            int[] result = new int[values.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = int.Parse(values[i]);
            return result;
        }
        private void set(string key, int[] value, string separator = ",")
        {
            this.set(key, String.Join<int>(separator, value));
        }

        // Settings properties

        // Private settings methods
        private Settings()
        {
            this.SetDefaults();
        }

        // Public settings methods
        public override string ToString()
        {
            return this.ToString(null);
        }
        public string ToString(char seperator)
        {
            return this.ToString(null, seperator);
        }
        public string ToString(List<string> properties, char seperator = ';')
        {
            return this.ToString(properties, seperator, Environment.NewLine);
        }
        public string ToString(List<string> properties, char seperator, string endSeperator)
        {
            // Write all hashlist values with keys in the properties list to a string, write all if the propertieslist is left null
            StringBuilder s = new StringBuilder();
            foreach (var tuple in this.hashList)
                if (properties == null || properties.Contains(tuple.Key))
                    s.Append(tuple.Key + seperator + tuple.Value + endSeperator);
            return s.ToString();
        }

        public void FromString(string s, char seperator = ';')
        {
            this.FromString(s, seperator, Environment.NewLine);
        }
        public void FromString(string s, char seperator, string endSeperator)
        {
            // Override hashlist with values from s (values not in s will be left intact)
            string[] lines = s.Split(endSeperator.ToCharArray());
            for (int i = 0; i < lines.Length; i++) {
                string[] keyVal = lines[i].Split(seperator);
                if (keyVal[0] != "")
                    this.hashList[keyVal[0]] = keyVal[1];
            }
        }

        /// <summary>
        /// Load the settings from file
        /// </summary>
        /// <returns>Whether there was an error loading</returns>
        public bool Load()
        {
            bool noError = false;
            // If the file doesnt exist, load the defaults
            if (!File.Exists(Settings.path))
                return false;
            try {
                using (StreamReader file = new StreamReader(Settings.path)) {
                    this.FromString(file.ReadToEnd());
                    noError = true;
                }
            }
            catch {
                //MessageScreen.Show("(!) The inifile was incorrectly modified." + System.Environment.NewLine + "The highscores will be resetted.", Color.Black, 0, 2);
                //this.ResetHighscores();
                return false;
            }

            return noError;
        }

        /// <summary>
        /// Save the settings to file
        /// </summary>
        /// <returns>Whether there was an error saving</returns>
        public bool Save()
        {
            bool noError = false;
            try {
                using (StreamWriter file = new StreamWriter(Settings.path)) {
                    file.WriteLine(this.ToString());
                    noError = true;
                }
            }
            catch {
                return false;
            }
            return noError;
        }

        /// <summary>
        /// Set the default values.
        /// </summary>
        public void SetDefaults()
        {
            this.hashList = new Dictionary<string, string>();
        }

        /// <summary>
        /// Parse an vector2 (with integer values) to a string
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string Vec2Str(Vector2 v, char seperator = ',')
        {
            return ((int)v.X).ToString() + seperator + ((int)v.Y).ToString();
        }
        /// <summary>
        /// Parse a string to a vector2 (with integer values)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static Vector2 Str2Vec(string s, char seperator = ',')
        {
            string[] vs = s.Split(seperator);
            return new Vector2(int.Parse(vs[0]), int.Parse(vs[1]));
        }

        /// <summary>
        /// Parse an vector2 (with floating point values) to a string
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static string Vec2StrF(Vector2 v, char seperator = ',')
        {
            return v.X.ToString() + seperator + v.Y.ToString();
        }
        /// <summary>
        /// Parse a string to a vector2 (with floating point values)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="seperator"></param>
        /// <returns></returns>
        public static Vector2 Str2VecF(string s, char seperator = ',')
        {
            string[] vs = s.Split(seperator);
            return new Vector2(float.Parse(vs[0]), float.Parse(vs[1]));
        }

        #endregion

        public string Name
        {
            get { return this.get("name", "Player"); }
            set
            {
                if (!LobbyScreen.ConstList.Contains(value))
                    this.set("name", value);
            }
        }

        public int Lives
        {
            get { return this.get("lives", 2); }
            set { this.set("lives", value); }
        }

        public bool Speed
        {
            get { return this.get("speed", true); }
            set { this.set("speed", value); }
        }

        public string Map
        {
            get { return this.get("map", "data/levels/level1.bmp"); }
            set { this.set("map", value); }
        }

        public int[] Players
        {
            get { return this.get("players", new int[] { 0, 4, 2, 2, 3, 4 }); }
            set { this.set("players", value); }
        }

        public IPAddress IP
        {
            get { return IPAddress.Parse(this.get("ip", new IPAddress(new byte[] { 127, 0, 0, 1 }).ToString())); }
            set { this.set("ip", value.ToString()); }
        }

        public int Port
        {
            get { return this.get("port", 9595); }
            set { this.set("port", value); }
        }
    }
}
