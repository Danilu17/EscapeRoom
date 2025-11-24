using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using EscapeRoom.Core;
using EscapeRoom.Entities;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace EscapeRoom.Screens
{
    public static class SisterRoomColliders
    {
        // ---------- Paredes ----------
        public static int TopThicknessPx = 300;
        public static int LeftThicknessPx = 115;
        public static int RightThicknessPx = 200;
        public static int BottomThicknessPx = 15;

        // ---------- Puerta izquierda ----------
        public static int DoorLeftHeightPx = 800;
        public static int DoorLeftOffsetYPx = 550;

        // ---------- OFFSETS OBJETOS (EDITABLES A MANO) ----------
        // Cama rosa
        public static int CamaOffsetLeftPx = 40;
        public static int CamaOffsetTopPx = 60;
        public static int CamaOffsetRightPx = 60;
        public static int CamaOffsetBottomPx = 200;

        // Mesita
        public static int MesitaOffsetLeftPx = 20;
        public static int MesitaOffsetTopPx = 20;
        public static int MesitaOffsetRightPx = 20;
        public static int MesitaOffsetBottomPx = 200;

        public static void Build(
            Rectangle borde,
            float scale,
            List<Rectangle> solids,
            out Rectangle doorLeftRect,
            Rectangle cama,
            Rectangle mesita)
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

            // ---------- PAREDES SUPERIOR + DERECHA ----------
            solids.Add(new Rectangle(left, top, right - left, tTop));
            solids.Add(new Rectangle(right - tRight, top, tRight, bottom - top));

            // ---------- PARED IZQUIERDA CON HUECO ----------
            int doorH = (int)(DoorLeftHeightPx * scale);
            int doorY = top + (int)(DoorLeftOffsetYPx * scale);

            solids.Add(new Rectangle(left, top, tLeft, doorY - top));
            solids.Add(new Rectangle(left, doorY + doorH, tLeft, bottom - (doorY + doorH)));

            doorLeftRect = new Rectangle(left, doorY, tLeft, doorH);

            // ---------- PARED INFERIOR ----------
            solids.Add(new Rectangle(left, bottom - tBot, right - left, tBot));

            // ---------- CAMA (con offsets personalizados) ----------
            int camaX = cama.X + (int)(CamaOffsetLeftPx * scale);
            int camaY = cama.Y + (int)(CamaOffsetTopPx * scale);
            int camaW = cama.Width - (int)((CamaOffsetLeftPx + CamaOffsetRightPx) * scale);
            int camaH = cama.Height - (int)((CamaOffsetTopPx + CamaOffsetBottomPx) * scale);

            solids.Add(new Rectangle(camaX, camaY, camaW, camaH));

            // ---------- MESITA (con offsets personalizados) ----------
            int mesitaX = mesita.X + (int)(MesitaOffsetLeftPx * scale);
            int mesitaY = mesita.Y + (int)(MesitaOffsetTopPx * scale);
            int mesitaW = mesita.Width - (int)((MesitaOffsetLeftPx + MesitaOffsetRightPx) * scale);
            int mesitaH = mesita.Height - (int)((MesitaOffsetTopPx + MesitaOffsetBottomPx) * scale);

            solids.Add(new Rectangle(mesitaX, mesitaY, mesitaW, mesitaH));
        }
    }
}
