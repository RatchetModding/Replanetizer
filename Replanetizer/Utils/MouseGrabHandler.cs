using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Replanetizer.Utils
{
    public class MouseGrabHandler
    {
        private bool _isGrabbed;
        /// <summary>
        /// the mouse button to check for being held
        /// </summary>
        public MouseButton MouseButton { get; set; }

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
            var isDown = wnd.MouseState.IsButtonDown(MouseButton);
            var wasDown = wnd.MouseState.WasButtonDown(MouseButton);

            if (!isDown)
            {
                if (wasDown && _isGrabbed)
                {
                    // Released click while cursor was being grabbed; unhide
                    // the cursor
                    _isGrabbed = false;
                    wnd.CursorVisible = true;
                    wnd.CursorGrabbed = false;
                }
                return false;
            }

            if (!wasDown)
            {
                // Began pressing click
                if (!allowNewGrab)
                    return false;

                // Hide the cursor and allow it to move without any bounds
                _isGrabbed = true;
                wnd.CursorVisible = false;
                wnd.CursorGrabbed = true;
            }

            return _isGrabbed;
        }
    }
}
