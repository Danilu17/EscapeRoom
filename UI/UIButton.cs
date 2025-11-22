using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeRoom.Core;

namespace EscapeRoom.UI
{
    public class UIButton
    {
        public Rectangle Bounds;
        public string Text;
        public Color Bg = Color.Black;
        public Color Fg = Color.White;
        public SpriteFont Font; // Por qué: controlar tamaño desde afuera

        public UIButton(Rectangle bounds, string text)
        {
            Bounds = bounds; Text = text;
        }

        public bool Update()
        {
            var (mx, my) = Input.MousePos;
            bool hover = Bounds.Contains(mx, my);
            if (hover && Input.LeftClick) return true;
            if (Input.KeyPressed(Keys.Enter)) return true;
            return false;
        }

        public void Draw(SpriteBatch sb)
        {
            DrawUtil.FillRect(sb, Bounds, Bg);
            var size = Assets.Fuente.MeasureString(Text);
            var pos = new Vector2(
                Bounds.X + (Bounds.Width - size.X) / 2f,
                Bounds.Y + (Bounds.Height - size.Y) / 2f
            );
            sb.DrawString(Assets.Fuente, Text, pos, Fg);
        }
    }
}