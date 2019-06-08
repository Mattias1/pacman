using System;
using System.Collections.Generic;
using Cireon.Audio;
using OpenTK.Audio;

namespace PacMan
{
    public class Audio
    {
        SoundFile[] omnomnoms;
        int lastRandom;

        public Audio()
        {
            
            // Load all eating sounds
            this.omnomnoms = new SoundFile[4];
            for (int i = 0; i < this.omnomnoms.Length; i++) {
                this.omnomnoms[i] = SoundFile.FromOgg("data/sounds/omnomnom" + (i + 1).ToString() + ".ogg");
            }
        }

        public void PlayOmnomnom()
        {
            this.lastRandom += 1 + GlobalRandom.Next(this.omnomnoms.Length - 1);
            this.lastRandom %= this.omnomnoms.Length;
            Source s = this.omnomnoms[this.lastRandom].GenerateSource();
            s.Play();
        }
    }
}
