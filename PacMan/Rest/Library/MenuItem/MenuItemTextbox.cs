using System;
using System.Text;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using amulware.Graphics;

namespace PacMan
{
    public class MenuItemTextbox : MenuItem
    {
        // Private members
        protected StringBuilder value;

        // Public members and properties
        /// <summary>
        /// The maximum length of the value
        /// </summary>
        public int MaxLength
        {
            get { return this.value.Capacity; }
            set { this.value.Capacity = value; }
        }
        /// <summary>
        /// The typed text
        /// </summary>
        public string Value
        {
            get { return this.value.ToString(); }
            set
            {
                bool cursorAtEnd = this.CursorPosition == this.value.Length;
                this.value = new StringBuilder(value);
                if (cursorAtEnd)
                    this.CursorPosition = this.value.Length;
            }
        }
        /// <summary>
        /// The seperator between the description text and the menuItems
        /// Default is a colon and a space ": ".
        /// </summary>
        public string Seperator;
        /// <summary>
        /// The text that is drawn to the screen.
        /// </summary>
        public override string DrawnText
        {
            get { return base.DrawnText + this.Seperator + this.Value; }
        }
        /// <summary>
        /// The position of the cursor
        /// </summary>
        public int CursorPosition;
        /// <summary>
        /// Whether the enter key was pressed this update.
        /// </summary>
        public bool EnterPressed { get; protected set; }

        // Methods
        /// <summary>
        /// A menuItemList. This is a Dropdown box for XNA.
        /// </summary>
        /// <param name="text">The description text.</param>
        /// <param name="items">The possibilities this </param>
        public MenuItemTextbox(string text, Graphics graphics, int maxLength = 20)
            : base(text, graphics)
        {
            this.value = new StringBuilder(maxLength);
            this.Seperator = ": ";
        }

        public override void Update(UpdateEventArgs e)
        {
            if (this.isSelected) {
                List<Key> keyValues = InputManager.Keys.GetHitKeys();
                this.EnterPressed = MenuItemTextbox.ManipulateString(keyValues, this.value, ref this.CursorPosition, InputManager.Keys.Down(Key.ShiftLeft) || InputManager.Keys.Down(Key.ShiftRight));
            }

            base.Update(e);
        }

        public override void Click(object o, EventArgs e)
        {
            // Change selected value
            this.CursorPosition = this.value.Length;

            // Do the base click
            base.Click(o, e);
        }

        /// <summary>
        /// Mamipulate a stringbuilder instance given a set of key values
        /// </summary>
        /// <param name="v">The keyvalues</param>
        /// <param name="s">The stringbuilder</param>
        /// <returns>Whether the enter key was pressed or not.</returns>
        public static bool ManipulateString(List<Key> keyValues, StringBuilder stringBuilder, ref int cursorPosition, bool shiftPressed)
        {
            foreach (Key key in keyValues) {
                // Handle the enter press
                if (key == Key.Enter) {
                    return true;
                }
                // Handle the backspace button
                else if (key == Key.BackSpace) {
                    if (cursorPosition > 0)
                        stringBuilder.Remove(--cursorPosition, 1);
                }
                // Handle the delete button
                else if (key == Key.Delete) {
                    if (cursorPosition < stringBuilder.Length)
                        stringBuilder.Remove(cursorPosition, 1);
                }
                // Handle all keys that make the string longer
                else if (stringBuilder.Length < stringBuilder.Capacity) {
                    // Get the key as a char
                    string keyS = key.ToString();
                    char keyChar = keyS[0];
                    if (keyS.Length != 1) {
                        if (keyS.Length == 7) {
                            if (keyS.Substring(0, 6) == "Number")
                                keyChar = keyS[6];
                        }
                        else {
                            keyChar = '?';
                        }
                    }
                    // Handle the spacebar
                    if (key == Key.Space) {
                        stringBuilder.Insert(cursorPosition++, ' ');
                    }
                    // Handle the period
                    else if (key == Key.Period) {
                        stringBuilder.Insert(cursorPosition++, '.');
                    }
                    // Handle all keys that are either letters or numbers
                    else if (char.IsLetterOrDigit(keyChar)) {
                        if ('A' <= keyChar && keyChar <= 'Z')
                            stringBuilder.Insert(cursorPosition++, shiftPressed ? keyChar : (char)(keyChar - 'A' + 'a'));
                        if ('0' <= keyChar && keyChar <= '9')
                            stringBuilder.Insert(cursorPosition++, keyChar);
                    }
                }
            }
            return false;
        }
    }
}
