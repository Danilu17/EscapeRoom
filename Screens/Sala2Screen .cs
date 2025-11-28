using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using EscapeRoom.Core;
using EscapeRoom.Entities;

namespace EscapeRoom.Screens
{
    public class Sala2Screen : Screen
    {
        Evan _evan;
        readonly List<Rectangle> _solids = new();
        bool _debug;

        Rectangle _dstPiso, _dstBorde;
        Rectangle _dstAlfombra, _dstSillon, _dstTv;
        Rectangle _doorRightRect;
        float _scale;

        readonly float _entryY;
        readonly float _evanScale;

        public static float SpawnOffsetX_Sala2 = -5f;
        public static float SpawnOffsetY_Sala2 = 0f;

        public Sala2Screen(float entryY, float evanScale)
        {
            _entryY = entryY;
            _evanScale = evanScale;
        }

        public override void OnPush()
        {
            var vp = Device.Viewport;

            _dstPiso = ScaleToHeight(Assets.Fondo3, vp.Width, vp.Height);
            _dstBorde = ScaleToHeight(Assets.Fondo3P, vp.Width, vp.Height);
            _scale = (float)_dstPiso.Height / Assets.Fondo3.Height;

            float alfombraFactor = 0.50f, sillonFactor = 0.47f, tvFactor = 0.35f;
            var alfombraSize = new Point((int)(Assets.Alfombra.Width * _scale * alfombraFactor), (int)(Assets.Alfombra.Height * _scale * alfombraFactor));
            var sillonSize = new Point((int)(Assets.Sillon.Width * _scale * sillonFactor), (int)(Assets.Sillon.Height * _scale * sillonFactor));
            var tvSize = new Point((int)(Assets.Tv.Width * _scale * tvFactor), (int)(Assets.Tv.Height * _scale * tvFactor));

            _dstTv = new Rectangle(_dstPiso.Left + (int)(220 * _scale), _dstPiso.Top + (int)(150 * _scale), tvSize.X, tvSize.Y);
            _dstSillon = new Rectangle(_dstPiso.Center.X - sillonSize.X / 6, _dstPiso.Top + (int)(220 * _scale), sillonSize.X, sillonSize.Y);
            _dstAlfombra = new Rectangle(_dstPiso.Center.X - alfombraSize.X / 2, _dstPiso.Bottom - alfombraSize.Y - (int)(120 * _scale), alfombraSize.X, alfombraSize.Y);

            Sala2Colliders.Build(_dstBorde, _scale, _solids, out _doorRightRect, _dstSillon, _dstTv);

            var evTex = Assets.EvanFrente1;
            int evW = (int)(evTex.Width * _evanScale);
            int evH = (int)(evTex.Height * _evanScale);

            var spawn = SpawnHelper.SpawnAtDoor(_doorRightRect, evW, evH, DoorSide.Right, desiredCenter: _entryY, inset: 5f);
            spawn.X += SpawnOffsetX_Sala2;
            spawn.Y += SpawnOffsetY_Sala2;

            _evan = new Evan(spawn) { Scale = _evanScale, Speed = 140f };
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

            var evCenter = new Point(
                (int)(_evan.Pos.X + Assets.EvanFrente1.Width * _evan.Scale / 2),
                (int)(_evan.Pos.Y + Assets.EvanFrente1.Height * _evan.Scale / 2));

            if (_doorRightRect.Contains(evCenter))
            {
                ScreenManager.Replace(new SalaScreen(SalaEntryPoint.FromSala2, evCenter.Y, _evan.Scale)); return;
            }
        }

        public override void Draw(GameTime gt)
        {
            Batcher.Begin();
            Batcher.Draw(Assets.Fondo3, _dstPiso, Color.White);
            Batcher.Draw(Assets.Fondo3P, _dstBorde, Color.White);
            Batcher.Draw(Assets.Alfombra, _dstAlfombra, Color.White);
            Batcher.Draw(Assets.Sillon, _dstSillon, Color.White);
            Batcher.Draw(Assets.Tv, _dstTv, Color.White);
            _evan.Draw(Batcher);

            if (_debug)
            {
                foreach (var r in _solids) Batcher.Draw(Assets.Pixel, r, new Color(255, 0, 0, 90));
                Batcher.Draw(Assets.Pixel, _doorRightRect, new Color(0, 255, 0, 90));
            }
            Batcher.End();
        }
    }
}