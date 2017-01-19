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
    /// Clase representante de las plataformas flotantes
    /// </summary>
    public sealed class FloatingPlatform : MovingPlatform
    {
        #region Campos

        // Animaciones
        private AnimationSet animations;

        // Variable auxiliar para guardar la velocidad de la plataforma
        private float tempSpeed;

        // Posibles orientaciones de la plataforma
        public enum Orientations
        {
            Horizontal,
            Vertical
        };
        private Orientations orientation;

        // Posibles direcciones de movimiento de la plataforma
        public enum Directions
        {
            Left,
            Right,
            Up,
            Down
        };
        private Directions initialDirection;
        private Directions currentDirection;

        // Variables constantes
        private const int DefaultWidth = 75;
        private const int DefaultHeight = 45;
        private const float DefaultMaxSpeed = 2f;
        private const float DefaultAcceleration = 0.07f;

        #endregion

        #region Propiedades

        /// <summary>
        /// Indica que la plataforma debe de continuar actualizándose si está 
        /// en el mismo cuarto que megaman
        /// </summary>
        public override bool UpdateIfOnRoom
        {
            get { return true; }
        }

        #endregion

        #region Constructor e inicialización

        /// <summary>
        /// Constructor que inicializa la plataforma
        /// </summary>
        /// <param name="megaman">Referencia a la instancia de megaman</param>
        /// <param name="initialPosition">Posición inicial de la plataforma</param>
        /// <param name="orientation">Orientación de la plataforma</param>
        /// <param name="initialDirection">Dirección inicial de la plataforma</param>
        public FloatingPlatform(Megaman megaman, Vector2 initialPosition, 
            Orientations orientation, Directions initialDirection)
            : base(megaman, initialPosition, DefaultWidth, DefaultHeight, Color.Orange)
        {
            this.orientation = orientation;
            this.initialDirection = initialDirection;

            LoadContent();
            ResetObject();

            jumpOverPlatform = true;
        }

        /// <summary>
        /// Método que carga el contenido de la plataforma
        /// </summary>
        private void LoadContent()
        {
            animations = Resources.ANIM_FloatingPlatform.CreateCopy();
        }

        #endregion

        #region Actualización

        /// <summary>
        /// Método que actualiza la plataforma
        /// </summary>
        /// <param name="gameTime"></param>
        public override void OnUpdate(GameTime gameTime)
        {
            HandleMovement();
            Animate(gameTime);
        }

        /// <summary>
        /// Método que se encarga de manejar el movimiento de la plataforma
        /// </summary>
        private void HandleMovement()
        {
            Speed = Vector2.Zero;

            // Dependiendo de la dirección de la plataforma se revisa si tiene un tile límite, 
            // en caso de tenerlo se cambia la dirección de movimiento, de lo contrario
            // se realiza movimiento con aceleración en la dirección indicada.
            if (orientation == Orientations.Horizontal)
            {
                if (currentDirection == Directions.Right)
                {
                    if (CheckForTileLimit())
                    {
                        ChangueDirection();
                    }
                    else
                    {
                        tempSpeed = MathHelper.Clamp(tempSpeed + DefaultAcceleration, -DefaultMaxSpeed, 
                            DefaultMaxSpeed);
                    }
                }
                else
                {
                    if (CheckForTileLimit())
                    {
                        ChangueDirection();
                    }
                    else
                    {
                        tempSpeed = MathHelper.Clamp(tempSpeed - DefaultAcceleration, -DefaultMaxSpeed, 
                            DefaultMaxSpeed);
                    }
                }

                Speed.X = tempSpeed;
            }
            else
            {
                if (currentDirection == Directions.Up)
                {
                    if (CheckForTileLimit())
                    {
                        ChangueDirection();
                    }
                    else
                    {
                        tempSpeed = MathHelper.Clamp(tempSpeed - DefaultAcceleration, -DefaultMaxSpeed, 
                            DefaultMaxSpeed);
                    }
                }
                else
                {
                    if (CheckForTileLimit())
                    {
                        ChangueDirection();
                    }
                    else
                    {
                        tempSpeed = MathHelper.Clamp(tempSpeed + DefaultAcceleration, -DefaultMaxSpeed, 
                            DefaultMaxSpeed);
                    }
                }

                Speed.Y = tempSpeed;
            }
        }

        /// <summary>
        /// Método que revisa si la plataforma colisiona con un tile límite
        /// </summary>
        /// <returns>Regresa true en caso de que la plataforma colisione con 
        /// un tile límite</returns>
        private bool CheckForTileLimit()
        {
            float x = 0;
            float y = 0;
            int type;

            // Dependiendo de la dirección de movimiento se determina el punto
            // en el que se revisarán colisiones
            switch (currentDirection)
            {
                case Directions.Right:
                    x = Right;
                    y = Center.Y;
                    break;
                case Directions.Left:
                    x = Left;
                    y = Center.Y;
                    break;
                case Directions.Up:
                    x = Center.X;
                    y = Top;
                    break;
                case Directions.Down:
                    x = Center.X;
                    y = Bottom;
                    break;
            }

            // Se obtiene el tipo de tile
            type = GameTiles.CheckTile(Global.MovingPlatformsLayer, (int)x, (int)y);

            // Evaluación del tipo de tile
            return type == GameTiles.FloatingPlatform_Limit;
        }

        /// <summary>
        /// Método que simplemente cambia la dirección de movimiento de la plataforma
        /// </summary>
        private void ChangueDirection()
        {
            if (orientation == Orientations.Horizontal)
            {
                if (currentDirection == Directions.Right)
                {
                    currentDirection = Directions.Left;
                }
                else
                {
                    currentDirection = Directions.Right;
                }
            }
            else
            {
                if (currentDirection == Directions.Up)
                {
                    currentDirection = Directions.Down;
                }
                else
                {
                    currentDirection = Directions.Up;
                }
            }
        }

        #endregion

        #region Animación

        /// <summary>
        /// Método que actualiza la plataforma
        /// </summary>
        /// <param name="gameTime"></param>
        private void Animate(GameTime gameTime)
        {
            animations.Update(gameTime);
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
            // Reinicio de la dirección
            if (orientation == Orientations.Horizontal)
            {
                if (initialDirection == Directions.Right)
                {
                    currentDirection = Directions.Right;
                }
                else
                {
                    currentDirection = Directions.Left;
                }
            }
            else
            {
                if (initialDirection == Directions.Up)
                {
                    currentDirection = Directions.Up;
                }
                else
                {
                    currentDirection = Directions.Down;
                }
            }

            animations.StartAnimation("Default");
            tempSpeed = 0f;

            base.ResetObject();
        }

        #endregion
    }
}
