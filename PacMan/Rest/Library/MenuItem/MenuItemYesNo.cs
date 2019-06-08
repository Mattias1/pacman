using System;
using System.Collections.Generic;
using OpenTK;
using amulware.Graphics;

namespace PacMan
{
    public class MenuItemYesNo : MenuItemList
    {
        /// <summary>
        /// A menuItemList with possibilities Yes and No
        /// </summary>
        /// <param name="text">The description text.</param>
        public MenuItemYesNo(string text, Graphics graphics)
            : base(text, new List<string>(2) { "Yes", "No" }, graphics) {

        }
    }
}
