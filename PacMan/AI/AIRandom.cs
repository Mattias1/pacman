using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class AIRandom : AI
    {
        public AIRandom() { }

        public override Vector2 GetUpdateDir()
        {
            // A hardcoded hack, because ghosts often get stuck at their spawn
            if (this.Level.IsGhostSpawn(this.Puppet.GetGridPos()))
                return this.getAwayFromGhostSpawn();

            // Otherwise, move randomly
            return getRandomTurn();
        }

        protected Vector2 getRandomTurn()
        {
            Vector2 dir = Vector2.Zero;
            int nr = AI.Random.Next(4);
            if (nr == 0)
                dir = -Vector2.UnitY;
            if (nr == 1)
                dir = Vector2.UnitX;
            if (nr == 2)
                dir = Vector2.UnitY;
            if (nr == 3)
                dir = -Vector2.UnitX;
            if (GameMath.OppositeDirections(dir, this.Puppet.Velocity))
                return Vector2.Zero;
            return dir;
        }

        protected Vector2 getClosestDir(Vector2 target)
        {
            Vector2[] dirs = this.getPossibleDirections();
            int result = 0;
            float lengthSquared = (this.Puppet.Position + dirs[result] - target).LengthSquared;
            for (int i = 1; i < dirs.Length; i++)
                if ((this.Puppet.Position + dirs[i] - target).LengthSquared < lengthSquared) {
                    result = i;
                    lengthSquared = (this.Puppet.Position + dirs[i] - target).LengthSquared;
                }
            return dirs[result];
        }

        protected Vector2 getEuclidianDir(Vector2 target, bool minimize = true)
        {
            Vector2 fdir = Vector2.Zero;
            return this.getEuclidianDir(target, ref fdir, minimize);
        }
        protected Vector2 getEuclidianDir(Vector2 target, ref Vector2 forbiddenDir, bool minimize = true)
        {
            // Initialize
            Vector2[] dirs = this.getPossibleDirections();
            float factor = minimize ? 1 : -1;
            int result = 0;
            float lengthSquared = float.MaxValue;

            // Find the closest (or furthest) valid dir
            for (int i = 0; i < dirs.Length; i++)
                if (dirs[i] != forbiddenDir && this.isValidDir(dirs[i]) && factor * (this.Puppet.Position + dirs[i] - target).LengthSquared < lengthSquared) {
                    result = i;
                    lengthSquared = factor * (this.Puppet.Position + dirs[i] - target).LengthSquared;
                }

            // Mark the opposite direction as invalid, to stop the AI from continuously switching.
            forbiddenDir = -dirs[result];
            return dirs[result];
        }

        protected Vector2 getAwayFromGhostSpawn()
        {
            Vector2 ghostSpawnExit = new Vector2(10, 8);
            return this.getEuclidianDir(this.Level.GridToWorld(ghostSpawnExit));
        }
    }
}
