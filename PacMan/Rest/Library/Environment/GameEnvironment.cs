using System;
using System.Collections.Generic;
using amulware.Graphics;
using OpenTK.Input;

namespace Squarrel
{
    public class GameEnvironment<Env> where Env : GameEnvironment<Env>
    {
        Dictionary<string, WorldObject<Env>> worldObjects;
        LinkedList<DrawableWorldObject<Env>> drawableWorldObjects;

        public KeyboardDevice Keyboard {
            get;
            private set;
        }

        public MouseDevice Mouse {
            get;
            private set;
        }

        public GameEnvironment(Program p) {
            this.worldObjects = new Dictionary<string, WorldObject<Env>>();
            this.drawableWorldObjects = new LinkedList<DrawableWorldObject<Env>>();

            this.Keyboard = p.Keyboard;
            this.Mouse = p.Mouse;
        }

        public void AddWorldObject(string key, WorldObject<Env> wo) {
            this.worldObjects.Add(key, wo);
            if (wo is DrawableWorldObject<Env>)
                this.drawableWorldObjects.AddLast((DrawableWorldObject<Env>)wo);
        }

        public WorldObject<Env> GetWorldObject(string key) {
            if (this.worldObjects.ContainsKey(key))
                return this.worldObjects[key];
            else
                return null;
        }

        public T GetWorldObject<T>(string key) where T : WorldObject<Env> {
            WorldObject<Env> wo = this.GetWorldObject(key);
            if (wo != null)
                return wo as T;
            else
                return null;
        }

        public void RemoveWorldObject(string key) {
            WorldObject<Env> wo = this.GetWorldObject(key);
            this.worldObjects.Remove(key);
            if (wo is DrawableWorldObject<Env>)
                this.drawableWorldObjects.Remove((DrawableWorldObject<Env>)wo);
        }

        public virtual void Update(UpdateEventArgs e) {
            WorldObject<Env>[] objArray = new WorldObject<Env>[this.worldObjects.Count];
            this.worldObjects.Values.CopyTo(objArray, 0);

            foreach (WorldObject<Env> wo in objArray)
                wo.Update(e);
        }

        public virtual void Draw(UpdateEventArgs e) {
            foreach (DrawableWorldObject<Env> wo in this.drawableWorldObjects)
                wo.Draw(e);
        }
    }
}
