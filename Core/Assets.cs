using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace EscapeRoom.Core
{
    public static class Assets
    {
        public static SpriteFont Fuente;      // Start + Intro
        public static SpriteFont FuenteTitle; // grande (título)
        public static SpriteFont FuenteInstr; // instrucciones e historia

        // UI/Intro
        public static Texture2D Manu;

        // Bedroom
        public static Texture2D Piso, Borde, Cama, Mesita;

        // Sala
        public static Texture2D SalaPiso, SalaBorde, Cuadros, Reloj;

        // Evan (8 sprites)
        public static Texture2D EvanFrente1, EvanFrente2;
        public static Texture2D EvanEspalda1, EvanEspalda2;
        public static Texture2D EvanIzquierda1, EvanIzquierda2;
        public static Texture2D EvanDerecha1, EvanDerecha2;

        public static Texture2D Pixel;

        public static void Initialize(ContentManager content, GraphicsDevice gd)
        {
            Fuente = content.Load<SpriteFont>("Fonts/fuente");
            FuenteTitle = content.Load<SpriteFont>("Fonts/titulo");
            FuenteInstr = content.Load<SpriteFont>("Fonts/fuenteInstr");
            Manu = content.Load<Texture2D>("Textures/menu");

            // Bedroom
            Piso = content.Load<Texture2D>("Textures/piso");
            Borde = content.Load<Texture2D>("Textures/borde");
            Cama = content.Load<Texture2D>("Textures/cama");
            Mesita = content.Load<Texture2D>("Textures/mesitadeluz");

            // Sala (usa tus nombres exactos en Content/Textures)
            SalaPiso = content.Load<Texture2D>("Textures/Fondo1P");
            SalaBorde = content.Load<Texture2D>("Textures/Fondo1");
            Cuadros = content.Load<Texture2D>("Textures/Cuadros");
            Reloj = content.Load<Texture2D>("Textures/Reloj");

            // Evan
            EvanFrente1 = content.Load<Texture2D>("Textures/EvanFrente1");
            EvanFrente2 = content.Load<Texture2D>("Textures/EvanFrente2");
            EvanEspalda1 = content.Load<Texture2D>("Textures/EvanEspalda1");
            EvanEspalda2 = content.Load<Texture2D>("Textures/EvanEspalda2");
            EvanIzquierda1 = content.Load<Texture2D>("Textures/EvanIzquierda1");
            EvanIzquierda2 = content.Load<Texture2D>("Textures/EvanIzquierda2");
            EvanDerecha1 = content.Load<Texture2D>("Textures/EvanDerecha1");
            EvanDerecha2 = content.Load<Texture2D>("Textures/EvanDerecha2");

            Pixel = new Texture2D(gd, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }
    }
}