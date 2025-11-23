//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using EscapeRoom.Core;
//using EscapeRoom.Entities;
//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;

//namespace EscapeRoom.Screens
//{
//    public class SisterRoomScreen : Screen
//    {
//        Evan _evan;
//        readonly List<Rectangle> _solids = new();
//        bool _debug;

//        Rectangle _dstPiso, _dstPared;
//        Rectangle _dstCamaRosa, _dstMesita, _dstRecuadro;

//        Rectangle _doorLeftRect;
//        float _scale;

//        readonly float _entryYFromHall;
//        readonly float _evanScale;

//        public SisterRoomScreen(float entryYFromHall, float evanScale)
//        {
//            _entryYFromHall = entryYFromHall;
//            _evanScale = evanScale;
//        }

//        public override void OnPush()
//        {
//            var vp = Device.Viewport;

//            // Piso y Pared
//            _dstPiso = ScaleToHeight(Assets.Fondo2, vp.Width, vp.Height);
//            _dstPared = ScaleToHeight(Assets.Fondo2P, vp.Width, vp.Height);
//            _scale = (float)_dstPiso.Height / Assets.Fondo2.Height;

//            // Muebles
//            _dstCamaRosa = PlaceScaled(Assets.CamaRosa, 0.65f, 0.45f);
//            _dstMesita = PlaceScaled(Assets.Mesita, 0.25f, 0.45f);
//            _dstRecuadro = PlaceScaled(Assets.Recuadro, 0.60f, 0.20f);

//            // COLISIONES
//            SisterRoomColliders.Build(
//                _dstPared,
//                _scale,
//                _solids,
//                out _doorLeftRect
//            );

//            // Spawn de Evan entrando desde la puerta derecha
//            var tex = Assets.EvanFrente1;
//            int evW = (int)(tex.Width * _evanScale);
//            int evH = (int)(tex.Height * _evanScale);

//            _evan = new Evan(
//                new Vector2(
//                    _doorLeftRect.Right + 10,     // aparece a la derecha del hueco
//                    _entryYFromHall - evH / 2f
//                )
//            )
//            {
//                Scale = _evanScale,
//                Speed = 140f
//            };
//        }

//        Rectangle ScaleToHeight(Texture2D tex, int vpW, int vpH)
//        {
//            float scale = (float)vpH / tex.Height;
//            int w = (int)(tex.Width * scale);
//            int x = (vpW - w) / 2;
//            return new Rectangle(x, 0, w, vpH);
//        }

//        Rectangle PlaceScaled(Texture2D tex, float percentX, float percentY)
//        {
//            int w = (int)(tex.Width * _scale);
//            int h = (int)(tex.Height * _scale);

//            int x = _dstPiso.Left + (int)(_dstPiso.Width * percentX - w / 2f);
//            int y = _dstPiso.Top + (int)(_dstPiso.Height * percentY - h / 2f);

//            return new Rectangle(x, y, w, h);
//        }

//        public override void Update(GameTime gt)
//        {
//            if (Input.KeyPressed(Keys.F1)) _debug = !_debug;

//            _evan.Update(gt, _solids);

//            var evCenter = EvanCenter();

//            // Salir de este cuarto → volver a Sala
//            if (_doorLeftRect.Contains(evCenter))
//            {
//                ScreenManager.Replace(
//                    new SalaScreen(
//                        entryX: _doorLeftRect.Right + 12,
//                        entryYFromSister: evCenter.Y,
//                        evanScale: _evan.Scale
//                    )
//                );
//                return;
//            }
//        }

//        Point EvanCenter()
//        {
//            int w = (int)(Assets.EvanFrente1.Width * _evan.Scale);
//            int h = (int)(Assets.EvanFrente1.Height * _evan.Scale);
//            return new Point(
//                (int)(_evan.Pos.X + w / 2f),
//                (int)(_evan.Pos.Y + h / 2f)
//            );
//        }

//        public override void Draw(GameTime gt)
//        {
//            var vp = Device.Viewport;
//            Batcher.Begin();

//            // Fondo
//            Batcher.Draw(Assets.Fondo2, _dstPiso, Color.White);
//            Batcher.Draw(Assets.Fondo2P, _dstPared, Color.White);

//            // Muebles
//            Batcher.Draw(Assets.CamaRosa, _dstCamaRosa, Color.White);
//            Batcher.Draw(Assets.Mesita, _dstMesita, Color.White);
//            Batcher.Draw(Assets.Recuadro, _dstRecuadro, Color.White);

//            // Evan
//            _evan.Draw(Batcher);

//            if (_debug)
//            {
//                foreach (var r in _solids)
//                    Batcher.Draw(Assets.Pixel, r, new Color(255, 0, 0, 90));

//                Batcher.Draw(Assets.Pixel, _doorLeftRect, new Color(0, 160, 255, 80));
//            }

//            Batcher.End();
//        }
//    }
//}
