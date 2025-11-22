using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeRoom.Core
{
    public static class ScreenManager
    {
        static readonly Stack<Screen> _stack = new();
        public static Game Game { get; private set; }
        public static SpriteBatch Batcher { get; private set; }

        public static void Initialize(Game game, SpriteBatch batch)
        {
            Game = game;
            Batcher = batch;
        }

        public static void Push(Screen s)
        {
            _stack.Push(s);
            s.OnPush();
        }

        public static void Replace(Screen s)
        {
            if (_stack.Count > 0) _stack.Pop().OnPop();
            Push(s);
        }

        public static void Update(GameTime gt)
        {
            if (_stack.Count > 0) _stack.Peek().Update(gt);
        }

        public static void Draw(GameTime gt)
        {
            if (_stack.Count > 0) _stack.Peek().Draw(gt);
        }
    }
}