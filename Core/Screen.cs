using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeRoom.Core
{
    public abstract class Screen
    {
        protected GraphicsDevice Device => ScreenManager.Game.GraphicsDevice;
        protected SpriteBatch Batcher => ScreenManager.Batcher;

        public virtual void OnPush() { }
        public virtual void OnPop() { }
        public virtual void Update(GameTime gt) { }
        public virtual void Draw(GameTime gt) { }
    }
}