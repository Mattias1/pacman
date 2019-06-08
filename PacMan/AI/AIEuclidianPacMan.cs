using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    class AIEuclidianPacMan : AIRandom
    {
        PacMan me;
        Vector2 forbiddenDir;
        Ghost.States[] LastStates;
        bool aGhostCanSeeMe;
        bool aGhostCouldSeeMeLastUpdate;

        public AIEuclidianPacMan()
        { }
        public override void Initialize(CameraGameObject puppet)
        {
            base.Initialize(puppet);

            this.forcePuppetToBePacMan();
            this.me = (PacMan)puppet;
            this.forbiddenDir = Vector2.Zero;
            this.LastStates = new Ghost.States[this.Level.Ghosts.Count];
            this.updateGhostStates();
            this.aGhostCanSeeMe = false;
            this.aGhostCouldSeeMeLastUpdate = false;
        }

        public override Vector2 GetUpdateDir()
        {
            // A hardcoded hack, because it's easy to get stuck at the ghost spawn, and I don't want to be there anyway
            if (this.Level.IsGhostSpawn(this.Puppet.GetGridPos()))
                return this.getAwayFromGhostSpawn();
            this.updateAGhostCanSeeMe(1.2f);

            // If nothing better pops up later, do a random turn, to get orbs or something
            Vector2 result = this.getRandomTurn();

            // Try to eat the closest ghost
            float length = float.MaxValue;
            Vector2 forbiddenDirClone = this.forbiddenDir;
            if (this.aGhostCanSeeMe) {
                foreach (Ghost ghost in this.Ghosts)
                    if (ghost != null && ghost.State == Ghost.States.Vulnerable && this.CanGhostSeePacMan(ghost, 0.4f)
                        && (this.Puppet.Position - ghost.Position).LengthSquared < length) {
                        length = (this.Puppet.Position - ghost.Position).LengthSquared;
                        result = this.getEuclidianDir(ghost.Position, ref forbiddenDirClone);
                    }

                // Run away from live ghosts (they're scary)
                length = float.MinValue;
                Vector2[] dirs = this.getPossibleDirections();
                foreach (Vector2 dir in dirs)
                    if (dir != this.forbiddenDir && this.isValidDir(dir)) {
                        float l = this.visibleGhostImprovement(dir, 1.2f);
                        if (l > length) {
                            length = l;
                            result = dir;
                        }
                    }
            }

            // Sounds like a plan
            this.forbiddenDir = -result;
            return result;
        }

        public override void UpdateAfterMove(UpdateEventArgs e, Vector2 direction)
        {
            bool ghostStateDifference = false;
            for (int i = 0; i < this.LastStates.Length; i++)
                if (this.Level.Ghosts[i] != null && this.LastStates[i] != this.Level.Ghosts[i].State) {
                    ghostStateDifference = true;
                    break;
                }
            if (this.me.Velocity == Vector2.Zero || ghostStateDifference || this.aGhostCanSeeMe != this.aGhostCouldSeeMeLastUpdate)
                this.forbiddenDir = Vector2.Zero;
            this.updateGhostStates();
            this.aGhostCouldSeeMeLastUpdate = this.aGhostCanSeeMe;
        }

        protected float visibleGhostImprovement(Vector2 dir, float factor)
        {
            float result = 0f;
            foreach (Ghost ghost in this.Ghosts)
                if (ghost != null && ghost.State == Ghost.States.Alive && this.CanGhostSeePacMan(ghost, factor))
                    result += (this.Puppet.Position + dir - ghost.Position).LengthFast - (this.Puppet.Position - ghost.Position).LengthFast;
            return result;
        }

        private void updateGhostStates()
        {
            for (int i = 0; i < this.LastStates.Length; i++)
                if (this.Ghosts[i] != null)
                    this.LastStates[i] = this.Ghosts[i].State;
        }

        private void updateAGhostCanSeeMe(float factor)
        {
            this.aGhostCanSeeMe = false;
            foreach (Ghost ghost in this.Ghosts)
                if (ghost != null)
                    if (this.CanGhostSeePacMan(ghost, factor)) {
                        this.aGhostCanSeeMe = true;
                        return;
                    }
        }
    }
}
