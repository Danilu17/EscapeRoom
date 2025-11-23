using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EscapeRoom.Core;
using EscapeRoom.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace EscapeRoom.Screens
{
    public class SalaScreen : Screen
    {
        Evan _evan;
        readonly List<Rectangle> _solids = new();
        bool _debug;

        Rectangle _dstBackground;
        Rectangle _dstPiso, _dstBorde;
        Rectangle _dstReloj, _dstCuadrosL, _dstCuadrosR;
        Rectangle _doorBottomRect;
        float _scale;

        readonly float _entryX;
        readonly float _evanScale;

        // -----------------------------------------
        // SPAWN EN LA SALA — Evan pegado a la puerta
        // -----------------------------------------
        public static float SpawnOffsetX_Sala = 0f;
        public static float SpawnOffsetY_Sala = 100f;

        public SalaScreen(float entryX, float evanScale)
        {
            _entryX = entryX;
            _evanScale = evanScale;
        }

        public override void OnPush()
        {
            var vp = Device.Viewport;

            // Fondo
            _dstBackground = ScaleToHeight(Assets.Fondo3P, vp.Width, vp.Height);

            // Piso y borde
            _dstPiso = ScaleToHeight(Assets.SalaPiso, vp.Width, vp.Height);
            _dstBorde = ScaleToHeight(Assets.SalaBorde, vp.Width, vp.Height);
            _scale = (float)_dstPiso.Height / Assets.SalaPiso.Height;

            // RELOJ Y CUADROS
            float cuadroFactor = 0.22f;
            float relojFactor = 0.45f;

            var relojSize = new Point(
                (int)(Assets.Reloj.Width * _scale * relojFactor),
                (int)(Assets.Reloj.Height * _scale * relojFactor));

            var cuadros1Size = new Point(
                (int)(Assets.Cuadros1.Width * _scale * cuadroFactor),
                (int)(Assets.Cuadros1.Height * _scale * cuadroFactor));

            var cuadros2Size = new Point(
                (int)(Assets.Cuadros2.Width * _scale * cuadroFactor),
                (int)(Assets.Cuadros2.Height * _scale * cuadroFactor));

            _dstReloj = new Rectangle(
                _dstPiso.Center.X - relojSize.X / 1,
                _dstPiso.Top + (int)(40 * _scale),
                relojSize.X, relojSize.Y);

            _dstCuadrosL = new Rectangle(
                _dstPiso.Left + (int)(200 * _scale),
                _dstPiso.Top + (int)(20 * _scale),
                cuadros1Size.X, cuadros1Size.Y);

            _dstCuadrosR = new Rectangle(
                _dstPiso.Right - (int)(800 * _scale),
                _dstPiso.Top + (int)(40 * _scale),
                cuadros2Size.X, cuadros2Size.Y);

            // COLLIDERS
            SalaColliders.Build(_dstBorde, _scale, _solids, out _doorBottomRect);

            // SPAWN EVAN — pegado a la puerta inferior
            var evTex = Assets.EvanFrente1;
            int evW = (int)(evTex.Width * _evanScale);
            int evH = (int)(evTex.Height * _evanScale);

            float x = _entryX - evW / 2f + SpawnOffsetX_Sala;
            float y = _doorBottomRect.Y - evH + SpawnOffsetY_Sala;

            _evan = new Evan(new Vector2(x, y))
            {
                Scale = _evanScale,
                Speed = 140f
            };
        }

        static Rectangle ScaleToHeight(Texture2D tex, int viewportW, int viewportH)
        {
            float scale = (float)viewportH / tex.Height;
            int w = (int)(tex.Width * scale);
            int x = (viewportW - w) / 2;
            return new Rectangle(x, 0, w, viewportH);
        }

        public override void Update(GameTime gt)
        {
            if (Input.KeyPressed(Keys.F1)) _debug = !_debug;

            _evan.Update(gt, _solids);

            // PASAR AL CUARTO
            var evCenter = EvanCenter();
            if (_doorBottomRect.Contains(evCenter))
            {
                ScreenManager.Replace(new BedroomScreen(evCenter.X, _evan.Scale));
                return;
            }
        }

        Point EvanCenter()
        {
            int evW = (int)(Assets.EvanFrente1.Width * _evan.Scale);
            int evH = (int)(Assets.EvanFrente1.Height * _evan.Scale);
            return new Point((int)(_evan.Pos.X + evW / 2f), (int)(_evan.Pos.Y + evH / 2f));
        }

        public override void Draw(GameTime gt)
        {
            Batcher.Begin();

            Batcher.Draw(Assets.Fondo3P, _dstBackground, Color.White);
            Batcher.Draw(Assets.SalaPiso, _dstPiso, Color.White);
            Batcher.Draw(Assets.SalaBorde, _dstBorde, Color.White);

            Batcher.Draw(Assets.Cuadros1, _dstCuadrosL, Color.White);
            Batcher.Draw(Assets.Cuadros2, _dstCuadrosR, Color.White);
            Batcher.Draw(Assets.Reloj, _dstReloj, Color.White);

            _evan.Draw(Batcher);

            if (_debug)
            {
                foreach (var r in _solids)
                    Batcher.Draw(Assets.Pixel, r, new Color(255, 0, 0, 90));

                Batcher.Draw(Assets.Pixel, _doorBottomRect, new Color(0, 255, 0, 60));
            }

            Batcher.End();
        }
    }
}
