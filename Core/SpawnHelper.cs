using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace EscapeRoom.Core
{
    public enum DoorSide { Left, Right, Top, Bottom }

    public static class SpawnHelper
    {
        // Por qué: centraliza alineación, clamp y "meterse" adentro sin colisionar.
        public static Vector2 SpawnAtDoor(
            Rectangle door,
            int evW, int evH,
            DoorSide side,
            float desiredCenter = float.NaN,
            float margin = 2f,
            float inset = 5f)
        {
            switch (side)
            {
                case DoorSide.Left:
                    {
                        float cy = float.IsNaN(desiredCenter) ? door.Center.Y : desiredCenter;
                        cy = MathHelper.Clamp(cy, door.Top + evH / 2f + margin, door.Bottom - evH / 2f - margin);
                        float y = cy - evH / 2f;
                        float x = door.Right + inset;
                        return new Vector2(x, y);
                    }
                case DoorSide.Right:
                    {
                        float cy = float.IsNaN(desiredCenter) ? door.Center.Y : desiredCenter;
                        cy = MathHelper.Clamp(cy, door.Top + evH / 2f + margin, door.Bottom - evH / 2f - margin);
                        float y = cy - evH / 2f;
                        float x = door.Left - evW - inset;
                        return new Vector2(x, y);
                    }
                case DoorSide.Top:
                    {
                        float cx = float.IsNaN(desiredCenter) ? door.Center.X : desiredCenter;
                        cx = MathHelper.Clamp(cx, door.Left + evW / 2f + margin, door.Right - evW / 2f - margin);
                        float x = cx - evW / 2f;
                        float y = door.Bottom + inset;
                        return new Vector2(x, y);
                    }
                case DoorSide.Bottom:
                default:
                    {
                        float cx = float.IsNaN(desiredCenter) ? door.Center.X : desiredCenter;
                        cx = MathHelper.Clamp(cx, door.Left + evW / 2f + margin, door.Right - evW / 2f - margin);
                        float x = cx - evW / 2f;
                        float y = door.Top - evH - inset;
                        return new Vector2(x, y);
                    }
            }
        }
    }

    // Tests ligeros (Debug.Assert-like sin dependencias)
    public static class SpawnHelperTests
    {
        public static void Run()
        {
            // Rect hueco vertical (puerta derecha)
            var doorV = new Rectangle(1000, 200, 20, 600);
            int evW = 64, evH = 96;

            // 1) Clamp vertical: desiredCenter por fuera → entra clampeado
            var p1 = SpawnHelper.SpawnAtDoor(doorV, evW, evH, DoorSide.Right, desiredCenter: 10000);
            System.Diagnostics.Debug.Assert(p1.Y >= doorV.Top && (p1.Y + evH) <= doorV.Bottom, "Clamp Y fallo (Right)");

            // 2) Pos lado correcto (Right → x a la izquierda del hueco)
            System.Diagnostics.Debug.Assert(p1.X + evW <= doorV.Left, "Posición X incorrecta (Right)");

            // 3) Puerta izquierda
            var p2 = SpawnHelper.SpawnAtDoor(doorV, evW, evH, DoorSide.Left, desiredCenter: -9999);
            System.Diagnostics.Debug.Assert(p2.Y >= doorV.Top && (p2.Y + evH) <= doorV.Bottom, "Clamp Y fallo (Left)");
            System.Diagnostics.Debug.Assert(p2.X >= doorV.Right, "Posición X incorrecta (Left)");

            // 4) Puerta inferior (horizontal)
            var doorH = new Rectangle(300, 900, 400, 20);
            var p3 = SpawnHelper.SpawnAtDoor(doorH, evW, evH, DoorSide.Bottom, desiredCenter: -9999);
            System.Diagnostics.Debug.Assert(p3.X >= doorH.Left && (p3.X + evW) <= doorH.Right, "Clamp X fallo (Bottom)");
            System.Diagnostics.Debug.Assert(p3.Y + evH <= doorH.Top, "Posición Y incorrecta (Bottom)");
        }
    }
}
