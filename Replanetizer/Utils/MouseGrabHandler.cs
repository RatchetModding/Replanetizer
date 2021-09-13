using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Replanetizer.Utils
{
    public class MouseGrabHandler
    {
        /// <returns>whether the user is holding right click to rotate</returns>
        public bool TryGrabMouse(Window wnd, MouseButton mouseButton)
        {
            var isDown = wnd.MouseState.IsButtonDown(mouseButton);
            var wasDown = wnd.MouseState.WasButtonDown(mouseButton);

            if (!isDown)
            {
                if (wasDown)
                {
                    // Released right click; unhide the cursor
                    wnd.CursorVisible = true;
                    wnd.CursorGrabbed = false;
                }
                return false;
            }

            if (!wasDown)
            {
                // Began pressing right click; hide the cursor and allow it to
                // move without any bounds
                wnd.CursorVisible = false;
                wnd.CursorGrabbed = true;
            }

            return true;
        }
    }
}
