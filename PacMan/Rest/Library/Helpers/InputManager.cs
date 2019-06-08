using System;
using System.Drawing;
using System.Collections.Generic;
using amulware.Graphics;
using OpenTK;
using OpenTK.Input;

namespace PacMan
{
    /// <summary>
    /// The class for managing input.
    /// </summary>
    public static class InputManager
    {
        private static Program gameProgram;
        public static bool IgnoreWhenInactive = true;
        // Keyboard management
        public static KeyboardManager Keys { get; private set; }
        // Manual mouse management
        static Vector2 mPos, mPosOld;
        static bool leftDown, leftPressed, leftUp, rightDown, rightPressed, rightUp;
        static bool leftDownEvt, leftUpEvt, rightDownEvt, rightUpEvt;

        internal static void Initialize(Program program)
        {
            gameProgram = program;
            Keys = new KeyboardManager();
            gameProgram.Mouse.ButtonDown += (o, e) => {
                if (e.Button == MouseButton.Left)
                    leftDownEvt = true;
                if (e.Button == MouseButton.Right)
                    rightDownEvt = true;
            };
            gameProgram.Mouse.ButtonUp += (o, e) => {
                if (e.Button == MouseButton.Left)
                    leftUpEvt = true;
                if (e.Button == MouseButton.Right)
                    rightUpEvt = true;
            };
        }

        internal static void Update()
        {
            // Update the keyboard manager
            Keys.Update(gameProgram.Keyboard);

            // Manually manage the mouse variables
            mPosOld = mPos;
            mPos = new Vector2(gameProgram.Mouse.X, gameProgram.Mouse.Y);
            if (leftDownEvt) {
                leftDown = true;
                leftPressed = true;
                leftUp = false;
                leftDownEvt = false;
            }
            else if (leftUpEvt) {
                leftDown = false;
                leftPressed = false;
                leftUp = true;
                leftUpEvt = false;
            }
            else {
                leftDown = false;
                leftUp = false;
            }
            if (rightDownEvt) {
                rightDown = true;
                rightPressed = true;
                rightUp = false;
                rightDownEvt = false;
            }
            else if (rightUpEvt) {
                rightDown = false;
                rightPressed = false;
                rightUp = true;
                rightUpEvt = false;
            }
            else {
                rightDown = false;
                rightUp = false;
            }
        }

        /// <summary>
        /// Whether or not any of the Left mouse button, Enter, Spacebar or Escape keys is hit.
        /// </summary>
        /// <param name="escape">Whther or not the escape key counts as confirm key</param>
        /// <returns></returns>
        public static bool IsConfirmHit(bool escape = true)
        {
            return IsLeftMouseHit() || Keys.Hit(Key.Enter) || Keys.Hit(Key.Space) || (Keys.Hit(Key.Escape) && escape);

        }

        #region Mouse methods
        public static Vector2 MousePosition()
        {
            return mPos;
        }
        public static bool MouseMoved()
        {
            return mPos.X != mPosOld.X || mPos.Y != mPosOld.Y;
        }
        public static bool IsMouseInRectangle(Rectangle rect)
        {
            return rect.Contains((int)mPos.X, (int)mPos.Y);
        }
        public static bool IsLeftMousePressed()
        {
            return leftPressed;
        }
        public static bool IsLeftMouseReleased()
        {
            return leftUp;
        }
        public static bool IsLeftMouseHit()
        {
            return leftDown;
        }
        public static bool IsRightMousePressed()
        {
            return rightPressed;
        }
        public static bool IsRightMouseReleased()
        {
            return rightUp;
        }
        public static bool IsRightMouseHit()
        {
            return rightDown;
        }
        //public static int DeltaScroll()
        //{
        //    return this
        //}
        #endregion Mouse methods
    }
}