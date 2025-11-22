using EscapeRoom.Core;
using EscapeRoom.Core;
using EscapeRoom.Entities;
using EscapeRoom.Entities;
using EscapeRoom.UI;
using EscapeRoom.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
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

        // Tutorial (se muestra 4s pero no bloquea)
        double _tutorialTimer = 4.0;
        readonly string[] _tutorial = {
            "Usa WASD para moverte.",
            "Presiona E para interactuar con objetos.",
            "Presiona Enter para continuar dialogos."
        };

        // HUD (pensamientos iniciales – opcional; puedes quitar si no lo usas aquí)
        readonly string[] _thoughts = {
            "Donde estoy...? Otra vez este lugar...",
            "Siempre es el mismo cuarto, pero algo cambia cada noche.",
            "Donde estan mama y papa? Por que no me despiertan?",
            "No puedo dormir mas. Tengo que salir de aqui y encontrar la verdad."
        };
        DialogueBox _hud;

        // Arte y escalas
        Rectangle _dstPiso, _dstBorde, _dstCama, _dstMesita;
        float _scale;

        // Puertas (huecos en pared)
        Rectangle _doorTopRect;      // sale a Sala (arriba)
        Rectangle _doorBottomRect;   // entrada desde Sala (abajo) – opcional por si vuelves

        // Permite entrada desde otra pantalla conservando X o Y
        float? _entryXFromBottom; // si vienes desde abajo (Sala -> Bedroom)

        public BedroomScreen(float? entryXFromBottom = null, float evanScale = 0.14f)
        {
            _entryXFromBottom = entryXFromBottom;
            _evanScale = evanScale;
        }
        readonly float _evanScale;

        public override void OnPush()
        {
            var vp = Device.Viewport;

            _dstPiso = ScaleToHeight(Assets.Piso, vp.Width, vp.Height);
            _dstBorde = ScaleToHeight(Assets.Borde, vp.Width, vp.Height);
            _scale = (float)_dstPiso.Height / Assets.Piso.Height;

            // Muebles (ajusta a tu arte)
            var camaSize = new Point((int)(Assets.Cama.Width * _scale), (int)(Assets.Cama.Height * _scale));
            var mesaSize = new Point((int)(Assets.Mesita.Width * _scale), (int)(Assets.Mesita.Height * _scale));
            _dstCama = new Rectangle(_dstPiso.X + (int)(vp.Width * 0.10f), _dstPiso.Y + (int)(vp.Height * 0.10f), camaSize.X, camaSize.Y);
            _dstMesita = new Rectangle(_dstPiso.X + (int)(vp.Width * 0.02f), _dstPiso.Y + (int)(vp.Height * 0.1f), mesaSize.X, mesaSize.Y);

            // Evan spawn
            var evTex = Assets.EvanFrente1;
            var evW = (int)(evTex.Width * _evanScale);
            var evH = (int)(evTex.Height * _evanScale);

            Vector2 spawn;
            if (_entryXFromBottom.HasValue)
            {
                // regresa desde Sala: aparece apenas dentro del cuarto (borde inferior)
                float x = _entryXFromBottom.Value - evW / 2f;
                float y = _dstBorde.Bottom - (int)(73 * _scale) - evH - 2; // justo por encima del grosor inferior
                spawn = new Vector2(x, y);
            }
            else
            {
                // inicio normal: centro
                spawn = new Vector2((vp.Width - evW) / 2f, (vp.Height - evH) / 2f);
            }

            _evan = new Evan(spawn) { Scale = _evanScale, Speed = 140f };

            _hud = new DialogueBox(Assets.Fuente);
            _hud.Enqueue(_thoughts);

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

            // Grosores (ajustados a tu arte de borde)
            int topThickness = (int)(112 * _scale);
            int leftThickness = (int)(90 * _scale);
            int rightThickness = (int)(126 * _scale);
            int bottomThickness = (int)(73 * _scale);

            // Laterales e inferior sólidos
            _solids.Add(new Rectangle(left, top, leftThickness, bottom - top));
            _solids.Add(new Rectangle(right - rightThickness, top, rightThickness, bottom - top));
            _solids.Add(new Rectangle(left, bottom - bottomThickness, right - left, bottomThickness));
            _doorBottomRect = new Rectangle(left + (int)((right - left) * 0.40f), bottom - bottomThickness, (int)((right - left) * 0.20f), bottomThickness); // opcional

            // Superior con hueco desplazado (puerta hacia Sala)
            int doorWidth = (int)((right - left) * 0.22f);
            int doorOffsetX = (int)(45 * _scale);
            int doorX = ((left + right) / 2 - doorWidth / 2) + doorOffsetX;
            _solids.Add(new Rectangle(left, top, doorX - left, topThickness)); // tramo izq
            _solids.Add(new Rectangle(doorX + doorWidth, top, right - (doorX + doorWidth), topThickness)); // tramo der
            _doorTopRect = new Rectangle(doorX, top, doorWidth, topThickness);

            // Muebles (activa si quieres colisión)
            //  _solids.Add(_dstCama);
            // _solids.Add(_dstMesita);
        }

        public override void Update(GameTime gt)
        {
            if (Input.KeyPressed(Keys.F1)) _debug = !_debug;

            // Movimiento SIEMPRE
            _evan.Update(gt, _solids);

            // Pensamientos (Enter para avanzar; no bloquea)
            _hud.Update();

            // Tutorial fade
            if (_tutorialTimer > 0) _tutorialTimer -= gt.ElapsedGameTime.TotalSeconds;

            // --- Transición de cuarto -> Sala por arriba ---
            var evCenter = EvanCenter();
            if (_doorTopRect.Contains(evCenter))
            {
                ScreenManager.Replace(new SalaScreen(entryX: evCenter.X, evanScale: _evan.Scale));
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

            Batcher.Draw(Assets.Piso, _dstPiso, Color.White);
            Batcher.Draw(Assets.Cama, _dstCama, Color.White);
            Batcher.Draw(Assets.Mesita, _dstMesita, Color.White);
            _evan.Draw(Batcher);
            Batcher.Draw(Assets.Borde, _dstBorde, Color.White);

            // Tutorial
            if (_tutorialTimer > 0)
            {
                float a = MathHelper.Clamp((float)(_tutorialTimer / 4.0), 0f, 1f);
                Batcher.Draw(Assets.Pixel, new Rectangle(0, 0, vp.Width, vp.Height), new((byte)0, (byte)0, (byte)0, (byte)(a * 140f)));
                var lines = string.Join("\n\n", _tutorial);
                var size = Assets.FuenteInstr.MeasureString(lines);
                var pos = new Vector2((vp.Width - size.X) / 2f, (int)(vp.Height * 0.12f));
                Batcher.DrawString(Assets.FuenteInstr, lines, pos, Color.White);
            }

            // Pensamientos HUD
            _hud.Draw(Batcher, vp);

            if (_debug)
            {
                foreach (var r in _solids) Batcher.Draw(Assets.Pixel, r, new Color(255, 0, 0, 90));
                Batcher.Draw(Assets.Pixel, _doorTopRect, new Color(0, 255, 0, 60));
                Batcher.Draw(Assets.Pixel, _doorBottomRect, new Color(0, 0, 255, 40));
            }

            Batcher.End();
        }
    }
}

