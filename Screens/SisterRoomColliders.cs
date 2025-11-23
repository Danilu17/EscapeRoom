//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Xna.Framework;

//namespace EscapeRoom.Screens
//{
//    public static class SisterRoomColliders
//    {
//        // Grosor antes del scale (tomado del patrón de tus otras salas)
//        public static int TopThicknessPx = 110;
//        public static int LeftThicknessPx = 92;
//        public static int RightThicknessPx = 124;
//        public static int BottomThicknessPx = 78;

//        // Hueco izquierdo → vuelve a la Sala
//        public static float LeftDoorHeightPercent = 0.22f;
//        public static float LeftDoorYPercent = 0.58f; // ajustable: 0 = arriba, 1 = abajo

//        public static void Build(
//            Rectangle borde,
//            float scale,
//            List<Rectangle> solids,
//            out Rectangle doorLeftRect
//        )
//        {
//            solids.Clear();

//            int left = borde.Left;
//            int right = borde.Right;
//            int top = borde.Top;
//            int bottom = borde.Bottom;

//            int topTh = (int)(TopThicknessPx * scale);
//            int leftTh = (int)(LeftThicknessPx * scale);
//            int rightTh = (int)(RightThicknessPx * scale);
//            int bottomTh = (int)(BottomThicknessPx * scale);

//            // ----------- PARED SUPERIOR -----------
//            solids.Add(new Rectangle(left, top, right - left, topTh));

//            // ----------- PARED DERECHA COMPLETA -----------
//            solids.Add(new Rectangle(right - rightTh, top, rightTh, bottom - top));

//            // ----------- PARED INFERIOR COMPLETA -----------
//            solids.Add(new Rectangle(left, bottom - bottomTh, right - left, bottomTh));

//            // ----------- PARED IZQUIERDA (con hueco) -----------
//            int doorH = (int)((bottom - top) * LeftDoorHeightPercent);
//            int doorY = top + (int)((bottom - top) * LeftDoorYPercent);

//            // superior del hueco
//            solids.Add(new Rectangle(left, top, leftTh, doorY - top));

//            // inferior del hueco
//            solids.Add(new Rectangle(left, doorY + doorH, leftTh, bottom - (doorY + doorH)));

//            // puerta válida
//            doorLeftRect = new Rectangle(left, doorY, leftTh, doorH);
//        }
//    }
//}
