// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Replanetizer.Utils
{
    public class MouseGrabHandler
    {
        private bool isGrabbed;
        /// <summary>
        /// the mouse button to check for being held
        /// </summary>
        public MouseButton mouseButton { get; set; }

        /// <summary>
        /// Check that the mouse button is being held and grab the cursor.
        /// Grabbing will hide the cursor and allow it to move freely beyond
        /// the bounds of the window/screen.
        /// </summary>
        /// <param name="wnd">the window</param>
        /// <param name="allowNewGrab">whether a new click will begin grabbing</param>
        /// <returns>whether the cursor is being grabbed</returns>
        public bool TryGrabMouse(Window wnd, bool allowNewGrab)
        {
            var isDown = wnd.MouseState.IsButtonDown(mouseButton);
            var wasDown = wnd.MouseState.WasButtonDown(mouseButton);

            if (!isDown)
            {
                if (wasDown && isGrabbed)
                {
                    // Released click while cursor was being grabbed; unhide
                    // the cursor
                    isGrabbed = false;
                    wnd.CursorState = OpenTK.Windowing.Common.CursorState.Normal;
                }
                return false;
            }

            if (!wasDown)
            {
                // Began pressing click
                if (!allowNewGrab)
                    return false;

                // Hide the cursor and allow it to move without any bounds
                isGrabbed = true;
                wnd.CursorState = OpenTK.Windowing.Common.CursorState.Grabbed;
            }

            return isGrabbed;
        }
    }
}
