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
using System.Threading.Tasks;

namespace EscapeRoom.Screens
{
    public class BedroomScreen : Screen
    {
        Evan _evan;
        readonly List<Rectangle> _solids = new();
        bool _debug;

        // Tutorial (se muestra solo la primera vez)
        double _tutorialTimer = 0.0;
        readonly string[] _tutorial = {
            "Usa WASD para moverte.",
            "Presiona E para interactuar con objetos.",
            "Presiona Enter para continuar dialogos."
        };

        // Pensamientos iniciales (HUD inferior)
        readonly string[] _thoughts = {
            "Donde estoy...? Otra vez este lugar...",
            "Siempre es el mismo cuarto, pero algo cambia cada noche.",
            "Donde estan mama y papa? Por que no me despiertan?",
            "No puedo dormir mas. Tengo que salir de aqui y encontrar la verdad."
        };
        DialogueBox _hud;

        // Arte y escalas
        Rectangle _dstPiso, _dstBorde, _dstCama, _dstMesitadeluz;
        float _scale;

        // Puertas
        Rectangle _doorTopRect;      // puerta real hacia Sala (arriba)

        // Si venimos desde la Sala, guardamos la X de la puerta para reaparecer
        readonly float? _entryXFromHall;
        readonly float _evanScale;

        public BedroomScreen(float? entryXFromHall = null, float evanScale = 0.20f)
        {
            _entryXFromHall = entryXFromHall;
            _evanScale = evanScale;
        }

        public override void OnPush()
        {
            var vp = Device.Viewport;

            // Piso y borde escalados a la altura de la pantalla
            _dstPiso = ScaleToHeight(Assets.Piso, vp.Width, vp.Height);
            _dstBorde = ScaleToHeight(Assets.Borde, vp.Width, vp.Height);
            _scale = (float)_dstPiso.Height / Assets.Piso.Height;

            // Muebles
            var camaSize = new Point(
                (int)(Assets.Cama.Width * _scale),
                (int)(Assets.Cama.Height * _scale));

            var mesitadeluzSize = new Point(
                (int)(Assets.MesitaDeLuz.Width * _scale),
                (int)(Assets.MesitaDeLuz.Height * _scale));

            _dstCama = new Rectangle(
                _dstPiso.X + (int)(vp.Width * 0.7f),
                _dstPiso.Y + (int)(vp.Height * 0.1f),
                camaSize.X, camaSize.Y);

            _dstMesitadeluz = new Rectangle(
                _dstPiso.X + (int)(vp.Width * 0.1f),
                _dstPiso.Y + (int)(vp.Height * 0.1f),
                mesitadeluzSize.X, mesitadeluzSize.Y);

            // --- COLISIONES DESDE ARCHIVO SEPARADO ---
            BedroomColliders.Build(
                _dstBorde,
                _scale,
                _dstCama,
                _dstMesitadeluz,
                _solids,
                out _doorTopRect);

            // --- SPAWN DE EVAN ---
            var evTex = Assets.EvanFrente1;
            int evW = (int)(evTex.Width * _evanScale);
            int evH = (int)(evTex.Height * _evanScale);
            Vector2 spawn;

            if (_entryXFromHall.HasValue)
            {
                // Viene desde la Sala: entra por la PUERTA SUPERIOR, bajando
                float xCenter = _entryXFromHall.Value;
                float x = xCenter - evW / 2f;
                // justo por debajo del marco de la puerta
                float y = _doorTopRect.Bottom + 2;
                spawn = new Vector2(x, y);
            }
            else
            {
                // Primera vez en el juego: aparece en el centro del cuarto
                spawn = new Vector2(
                    (vp.Width - evW) / 2f,
                    (vp.Height - evH) / 2f);
            }

            _evan = new Evan(spawn) { Scale = _evanScale, Speed = 140f };

            // --- HUD Y TUTORIAL: SOLO LA PRIMERA VEZ ---
            _hud = new DialogueBox(Assets.Fuente);
            if (!GameFlags.BedroomIntroShown)
            {
                _hud.Enqueue(_thoughts);
                _tutorialTimer = 4.0;            // instrucciones arriba durante 4 segundos
                GameFlags.BedroomIntroShown = true;
            }
            else
            {
                _tutorialTimer = 0.0;            // no volver a mostrar instrucciones
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
            if (Input.KeyPressed(Keys.F1)) _debug = !_debug;

            // Movimiento y colisiones
            _evan.Update(gt, _solids);

            // HUD (pensamientos) – Enter para avanzar
            _hud.Update();

            // Tutorial fade (solo si se activó la primera vez)
            if (_tutorialTimer > 0)
                _tutorialTimer -= gt.ElapsedGameTime.TotalSeconds;

            // --- Transición de cuarto -> Sala por la PUERTA SUPERIOR ---
            var evCenter = EvanCenter();
            if (_doorTopRect.Contains(evCenter))
            {
                ScreenManager.Replace(new SalaScreen(entryX: evCenter.X, evanScale: _evan.Scale));
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
            var vp = Device.Viewport;
            Batcher.Begin();

            // ORDEN DE CAPAS:
            // 1) Piso (fondo)
            // 2) Paredes/borde
            // 3) Muebles
            // 4) Evan SIEMPRE ADELANTE
            Batcher.Draw(Assets.Piso, _dstPiso, Color.White);
            Batcher.Draw(Assets.Borde, _dstBorde, Color.White);
            Batcher.Draw(Assets.Cama, _dstCama, Color.White);
            Batcher.Draw(Assets.MesitaDeLuz, _dstMesitadeluz, Color.White);

            _evan.Draw(Batcher);

            // Tutorial de controles (solo primera vez)
            if (_tutorialTimer > 0)
            {
                float a = MathHelper.Clamp((float)(_tutorialTimer / 4.0), 0f, 1f);
                Batcher.Draw(Assets.Pixel,
                    new Rectangle(0, 0, vp.Width, vp.Height),
                    new Color((byte)0, (byte)0, (byte)0, (byte)(a * 140f)));

                string lines = string.Join("\n\n", _tutorial);
                var size = Assets.FuenteInstr.MeasureString(lines);
                var pos = new Vector2((vp.Width - size.X) / 2f, (int)(vp.Height * 0.12f));
                Batcher.DrawString(Assets.FuenteInstr, lines, pos, Color.White);
            }

            // Pensamientos HUD inferior
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
