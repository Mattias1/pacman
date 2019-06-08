using System;
using OpenTK;
using OpenTK.Input;
using amulware.Graphics;

namespace PacMan
{
    class HowToPlayScreen : Screen
    {
        BackgroundLevel bgLevel;
        ParagraphList text;

        public HowToPlayScreen(ScreenManager sM, BackgroundLevel bgLevel = null)
            : base(sM)
        {
            this.bgLevel = bgLevel;
            if (bgLevel == null) {
                Vector3 eye = sM.Graphics.Camera.Eye;
                this.bgLevel = new BackgroundLevel(sM, Settings.Get.Map);
                sM.Graphics.Camera.MoveTo(eye);
            }
            this.text = new ParagraphList(sM.Graphics.FontGeometry, new Vector2(120, 200), Color.White);

            // Some explanation text
            this.text.Add(new Paragraph("Controls"));
            this.text[0].AddLine("Use the arrow keys to move around.");
            this.text[0].AddLine("Use WASD to move the camera, and R to reset it.");
            this.text[0].AddLine("Hit P to pauze, and Esc when you get desperate.");

            this.text.Add(new Paragraph("Objective"));
            this.text[1].AddLine("PacMan's goal is to eat all the orbs without getting eaten by ghosts.");
            this.text[1].AddLine("The goal of the ghosts is to make sure PacMan fails that ASAP.");
        }

        public override void Update(UpdateEventArgs e)
        {
            if (InputManager.IsConfirmHit())
                this.Remove();

            base.Update(e);
        }

        public override void Draw(UpdateEventArgs e)
        {
            if (this.bgLevel != null)
                this.bgLevel.Draw(e);

            this.text.Draw(e);
        }
    }
}
