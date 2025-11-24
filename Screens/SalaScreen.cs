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
    public enum SalaEntryPoint
    {
        FromBedroom,
        FromSister
    }

    public class SalaScreen : Screen
    {
        Evan _evan;
        readonly List<Rectangle> _solids = new();
        bool _debug;

        Rectangle _dstBackground;
        Rectangle _dstPiso, _dstBorde;
        Rectangle _dstReloj, _dstCuadro1, _dstCuadro2;

        Rectangle _doorBottomRect;  // hacia dormitorio Evan
        Rectangle _doorRightRect;   // hacia cuarto hermana

        float _scale;
        readonly SalaEntryPoint _entryFrom;
        readonly float _entryCoord;
        readonly float _evanScale;

        public static float SpawnOffsetX_Sala = 0f;
        public static float SpawnOffsetY_Sala = 100f;

        public SalaScreen(SalaEntryPoint entryFrom, float entryCoord, float evanScale)
        {
            _entryFrom = entryFrom;
            _entryCoord = entryCoord;
            _evanScale = evanScale;
        }

        public override void OnPush()
        {
            var vp = Device.Viewport;

            _dstBackground = ScaleToHeight(Assets.Fondo3P, vp.Width, vp.Height);

            _dstPiso = ScaleToHeight(Assets.SalaPiso, vp.Width, vp.Height);
            _dstBorde = ScaleToHeight(Assets.SalaBorde, vp.Width, vp.Height);
            _scale = (float)_dstPiso.Height / Assets.SalaPiso.Height;

            // --------- Objetos sala ---------
            float relojFactor = 0.45f;
            float cuadroFactor = 0.22f;

            var relojSize = new Point(
                (int)(Assets.Reloj.Width * _scale * relojFactor),
                (int)(Assets.Reloj.Height * _scale * relojFactor));

            var cuadro1Size = new Point(
                (int)(Assets.Cuadros1.Width * _scale * cuadroFactor),
                (int)(Assets.Cuadros1.Height * _scale * cuadroFactor));

            var cuadro2Size = new Point(
                (int)(Assets.Cuadros2.Width * _scale * cuadroFactor),
                (int)(Assets.Cuadros2.Height * _scale * cuadroFactor));

            _dstReloj = new Rectangle(
                _dstPiso.Center.X - relojSize.X / 1,
                _dstPiso.Top + (int)(40 * _scale),
                relojSize.X, relojSize.Y);

            _dstCuadro1 = new Rectangle(
                _dstPiso.Left + (int)(200 * _scale),
                _dstPiso.Top + (int)(20 * _scale),
                cuadro1Size.X, cuadro1Size.Y);

            _dstCuadro2 = new Rectangle(
                _dstPiso.Right - cuadro2Size.X - (int)(400 * _scale),
                _dstPiso.Top + (int)(40 * _scale),
                cuadro2Size.X, cuadro2Size.Y);

            // --------- Colliders ---------
            SalaColliders.Build(
                _dstBorde,
                _scale,
                _solids,
                out _doorBottomRect,
                out _doorRightRect
            );

            // -------- SPawn Evan --------
            var evTex = Assets.EvanFrente1;
            int evW = (int)(evTex.Width * _evanScale);
            int evH = (int)(evTex.Height * _evanScale);

            if (_entryFrom == SalaEntryPoint.FromBedroom)
            {
                float x = _entryCoord - evW / 2f + SpawnOffsetX_Sala;
                float y = _doorBottomRect.Y - evH + SpawnOffsetY_Sala;

                _evan = new Evan(new Vector2(x, y))
                {
                    Scale = _evanScale,
                    Speed = 140f
                };
            }
            else // FromSister
            {
                float y = _entryCoord - evH / 2f;
                float x = _doorRightRect.Left - evW - 5;

                _evan = new Evan(new Vector2(x, y))
                {
                    Scale = _evanScale,
                    Speed = 140f
                };
            }
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
            if (Input.KeyPressed(Keys.F1))
                _debug = !_debug;

            _evan.Update(gt, _solids);

            var evCenter = EvanCenter();

            if (_doorBottomRect.Contains(evCenter))
            {
                ScreenManager.Replace(new BedroomScreen(evCenter.X, _evan.Scale));
                return;
            }

            if (_doorRightRect.Contains(evCenter))
            {
                ScreenManager.Replace(new SisterRoomScreen(evCenter.Y, _evan.Scale));
                return;
            }
        }

        Point EvanCenter()
        {
            int w = (int)(Assets.EvanFrente1.Width * _evan.Scale);
            int h = (int)(Assets.EvanFrente1.Height * _evan.Scale);
            return new Point((int)(_evan.Pos.X + w / 2f), (int)(_evan.Pos.Y + h / 2f));
        }

        public override void Draw(GameTime gt)
        {
            Batcher.Begin();

            Batcher.Draw(Assets.Fondo3P, _dstBackground, Color.White);
            Batcher.Draw(Assets.SalaPiso, _dstPiso, Color.White);
            Batcher.Draw(Assets.SalaBorde, _dstBorde, Color.White);

            Batcher.Draw(Assets.Cuadros1, _dstCuadro1, Color.White);
            Batcher.Draw(Assets.Cuadros2, _dstCuadro2, Color.White);
            Batcher.Draw(Assets.Reloj, _dstReloj, Color.White);

            _evan.Draw(Batcher);

            if (_debug)
            {
                foreach (var r in _solids)
                    Batcher.Draw(Assets.Pixel, r, new Color(255, 0, 0, 90));

                Batcher.Draw(Assets.Pixel, _doorBottomRect, new Color(0, 255, 0, 90));
                Batcher.Draw(Assets.Pixel, _doorRightRect, new Color(0, 180, 0, 90));
            }

            Batcher.End();
        }
    }
}
