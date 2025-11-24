using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace EscapeRoom.Screens
{
    public static class SalaColliders
    {
        public static int TopThicknessPx = 300;
        public static int LeftThicknessPx = 92;
        public static int RightThicknessPx = 90;
        public static int BottomThicknessPx = 15;

        public static float DoorBottomWidthPercent = 0.22f;
        public static float DoorBottomOffsetXPx = 61f;

        public static int DoorRightHeightPx = 800;
        public static int DoorRightOffsetYPx = 550;

        public static void Build(
            Rectangle borde,
            float scale,
            List<Rectangle> solids,
            out Rectangle doorBottomRect,
            out Rectangle doorRightRect)
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

            // Superior
            solids.Add(new Rectangle(left, top, right - left, tTop));

            // Izquierda
            solids.Add(new Rectangle(left, top, tLeft, bottom - top));

            // ----- Pared derecha con hueco -----
            int doorRH = (int)(DoorRightHeightPx * scale);
            int doorRY = top + (int)(DoorRightOffsetYPx * scale);

            solids.Add(new Rectangle(right - tRight, top, tRight, doorRY - top));
            solids.Add(new Rectangle(right - tRight, doorRY + doorRH, tRight, bottom - (doorRY + doorRH)));

            doorRightRect = new Rectangle(
                right - tRight,
                doorRY,
                tRight,
                doorRH);

            // ----- Pared inferior con hueco -----
            int doorBW = (int)((right - left) * DoorBottomWidthPercent);
            int doorBX = (left + right) / 2 - doorBW / 2 + (int)(DoorBottomOffsetXPx * scale);

            solids.Add(new Rectangle(left, bottom - tBot, doorBX - left, tBot));
            solids.Add(new Rectangle(doorBX + doorBW, bottom - tBot, right - (doorBX + doorBW), tBot));

            doorBottomRect = new Rectangle(
                doorBX,
                bottom - tBot,
                doorBW,
                tBot);
        }
    }
}
