using System;
using OpenTK;
using OpenTK.Input;
using amulware.Graphics;

namespace PacMan
{
    /// <summary>
    /// A class representing one of the drawing layers of the screen manager.
    /// </summary>
    public abstract class Screen
    {
        protected ScreenManager screenManager;

        private bool firstRun;
        private bool isTransparent;
        private bool isVisible;
        /// <summary>
        /// Whether there are screens visible "behind" this screen.
        /// </summary>
        public bool IsTransparent {
            get { return this.isTransparent; }
            protected set {
                this.screenManager.ManageTransparancyChange(this);
                this.isTransparent = value;
            }
        }
        /// <summary>
        /// Should the screen be drawn?
        /// </summary>
        public bool IsVisible {
            get { return this.isVisible; }
            protected set {
                this.screenManager.ManageVisibilityChange(this);
                this.isVisible = value;
            }
        }
        /// <summary>
        /// Should the screen be updated?
        /// </summary>
        public bool IsActive { get; protected set; }

        /// <summary>
        /// Should screens below this one update?
        /// </summary>
        public bool BlocksUpdating { get; protected set; }

        /// <summary>
        /// If true, this will only be updated if the screen would be drawn at a certain frame tick. 
        /// If false, this will even be updated when its not being drawn.
        /// </summary>
        public bool OnlyUpdateWhenDrawn { get; protected set; }

        public Screen(ScreenManager sManager, bool visible = true, bool transparent = false) {
            // Add a reference to the screenmanager. We will need it!
            this.screenManager = sManager;
            // Set some properties for the screen.
            this.isVisible = visible;
            this.isTransparent = transparent;
            this.IsActive = true;
            this.OnlyUpdateWhenDrawn = true;
            this.firstRun = true;
        }

        public virtual void Update(UpdateEventArgs e) {
            // First run
            if (this.firstRun) {
                this.OnLoad();
                this.firstRun = false;
            }

             // Escape press
             if (InputManager.Keys.Hit(Key.Escape)) {
                 this.OnEscapePress();
             }
        }
        public abstract void Draw(UpdateEventArgs e);

        /// <summary>
        /// This method gets executed just before the first update call
        /// </summary>
        public virtual void OnLoad() { }

        /// <summary>
        /// This method gets executed when the user presses escape
        /// </summary>
        public virtual void OnEscapePress() { }

        /// <summary>
        /// The method gets executed just before removing a screen.
        /// </summary>
        /// <returns>If it returns false, the screen will not be removed.</returns>
        public virtual bool OnRemove() {
            return true;
        }

        /// <summary>
        /// This method is executed when the screen gets covered by something else.
        /// </summary>
        /// <param name="s">The screen that covers this one.</param>
        protected virtual void onCover(Screen s) { }

        /// <summary>
        /// This method is executed when the screen covering this one is removed or becomes transparent.
        /// </summary>
        protected virtual void onUncover() { }

        /// <summary>
        /// Removes the screen from the screenManager
        /// </summary>
        /// <returns>Whether an occurence of the given screen was found and removed or not.</returns>
        public bool Remove() {
            return this.screenManager.RemoveScreen(this);
        }

        /// <summary>
        /// Cover this screen and send a covered event.
        /// </summary>
        /// <param name="s">The screen that covers this one.</param>
        public void Cover(Screen s) {
            this.onCover(s);
        }

        /// <summary>
        /// Uncover this screen and send an uncovered event.
        /// </summary>
        public void Uncover() {
            this.onUncover();
        }
    }
}
