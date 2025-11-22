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
    public class DialogueBox
    {
        private readonly Queue<string> _queue = new();
        private readonly SpriteFont _font;
        private readonly HashSet<char> _glyphs;        // Por qué: detectar faltantes sin exceptions
        private readonly int _margin = 18;
        private readonly int _padding = 14;
        private readonly char _fallback;

        public bool Visible => _queue.Count > 0;

        public DialogueBox(SpriteFont font, char fallback = '*')
        {
            _font = font;
            _fallback = fallback;
            _glyphs = new HashSet<char>(_font.Characters); // cache caracteres disponibles
        }

        public void Enqueue(params string[] lines)
        {
            foreach (var l in lines)
            {
                if (string.IsNullOrWhiteSpace(l)) continue;
                _queue.Enqueue(l);
            }
        }

        public bool Update()
        {
            if (!Visible) return true;
            if (Input.KeyPressed(Keys.Enter))
            {
                _queue.Dequeue();
                return !Visible;
            }
            return false;
        }

        public void Draw(SpriteBatch sb, Viewport vp)
        {
            if (!Visible) return;

            string raw = _queue.Peek();
            string text = Sanitize(raw);  // <- evita crash por glyphs faltantes

            int boxWidth = (int)(vp.Width * 0.88f);
            int boxHeight = (int)(vp.Height * 0.22f);
            int x = (vp.Width - boxWidth) / 2;
            int y = vp.Height - boxHeight - _margin;

            // Fondo
            sb.Draw(Assets.Pixel, new Rectangle(x, y, boxWidth, boxHeight), new Color(0, 0, 0, 180));

            // Texto envuelto
            string wrapped = WrapText(_font, text, boxWidth - _padding * 2);
            var pos = new Vector2(x + _padding, y + _padding);
            sb.DrawString(_font, wrapped, pos, Color.White);

            // Hint
            const string hint = "Oprima Enter para continuar";
            var hintSize = _font.MeasureString(hint);
            var hintPos = new Vector2(x + boxWidth - _padding - hintSize.X, y + boxHeight - _padding - hintSize.Y);
            sb.DrawString(_font, hint, hintPos, Color.White * 0.85f);
        }

        private string Sanitize(string s)
        {
            // Por qué: MonoGame lanza excepción si el SpriteFont no contiene un glifo al medir/dibujar.
            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
                sb.Append(_glyphs.Contains(ch) ? ch : _fallback);
            return sb.ToString();
        }

        private static string WrapText(SpriteFont font, string text, float maxLineWidth)
        {
            var result = new StringBuilder();
            foreach (var line in text.Replace("\r", "").Split('\n'))
            {
                string current = "";
                foreach (var word in line.Split(' '))
                {
                    string test = string.IsNullOrEmpty(current) ? word : current + " " + word;
                    if (font.MeasureString(test).X <= maxLineWidth)
                        current = test;
                    else
                    {
                        if (current.Length > 0) result.AppendLine(current);
                        current = word;
                    }
                }
                result.AppendLine(current);
            }
            return result.ToString().TrimEnd();
        }
    }
}
