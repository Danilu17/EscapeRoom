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
        static ContentManager _content;

        // Fuentes
        public static SpriteFont Fuente;
        public static SpriteFont FuenteTitle;
        public static SpriteFont FuenteInstr;

        // UI/Intro
        public static Texture2D Manu;

        // Cuarto de Evan
        public static Texture2D Piso;
        public static Texture2D Borde;
        public static Texture2D Cama;
        public static Texture2D MesitaDeLuz;

        // Sala
        public static Texture2D SalaPiso;
        public static Texture2D SalaBorde;
        public static Texture2D Cuadros1;
        public static Texture2D Cuadros2;
        public static Texture2D Reloj;

        // Cuarto hermana
        public static Texture2D CamaRosa;
        public static Texture2D Recuadro;
        public static Texture2D Sillon;

        // Puertas / TV
        public static Texture2D Puerta;
        public static Texture2D Tv;
        public static Texture2D Mesita;

        // Osito / susto
        public static Texture2D Osito;

        // Fondos
        public static Texture2D Alfombra;
        public static Texture2D Fondo2;
        public static Texture2D Fondo2P;
        public static Texture2D Fondo3;
        public static Texture2D Fondo3P;

        // Evan (8 sprites)
        public static Texture2D EvanFrente1, EvanFrente2;
        public static Texture2D EvanEspalda1, EvanEspalda2;
        public static Texture2D EvanIzquierda1, EvanIzquierda2;
        public static Texture2D EvanDerecha1, EvanDerecha2;

        // Pixel (debug)
        public static Texture2D Pixel;

        // ===== Minijuegos / extras =====
        public static Texture2D Player;        // player.png (Maze)
        public static Texture2D Enemigo;       // enemigo.png (Maze)
        public static Texture2D Llave;         // llave.png
        public static Texture2D Jumpscare;     // jumpscare.png
        public static Texture2D SustoOsito;    // sustoOsito.png

        public static Texture2D BallonBoy;     // BallonBoy.png (Globos)
        public static Texture2D Globo;         // Globo.png
        public static Texture2D Nube;          // Nube.png
        public static Texture2D Marco;         // Marco.png
        public static Texture2D Plataforma;    // Plataforma.png
        public static Texture2D Meta;          // meta.png

        public static Texture2D AutoEvan;      // auto_evan.png (Racing)
        public static Texture2D AutoEnemigo;   // auto_enemigo.png (Racing)

        public static void Initialize(ContentManager content, GraphicsDevice gd)
        {
            _content = content;

            // Fuentes
            Fuente = content.Load<SpriteFont>("Fonts/fuente");
            FuenteTitle = content.Load<SpriteFont>("Fonts/titulo");
            FuenteInstr = content.Load<SpriteFont>("Fonts/fuenteInstr");

            // UI / Intro
            Manu = content.Load<Texture2D>("Textures/menu");

            // Cuarto de Evan
            Piso = content.Load<Texture2D>("Textures/piso");
            Borde = content.Load<Texture2D>("Textures/borde");
            Cama = content.Load<Texture2D>("Textures/cama");
            MesitaDeLuz = content.Load<Texture2D>("Textures/mesitadeluz");

            // Sala / Hall
            SalaBorde = content.Load<Texture2D>("Textures/Fondo1P");
            SalaPiso = content.Load<Texture2D>("Textures/Fondo1");
            Cuadros1 = content.Load<Texture2D>("Textures/Cuadros1");
            Cuadros2 = content.Load<Texture2D>("Textures/Cuadros2");
            Reloj = content.Load<Texture2D>("Textures/Reloj");

            // Objetos adicionales
            CamaRosa = content.Load<Texture2D>("Textures/CamaRosa");
            Recuadro = content.Load<Texture2D>("Textures/Recuadro");
            Sillon = content.Load<Texture2D>("Textures/Sillon");
            Puerta = content.Load<Texture2D>("Textures/Puerta");
            Tv = content.Load<Texture2D>("Textures/Tv");
            Mesita = content.Load<Texture2D>("Textures/Mesita");

            // Osito
            Osito = content.Load<Texture2D>("Textures/osito");

            // Fondos
            Alfombra = content.Load<Texture2D>("Textures/Alfombra");
            Fondo2 = content.Load<Texture2D>("Textures/Fondo2");
            Fondo2P = content.Load<Texture2D>("Textures/Fondo2P");
            Fondo3 = content.Load<Texture2D>("Textures/Fondo3");
            Fondo3P = content.Load<Texture2D>("Textures/Fondo3P");

            // EVAN Sprites
            EvanFrente1 = content.Load<Texture2D>("Textures/EvanFrente1");
            EvanFrente2 = content.Load<Texture2D>("Textures/EvanFrente2");
            EvanEspalda1 = content.Load<Texture2D>("Textures/EvanEspalda1");
            EvanEspalda2 = content.Load<Texture2D>("Textures/EvanEspalda2");
            EvanIzquierda1 = content.Load<Texture2D>("Textures/EvanIzquierda1");
            EvanIzquierda2 = content.Load<Texture2D>("Textures/EvanIzquierda2");
            EvanDerecha1 = content.Load<Texture2D>("Textures/EvanDerecha1");
            EvanDerecha2 = content.Load<Texture2D>("Textures/EvanDerecha2");

            // Minijuegos / extras
            Player = content.Load<Texture2D>("Textures/player");
            Enemigo = content.Load<Texture2D>("Textures/enemigo");
            Llave = content.Load<Texture2D>("Textures/llave");
            Jumpscare = content.Load<Texture2D>("Textures/jumpscare");
            SustoOsito = content.Load<Texture2D>("Textures/sustoOsito");

            BallonBoy = content.Load<Texture2D>("Textures/BallonBoy");
            Globo = content.Load<Texture2D>("Textures/Globo");
            Nube = content.Load<Texture2D>("Textures/Nube");
            Marco = content.Load<Texture2D>("Textures/Marco");
            Plataforma = content.Load<Texture2D>("Textures/Plataforma");
            Meta = content.Load<Texture2D>("Textures/meta");

            AutoEvan = content.Load<Texture2D>("Textures/auto_evan");
            AutoEnemigo = content.Load<Texture2D>("Textures/auto_enemigo");

            // Pixel (debug)
            Pixel = new Texture2D(gd, 1, 1);
            Pixel.SetData(new[] { Color.White });
        }

        // Helper opcional por si lo usas en otros lados
        public static T Load<T>(string assetName) => _content.Load<T>(assetName);
    }
}
