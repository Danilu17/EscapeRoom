using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeRoom.Core
{
    public static class DrawUtil
    {
        public static Rectangle ScaleToHeight(Texture2D tex, int viewportW, int viewportH)
        {
            float scale = (float)viewportH / tex.Height;
            int w = (int)(tex.Width * scale);
            int x = (viewportW - w) / 2;
            return new Rectangle(x, 0, w, viewportH);
        }

        public static void FillRect(SpriteBatch sb, Rectangle r, Color c)
            => sb.Draw(Assets.Pixel, r, c);

        public static Vector2 CenteredTextPos(SpriteFont font, string text, int viewW, int y)
        {
            var size = font.MeasureString(text);
            return new Vector2((viewW - size.X) / 2f, y);
        }
    }
}
