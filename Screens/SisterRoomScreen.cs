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
using EscapeRoom.UI;
{
    
}

namespace EscapeRoom.Screens
{
    public class SisterRoomScreen : Screen
    {
        Evan _evan;
        readonly List<Rectangle> _solids = new();
        bool _debug;
        DialogueBox _hud;

        Rectangle _dstPiso, _dstBorde;
        Rectangle _dstCamaRosa, _dstMesita, _dstCuadro;

        Rectangle _doorLeftRect;
        float _scale;

        readonly float _entryY;
        readonly float _evanScale;

        public static float SpawnOffsetX_Sister = 0f;
        public static float SpawnOffsetY_Sister = 0f;

        public SisterRoomScreen(float entryY, float evanScale)
        {
            _entryY = entryY;
            _evanScale = evanScale;
        }

        public override void OnPush()
        {
            var vp = Device.Viewport;

            _dstPiso = ScaleToHeight(Assets.Fondo2, vp.Width, vp.Height);
            _dstBorde = ScaleToHeight(Assets.Fondo2P, vp.Width, vp.Height);
            _scale = (float)_dstPiso.Height / Assets.Fondo2.Height;

            // Tamaños muebles
            float camaFactor = 0.48f;
            float mesitaFactor = 0.35f;
            float cuadroFactor = 0.20f;

            var camaSize = new Point(
                (int)(Assets.CamaRosa.Width * _scale * camaFactor),
                (int)(Assets.CamaRosa.Height * _scale * camaFactor));

            var mesitaSize = new Point(
                (int)(Assets.Mesita.Width * _scale * mesitaFactor),
                (int)(Assets.Mesita.Height * _scale * mesitaFactor));

            var cuadroSize = new Point(
                (int)(Assets.Recuadro.Width * _scale * cuadroFactor),
                (int)(Assets.Recuadro.Height * _scale * cuadroFactor));
            //Ubicaciones muebles
            _dstCamaRosa = new Rectangle(
                _dstPiso.Right - camaSize.X - (int)(250 * _scale),
                _dstPiso.Top + (int)(350 * _scale),
                camaSize.X, camaSize.Y);

            _dstMesita = new Rectangle(
                _dstPiso.Left + (int)(140 * _scale),
                _dstPiso.Top + (int)(250 * _scale),
                mesitaSize.X, mesitaSize.Y);

            _dstCuadro = new Rectangle(
                _dstPiso.Center.X - cuadroSize.X - (int)(200 * _scale),
                _dstPiso.Top + (int)(60 * _scale),
                cuadroSize.X, cuadroSize.Y);

            SisterRoomColliders.Build(
                _dstBorde,
                _scale,
                _solids,
                out _doorLeftRect,
                _dstCamaRosa,
                _dstMesita
            );

            var evTex = Assets.EvanFrente1;
            int evW = (int)(evTex.Width * _evanScale);
            int evH = (int)(evTex.Height * _evanScale);

            float x = _doorLeftRect.Right + SpawnOffsetX_Sister;
            float y = _entryY - evH / 2f + SpawnOffsetY_Sister;

            _evan = new Evan(new Vector2(x, y))
            {
                Scale = _evanScale,
                Speed = 140f
            };
            _hud = new DialogueBox(Assets.Fuente);
            _hud.Enqueue("Presiona E para interactuar con los objetos.");
        }

        static Rectangle ScaleToHeight(Texture2D tex, int viewportW, int viewportH)
        {
            float sc = (float)viewportH / tex.Height;
            int w = (int)(tex.Width * sc);
            int x = (viewportW - w) / 2;
            return new Rectangle(x, 0, w, viewportH);
        }

        public override void Update(GameTime gt)
        {
            if (Input.KeyPressed(Keys.F1))
                _debug = !_debug;

            _evan.Update(gt, _solids);
            _hud.Update();

            if (Input.KeyPressed(Keys.E))
                HandleInteraction();

            var evCenter = new Point(
                (int)(_evan.Pos.X + Assets.EvanFrente1.Width * _evan.Scale / 2),
                (int)(_evan.Pos.Y + Assets.EvanFrente1.Height * _evan.Scale / 2));

            if (_doorLeftRect.Contains(evCenter))
            {
                ScreenManager.Replace(new SalaScreen(
                    SalaEntryPoint.FromSister,
                    evCenter.Y,
                    _evan.Scale
                ));
                return;
            }
        }
        Rectangle EvanBounds()
        {
            var tex = Assets.EvanFrente1;
            int w = (int)(tex.Width * _evan.Scale);
            int h = (int)(tex.Height * _evan.Scale);
            int shrinkY = (int)(h * 0.35f);
            return new Rectangle((int)_evan.Pos.X, (int)(_evan.Pos.Y + shrinkY), w, h - shrinkY);
        }

        bool IsNear(Rectangle target, int padding = 20)
        {
            var expanded = target;
            expanded.Inflate(padding, padding);
            return expanded.Intersects(EvanBounds());
        }

        void HandleInteraction()
        {
            if (IsNear(_dstCuadro))
            {
                ScreenManager.Push(new BalloonGameScreen());
                return;
            }

            if (IsNear(_dstCamaRosa) || IsNear(_dstMesita))
            {
                _hud.Enqueue("nada por aqui");
                return;
            }

            _hud.Enqueue("nada por aqui");
        }


        public override void Draw(GameTime gt)
        {
            Batcher.Begin();

            Batcher.Draw(Assets.Fondo2, _dstPiso, Color.White);
            Batcher.Draw(Assets.Fondo2P, _dstBorde, Color.White);

            Batcher.Draw(Assets.CamaRosa, _dstCamaRosa, Color.White);
            Batcher.Draw(Assets.Mesita, _dstMesita, Color.White);
            Batcher.Draw(Assets.Recuadro, _dstCuadro, Color.White);

            _evan.Draw(Batcher);

            _hud.Draw(Batcher, Device.Viewport);

            if (_debug)
            {
                foreach (var r in _solids)
                    Batcher.Draw(Assets.Pixel, r, new Color(255, 0, 0, 90));

                Batcher.Draw(Assets.Pixel, _doorLeftRect, new Color(0, 255, 0, 90));
            }

            Batcher.End();
        }
    }
}
