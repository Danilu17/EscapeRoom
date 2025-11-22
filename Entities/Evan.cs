using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using EscapeRoom.Core;

namespace EscapeRoom.Entities
{
    public class Evan
    {
        public Vector2 Pos;
        public float Speed = 140f;
        // el tamaño en bedroomscreen
        public float Scale = 0.10f;

        enum Dir { Down, Up, Left, Right }
        Dir _dir = Dir.Down;
        double _animAcc;
        int _frame; // 0/1
        bool _isMoving; // Guardar si se está moviendo

        public Evan(Vector2 spawn) { Pos = spawn; }

        Rectangle Bounds(Texture2D tex)
        {
            var w = (int)(tex.Width * Scale);
            var h = (int)(tex.Height * Scale);
            int shrinkY = (int)(h * 0.35f);
            return new Rectangle((int)Pos.X, (int)(Pos.Y + shrinkY), w, h - shrinkY);
        }

        Texture2D CurrentTexture(bool moving)
        {
            if (!moving) _animAcc = 0;
            int f = (_frame == 0 ? 1 : 2);
            return _dir switch
            {
                Dir.Down => (f == 1 ? Assets.EvanFrente1 : Assets.EvanFrente2),
                Dir.Up => (f == 1 ? Assets.EvanEspalda1 : Assets.EvanEspalda2),
                Dir.Left => (f == 1 ? Assets.EvanIzquierda1 : Assets.EvanIzquierda2),
                Dir.Right => (f == 1 ? Assets.EvanDerecha1 : Assets.EvanDerecha2),
                _ => Assets.EvanFrente1
            };
        }

        public void Update(GameTime gt, List<Rectangle> solids)
        {
            float dt = (float)gt.ElapsedGameTime.TotalSeconds;

            // Dirección
            Vector2 dir = Input.WasdVector();
            if (dir.Y > 0.1f) _dir = Dir.Down;
            else if (dir.Y < -0.1f) _dir = Dir.Up;
            else if (dir.X < -0.1f) _dir = Dir.Left;
            else if (dir.X > 0.1f) _dir = Dir.Right;

            _isMoving = dir.LengthSquared() > 0;

            if (_isMoving)
            {
                _animAcc += gt.ElapsedGameTime.TotalSeconds;
                if (_animAcc >= 1.0 / 8.0) { _animAcc = 0; _frame = 1 - _frame; }
            }

            var tex = CurrentTexture(_isMoving);
            Vector2 delta = dir * Speed * dt;

            // X sweep - detección de colisión pixel por pixel
            if (delta.X != 0)
            {
                int step = (int)System.MathF.Sign(delta.X);
                for (int i = 0; i < System.MathF.Abs(delta.X); i++)
                {
                    var b = Bounds(tex);
                    b.Offset(step, 0);
                    if (Collides(b, solids)) break;
                    Pos.X += step;
                }
            }

            // Y sweep - detección de colisión pixel por pixel
            if (delta.Y != 0)
            {
                int step = (int)System.MathF.Sign(delta.Y);
                for (int i = 0; i < System.MathF.Abs(delta.Y); i++)
                {
                    var b = Bounds(tex);
                    b.Offset(0, step);
                    if (Collides(b, solids)) break;
                    Pos.Y += step;
                }
            }
        }

        static bool Collides(Rectangle r, List<Rectangle> solids)
        {
            foreach (var s in solids)
                if (r.Intersects(s))
                    return true;
            return false;
        }

        public void Draw(SpriteBatch sb)
        {
            // Ahora pasamos el estado real de movimiento
            var tex = CurrentTexture(_isMoving);
            sb.Draw(tex, Pos, null, Color.White, 0f, Vector2.Zero, Scale, SpriteEffects.None, 0f);
        }
    }
}
