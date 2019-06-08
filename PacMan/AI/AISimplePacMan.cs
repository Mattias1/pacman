using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class AISimplePacMan : AIRandom
    {
        PacMan me;

        public AISimplePacMan() { }
        public override void Initialize(CameraGameObject puppet)
        {
            base.Initialize(puppet);

            this.forcePuppetToBePacMan();
            this.me = (PacMan)puppet;
        }

        public override Vector2 GetUpdateDir()
        {
            // A hardcoded hack, because ghosts often get stuck at their spawn
            if (this.Level.IsGhostSpawn(this.Puppet.GetGridPos()))
                return this.getAwayFromGhostSpawn();

            // If a ghost can (almost) see me, run away
            Vector2 result = this.getRandomTurn();
            float closest = 100f;
            foreach (Ghost ghost in this.Ghosts)
                if (ghost != null)
                    if (this.CanGhostSeePacMan(ghost, 1.3f)) {
                        Vector2 dir = this.getEuclidianDir(ghost.Position);
                        // Try to get away
                        if (this.isValidDir(-1 * dir) && ghost.State == Ghost.States.Alive)
                            result = -1 * dir;
                        // Unless the ghost is vulnerable and too close
                        if (ghost.State == Ghost.States.Vulnerable && this.CanGhostSeePacMan(ghost, 0.4f)
                            && (this.me.Position - ghost.Position).LengthSquared < closest && this.isValidDir(dir)) {
                            closest = (this.me.Position - ghost.Position).LengthSquared;
                            result = dir;
                        }
                    }

            // If noone can see us, just walk around randomly
            return result;
        }
    }
}
