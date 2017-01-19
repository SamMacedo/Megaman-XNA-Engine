#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Megaman_Final_Fight.GlobalClasses
{
    /// <summary>
    /// Clase que sirve para proporcionar funciones para facilitar el testing
    /// </summary>
    public static class Debug
    {
        #region Campos

        // Referencia al juego
        private static Game1 game;

        // Objeto para capturar pulsaciones de teclado
        private static KeyboardState keyboard;

        // Banderas
        private static bool canPressF1;
        private static bool canPressFpsKeys;

        // Variable para indicar los fps actuales
        private static float fps;

        private static double deltaTime;

        private static StringBuilder sbuilder;

        public static string WindowText;

        #endregion

        #region Inicialización

        public static void Initialize(Game1 game1)
        {
            canPressF1 = false;
            canPressFpsKeys = false;

            game = game1;

            fps = 60;

            sbuilder = new StringBuilder();

            WindowText = String.Empty;
        }

        #endregion

        #region Actualización

        public static void Update(GameTime gameTime)
        {
            keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.F1))
            {
                if (canPressF1)
                {
                    Global.DrawBoundingBoxes = !Global.DrawBoundingBoxes;
                }

                canPressF1 = false;
            }
            else
            {
                canPressF1 = true;
            }



            if (keyboard.IsKeyDown(Keys.Add))
            {
                if (canPressFpsKeys)
                {
                    fps = MathHelper.Clamp(fps + 5, 1, 60);
                    game.TargetElapsedTime = TimeSpan.FromSeconds(1f / fps);
                }

                canPressFpsKeys = false;
            }
            else if (keyboard.IsKeyDown(Keys.Subtract))
            {
                if (canPressFpsKeys)
                {
                    fps = MathHelper.Clamp(fps - 5, 1, 60);
                    game.TargetElapsedTime = TimeSpan.FromSeconds(1f / fps);
                }

                canPressFpsKeys = false;
            }
            else
            {
                canPressFpsKeys = true;
            }

            deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
        }

        #endregion

        #region Dibujado

        public static void Draw(SpriteBatch spritebatch)
        {
            game.Window.Title = WindowText;

            spritebatch.Begin();

            sbuilder.Clear();
            sbuilder.AppendFormat("FPS: {0:N2}", 1 / deltaTime);
            spritebatch.DrawString(Resources.FNT_font1, sbuilder, new Vector2(20, 50), Color.White);

            spritebatch.End();
        }

        #endregion
    }
}
