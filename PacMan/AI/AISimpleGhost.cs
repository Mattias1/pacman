using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class AISimpleGhost : AIRandom
    {
        Ghost me;

        public AISimpleGhost() { }
        public override void Initialize(CameraGameObject puppet)
        {
            base.Initialize(puppet);

            this.forcePuppetToBeGhost();
            this.me = (Ghost)puppet;
        }

        public override Vector2 GetUpdateDir()
        {
            // A hardcoded hack, because ghosts often get stuck at their spawn
            if (this.Level.IsGhostSpawn(this.Puppet.GetGridPos()))
                return this.getAwayFromGhostSpawn();

            // If we can see PacMan, choose the dir that gets us as close as possible (euclidian distance)
            if (this.CanGhostSeePacMan(this.me)) {
                Vector2 dir = this.getClosestDir(this.PacMan.Position);
                // Unless we are vulnerable and too close
                if (this.me.State == Ghost.States.Vulnerable && this.CanGhostSeePacMan(this.me, 0.7f))
                    dir *= -1;
                // Or unless the move is not possible
                if (!this.isValidDir(dir))
                    return this.getRandomTurn();
                return dir;
            }

            // If we cannot see PacMan, just walk around randomly
            return this.getRandomTurn();
        }
    }
}
