#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#endregion

namespace Megaman_Final_Fight.GlobalClasses
{
    /// <summary>
    /// Clase que se encarga de administrar y capturar las pulsaciones de teclado
    /// y de botones del control de xbox
    /// </summary>
    public static class InputManager
    {
        #region Campos

        // Indice actual de jugador
        public static PlayerIndex playerIndex;

        /// <summary>
        /// Distintos tipos de botones que permiten interactuar con el juego
        /// </summary>
        public enum Buttons
        {
            Up,
            Down,
            Left,
            Right,
            Shoot,
            Jump,
            Start,
            Back
        };

        // Variables para capturar las pulsaciones
        private static KeyboardState keyboardState;
        private static GamePadState gamePadState;

        // Límite a partir del cual se considera presionado un stick analógico del control
        // del xbox
        private const float deadZone = 0.25f;

        #endregion

        #region Métodos

        /// <summary>
        /// Método que simplemente se encarga de obtener pulsaciones de teclado y de 
        /// control de xbox
        /// </summary>
        public static void Update()
        {
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(playerIndex);
        }

        /// <summary>
        /// Método que determina si el botón indicado se encuentra presionado
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool ButtonDown(Buttons button)
        {
            switch (button)
            {
                case Buttons.Up:
                    return keyboardState.IsKeyDown(Keys.Up) 
                        || gamePadState.ThumbSticks.Left.Y > deadZone || gamePadState.DPad.Up == ButtonState.Pressed;

                case Buttons.Down:
                    return keyboardState.IsKeyDown(Keys.Down) 
                        || gamePadState.ThumbSticks.Left.Y < -deadZone || gamePadState.DPad.Down == ButtonState.Pressed;

                case Buttons.Right:
                    return keyboardState.IsKeyDown(Keys.Right)
                        || gamePadState.ThumbSticks.Left.X > deadZone || gamePadState.DPad.Right == ButtonState.Pressed;

                case Buttons.Left:
                    return keyboardState.IsKeyDown(Keys.Left) 
                        || gamePadState.ThumbSticks.Left.X < -deadZone || gamePadState.DPad.Left == ButtonState.Pressed;

                case Buttons.Jump:
                    return keyboardState.IsKeyDown(Keys.Z) || gamePadState.Buttons.A == ButtonState.Pressed;

                case Buttons.Shoot:
                    return keyboardState.IsKeyDown(Keys.X) || gamePadState.Buttons.X == ButtonState.Pressed;

                case Buttons.Start:
                    return keyboardState.IsKeyDown(Keys.Enter) || gamePadState.Buttons.Start == ButtonState.Pressed;

                case Buttons.Back:
                    return keyboardState.IsKeyDown(Keys.Back) || gamePadState.Buttons.Back == ButtonState.Pressed;
            }

            return false;
        }

        #endregion
    }
}
