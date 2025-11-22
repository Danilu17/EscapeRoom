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

        Rectangle _dstPiso, _dstBorde, _dstReloj, _dstCuadrosL, _dstCuadrosR;
        Rectangle _doorBottomRect; // hacia Bedroom
        float _scale;

        readonly float _entryX;
        readonly float _evanScale;

        public SalaScreen(float entryX, float evanScale)
        {
            _entryX = entryX; _evanScale = evanScale;
        }

        public override void OnPush()
        {
            var vp = Device.Viewport;

            _dstPiso = ScaleToHeight(Assets.SalaPiso, vp.Width, vp.Height);
            _dstBorde = ScaleToHeight(Assets.SalaBorde, vp.Width, vp.Height);
            _scale = (float)_dstPiso.Height / Assets.SalaPiso.Height;

            // Objetos (según tus referencias)
            var relojSize = new Point((int)(Assets.Reloj.Width * _scale), (int)(Assets.Reloj.Height * _scale));
            var cuadrosSize = new Point((int)(Assets.Cuadros.Width * _scale), (int)(Assets.Cuadros.Height * _scale));

            _dstReloj = new Rectangle(_dstPiso.Center.X - relojSize.X / 2, _dstPiso.Top + (int)(18 * _scale), relojSize.X, relojSize.Y);
            _dstCuadrosL = new Rectangle(_dstPiso.Left + (int)(80 * _scale), _dstPiso.Top + (int)(30 * _scale), cuadrosSize.X, cuadrosSize.Y);
            _dstCuadrosR = new Rectangle(_dstPiso.Center.X + (int)(180 * _scale), _dstPiso.Top + (int)(24 * _scale), cuadrosSize.X, cuadrosSize.Y);

            // Evan: aparece desde abajo manteniendo X y “emerge”
            var evTex = Assets.EvanFrente1;
            var evW = (int)(evTex.Width * _evanScale);
            var evH = (int)(evTex.Height * _evanScale);
            float x = _entryX - evW / 2f;
            float y = _dstBorde.Bottom + 6; // fuera de vista (debajo)
            _evan = new Evan(new Vector2(x, y)) { Scale = _evanScale, Speed = 140f };

            BuildCollidersAndDoors();
        }

        static Rectangle ScaleToHeight(Texture2D tex, int viewportW, int viewportH)
        {
            float scale = (float)viewportH / tex.Height;
            int w = (int)(tex.Width * scale);
            int x = (viewportW - w) / 2;
            return new Rectangle(x, 0, w, viewportH);
        }

        void BuildCollidersAndDoors()
        {
            _solids.Clear();

            int left = _dstBorde.Left;
            int right = _dstBorde.Right;
            int top = _dstBorde.Top;
            int bottom = _dstBorde.Bottom;

            int topThickness = (int)(110 * _scale);
            int leftThickness = (int)(92 * _scale);
            int rightThickness = (int)(124 * _scale);
            int bottomThickness = (int)(78 * _scale);

            // Pared superior (completa), laterales
            _solids.Add(new Rectangle(left, top, right - left, topThickness));
            _solids.Add(new Rectangle(left, top, leftThickness, bottom - top));
            _solids.Add(new Rectangle(right - rightThickness, top, rightThickness, bottom - top));

            // Pared inferior con hueco central (puerta hacia el cuarto)
            int doorWidth = (int)((right - left) * 0.20f);
            int doorX = _dstBorde.Center.X - doorWidth / 2;
            _solids.Add(new Rectangle(left, bottom - bottomThickness, doorX - left, bottomThickness));                 // izq
            _solids.Add(new Rectangle(doorX + doorWidth, bottom - bottomThickness, right - (doorX + doorWidth), bottomThickness)); // der
            _doorBottomRect = new Rectangle(doorX, bottom - bottomThickness, doorWidth, bottomThickness);
        }

        public override void Update(GameTime gt)
        {
            if (Input.KeyPressed(Keys.F1)) _debug = !_debug;

            // “Emerge” suave al entrar (empuje hacia arriba durante 0.6s)
            if (_evan.Pos.Y > _dstBorde.Bottom - 2)
                _evan.Pos.Y -= 90f * (float)gt.ElapsedGameTime.TotalSeconds;

            // Movimiento
            _evan.Update(gt, _solids);

            // Transición de Sala -> Cuarto por abajo
            var evCenter = EvanCenter();
            if (_doorBottomRect.Contains(evCenter))
            {
                ScreenManager.Replace(new BedroomScreen(entryXFromBottom: evCenter.X, evanScale: _evan.Scale));
                return;
            }
        }

        Point EvanCenter()
        {
            var evW = (int)(Assets.EvanFrente1.Width * _evan.Scale);
            var evH = (int)(Assets.EvanFrente1.Height * _evan.Scale);
            return new Point((int)(_evan.Pos.X + evW / 2f), (int)(_evan.Pos.Y + evH / 2f));
        }

        public override void Draw(GameTime gt)
        {
            var vp = Device.Viewport;
            Batcher.Begin();

            Batcher.Draw(Assets.SalaPiso, _dstPiso, Color.White);
            Batcher.Draw(Assets.Cuadros, _dstCuadrosL, Color.White);
            Batcher.Draw(Assets.Cuadros, _dstCuadrosR, Color.White);
            Batcher.Draw(Assets.Reloj, _dstReloj, Color.White);

            _evan.Draw(Batcher);
            Batcher.Draw(Assets.SalaBorde, _dstBorde, Color.White);

            if (_debug)
            {
                foreach (var r in _solids) Batcher.Draw(Assets.Pixel, r, new Color(255, 0, 0, 90));
                Batcher.Draw(Assets.Pixel, _doorBottomRect, new Color(0, 255, 0, 60));
            }

            Batcher.End();
        }
    }
}