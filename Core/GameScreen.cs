using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace EscapeRoom.Core
{
    public abstract class GameScreen : Screen
    {
        private bool _contentLoaded;

        // por qué: fallo temprano y claro si ScreenManager no fue inicializado
        protected Game Game => ScreenManager.Game
            ?? throw new InvalidOperationException("ScreenManager.Game es null. Llama ScreenManager.Initialize() antes de Push.");
        protected SpriteBatch SpriteBatch => ScreenManager.Batcher
            ?? throw new InvalidOperationException("ScreenManager.Batcher es null. Llama ScreenManager.Initialize() antes de Push.");

        public override void OnPush()
        {
            base.OnPush();

            if (_contentLoaded) return;

            try
            {
                LoadContent();
                _contentLoaded = true;
                System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] LoadContent OK");
            }
            catch (Exception ex)
            {
                // por qué: si truena la carga, lo verás en Output y no “parece” cierre silencioso
                System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] LoadContent FAILED: {ex}");
                throw;
            }
        }

        public override void OnPop()
        {
            base.OnPop();

            if (!_contentLoaded) return;

            try
            {
                UnloadContent();
                _contentLoaded = false;
                System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] UnloadContent OK");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] UnloadContent FAILED: {ex}");
                // no relanzamos; queremos salir igual
            }
        }

        /// <summary>Carga recursos de la pantalla (texturas, fuentes, etc.).</summary>
        protected abstract void LoadContent();

        /// <summary>Descarga/Dispose de recursos si aplica.</summary>
        protected virtual void UnloadContent() { }
    }
}
