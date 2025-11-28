using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace EscapeRoom.Screens
{
    public static class Sala2Colliders
    {
        public static int TopThicknessPx = 250;
        public static int LeftThicknessPx = 110;
        public static int RightThicknessPx = 110;
        public static int BottomThicknessPx = 20;

        // Puerta derecha (hacia Sala)
        public static int DoorRightHeightPx = 800;
        public static int DoorRightOffsetYPx = 550;

        // Offsets manuales del SILLÓN (en píxeles de textura)
        public static int SillonOffsetLeftPx = 20;
        public static int SillonOffsetTopPx = 40;
        public static int SillonOffsetRightPx = 20;
        public static int SillonOffsetBottomPx = 200;

        // Offsets manuales de la TV
        public static int TvOffsetLeftPx = 10;
        public static int TvOffsetTopPx = 20;
        public static int TvOffsetRightPx = 10;
        public static int TvOffsetBottomPx = 200;

        public static void Build(
            Rectangle borde,
            float scale,
            List<Rectangle> solids,
            out Rectangle doorRightRect,
            Rectangle sillon,
            Rectangle tv)
        {
            solids.Clear();

            int left = borde.Left;
            int right = borde.Right;
            int top = borde.Top;
            int bottom = borde.Bottom;

            int tTop = (int)(TopThicknessPx * scale);
            int tLeft = (int)(LeftThicknessPx * scale);
            int tRight = (int)(RightThicknessPx * scale);
            int tBot = (int)(BottomThicknessPx * scale);

            // -------- PARED SUPERIOR --------
            solids.Add(new Rectangle(left, top, right - left, tTop));

            // -------- PARED IZQUIERDA COMPLETA --------
            solids.Add(new Rectangle(left, top, tLeft, bottom - top));

            // -------- PARED DERECHA CON HUECO --------
            int doorH = (int)(DoorRightHeightPx * scale);
            int doorY = top + (int)(DoorRightOffsetYPx * scale);

            solids.Add(new Rectangle(right - tRight, top, tRight, doorY - top)); // arriba
            solids.Add(new Rectangle(right - tRight, doorY + doorH, tRight, bottom - (doorY + doorH))); // abajo

            doorRightRect = new Rectangle(right - tRight, doorY, tRight, doorH);

            // -------- PISO COMPLETO --------
            solids.Add(new Rectangle(left, bottom - tBot, right - left, tBot));

            // -------- SILLÓN (colisión ajustable) --------
            int sx = sillon.X + (int)(SillonOffsetLeftPx * scale);
            int sy = sillon.Y + (int)(SillonOffsetTopPx * scale);
            int sw = sillon.Width - (int)((SillonOffsetLeftPx + SillonOffsetRightPx) * scale);
            int sh = sillon.Height - (int)((SillonOffsetTopPx + SillonOffsetBottomPx) * scale);
            solids.Add(new Rectangle(sx, sy, sw, sh));

            // -------- TV (colisión ajustable) --------
            int tx = tv.X + (int)(TvOffsetLeftPx * scale);
            int ty = tv.Y + (int)(TvOffsetTopPx * scale);
            int tw = tv.Width - (int)((TvOffsetLeftPx + TvOffsetRightPx) * scale);
            int th = tv.Height - (int)((TvOffsetTopPx + TvOffsetBottomPx) * scale);
            solids.Add(new Rectangle(tx, ty, tw, th));
        }
    }
}
