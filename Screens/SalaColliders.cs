using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace EscapeRoom.Screens
{
    /// <summary>
    /// Todas las medidas de colisión de la Sala/Hall.
    /// Acá podés ajustar paredes y puerta inferior.
    /// </summary>
    public static class SalaColliders
    {
        // Grosor de paredes según la textura (antes del scale)
        public static int TopThicknessPx = 300;
        public static int LeftThicknessPx = 92;
        public static int RightThicknessPx = 90;
        public static int BottomThicknessPx = 17;

        // Puerta inferior (que conecta con el cuarto de Evan)
        public static float DoorWidthPercent = 0.22f; // porcentaje del ancho total
        public static float DoorOffsetXPx = 61f;   // + derecha, - izquierda (en píxeles de la textura)

        public static void Build(
            Rectangle borde,
            float scale,
            List<Rectangle> solids,
            out Rectangle doorBottomRect)
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

            // PARED SUPERIOR COMPLETA
            solids.Add(new Rectangle(left, top, right - left, topThickness));

            // LATERALES
            solids.Add(new Rectangle(left, top, leftThickness, bottom - top));
            solids.Add(new Rectangle(right - rightThickness, top, rightThickness, bottom - top));

            // PARED INFERIOR CON HUECO (puerta al cuarto)
            int doorWidth = (int)((right - left) * DoorWidthPercent);
            int doorOffsetX = (int)(DoorOffsetXPx * scale);

            // puerta centrada pero corrida por el offset
            int doorX = (left + right) / 2 - doorWidth / 2 + doorOffsetX;

            // Partes sólidas izquierda/derecha
            solids.Add(new Rectangle(left, bottom - bottomThickness, doorX - left, bottomThickness));
            solids.Add(new Rectangle(doorX + doorWidth, bottom - bottomThickness,
                                     right - (doorX + doorWidth), bottomThickness));

            // Hueco = puerta del medio inferior
            doorBottomRect = new Rectangle(doorX, bottom - bottomThickness, doorWidth, bottomThickness);
        }
    }
}
