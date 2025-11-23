using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace EscapeRoom.Screens
{
    /// <summary>
    /// TODAS las medidas de colisión del cuarto de Evan están acá.
    /// Si querés ajustar paredes o puerta o muebles, tocas estos números.
    /// </summary>
    public static class BedroomColliders
    {
        // Grosor de las paredes, en píxeles DE LA TEXTURA (antes del scale)
        public static int TopThicknessPx = 125;
        public static int LeftThicknessPx = 90;
        public static int RightThicknessPx = 126;
        public static int BottomThicknessPx = 50;

        // Tamaño y desplazamiento horizontal de la puerta superior
        public static float DoorWidthPercent = 0.22f;  // % del ancho total
        public static float DoorOffsetXPx = 45f;    // corre la puerta un poco a la derecha

        // ------------ COLLIDERS DE MUEBLES (EDITABLES A MANO) ------------

        // Offsets de la cama (en píxeles de la textura original, antes del scale)
        //  Izq / Arriba / Derecha / Abajo
        public static int CamaOffsetLeftPx = 20;
        public static int CamaOffsetTopPx = 40;
        public static int CamaOffsetRightPx = 20;
        public static int CamaOffsetBottomPx = 150;

        // Offsets de la mesita de luz
        public static int MesitaOffsetLeftPx = 5;
        public static int MesitaOffsetTopPx = 10;
        public static int MesitaOffsetRightPx = 5;
        public static int MesitaOffsetBottomPx = 130;

        /// <summary>
        /// Construye la lista de sólidos y devuelve el rectángulo de la puerta superior.
        /// </summary>
        public static void Build(
            Rectangle borde,
            float scale,
            Rectangle cama,
            Rectangle mesitadeluz,
            List<Rectangle> solids,
            out Rectangle doorTopRect)
        {
            solids.Clear();

            int left = borde.Left;
            int right = borde.Right;
            int top = borde.Top;
            int bottom = borde.Bottom;

            int topThickness = (int)(TopThicknessPx * scale);
            int leftThickness = (int)(LeftThicknessPx * scale);
            int rightThickness = (int)(RightThicknessPx * scale);
            int bottomThickness = (int)(BottomThicknessPx * scale);

            // --- PAREDES ---

            // LATERALES + PARED INFERIOR SÓLIDA
            solids.Add(new Rectangle(left, top, leftThickness, bottom - top));                  // izquierda
            solids.Add(new Rectangle(right - rightThickness, top, rightThickness, bottom - top)); // derecha
            solids.Add(new Rectangle(left, bottom - bottomThickness, right - left, bottomThickness)); // abajo

            // PARED SUPERIOR CON HUECO (puerta real hacia la Sala)
            int doorWidth = (int)((right - left) * DoorWidthPercent);
            int doorOffsetX = (int)(DoorOffsetXPx * scale);
            int doorX = ((left + right) / 2 - doorWidth / 2) + doorOffsetX;

            // trozo izquierdo y derecho sólidos
            solids.Add(new Rectangle(left, top, doorX - left, topThickness));
            solids.Add(new Rectangle(doorX + doorWidth, top, right - (doorX + doorWidth), topThickness));

            // hueco = puerta
            doorTopRect = new Rectangle(doorX, top, doorWidth, topThickness);

            // --- MUEBLES CON COLISIÓN PERSONALIZADA ---

            // CAMA
            int camaX = cama.X + (int)(CamaOffsetLeftPx * scale);
            int camaY = cama.Y + (int)(CamaOffsetTopPx * scale);
            int camaW = cama.Width - (int)((CamaOffsetLeftPx + CamaOffsetRightPx) * scale);
            int camaH = cama.Height - (int)((CamaOffsetTopPx + CamaOffsetBottomPx) * scale);

            solids.Add(new Rectangle(camaX, camaY, camaW, camaH));

            // MESITA DE LUZ
            int mesitaX = mesitadeluz.X + (int)(MesitaOffsetLeftPx * scale);
            int mesitaY = mesitadeluz.Y + (int)(MesitaOffsetTopPx * scale);
            int mesitaW = mesitadeluz.Width - (int)((MesitaOffsetLeftPx + MesitaOffsetRightPx) * scale);
            int mesitaH = mesitadeluz.Height - (int)((MesitaOffsetTopPx + MesitaOffsetBottomPx) * scale);

            solids.Add(new Rectangle(mesitaX, mesitaY, mesitaW, mesitaH));
        }
    }
}
