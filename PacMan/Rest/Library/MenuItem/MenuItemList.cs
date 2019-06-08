using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class MenuItemList : MenuItem
    {
        // Private members
        private int selectedIndex;
        private List<string> values;

        // Public members and properties
        /// <summary>
        /// The list with possibilities for this menuItem
        /// </summary>
        public List<string> Values
        {
            get { return this.values; }
            set {
                this.values = value;
                this.SelectedIndex = 0;
            }
        }
        /// <summary>
        /// The seperator between the description text and the menuItems
        /// Default is a colon and a space ": ".
        /// </summary>
        public string Seperator;
        /// <summary>
        /// The selected index
        /// </summary>
        public virtual int SelectedIndex {
            get { return this.selectedIndex; }
            set {
                this.selectedIndex = value;
                // Make sure its in the corrected range - seeing it as a torus
                if (this.Values.Count == 0) {
                    this.selectedIndex = 0;
                    return;
                }
                while (this.selectedIndex >= this.Values.Count)
                    this.selectedIndex -= this.Values.Count;
                while (this.selectedIndex < 0)
                    this.selectedIndex += this.Values.Count;
                // Send the event
                if (this.selectedIndex != value && this.OnSelectedIndexChanged != null) {
                    this.SelectedIndexChanged = true;
                    this.OnSelectedIndexChanged(this, new EventArgs());
                }
            }
        }
        /// <summary>
        /// The selected value
        /// </summary>
        public virtual string SelectedValue {
            get { return this.Values[this.SelectedIndex]; }
        }
        /// <summary>
        /// The text that is drawn to the screen.
        /// </summary>
        public override string DrawnText {
            get {
                if (this.Values.Count > 0) {
                    if (base.DrawnText == "")
                        return this.Values[this.SelectedIndex];
                    return base.DrawnText + this.Seperator + this.Values[this.SelectedIndex];
                }
                else {
                    return base.DrawnText;
                }
            }
        }
        /// <summary>
        /// Whether or not the selected index changed the last update
        /// </summary>
        public bool SelectedIndexChanged { get; private set; }

        // Events
        public event EventHandler OnSelectedIndexChanged;

        // Methods
        /// <summary>
        /// A menuItemList. This is a Dropdown box for XNA.
        /// </summary>
        /// <param name="text">The description text.</param>
        /// <param name="items">The possibilities this </param>
        public MenuItemList(string text, List<string> values, Graphics graphics)
            : base(text, graphics) {
            this.Values = values;
            this.Seperator = ": ";
        }

        public override void Update(UpdateEventArgs e)
        {
            this.SelectedIndexChanged = false;
        }

        public override void Click(object o, EventArgs e) {
            // Change selected value
            this.SelectedIndex++;

            // Do the base click
            base.Click(o, e);
        }

        public override void RightMouseClick(object o, EventArgs e) {
            // Change selected value
            this.SelectedIndex--;

            base.RightMouseClick(o, e);
        }
    }
}
