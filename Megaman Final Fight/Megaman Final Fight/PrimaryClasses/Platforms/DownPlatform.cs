#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Megaman_Final_Fight.GlobalClasses;
using Megaman_Final_Fight.HelperClasses;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using AnimationSystem;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses.Platforms
{
    /// <summary>
    /// Clase representante de las plataformas verdes que se impulsan hacia abajo
    /// </summary>
    public sealed class DownPlatform : MovingPlatform
    {
        #region Campos

        // Indica si la plataforma está activada (realizando movimiento) 
        private bool activated;

        // Indica si la plataforma está en el proceso de rebote
        private bool onBounce;

        // Tiempo que la plataforma durará en el rebote
        private int bounceTime;

        private int AfterBounceWaitTime;
        private bool onAfterBounceWaitTime;

        // Animaciones del a plataforma
        private AnimationSet animations;

        // Constantes
        private const int MaxBounceTime = 100;
        private const int MaxAfterBounceWaitTime = 100;
        private const float NormalSpeed = 10f;
        private const float BounceSpeed = 2.5f;

        #endregion

        #region Propiedades

        /// <summary>
        /// Indica que una vez que la plataforma salió de la cámara entonces se resetea
        /// </summary>
        public override bool UpdateIfOnRoom
        {
            get { return false; }
        }

        #endregion

        #region Constructor e inicialización

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="megaman">Referencia a la instancia de megaman</param>
        /// <param name="initialPosition">Posición inicial de la plataforma</param>
        public DownPlatform(Megaman megaman, Vector2 initialPosition)
            : base(megaman, initialPosition, 128, 64, Color.Red)
        {
            LoadContent();
            ResetObject();
        }

        /// <summary>
        /// Método que carga el contenido de las animaciones
        /// </summary>
        public void LoadContent()
        {
            animations = Resources.ANIM_DownPlatform.CreateCopy();
        }

        #endregion

        #region Actualización

        /// <summary>
        /// Método que actualiza la plataforma
        /// </summary>
        /// <param name="gameTime"></param>
        public override void OnUpdate(GameTime gameTime)
        {
            animations.Update(gameTime);

            Speed = Vector2.Zero;

            // Movimiento de la plataforma activada
            if (activated)
            {
                // Aquí se maneja el movimiento de la plataforma, después de realizar el movimiento
                // de rebote durante cierto tiempo, se cancela el rebote y se inicia el proceso
                // de movimiento hacia abajo
                if (onBounce)
                {
                    Speed.Y = BounceSpeed;

                    bounceTime += gameTime.ElapsedGameTime.Milliseconds;

                    if (bounceTime >= MaxBounceTime)
                    {
                        onBounce = false;
                        onAfterBounceWaitTime = true;
                    }
                }
                else
                {
                    if (onAfterBounceWaitTime)
                    {
                        AfterBounceWaitTime += gameTime.ElapsedGameTime.Milliseconds;

                        if (AfterBounceWaitTime >= MaxAfterBounceWaitTime)
                        {
                            onAfterBounceWaitTime = false;

                            Resources.SFX_DownMovingPlatform.Play();
                        }
                    }
                    else
                    {
                        Speed.Y = NormalSpeed;
                    }
                }
            }
            // Activar si megaman se para encima de la plataforma
            else
            {
                if (HasMegamanAbove)
                {
                    onBounce = true;
                    activated = true;
                }
            }
        }

        #endregion

        #region Dibujado

        /// <summary>
        /// Método que dibuja la plataforma
        /// </summary>
        /// <param name="spritebatch"></param>
        public override void OnDraw(SpriteBatch spritebatch)
        {
            animations.Draw(spritebatch, CenteredDrawPosition, Color.White);
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Método que reinicia la plataforma
        /// </summary>
        public override void ResetObject()
        {
            activated = false;
            onBounce = false;
            bounceTime = 0;

            AfterBounceWaitTime = 0;
            onAfterBounceWaitTime = false;

            animations.RestartAnimation();
            animations.StartAnimation("Default");

            base.ResetObject();
        }

        #endregion
    }
}
