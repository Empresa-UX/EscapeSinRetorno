using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace EscapeSinRetorno.Source.UI
{
    public static class InputManager
    {
        private static MouseState currentMouseState;
        private static MouseState previousMouseState;
        private static KeyboardState currentKeyboardState;
        private static KeyboardState previousKeyboardState;

        public static void Update()
        {
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
        }

        public static bool IsLeftMouseButtonPressed()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed &&
                   previousMouseState.LeftButton == ButtonState.Released;
        }

        public static Point MousePosition => currentMouseState.Position;

        public static bool IsKeyPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) &&
                   !previousKeyboardState.IsKeyDown(key);
        }
    }
}