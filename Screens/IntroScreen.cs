using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EscapeRoom.Core;
using Microsoft.Xna.Framework.Input;

namespace EscapeRoom.Screens
{
    public class IntroScreen : Screen
    {
        // Frase EXACTA del requerimiento:
        private const string Linea = "Alguna vez tuviste un suenio tan real... que dolia?";

        private string _mostrado = string.Empty;
        private double _acum;
        private const double Velocidad = 0.040; // seg por letra (~25 cps)

        public override void Update(GameTime gt)
        {
            // Typewriter
            _acum += gt.ElapsedGameTime.TotalSeconds;
            while (_acum >= Velocidad && _mostrado.Length < Linea.Length)
            {
                _acum -= Velocidad;
                _mostrado = Linea.Substring(0, _mostrado.Length + 1);
            }

            // Enter SOLO cuando terminó de escribirse
            if (_mostrado.Length == Linea.Length && Input.KeyPressed(Keys.Enter))
            {
                ScreenManager.Replace(new BedroomScreen()); // siguiente paso (placeholder)
            }
        }

        public override void Draw(GameTime gt)
        {
            var vp = Device.Viewport;
            Batcher.Begin();

            // Fondo negro pleno
            DrawUtil.FillRect(Batcher, new Rectangle(0, 0, vp.Width, vp.Height), Color.Black);

            // Texto blanco centrado (X y Y)
            var text = _mostrado;
            var size = Assets.Fuente.MeasureString(text);
            var pos = new Vector2(
                (vp.Width - size.X) / 2f,
                (vp.Height - size.Y) / 2f
            );
            Batcher.DrawString(Assets.Fuente, text, pos, Color.White);

            Batcher.End();
        }
    }
}