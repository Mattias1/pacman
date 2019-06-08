using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public abstract class AI
    {
        // Some nice properties and variables to have
        public CameraGameObject Puppet { get; private set; }
        public Level Level
        {
            get { return this.Puppet.Level; }
        }
        public PacMan PacMan
        {
            get { return this.Level.PacMan; }
        }
        public List<Ghost> Ghosts
        {
            get { return this.Level.Ghosts; }
        }
        public static Random Random = null;

        // The important methods
        public AI()
        {
            if (AI.Random == null)
                AI.Random = new Random();
        }
        public virtual void Initialize(CameraGameObject puppet)
        {
            this.Puppet = puppet;
        }

        public virtual void UpdateBeforeMove(UpdateEventArgs e) { }

        public abstract Vector2 GetUpdateDir();

        public virtual void UpdateAfterMove(UpdateEventArgs e, Vector2 direction) { }

        // Some nice helper methods
        public bool isValidDir(Vector2 dir)
        {
            return this.Level.IsMoveable(this.Puppet.GetGridPos() + dir);
        }

        public bool CanGhostSeePacMan(Ghost ghost, float factor = 1f)
        {
            return (ghost.Position - this.PacMan.Position).LengthSquared < Ghost.SEEINGRANGE * factor * Ghost.SEEINGRANGE * factor;
        }

        protected void forcePuppetToBeGhost()
        {
            if (this.Puppet.GetType() != typeof(Ghost))
                throw new Exception("This AI is for ghosts only.");
        }
        protected void forcePuppetToBePacMan()
        {
            if (this.Puppet.GetType() != typeof(PacMan))
                throw new Exception("This AI is for pacman only.");
        }

        protected Vector2[] getPossibleDirections()
        {
            return new Vector2[] { -Vector2.UnitY, Vector2.UnitX, Vector2.UnitY, -Vector2.UnitX }; // N E S W
        }
    }
}
