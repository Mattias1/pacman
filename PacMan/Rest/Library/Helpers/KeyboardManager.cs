using System.Collections.Generic;
using OpenTK.Input;

namespace PacMan
{
    /* 
     * Todo:
     * Create it (once)
     * Update it (every frame)
     * Add keys  (the ones to check)
     */
    public class KeyboardManager
    {
        private class KeyEntry
        {
            public readonly Key Key;

            private bool lastFrame = false;
            private bool thisFrame = false;

            public KeyEntry(Key key)
            {
                this.Key = key;
            }

            public void Update(KeyboardDevice keyboard)
            {
                this.lastFrame = this.thisFrame;
                this.thisFrame = keyboard[this.Key];
            }

            public void Flush()
            {
                this.lastFrame = false;
                this.thisFrame = false;
            }

            public void Flush(KeyboardDevice keyboard)
            {
                this.lastFrame = keyboard[this.Key];
                this.thisFrame = this.lastFrame;
            }

            public bool Down()
            {
                return this.thisFrame;
            }

            public bool Hit()
            {
                return this.thisFrame && !this.lastFrame;
            }

            public bool Up()
            {
                return this.lastFrame && !this.thisFrame;
            }
        }

        private LinkedList<KeyEntry> keyList = new LinkedList<KeyEntry>();
        private Dictionary<Key, KeyEntry> keyDictionary = new Dictionary<Key, KeyEntry>();

        public void AddKey(Key key)
        {
            if (this.keyDictionary.ContainsKey(key))
                return;
            KeyEntry entry = new KeyEntry(key);
            this.keyList.AddLast(entry);
            this.keyDictionary.Add(key, entry);
        }

        public void AddKeys(params Key[] keys)
        {
            foreach (Key key in keys)
                this.AddKey(key);
        }

        #region AddKeys

        /// <summary>
        /// Add the arrow keys (Up, Down, Right, Left)
        /// </summary>
        /// <returns></returns>
        public KeyboardManager AddArrows()
        {
            this.AddKeys(Key.Up, Key.Down, Key.Right, Key.Left);
            return this;
        }
        /// <summary>
        /// Add Enter, Space, Escape and the arrow keys (Up, Down, Right, Left)
        /// </summary>
        /// <returns></returns>
        public KeyboardManager AddMenu()
        {
            this.AddKeys(Key.Enter, Key.Space, Key.Escape);
            this.AddArrows();
            return this;
        }
        /// <summary>
        /// Add the WASD keys
        /// </summary>
        /// <returns></returns>
        public KeyboardManager AddWASD()
        {
            this.AddKeys(Key.W, Key.A, Key.S, Key.D);
            return this;
        }
        /// <summary>
        /// Add the keys for all the letters of the alphabet
        /// </summary>
        /// <returns></returns>
        public KeyboardManager AddAtoZ()
        {
            this.AddKeys(Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G,
                Key.H, Key.I, Key.J, Key.K, Key.L, Key.M, Key.N, Key.O, Key.P,
                Key.Q, Key.R, Key.S, Key.T, Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z);
            return this;
        }
        /// <summary>
        /// Add all the number keys
        /// </summary>
        /// <returns></returns>
        public KeyboardManager Add0to9()
        {
            this.AddKeys(Key.Number0, Key.Number1, Key.Number2, Key.Number3, 
                Key.Number4, Key.Number5, Key.Number6, Key.Number7, Key.Number8, Key.Number9);
            return this;
        }

        #endregion

        public void Update(KeyboardDevice keyboard)
        {
            foreach (KeyEntry entry in this.keyList)
                entry.Update(keyboard);
        }

        public void Flush(KeyboardDevice keyboard)
        {
            foreach (KeyEntry entry in this.keyList)
                entry.Flush(keyboard);
        }

        public void Flush()
        {
            foreach (KeyEntry entry in this.keyList)
                entry.Flush();
        }

        public bool Hit(Key key)
        {
            return this.keyDictionary[key].Hit();
        }

        public bool Down(Key key)
        {
            return this.keyDictionary[key].Down();
        }

        public bool Up(Key key)
        {
            return this.keyDictionary[key].Up();
        }

        public List<Key> GetHitKeys()
        {
            List<Key> keys = new List<Key>();
            foreach (KeyEntry entry in this.keyList)
                if (entry.Hit())
                    keys.Add(entry.Key);
            return keys;
        }
    }
}
