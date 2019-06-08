using System;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class MenuItem
    {
        public enum HorizontalAlign { Left, CopyLeft, Center, CopyRight /* Pun intended */, Right }
        public enum VerticalAlign { Top, CopyTop, Middle, CopyBottom, Bottom }

        // Properties
        /// <summary>
        /// Whether this menuItem is selected or not
        /// </summary>
        protected bool isSelected { get; private set; }

        /// <summary>
        /// The graphics object
        /// </summary>
        public Graphics Graphics { get; private set; }
        /// <summary>
        /// The description text for this menuItem.
        /// </summary>
        public string Text;
        /// <summary>
        /// The position of this menuItem.
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// The size of this menuItem.
        /// </summary>
        public Vector2 Size;
        /// <summary>
        /// The height of the font.
        /// </summary>
        public int FontSize;
        /// <summary>
        /// The colour of the menuItem
        /// </summary>
        public Color Colour;
        /// <summary>
        /// The colour of the menuItem when it's selected
        /// </summary>
        public Color SelectedColour;
        /// <summary>
        /// Get the hitbox of this menuItem.
        /// </summary>
        public System.Drawing.Rectangle HitBox
        {
            get { return new System.Drawing.Rectangle((int)(this.Position.X + 0.5f), (int)(this.Position.Y + 0.5f), (int)(this.Size.X + 0.5f), (int)(this.Size.Y + 0.5f)); }
        }
        /// <summary>
        /// The text that is drawn to the screen
        /// </summary>
        public virtual string DrawnText
        {
            get { return this.Text; }
        }
        /// <summary>
        /// The horizontal alignment of the text inside this menuItem
        /// </summary>
        public HorizontalAlign TextAlign;
        /// <summary>
        /// The vertical alignment of the text inside this menuItem
        /// </summary>
        public VerticalAlign TextVAlign;    // Not yet implemented
        /// <summary>
        /// Extra offset on top of the TextAlignment
        /// </summary>
        public Vector2 TextOffset;

        // Events
        public event EventHandler OnSelected;
        public event EventHandler OnDeselected;
        public event EventHandler OnClick;

        // Methods
        /// <summary>
        /// A menuItem with just text
        /// </summary>
        /// <param name="text">The description text.</param>
        public MenuItem(string text, Graphics graphics)
        {
            this.Text = text;
            this.Graphics = graphics;
            this.SetDefaults();
        }

        // Methods to set layout
        /// <summary>
        /// Set the default properties on this MenuItem
        /// </summary>
        public virtual void SetDefaults()
        {
            this.Size = new Vector2(200, 40);
            this.FontSize = 40;
            this.Colour = Color.Black;
            this.SelectedColour = Color.Yellow;
            this.TextAlign = HorizontalAlign.Center;
            this.TextVAlign = VerticalAlign.Bottom;
            this.TextOffset = Vector2.Zero;
        }

        /// <summary>
        /// Copy the properties from another MenuItem
        /// </summary>
        /// <param name="mi">The MenuItem to copy from</param>
        public virtual void CopyLayoutFrom(MenuItem mi)
        {
            this.Size = mi.Size;
            this.Colour = mi.Colour;
            this.SelectedColour = mi.SelectedColour;
            this.TextAlign = mi.TextAlign;
            this.TextVAlign = mi.TextVAlign;
            this.TextOffset = mi.TextOffset;
            this.OnClick += mi.OnClick;
            this.OnDeselected += mi.OnDeselected;
            this.OnSelected += mi.OnSelected;
        }

        /// <summary>
        /// Position this menuItem inside a screen
        /// </summary>
        /// <param name="sM">The screenmanager</param>
        /// <param name="h">The horizontal alignment</param>
        /// <param name="v">The vertical alignment</param>
        /// <param name="margin">The margin</param>
        public virtual void PositionInside(ScreenManager sM, HorizontalAlign h, VerticalAlign v, float margin = 0)
        {
            float x = 0;
            float y = 0;

            if (h == HorizontalAlign.Left)
                x = margin;
            if (h == HorizontalAlign.CopyLeft)
                x = 0;
            if (h == HorizontalAlign.Center)
                x = (sM.ScreenResolution.X - this.Size.X) / 2;
            if (h == HorizontalAlign.CopyRight)
                x = sM.ScreenResolution.X - this.Size.X;
            if (h == HorizontalAlign.Right)
                x = sM.ScreenResolution.X - this.Size.X - margin;

            if (v == VerticalAlign.Top)
                y = margin;
            if (v == VerticalAlign.CopyTop)
                y = 0;
            if (v == VerticalAlign.Middle)
                y = (sM.ScreenResolution.Y - this.Size.Y) / 2;
            if (v == VerticalAlign.CopyBottom)
                y = sM.ScreenResolution.Y - this.Size.Y;
            if (v == VerticalAlign.Bottom)
                y = sM.ScreenResolution.Y - this.Size.Y - margin;

            this.Position = new Vector2(x, y);
        }

        public virtual void PositionInsideMenuScreen(MenuScreen menu, HorizontalAlign h, VerticalAlign v, float margin = 0)
        {
            float x = 0;
            float y = 0;

            if (h == HorizontalAlign.Left)
                x = menu.MenuOffset.X + margin;
            if (h == HorizontalAlign.CopyLeft)
                x = menu.MenuOffset.X;
            if (h == HorizontalAlign.Center)
                x = menu.MenuOffset.X + (menu.MenuSize.X - this.Size.X) / 2;
            if (h == HorizontalAlign.CopyRight)
                x = menu.MenuSize.X - this.Size.X;
            if (h == HorizontalAlign.Right)
                x = menu.MenuOffset.X + menu.MenuSize.X - this.Size.X - margin;

            if (v == VerticalAlign.Top)
                y = menu.MenuOffset.Y + margin;
            if (v == VerticalAlign.CopyTop)
                y = menu.MenuOffset.Y;
            if (v == VerticalAlign.Middle)
                y = menu.MenuOffset.Y + (menu.MenuSize.Y - this.Size.Y) / 2;
            if (v == VerticalAlign.CopyBottom)
                y = menu.MenuOffset.Y + menu.MenuSize.Y - this.Size.Y;
            if (v == VerticalAlign.Bottom)
                y = menu.MenuOffset.Y + menu.MenuSize.Y - this.Size.Y - margin;

            this.Position = new Vector2(x, y);
        }

        /// <summary>
        /// Position this menuItem relative to another menuItem
        /// </summary>
        /// <param name="sM">The screenmanager</param>
        /// <param name="h">The horizontal alignment</param>
        /// <param name="v">The vertical alignment</param>
        /// <param name="margin">The margin</param>
        public virtual void PositionFrom(MenuItem menuItem, HorizontalAlign h, VerticalAlign v, float margin = 0)
        {
            float x = 0;
            float y = 0;

            if (h == HorizontalAlign.Left)
                x = menuItem.Position.X - this.Size.X - margin;
            if (h == HorizontalAlign.CopyLeft)
                x = menuItem.Position.X;
            if (h == HorizontalAlign.Center)
                x = menuItem.Position.X + (menuItem.Size.X - this.Size.X) / 2;
            if (h == HorizontalAlign.CopyRight)
                x = menuItem.Position.X + menuItem.Size.X - this.Size.X;
            if (h == HorizontalAlign.Right)
                x = menuItem.Position.X + menuItem.Size.X + margin;

            if (v == VerticalAlign.Top)
                y = menuItem.Position.Y - this.Size.Y - margin;
            if (v == VerticalAlign.CopyTop)
                y = menuItem.Position.Y;
            if (v == VerticalAlign.Middle)
                y = menuItem.Position.Y + (menuItem.Size.Y - this.Size.Y) / 2;
            if (v == VerticalAlign.CopyBottom)
                y = menuItem.Position.Y + menuItem.Size.Y - this.Size.Y;
            if (v == VerticalAlign.Bottom)
                y = menuItem.Position.Y + menuItem.Size.Y + margin;

            this.Position = new Vector2(x, y);
        }

        // Methods to manage the behaviour
        public virtual void Update(UpdateEventArgs e) { }

        public virtual void Draw(UpdateEventArgs e)
        {
            Color backupColor = this.Graphics.MenuFontGeometry.Color;
            float backupHeight = this.Graphics.MenuFontGeometry.Height;
            this.Graphics.MenuFontGeometry.Color = this.isSelected ? this.SelectedColour : this.Colour;
            this.Graphics.MenuFontGeometry.Height = this.FontSize;
            this.Graphics.MenuFontGeometry.DrawString(this.Position + this.alignmentOffset(), this.DrawnText, this.alignmentValue());
            this.Graphics.MenuFontGeometry.Color = backupColor;
            this.Graphics.MenuFontGeometry.Height = backupHeight;
        }

        /// <summary>
        /// Deselect this menuItem and send a deselected event
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public virtual void Deselect(object o, EventArgs e)
        {
            // Deselect myself
            this.isSelected = false;

            // Send events
            if (this.OnDeselected != null)
                this.OnDeselected(o, e);
        }
        /// <summary>
        /// Select this menuItem and send a selected event
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public virtual void Select(object o, EventArgs e)
        {
            // Select myself
            this.isSelected = true;

            // Send events
            if (this.OnSelected != null)
                this.OnSelected(o, e);
        }
        /// <summary>
        /// Click this menuItem and send a click event
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public virtual void Click(object o, EventArgs e)
        {
            // Send events
            if (this.OnClick != null)
                this.OnClick(o, e);
        }
        /// <summary>
        /// Richt-mouse-click this menuItem and send a normal click event
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        public virtual void RightMouseClick(object o, EventArgs e)
        {
            // Send events
            if (this.OnClick != null)
                this.OnClick(o, e);
        }

        private Vector2 alignmentOffset()
        {
            switch (this.TextAlign) {
            case HorizontalAlign.CopyLeft:
                return this.TextOffset;
            case HorizontalAlign.Left:
                return this.TextOffset;
            case HorizontalAlign.Center:
                return this.TextOffset + 0.5f * this.Size.X * Vector2.UnitX;
            case HorizontalAlign.Right:
                return this.TextOffset + this.Size.X * Vector2.UnitX;
            case HorizontalAlign.CopyRight:
                return this.TextOffset + this.Size.X * Vector2.UnitX;
            }
            throw new Exception("Unknown alignment");
        }

        private float alignmentValue()
        {
            // Return the alignment float used in the drawstring method
            switch (this.TextAlign) {
            case HorizontalAlign.CopyLeft:
                return 0;
            case HorizontalAlign.Left:
                return 0;
            case HorizontalAlign.Center:
                return 0.5f;
            case HorizontalAlign.Right:
                return 1;
            case HorizontalAlign.CopyRight:
                return 1;
            }
            throw new Exception("Unknown alignment");
        }
    }
}
