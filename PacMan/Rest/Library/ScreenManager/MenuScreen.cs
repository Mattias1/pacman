using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Input;
using amulware.Graphics;

namespace PacMan
{
    public abstract class MenuScreen : Screen
    {
        private List<MenuItem>[] menuItems;
        public int NrOfColumns
        {
            get { return this.menuItems.Length; }
            set
            {
                if (value <= 0)
                    throw new Exception("Value must be larger than 0");
                // Copy the existing menuItem lists to the new array, and use the new array as menuItems list
                List<MenuItem>[] items = new List<MenuItem>[value];
                for (int i = 0; i < value; i++)
                    if (i < this.menuItems.Length)
                        items[i] = this.menuItems[i];
                    else
                        items[i] = new List<MenuItem>();
                this.menuItems = items;
            }
        }
        public Vector2 MenuOffset;
        public Vector2 MenuSize;
        public int SelectedColumn { get; protected set; }
        public int SelectedRow { get; protected set; }
        /// <summary>
        /// If true, up and down keys will only affect the selected row, and right and left keys will only affect the selected column.
        /// If false, up is the same as left, down the same as right, and by going down in the last row, you will go to the next column.
        /// </summary>
        public bool HorizontalAndVerticalArrowKeys;
        /// <summary>
        /// The background sprite
        /// </summary>
        protected Texture backgroundSprite;

        public MenuScreen(ScreenManager sM, bool visible = true, bool transparent = false)
            : base(sM, visible, transparent)
        {
            this.menuItems = new List<MenuItem>[1];
            for (int i = 0; i < this.menuItems.Length; i++)
                this.menuItems[i] = new List<MenuItem>();
            this.MenuOffset = Vector2.Zero;
            this.MenuSize = this.screenManager.ScreenResolution;
            this.SelectedColumn = 0;
            this.SelectedRow = 0;
        }

        public override void Update(UpdateEventArgs e)
        {
            base.Update(e);

            // Get the old selected position
            int oldCol = this.SelectedColumn;
            int oldRow = this.SelectedRow;

            // Update which menuItem is selected for the keyboard
            if (InputManager.Keys.Hit(Key.Right))
                if (this.HorizontalAndVerticalArrowKeys)
                    this.SelectedColumn++;
                else
                    this.SelectedRow++;
            if (InputManager.Keys.Hit(Key.Left))
                if (this.HorizontalAndVerticalArrowKeys)
                    this.SelectedColumn--;
                else
                    this.SelectedRow--;
            if (InputManager.Keys.Hit(Key.Up))
                this.SelectedRow--;
            if (InputManager.Keys.Hit(Key.Down))
                this.SelectedRow++;
            this.correctSelectedValues();

            // Update which menuItem is selected for the mouse
            if (InputManager.MouseMoved()) {
                for (int c = 0; c < this.menuItems.Length; c++)
                    for (int r = 0; r < this.menuItems[c].Count; r++)
                        if (InputManager.IsMouseInRectangle(this.menuItems[c][r].HitBox)) {
                            this.SelectedColumn = c;
                            this.SelectedRow = r;
                        }
            }

            // Send the events
            if (oldCol != this.SelectedColumn || oldRow != this.SelectedRow) {
                if (this.menuItems[oldCol].Count > 0)
                    this.menuItems[oldCol][oldRow].Deselect(this, new EventArgs());
                if (this.menuItems[this.SelectedColumn].Count > 0)
                    this.menuItems[this.SelectedColumn][this.SelectedRow].Select(this, new EventArgs());
            }
            bool mouseInRect = false;
            if (this.menuItems[this.SelectedColumn].Count > 0)
                mouseInRect = InputManager.IsMouseInRectangle(this.menuItems[this.SelectedColumn][this.SelectedRow].HitBox);
            if (InputManager.Keys.Hit(Key.Enter) || InputManager.Keys.Hit(Key.Space) || (InputManager.IsLeftMouseHit() && mouseInRect)) {
                if (this.menuItems[this.SelectedColumn].Count > 0)
                    this.menuItems[this.SelectedColumn][this.SelectedRow].Click(this, new EventArgs());
            }
            if (InputManager.IsRightMouseHit() && mouseInRect) {
                if (this.menuItems[this.SelectedColumn].Count > 0)
                    this.menuItems[this.SelectedColumn][this.SelectedRow].RightMouseClick(this, new EventArgs());
            }

            // Update all menuItems
            foreach (List<MenuItem> list in this.menuItems) {
                foreach (MenuItem item in list)
                    item.Update(e);
            }
        }

        public override void Draw(UpdateEventArgs e)
        {
            // Draw the background
            //if (this.backgroundSprite != null)
            //    spriteBatch.Draw(this.backgroundSprite, Vector2.Zero, Color.White);

            // Draw all menuItems
            foreach (List<MenuItem> list in this.menuItems)
                foreach (MenuItem item in list)
                    item.Draw(e);
        }

        /// <summary>
        /// This method gets executed (just before) the first update call
        /// </summary>
        public override void OnLoad()
        {
            // The firstrun
            if (this.menuItems[this.SelectedColumn].Count > 0)
                this.menuItems[this.SelectedColumn][this.SelectedRow].Select(this, new EventArgs());
        }

        /// <summary>
        /// Get the number of rows the current column has
        /// </summary>
        /// <param name="column">The column you want to check</param>
        /// <returns>The number of rows</returns>
        public int NrOfRows(int column = 0)
        {
            return this.menuItems[column].Count;
        }

        /// <summary>
        /// Get the menuItem at position (row, column)
        /// </summary>
        /// <param name="row">The row</param>
        /// <param name="column">The column</param>
        /// <returns></returns>
        public MenuItem GetMenuItem(int row, int column = 0)
        {
            return this.menuItems[column][row];
        }

        /// <summary>
        /// Add a menuItem
        /// </summary>
        /// <param name="item">The menuItem to add</param>
        /// <param name="column">The column to add it to</param>
        public void AddMenuItem(MenuItem item, int column = 0)
        {
            this.InsertMenuItem(item, this.menuItems[column].Count, column);
        }

        /// <summary>
        /// Insert a menuItem at a specifix index (row)
        /// </summary>
        /// <param name="item">The menuItem to add</param>
        /// <param name="row">The row (index) to insert it in</param>
        /// <param name="column">The column to add it to</param>
        public void InsertMenuItem(MenuItem item, int row, int column = 0)
        {
            this.menuItems[column].Insert(row, item);
        }

        /// <summary>
        /// Select a specific menuItem
        /// </summary>
        /// <param name="item">The menuItem to select</param>
        public void SelectMenuItem(MenuItem item)
        {
            int oldCol = this.SelectedColumn;
            int oldRow = this.SelectedRow;

            for (int c = 0; c < this.NrOfColumns; c++)
                for (int r = 0; r < this.NrOfRows(c); r++)
                    if (this.menuItems[c][r] == item) {
                        this.SelectedColumn = c;
                        this.SelectedRow = r;
                        c = this.NrOfColumns; // break both for loops
                        break;
                    }

            if (oldCol != this.SelectedColumn || oldRow != this.SelectedRow) {
                if (this.menuItems[oldCol].Count > 0)
                    this.menuItems[oldCol][oldRow].Deselect(this, new EventArgs());
                item.Select(this, new EventArgs());
            }
        }

        /// <summary>
        /// Position the menu inside the screen and return the resulting Vector2
        /// </summary>
        /// <param name="align">The horizontal alignment</param>
        /// <param name="valign">The vertical alignment</param>
        public Vector2 PositionMenuScreen(MenuItem.HorizontalAlign align, MenuItem.VerticalAlign valign)
        {
            // Top and left
            Vector2 result = Vector2.Zero;
            // Center and middle
            if (align == MenuItem.HorizontalAlign.Center)
                result.X = (this.screenManager.ScreenResolution.X - this.MenuSize.X) / 2;
            if (valign == MenuItem.VerticalAlign.Middle)
                result.Y = (this.screenManager.ScreenResolution.Y - this.MenuSize.Y) / 2;
            // Bottom and right
            if (align == MenuItem.HorizontalAlign.Right)
                result.X = this.screenManager.ScreenResolution.X - this.MenuSize.X;
            if (valign == MenuItem.VerticalAlign.Bottom)
                result.Y = this.screenManager.ScreenResolution.Y - this.MenuSize.Y;
            // Position the menuScreen and return the result
            this.MenuOffset = result;
            return result;
        }

        /// <summary>
        /// Orders the menuItems in this menuScreen in a table fashion, using the style of one menuItem.
        /// </summary>
        /// <param name="layoutMenuItem">The menuItem to copy the properties from (set null to not copy properties)</param>
        /// <param name="calculateNewSize">Whether or not to override the menuItem's size</param>
        /// <param name="menuItemHeight">The height part of the menuItem's size</param>
        public void FastLayout(MenuItem layoutMenuItem, float margin = 10)
        {
            for (int column = 0; column < this.menuItems.Length; column++)
                for (int row = 0; row < this.menuItems[column].Count; row++) {
                    MenuItem menuItem = this.menuItems[column][row];
                    // Set the position
                    if (row == 0 && column == 0)
                        menuItem.PositionInsideMenuScreen(this, MenuItem.HorizontalAlign.Left, MenuItem.VerticalAlign.Top, 0);
                    else if (row == 0)
                        menuItem.PositionFrom(this.GetMenuItem(row, column - 1), MenuItem.HorizontalAlign.Right, MenuItem.VerticalAlign.CopyTop, margin);
                    else
                        menuItem.PositionFrom(this.GetMenuItem(row - 1, column), MenuItem.HorizontalAlign.CopyLeft, MenuItem.VerticalAlign.Bottom, margin);
                    // Set the layout
                    menuItem.CopyLayoutFrom(layoutMenuItem);
                }
        }

        /// <summary>
        /// Calculate the size for a new menuItem, based on the nr of columns the menu has
        /// </summary>
        /// <param name="item">The menuItem</param>
        /// <returns>The size</returns>
        protected virtual Vector2 calculateMenuItemSize(float height = 30)
        {
            return new Vector2(this.MenuSize.X / this.NrOfColumns, height);
        }

        /// <summary>
        /// Correct the selected column and row values to a valid range (seeing it as a torus)
        /// </summary>
        protected void correctSelectedValues()
        {
            // If there is a difference in up and left keys
            if (this.HorizontalAndVerticalArrowKeys || this.NrOfColumns == 1) {
                // Correct the selected column
                while (this.SelectedColumn < 0)
                    this.SelectedColumn += this.menuItems.Length;
                while (this.SelectedColumn >= this.menuItems.Length)
                    this.SelectedColumn -= this.menuItems.Length;

                // Correct the selected row
                if (this.SelectedRow >= this.menuItems[this.SelectedColumn].Count)
                    this.SelectedRow = 0;
                if (this.SelectedRow < 0)
                    this.SelectedRow = this.menuItems[this.SelectedColumn].Count - 1;
            }
            // If there is no difference between up and left keys
            else {
                if (this.SelectedRow < 0) {
                    // Adjust the column
                    this.SelectedColumn--;
                    while (this.SelectedColumn < 0)
                        this.SelectedColumn += this.menuItems.Length;
                    // Adust the row
                    this.SelectedRow = this.menuItems[this.SelectedColumn].Count - 1;
                }
                if (this.SelectedRow >= this.menuItems[this.SelectedColumn].Count) {
                    // Adjust the column
                    this.SelectedColumn++;
                    while (this.SelectedColumn >= this.menuItems.Length)
                        this.SelectedColumn -= this.menuItems.Length;
                    // Adjust the row
                    this.SelectedRow = 0;
                }
            }
        }

        /// <summary>
        /// Correct all positions from all menuItems
        /// </summary>
        protected void correctMenuItemPositions()
        {
            for (int col = 0; col < this.menuItems.Length; col++)
                for (int row = 0; row < this.menuItems[col].Count; row++) {
                    MenuItem item = this.GetMenuItem(row, col);
                    // Set the position
                    if (row == 0) {
                        item.Position = this.MenuOffset + new Vector2(item.Size.X * col, 0);
                    }
                    else {
                        MenuItem before = this.GetMenuItem(row - 1, col);
                        item.Position = before.Position + new Vector2(0, before.Size.Y);
                    }
                }
        }
        /// <summary>
        /// Correct all sizes from all menuItems
        /// </summary>
        protected void correctMenuItemSizes()
        {
            for (int col = 0; col < this.menuItems.Length; col++)
                for (int row = 0; row < this.menuItems[col].Count; row++) {
                    MenuItem item = this.GetMenuItem(row, col);
                    item.Size = this.calculateMenuItemSize();
                }
        }
    }
}
