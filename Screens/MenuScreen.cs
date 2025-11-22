using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EscapeRoom.Core;
using EscapeRoom.UI;

namespace EscapeRoom.Screens
{
    public class MenuScreen : Screen
    {
        UIButton _startBtn;

        public override void OnPush()
        {
            var vp = Device.Viewport;
            var txtSize = Assets.Fuente.MeasureString("Start");
            var btnRect = new Rectangle(
                (int)((vp.Width - txtSize.X - 80) / 2f),
                (int)(vp.Height * 0.60f),
                (int)(txtSize.X + 80),
                (int)(txtSize.Y + 20)
            );
            _startBtn = new UIButton(btnRect, "Start");
        }

        public override void Update(GameTime gt)
        {
            if (_startBtn.Update())
                ScreenManager.Replace(new IntroScreen()); // placeholder
        }

        public override void Draw(GameTime gt)
        {
            var vp = Device.Viewport;
            Batcher.Begin();

            // Fondo rojo manu.png escalado a alto, centrado horizontalmente
            var dst = DrawUtil.ScaleToHeight(Assets.Manu, vp.Width, vp.Height);
            Batcher.Draw(Assets.Manu, dst, Color.White);

            // Título con fuente grande
            string titulo = "Nightmare: Evans Mind";
            var size = Assets.FuenteTitle.MeasureString(titulo);
            var pos = new Vector2((vp.Width - size.X) / 2f, (int)(vp.Height * 0.20f));
            Batcher.DrawString(Assets.FuenteTitle, titulo, pos, Color.White);

            _startBtn.Draw(Batcher);

            Batcher.End();
        }
    }
}