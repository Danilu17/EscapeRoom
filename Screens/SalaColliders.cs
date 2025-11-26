using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace EscapeRoom.Screens
{
    /// <summary>
    /// Colisiones de la sala central:
    /// - Puerta inferior → Bedroom
    /// - Puerta derecha → Cuarto hermana
    /// - Puerta izquierda → Sala2
    /// </summary>
    public static class SalaColliders
    {
        public static int TopThicknessPx = 300;
        public static int LeftThicknessPx = 92;
        public static int RightThicknessPx = 90;
        public static int BottomThicknessPx = 13;

        // Puerta inferior (cuarto de Evan)
        public static float DoorBottomWidthPercent = 0.22f;
        public static float DoorBottomOffsetXPx = 61f;

        // Puerta derecha (cuarto hermana)
        public static int DoorRightHeightPx = 800;
        public static int DoorRightOffsetYPx = 550;

        // Puerta izquierda (Sala2)
        public static int DoorLeftHeightPx = 800;
        public static int DoorLeftOffsetYPx = 550;

        public static void Build(
            Rectangle borde,
            float scale,
            List<Rectangle> solids,
            out Rectangle doorBottomRect,
            out Rectangle doorRightRect,
            out Rectangle doorLeftRect)
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

            // ---------------- TOP ----------------
            solids.Add(new Rectangle(left, top, right - left, tTop));

            // ------------- LEFT WALL + DOOR (Sala2) -------------
            int doorLH = (int)(DoorLeftHeightPx * scale);
            int doorLY = top + (int)(DoorLeftOffsetYPx * scale);

            // tramo sólido arriba
            solids.Add(new Rectangle(left, top, tLeft, doorLY - top));
            // tramo sólido abajo
            solids.Add(new Rectangle(left, doorLY + doorLH, tLeft, bottom - (doorLY + doorLH)));
            // hueco = puerta izquierda
            doorLeftRect = new Rectangle(left, doorLY, tLeft, doorLH);

            // ------------- RIGHT WALL + DOOR (Sister) -------------
            int doorRH = (int)(DoorRightHeightPx * scale);
            int doorRY = top + (int)(DoorRightOffsetYPx * scale);

            solids.Add(new Rectangle(right - tRight, top, tRight, doorRY - top));                 // tramo superior
            solids.Add(new Rectangle(right - tRight, doorRY + doorRH, tRight, bottom - (doorRY + doorRH))); // tramo inferior

            doorRightRect = new Rectangle(right - tRight, doorRY, tRight, doorRH);

            // ---------------- BOTTOM + DOOR (Bedroom) ----------------
            int doorBW = (int)((right - left) * DoorBottomWidthPercent);
            int doorBX = (left + right) / 2 - doorBW / 2 + (int)(DoorBottomOffsetXPx * scale);

            solids.Add(new Rectangle(left, bottom - tBot, doorBX - left, tBot)); // izquierda sólida
            solids.Add(new Rectangle(doorBX + doorBW, bottom - tBot,
                                     right - (doorBX + doorBW), tBot));         // derecha sólida

            doorBottomRect = new Rectangle(doorBX, bottom - tBot, doorBW, tBot);
        }
    }
}
