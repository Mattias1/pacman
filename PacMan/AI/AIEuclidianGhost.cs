using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class AIEuclidianGhost : AIRandom
    {
        Ghost me;
        Vector2 forbiddenDir;
        Ghost.States LastState;

        public AIEuclidianGhost() { }
        public override void Initialize(CameraGameObject puppet)
        {
            base.Initialize(puppet);

            this.forcePuppetToBeGhost();
            this.me = (Ghost)puppet;
            this.forbiddenDir = Vector2.Zero;
            this.LastState = this.me.State;
        }

        public override Vector2 GetUpdateDir()
        {
            // A hardcoded hack, because ghosts often get stuck at their spawn
            if (this.Level.IsGhostSpawn(this.Puppet.GetGridPos()))
                return this.getAwayFromGhostSpawn();

            if (this.CanGhostSeePacMan(this.me)) {
                // If we can see PacMan, choose the dir that gets us as close as possible (euclidian distance)
                if (this.me.State == Ghost.States.Alive)
                    return this.getEuclidianDir(this.PacMan.Position, ref this.forbiddenDir);
                // Unless we are vulnerable and too close
                if (this.me.State == Ghost.States.Vulnerable && this.CanGhostSeePacMan(this.me, 0.7f))
                    return this.getEuclidianDir(this.PacMan.Position, ref this.forbiddenDir, false);
            }

            // If we cannot see PacMan, just walk around randomly
            return this.getRandomTurn();
        }

        public override void UpdateAfterMove(UpdateEventArgs e, Vector2 direction)
        {
            if (this.me.Velocity == Vector2.Zero || this.LastState != this.me.State)
                this.forbiddenDir = Vector2.Zero;
            this.LastState = this.me.State;
        }
    }
}
