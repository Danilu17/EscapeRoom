using EscapeRoom.Core;
using EscapeRoom.Entities;
using EscapeRoom.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EscapeRoom.Screens
{
    public class BedroomScreen : Screen
    {
        Evan _evan;
        readonly List<Rectangle> _solids = new();
        bool _debug;

        double _tutorialTimer = 0.0;

        readonly string[] _tutorial = {
            "Usa WASD para moverte.",
            "Presiona E para interactuar con objetos.",
            "Presiona Enter para continuar dialogos."
        };

        readonly string[] _thoughts = {
            "Donde estoy...? Otra vez este lugar...",
            "Siempre es el mismo cuarto, pero algo cambia cada noche.",
            "Donde estan mama y papa? Por que no me despiertan?",
            "No puedo dormir mas. Tengo que salir de aqui y encontrar la verdad."
        };

        DialogueBox _hud;

        Rectangle _dstPiso, _dstBorde, _dstCama, _dstMesitadeluz;
        float _scale;

        Rectangle _doorTopRect;

        readonly float? _entryXFromHall;
        readonly float _evanScale;

        public static float SpawnOffsetX_Bedroom = 0f;
        public static float SpawnOffsetY_Bedroom = -150f;

        public BedroomScreen(float? entryXFromHall = null, float evanScale = 0.20f)
        {
            _entryXFromHall = entryXFromHall;
            _evanScale = evanScale;
        }

        public override void OnPush()
        {
            var vp = Device.Viewport;

            _dstPiso = ScaleToHeight(Assets.Piso, vp.Width, vp.Height);
            _dstBorde = ScaleToHeight(Assets.Borde, vp.Width, vp.Height);
            _scale = (float)_dstPiso.Height / Assets.Piso.Height;

            float camaFactor = 1.3f;
            float mesitaFactor = 1.2f;

            var camaSize = new Point(
                (int)(Assets.Cama.Width * _scale * camaFactor),
                (int)(Assets.Cama.Height * _scale * camaFactor));

            var mesitaSize = new Point(
                (int)(Assets.MesitaDeLuz.Width * _scale * mesitaFactor),
                (int)(Assets.MesitaDeLuz.Height * _scale * mesitaFactor));

            _dstCama = new Rectangle(
                _dstPiso.X + (int)(vp.Width * 0.67f),
                _dstPiso.Y + (int)(vp.Height * 0.1f),
                camaSize.X, camaSize.Y);

            _dstMesitadeluz = new Rectangle(
                _dstPiso.X + (int)(vp.Width * 0.1f),
                _dstPiso.Y + (int)(vp.Height * 0.1f),
                mesitaSize.X, mesitaSize.Y);

            BedroomColliders.Build(
                _dstBorde,
                _scale,
                _dstCama,
                _dstMesitadeluz,
                _solids,
                out _doorTopRect
            );

            var evTex = Assets.EvanFrente1;
            int evW = (int)(evTex.Width * _evanScale);
            int evH = (int)(evTex.Height * _evanScale);

            Vector2 spawn;

            if (_entryXFromHall.HasValue)
            {
                float x = _entryXFromHall.Value - evW / 2f + SpawnOffsetX_Bedroom;
                float y = _doorTopRect.Bottom + SpawnOffsetY_Bedroom;

                spawn = new Vector2(x, y);
            }
            else
            {
                spawn = new Vector2(
                    (vp.Width - evW) / 2f,
                    (vp.Height - evH) / 2f);
            }

            _evan = new Evan(spawn) { Scale = _evanScale, Speed = 140f };

            _hud = new DialogueBox(Assets.Fuente);
            if (!GameFlags.BedroomIntroShown)
            {
                _hud.Enqueue(_thoughts);
                _tutorialTimer = 4.0;
                GameFlags.BedroomIntroShown = true;
            }
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
            if (_tutorialTimer > 0)
                _tutorialTimer -= gt.ElapsedGameTime.TotalSeconds;

            var evCenter = new Point(
                (int)(_evan.Pos.X + Assets.EvanFrente1.Width * _evan.Scale / 2),
                (int)(_evan.Pos.Y + Assets.EvanFrente1.Height * _evan.Scale / 2));

            if (_doorTopRect.Contains(evCenter))
            {
                ScreenManager.Replace(new SalaScreen(
                    SalaEntryPoint.FromBedroom,
                    evCenter.X,
                    _evan.Scale
                ));
                return;
            }
        }

        public override void Draw(GameTime gt)
        {
            var vp = Device.Viewport;
            Batcher.Begin();

            Batcher.Draw(Assets.Piso, _dstPiso, Color.White);
            Batcher.Draw(Assets.Borde, _dstBorde, Color.White);

            Batcher.Draw(Assets.Cama, _dstCama, Color.White);
            Batcher.Draw(Assets.MesitaDeLuz, _dstMesitadeluz, Color.White);

            _evan.Draw(Batcher);

            if (_tutorialTimer > 0)
            {
                float a = MathHelper.Clamp((float)(_tutorialTimer / 4.0), 0f, 1f);
                Batcher.Draw(
                    Assets.Pixel,
                    new Rectangle(0, 0, vp.Width, vp.Height),
                    new Color((byte)0, (byte)0, (byte)0, (byte)(a * 140f)));
            }

            _hud.Draw(Batcher, vp);

            if (_debug)
            {
                foreach (var r in _solids)
                    Batcher.Draw(Assets.Pixel, r, new Color(255, 0, 0, 90));

                Batcher.Draw(Assets.Pixel, _doorTopRect, new Color(0, 255, 0, 60));
            }

            Batcher.End();
        }
    }
}
