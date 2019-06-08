using System;
using amulware.Graphics;
using OpenTK.Input;

namespace Squarrel
{
    public abstract class WorldObject<Env> where Env : GameEnvironment<Env>
    {
        protected Env environment {
            get;
            private set;
        }

        protected KeyboardDevice keyboard {
            get { return this.environment.Keyboard; }
        }

        protected MouseDevice mouse {
            get { return this.environment.Mouse; }
        }

        public WorldObject(Env environment) {
            this.environment = environment;
        }

        abstract public void Update(UpdateEventArgs e);
    }
}
