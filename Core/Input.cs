using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace EscapeRoom.Core
{
    public static class Input
    {
        private static KeyboardState _kb, _kbPrev;
        private static MouseState _ms, _msPrev;

        public static void Update()
        {
            _kbPrev = _kb; _kb = Keyboard.GetState();
            _msPrev = _ms; _ms = Mouse.GetState();
        }

        // Teclado
        public static bool KeyDown(Keys k) => _kb.IsKeyDown(k);
        public static bool KeyUp(Keys k) => _kb.IsKeyUp(k);
        public static bool KeyPressed(Keys k) => _kb.IsKeyDown(k) && _kbPrev.IsKeyUp(k);      // por qué: detectar flanco
        public static bool KeyReleased(Keys k) => _kb.IsKeyUp(k) && _kbPrev.IsKeyDown(k);     // por qué: detectar soltar
        public static bool AnyKeyPressed() => _kb.GetPressedKeys().Length > 0 && _kbPrev.GetPressedKeys().Length == 0;

        // Mouse
        public static bool LeftClick => _ms.LeftButton == ButtonState.Pressed && _msPrev.LeftButton == ButtonState.Released;
        public static bool RightClick => _ms.RightButton == ButtonState.Pressed && _msPrev.RightButton == ButtonState.Released;
        public static Point MousePoint => new Point(_ms.X, _ms.Y);
        public static (int x, int y) MousePos => (_ms.X, _ms.Y);

        // Utilidades
        public static bool RequestExit => _kb.IsKeyDown(Keys.Escape);

        // Movimiento WASD normalizado
        public static Vector2 WasdVector()
        {
            int x = (_kb.IsKeyDown(Keys.D) ? 1 : 0) - (_kb.IsKeyDown(Keys.A) ? 1 : 0);
            int y = (_kb.IsKeyDown(Keys.S) ? 1 : 0) - (_kb.IsKeyDown(Keys.W) ? 1 : 0);
            var v = new Vector2(x, y);
            if (v.LengthSquared() > 1f) v.Normalize(); // por qué: velocidad constante en diagonal
            return v;
        }
    }
}
