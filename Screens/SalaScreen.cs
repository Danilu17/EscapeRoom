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
    public enum SalaEntryPoint { FromBedroom, FromSister, FromSala2 }

    public class SalaScreen : Screen
    {
        Evan _evan;
        readonly List<Rectangle> _solids = new();
        bool _debug;

        Rectangle _dstBackground;
        Rectangle _dstPiso, _dstBorde;
        Rectangle _dstReloj, _dstCuadro1, _dstCuadro2;

        Rectangle _doorBottomRect;  // Bedroom
        Rectangle _doorRightRect;   // Sister
        Rectangle _doorLeftRect;    // Sala2

        float _scale;
        readonly SalaEntryPoint _entryFrom;
        readonly float _entryCoord;
        readonly float _evanScale;

        public static float SpawnOffsetX_Sala = 0f;
        public static float SpawnOffsetY_Sala = 100f;

        static readonly Color DebugSolidColor = new Color(255, 0, 0, 90);
        static readonly Color DebugDoorColor = new Color(0, 255, 0, 90);

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

            _dstReloj = new Rectangle(_dstPiso.Center.X - relojSize.X / 1, _dstPiso.Top + (int)(40 * _scale), relojSize.X, relojSize.Y);
            _dstCuadro1 = new Rectangle(_dstPiso.Left + (int)(200 * _scale), _dstPiso.Top + (int)(20 * _scale), cuadro1Size.X, cuadro1Size.Y);
            _dstCuadro2 = new Rectangle(_dstPiso.Right - cuadro2Size.X - (int)(400 * _scale), _dstPiso.Top + (int)(40 * _scale), cuadro2Size.X, cuadro2Size.Y);

            // Colliders
            SalaColliders.Build(_dstBorde, _scale, _solids, out _doorBottomRect, out _doorRightRect, out _doorLeftRect);

            // Spawn
            var evTex = Assets.EvanFrente1;
            int evW = (int)(evTex.Width * _evanScale);
            int evH = (int)(evTex.Height * _evanScale);

            Vector2 spawn;
            switch (_entryFrom)
            {
                case SalaEntryPoint.FromBedroom:
                    {
                        // Mantengo tu offset Y para dejarlo bien adentro del cuarto
                        spawn = SpawnHelper.SpawnAtDoor(_doorBottomRect, evW, evH, DoorSide.Bottom, desiredCenter: _entryCoord, inset: 5f);
                        spawn.X += SpawnOffsetX_Sala;
                        spawn.Y += SpawnOffsetY_Sala; // atención: +Y baja; ajusta si lo querés más arriba
                        break;
                    }
                case SalaEntryPoint.FromSister:
                    {
                        spawn = SpawnHelper.SpawnAtDoor(_doorRightRect, evW, evH, DoorSide.Right, desiredCenter: _entryCoord, inset: 5f);
                        break;
                    }
                case SalaEntryPoint.FromSala2:
                default:
                    {
                        spawn = SpawnHelper.SpawnAtDoor(_doorLeftRect, evW, evH, DoorSide.Left, desiredCenter: _entryCoord, inset: 5f);
                        break;
                    }
            }

            _evan = new Evan(spawn) { Scale = _evanScale, Speed = 140f };

            // Opcional: correr tests en DEBUG una vez
#if DEBUG
            EscapeRoom.Core.SpawnHelperTests.Run();
#endif
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
            var evCenter = EvanCenter();

            if (_doorBottomRect.Contains(evCenter))
            {
                ScreenManager.Replace(new BedroomScreen(evCenter.X, _evan.Scale)); return;
            }
            if (_doorRightRect.Contains(evCenter))
            {
                ScreenManager.Replace(new SisterRoomScreen(evCenter.Y, _evan.Scale)); return;
            }
            if (_doorLeftRect.Contains(evCenter))
            {
                ScreenManager.Replace(new Sala2Screen(evCenter.Y, _evan.Scale)); return;
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
                foreach (var r in _solids) Batcher.Draw(Assets.Pixel, r, DebugSolidColor);
                Batcher.Draw(Assets.Pixel, _doorBottomRect, DebugDoorColor);
                Batcher.Draw(Assets.Pixel, _doorRightRect, DebugDoorColor);
                Batcher.Draw(Assets.Pixel, _doorLeftRect, DebugDoorColor);
            }
            Batcher.End();
        }
    }
}