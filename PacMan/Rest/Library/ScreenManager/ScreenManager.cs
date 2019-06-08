using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class ScreenManager
    {
        private List<Screen> screens = new List<Screen>();
        public Program Program { get; private set; }
        public Graphics Graphics { get; private set; }

        /// <summary>
        /// Gets or sets the screenresolution
        /// </summary>
        public Vector2 ScreenResolution {
            get {
                return new Vector2(this.Program.ClientSize.Width, this.Program.ClientSize.Height);
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Whether the game runs in fullscreen or not
        /// </summary>
        public bool FullScreen {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Whether the cursor is visible or not
        /// </summary>
        public bool MouseVisible {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public ScreenManager(Program program, Graphics graphics) {
            this.Program = program;
            this.Graphics = graphics;
            InputManager.Initialize(program);
        }

        public void Update(UpdateEventArgs e) {
            // Update the inputmanager
            InputManager.Update();

            // Make a copy of the screen list, so that you dont get in an infinite updateLoop (can happen if you add a screen in an update loop).
            // TODO: this is really inefficient. Removing should be smarter in the remove method.
            Screen[] screenCopy = this.screens.ToArray();
            bool wouldBeDrawn = true;

            // Loop through all the screens and update them if they are active
            for (int i = 0; i < screenCopy.Length; i++) {
                if (screenCopy[i].IsActive && (wouldBeDrawn || (screenCopy[i].IsVisible && !screenCopy[i].OnlyUpdateWhenDrawn)))
                    screenCopy[i].Update(e);

                if (screenCopy[i].IsActive && screenCopy[i].BlocksUpdating)
                    return;
                if (screenCopy[i].IsVisible && !screenCopy[i].IsTransparent)
                    wouldBeDrawn = false;
            }
        }

        /// <summary>
        /// Draws all visible screens in order until it comes across a visible non-transparent screen.
        /// </summary>
        public void Draw(UpdateEventArgs e) {
            // Draw the last screen first, because the spritebatch overwrites.
            int i;
            // Loop until you find a (visible) screen that blocks further drawing
            for (i = 0; i < this.screens.Count; i++)
                if (this.screens[i].IsVisible && !this.screens[i].IsTransparent)
                    break;
            // If all screens are drawn, be sure to correct the index
            if (i == this.screens.Count)
                i--;
            // Draw all the (visible) screens
            for (; i >= 0; i--)
                if (this.screens[i].IsVisible)
                    this.screens[i].Draw(e);
        }

        /// <summary>
        /// Adds a new screen on top of the list of screens.
        /// </summary>
        /// <param name="s">The screen to be added</param>
        public void AddScreen(Screen s) {
            // Let's add the new screen on top of the other screens.
            this.screens.Insert(0, s);

            // Manage the covered events
            if (!s.IsTransparent && s.IsVisible) {
                int i = 0;
                while (++i < this.screens.Count) // PROGRAMMER NOTE: this uses the same variable as the main loop, be cautious if you modify the code
                    if (this.screens[i].IsVisible) {
                        // Send the event
                        this.screens[i].Cover(s);

                        // Stop sending events, these screens already where covered
                        if (!this.screens[i].IsTransparent)
                            break;
                    }
            }
            return;
        }

        /// <summary>
        /// Removes the given screen from the list of screens.
        /// </summary>
        /// <param name="s">The screen to be removed</param>
        /// <returns>Whether an occurence of the given screen was found and removed or not.</returns>
        public bool RemoveScreen(Screen s) {
            // Check if the screen is being drawn (if its transparant or not visible its not going to uncover anything
            bool needToCheck = !s.IsTransparent && s.IsVisible && this.IsBeingDrawn(s);

            // Manually search and remove the screen
            for (int i = 0; i < this.screens.Count; i++)
                if (this.screens[i] == s) {
                    // Remove the screen
                    this.screens.RemoveAt(i);

                    // Manage the uncovered events
                    if (needToCheck)
                        for (; i < this.screens.Count; i++) // PROGRAMMER NOTE: this uses the same variable as the main loop, be cautious if you modify the code
                            if (this.screens[i].IsVisible) {
                                // Send the event
                                this.screens[i].Uncover();

                                // Stop sending events, these screens already where covered
                                if (!this.screens[i].IsTransparent)
                                    break;
                            }
                    return true;
                }
            return false;
        }

        /// <summary>
        /// Checks if the screen is being drawn.
        /// </summary>
        /// <param name="s">The screen to check</param>
        /// <returns>True if the screen will be drawn under the current circumstances.</returns>
        public bool IsBeingDrawn(Screen s) {
            // Loop through all screens until we find our screen or a visible non-transparent screen.
            for (int i = 0; i < this.screens.Count; i++) {
                // We found our screen, so it is not blocked by another one.
                if (this.screens[i] == s)
                    return true;
                // Now, let's check if the current screen is blocking our view.
                if (!this.screens[i].IsTransparent && this.screens[i].IsVisible)
                    return false;
            }

            // Well, this is odd. Our screen is not even there!
            throw new Exception("The screen you were looking for has not been found.");
        }

        /// <summary>
        /// Exits the current game.
        /// </summary>
        public void Exit() {
            this.Program.Close();
        }

        /// <summary>
        /// Manage changing the transparancy of a screen by sending cover or uncover events
        /// Called from the screen's IsTransparent property
        /// </summary>
        /// <param name="s">The screen of which you want to change the transparancy</param>
        public void ManageTransparancyChange(Screen s) {
            // If s is transparent its not going to (un)cover a screen.
            if (!s.IsVisible)
                return;

            bool sIsFound = false;

            // Possible cover events (it will become solid)
            if (s.IsTransparent) {
                for (int i = 0; i < this.screens.Count; i++) {
                    if (this.screens[i] == s)
                        sIsFound = true;
                    else
                        if (this.screens[i].IsVisible) {
                            if (sIsFound)
                                this.screens[i].Cover(s);
                            // After this screen no other screen can be uncovered anymore.
                            if (!this.screens[i].IsTransparent)
                                return;
                        }
                }
            }

            // Possible uncover events (it will become transparant)
            else {
                for (int i = 0; i < this.screens.Count; i++) {
                    if (this.screens[i] == s)
                        sIsFound = true;
                    else
                        if (this.screens[i].IsVisible) {
                            // If we already found s, this screen will be uncovered.
                            if (sIsFound)
                                this.screens[i].Uncover();
                            // After this screen no other screen can be uncovered anymore.
                            if (!this.screens[i].IsTransparent)
                                return;
                        }
                }
            }
        }
        /// <summary>
        /// Manage changing the visibility of a screen by sending cover or uncover events
        /// Called from the screen's IsVisible property
        /// </summary>
        /// <param name="s">The screen of which you want to change the visibility</param>
        public void ManageVisibilityChange(Screen s) {
            // If s is transparent its not going to (un)cover a screen.
            if (s.IsTransparent)
                return;

            bool sIsFound = false;

            // Possible uncover events (it will become invisible)
            if (s.IsVisible) {
                for (int i = 0; i < this.screens.Count; i++) {
                    if (this.screens[i] == s)
                        sIsFound = true;
                    else
                        if (this.screens[i].IsVisible) {
                            // If we already found s, this screen will be uncovered.
                            if (sIsFound)
                                this.screens[i].Uncover();
                            // After this screen no other screen can be uncovered anymore.
                            if (!this.screens[i].IsTransparent)
                                return;
                        }
                }
            }

            // Possible cover events (it will become visible)
            else {
                for (int i = 0; i < this.screens.Count; i++) {
                    if (this.screens[i] == s)
                        sIsFound = true;
                    else
                        if (this.screens[i].IsVisible) {
                            if (sIsFound)
                                this.screens[i].Cover(s);
                            // After this screen no other screen can be uncovered anymore.
                            if (!this.screens[i].IsTransparent)
                                return;
                        }
                }
            }
        }
    }
}
