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
    /// Clase representante de la plataforma que después de cada cierto tiempo
    /// cambia la orientación de los picos
    /// </summary>
    public sealed class SpikePlatform : MovingPlatform
    {
        #region Campos

        // Animaciones de la plataforma
        private AnimationSet animations;

        // Contadores de tiempo
        private Timer idleTimer;
        private Timer flashTimer;
        private Timer flipTimer;

        // Posibles orientaciones de los picos de la plataforma
        public enum Orientation
        {
            Horizontal,
            Vertical
        }
        private Orientation initialOrientation;
        private Orientation currentOrientation;

        // Objetos para los picos de la plataforma
        private Spikes leftSpike;
        private Spikes rightSpike;
        private Spikes topSpike;
        private Spikes bottomSpike;

        // Variables Constantes
        private const int Damage = 10;
        private const int DefaultWidth = 64;
        private const int DefaultHeight = 64;
        private const int HorizontalSpikesWidth = 18;
        private const int HorizontalSpikesHeight = 50;
        private const int VerticalSpikesWidth = 50;
        private const int VerticalSpikesHeight = 18;

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
        /// <param name="initialOrientation">Orientación inicial de la plataforma</param>
        public SpikePlatform(Megaman megaman, Vector2 initialPosition, Orientation initialOrientation)
            : base(megaman, initialPosition, DefaultWidth, DefaultHeight, Color.SkyBlue)
        {
            this.initialOrientation = initialOrientation;

            // Inicialización de contadores de tiempo
            idleTimer = new Timer(3000);
            flashTimer = new Timer(1200);
            flipTimer = new Timer(380);

            // Inicialización de picos
            leftSpike = new Spikes();
            rightSpike = new Spikes();
            topSpike = new Spikes();
            bottomSpike = new Spikes();

            // Asignación de las dimensiones que no cambian a los picos
            leftSpike.Height = HorizontalSpikesHeight;
            rightSpike.Height = HorizontalSpikesHeight;
            topSpike.Width = VerticalSpikesWidth;
            bottomSpike.Width = VerticalSpikesWidth;

            LoadContent();
            ResetObject();

            // Suscripción a eventos
            idleTimer.TimerReachedZero += new Timer.TimerStopedHandler(idleTimer_TimerReachedZero);
            flashTimer.TimerReachedZero += new Timer.TimerStopedHandler(flashTimer_TimerReachedZero);
            flipTimer.TimerReachedZero += new Timer.TimerStopedHandler(flipTimer_TimerReachedZero);
        }

        /// <summary>
        /// Método que carga el contenido de la plataforma
        /// </summary>
        private void LoadContent()
        {
            animations = Resources.ANIM_SpikePlatform.CreateCopy();
        }

        #endregion

        #region Suscripciones a eventos necesarios

        /// <summary>
        /// En cuanto finaliza el tiempo de espera entonces comienza el contador de flasheo
        /// para indicar un próximo cambio de orientación
        /// </summary>
        void idleTimer_TimerReachedZero()
        {
            flashTimer.Start();
        }

        /// <summary>
        /// En cuanto finaliza el tiempo de flasheo entonces comienza el contador de volteo
        /// para poder reproducir la animación
        /// </summary>
        void flashTimer_TimerReachedZero()
        {
            flipTimer.Start();
        }

        /// <summary>
        /// En cuanto termina el tiempo de volteo entonces se comienza el contador de tiempo
        /// de espera para continuar intercalando entre orientaciones, tambié se cambia la orientación 
        /// y se reinician los picos de la plataforma
        /// </summary>
        void flipTimer_TimerReachedZero()
        {
            idleTimer.Start();

            if (currentOrientation == Orientation.Horizontal)
            {
                currentOrientation = Orientation.Vertical;
            }
            else
            {
                currentOrientation = Orientation.Horizontal;
            }

            ResetSpikes();
        }

        #endregion

        #region Actualización

        /// <summary>
        /// Método que actualiza la plataforma
        /// </summary>
        /// <param name="gameTime"></param>
        public override void OnUpdate(GameTime gameTime)
        {
            // Actualización de contadores de tiempo
            idleTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            flashTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            flipTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            HandleSpikesPositioning();

            CheckMegamanDamage();

            Animate(gameTime);
        }

        /// <summary>
        /// Método que se encarga de posicionar correctamente los picos en la plataforma
        /// en base al tamaño de los mismos
        /// </summary>
        private void HandleSpikesPositioning()
        {
            if (flipTimer.IsActive)
            {
                float amount = flipTimer.ElapsedTimePercentage;

                // Variables para almacenar las nuevas dimensiones de los picos
                int newHorizontalWidth;
                int newVerticalHeight;

                // Dependiendo de la dirección se cambian las dimensiones de los picos
                if (currentOrientation == Orientation.Horizontal)
                {
                    // Se utiliza la función Lerp para interpolar entre las dimensiones, se hace una suma al amount
                    // de los picos que se están escondiendo para reducir el tamaño mas rápidamente, y por otra
                    // parte se hace una resta a los picos que están saliendo para que su tamaño permanezca una pequeña
                    // cantidad de tiempo en cero.
                    newHorizontalWidth = (int)MathHelper.Lerp(HorizontalSpikesWidth, 0, Math.Min(1, amount + 0.60f));
                    newVerticalHeight = (int)MathHelper.Lerp(0, VerticalSpikesHeight, Math.Max(0, amount - 0.50f));
                }
                else
                {
                    newHorizontalWidth = (int)MathHelper.Lerp(0, HorizontalSpikesWidth, Math.Max(0, amount - 0.50f));
                    newVerticalHeight = (int)MathHelper.Lerp(VerticalSpikesHeight, 0, Math.Min(1, amount + 0.60f));
                }

                // Ajuste de dimensiones de los picos
                leftSpike.Width = newHorizontalWidth;
                rightSpike.Width = newHorizontalWidth;
                topSpike.Height = newVerticalHeight;
                bottomSpike.Height = newVerticalHeight;
            }

            // Posicionado de los picos izquierdo y derecho
            leftSpike.Position.X = Left - leftSpike.Width;
            leftSpike.Position.Y = Center.Y - (leftSpike.Height / 2);
            rightSpike.Position.X = Right;
            rightSpike.Position.Y = Center.Y - (rightSpike.Height / 2);

            // Posicionado de los picos superior e inferior
            topSpike.Position.X = Center.X - (topSpike.Width / 2);
            topSpike.Position.Y = Top - topSpike.Height;
            bottomSpike.Position.X = Center.X - (bottomSpike.Width / 2);
            bottomSpike.Position.Y = Bottom;
        }

        #endregion

        #region Animación

        /// <summary>
        /// Método que anima la plataforma
        /// </summary>
        /// <param name="gameTime"></param>
        private void Animate(GameTime gameTime)
        {
            if (idleTimer.IsActive)
            {
                if (currentOrientation == Orientation.Horizontal)
                {
                    animations.StartAnimation("IdleHorizontal");
                }
                else
                {
                    animations.StartAnimation("IdleVertical");
                }
            }
            else
            {
                if (flashTimer.IsActive)
                {
                    if (currentOrientation == Orientation.Horizontal)
                    {
                        animations.StartAnimation("FlashHorizontal");
                    }
                    else
                    {
                        animations.StartAnimation("FlashVertical");
                    }
                }
                else
                {
                    if (currentOrientation == Orientation.Horizontal)
                    {
                        animations.StartAnimation("FlipHorizontal");
                    }
                    else
                    {
                        animations.StartAnimation("FlipVertical");
                    }
                }
            }

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
            // Dibujado de la plataforma
            animations.Draw(spritebatch, CenteredDrawPosition, Color.White);
            
            // Dibujado de los picos
            leftSpike.Draw(spritebatch);
            rightSpike.Draw(spritebatch);
            topSpike.Draw(spritebatch);
            bottomSpike.Draw(spritebatch);
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Método que se encarga de resetear la plataforma
        /// </summary>
        public override void ResetObject()
        {
            // Asignación de orientación y animación
            if (initialOrientation == Orientation.Horizontal)
            {
                currentOrientation = Orientation.Horizontal;

                animations.StartAnimation("IdleHorizontal");
            }
            else
            {
                currentOrientation = Orientation.Vertical;

                animations.StartAnimation("IdleVertical");
            }

            ResetSpikes();

            // Reinicio de contadores de tiempo
            idleTimer.Restart();
            flashTimer.Stop();
            flipTimer.Stop();

            base.ResetObject();
        }

        /// <summary>
        /// Método que asigna las dimensiones a los picos dependiendo de la orientación
        /// </summary>
        private void ResetSpikes()
        {
            if (currentOrientation == Orientation.Horizontal)
            {
                leftSpike.Width = HorizontalSpikesWidth;
                rightSpike.Width = HorizontalSpikesWidth;
                topSpike.Height = 0;
                bottomSpike.Height = 0;
            }
            else
            {
                leftSpike.Width = 0;
                rightSpike.Width = 0;
                topSpike.Height = VerticalSpikesHeight;
                bottomSpike.Height = VerticalSpikesHeight;
            }
        }

        /// <summary>
        /// Método que revisa si la plataforma puede infligir daño a megaman, y 
        /// en caso de ser así lo realiza
        /// </summary>
        private void CheckMegamanDamage()
        {
            // Revisión de colisiones con cada pico
            if (leftSpike.BoundingBox.Intersects(megaman.BoundingBox))
            {
                megaman.InflictDamage(Damage);
            }
            else if (rightSpike.BoundingBox.Intersects(megaman.BoundingBox))
            {
                megaman.InflictDamage(Damage);
            }
            else if (topSpike.BoundingBox.Intersects(megaman.BoundingBox))
            {
                megaman.InflictDamage(Damage);
            }
            else if (bottomSpike.BoundingBox.Intersects(megaman.BoundingBox))
            {
                megaman.InflictDamage(Damage);
            }
        }

        #endregion
    }

    #region Clase auxiliar "Spikes"

    /// <summary>
    /// Clase auxiliar para representar la instancia de los picos de la plataforma
    /// </summary>
    sealed class Spikes : GameObject
    {
        /// <summary>
        /// Constructor que inicializa la plataforma
        /// </summary>
        public Spikes()
            : base(0, 0, Color.LightGray)
        {
            DrawRelativeToCamera = true;
        }
    }

    #endregion
}
