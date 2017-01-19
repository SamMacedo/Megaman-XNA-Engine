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
    /// Clase representante de las plataformas que explotan al ser activadas y finalizar el conteo
    /// </summary>
    public sealed class BombPlatform : MovingPlatform
    {
        #region Campos

        // Animaciones
        private AnimationSet animations;

        // Contadores de tiempo
        private Timer countdownTimer;
        private Timer activationWaitTimer;
        private Timer beforeExplosionTimer;
        private Timer explosionTimer;
        
        // Banderas para el manejo de estados de la plataforma
        private bool exploded;
        private bool activated;

        // Objeto para manejar las colisiones de la explosión
        Explosion explosion;

        // Posibles direcciones de la plataforma
        public enum Orientations
        {
            Up,
            Left,
            Right,
        };
        private Orientations orientation;

        // Constantes
        private const int NumberIntervalTime = 500;
        private const int HorizontalWidth = 80;
        private const int HorizontalHeight = 64;
        private const int VerticalWidth = 64;
        private const int VerticalHeight = 80;
        private const int NumberWidth = 53;
        private const int NumberHeight = 53;

        // Arreglo que contiene la ubicación de cada número dentro del spriteSheet
        private static readonly Rectangle[] NumbersSourceRectangles = 
        {
            new Rectangle(0, 0, NumberWidth, NumberHeight),
            new Rectangle(53, 0, NumberWidth, NumberHeight),
            new Rectangle(0, 53, NumberWidth, NumberHeight),
            new Rectangle(53, 53, NumberWidth, NumberHeight),
            new Rectangle(106, 0, NumberWidth, NumberHeight),
            new Rectangle(106, 53, NumberWidth, NumberHeight),
        };

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

        /// <summary>
        /// Indica si la plataforma puede actualizarse
        /// </summary>
        public override bool CanUpdate
        {
            get
            {
                if (exploded)
                {
                    return false;
                }

                return base.CanUpdate;
            }
        }

        /// <summary>
        /// Indica si la plataforma puede dibujarse
        /// </summary>
        public override bool CanDraw
        {
            get
            {
                if (exploded || explosionTimer.IsActive)
                {
                    return false;
                }

                return base.CanDraw;
            }
        }

        #endregion

        #region Constructor e inicialización

        /// <summary>
        /// Constructor que inicializa la plataforma
        /// </summary>
        /// <param name="megaman">Referencia a la instancia de megaman</param>
        /// <param name="initialPosition">Posición inicial de la plataforma</param>
        /// <param name="orientation">Orientación de la plataforma</param>
        /// <param name="time">Tiempo inicial de la plataforma</param>
        public BombPlatform(Megaman megaman, Vector2 initialPosition, Orientations orientation, byte time)
            : base(megaman, initialPosition, 40, 40, Color.Gray)
        {
            this.orientation = orientation;

            // Inicialización de timers
            countdownTimer = new Timer(time * NumberIntervalTime);
            activationWaitTimer = new Timer(500);
            beforeExplosionTimer = new Timer(500);
            explosionTimer = new Timer(250);

            LoadContent();
            Initialize();
            ResetObject();

            // Suscripciones a eventos necesarios
            countdownTimer.TimerReachedZero += new Timer.TimerStopedHandler(countdown_TimerReachedZero);
            activationWaitTimer.TimerReachedZero += new Timer.TimerStopedHandler(activationWaitTimer_TimerReachedZero);
            beforeExplosionTimer.TimerReachedZero += new Timer.TimerStopedHandler(beforeExplosionTimer_TimerReachedZero);
            explosionTimer.TimerReachedZero += new Timer.TimerStopedHandler(explosionTimer_TimerReachedZero);
        }

        /// <summary>
        /// Método que inicializa la plataforma de acuerdo a la orientación de la misma
        /// </summary>
        private void Initialize()
        {
            // Orientación vertical, simplemente se ajusta el tamaño
            if (orientation == Orientations.Up)
            {
                Width = VerticalWidth;
                Height = VerticalHeight;
            }
            // Orientación horizontal, se gira de acuerdo si es a la derecha o izquierda
            // y se ajusta el tamaño
            else
            {
                if (orientation == Orientations.Left)
                {
                    animations.Angle = -(float)Math.PI / 2;
                    animations.Flipped = true;
                }
                else
                {
                    animations.Angle = (float)Math.PI / 2;
                }

                Width = HorizontalWidth;
                Height = HorizontalHeight;
            }

            // Inicialización de la explosión
            explosion = new Explosion(megaman);
            explosion.Position.X = Center.X - (explosion.Width / 2);
            explosion.Position.Y = Center.Y - (explosion.Height / 2);
        }

        /// <summary>
        /// Método que carga el contenido de la plataforma
        /// </summary>
        private void LoadContent()
        {
            animations = Resources.ANIM_BombPlatform.CreateCopy();
        }

        #endregion

        #region Suscripciones a eventos

        /// <summary>
        /// En cuanto finaliza el tiempo de espera comienza la cuenta regresiva
        /// </summary>
        void activationWaitTimer_TimerReachedZero()
        {
            countdownTimer.Start();
        }

        /// <summary>
        /// En cuanto finaliza la cuenta regresiva empieza el conteo de tiempo 
        /// para iniciar la explosión
        /// </summary>
        void countdown_TimerReachedZero()
        {
            beforeExplosionTimer.Start();
        }

        /// <summary>
        /// Inicio de la explosión al terminar el conteo final
        /// </summary>
        void beforeExplosionTimer_TimerReachedZero()
        {
            explosionTimer.Start();
        }

        /// <summary>
        /// Finalización de la explosión
        /// </summary>
        void explosionTimer_TimerReachedZero()
        {
            exploded = true;
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
            countdownTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            activationWaitTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            beforeExplosionTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            explosionTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            if (!activated)
            {
                CheckForActivation();
            }

            if (explosionTimer.IsActive)
            {
                explosion.CheckCollisionWithMegaman();
            }

            Animate(gameTime);
        }

        #endregion

        #region Manejo de la activación

        /// <summary>
        /// Método que revisa si la plataforma puede ser activada
        /// </summary>
        private void CheckForActivation()
        {
            // Dependiendo de la orientación de la plataforma se revisa si megaman está
            // en la posición adecuada, de ser así se inicia la cuenta regresiva
            if (orientation == Orientations.Up)
            {
                if (HasMegamanAbove)
                {
                    StartCountdown();
                }
            }
            else if (orientation == Orientations.Left)
            {
                if (HasMegamanToLeft)
                {
                    StartCountdown();
                }
            }
            else
            {
                if (HasMegamanToRight)
                {
                    StartCountdown();
                }
            }
        }

        /// <summary>
        /// Método que inicia la cuenta regresiva
        /// </summary>
        private void StartCountdown()
        {
            activationWaitTimer.Start();
            activated = true;
        }

        #endregion

        #region Animación

        /// <summary>
        /// Método que se encarga de animar la plataforma
        /// </summary>
        /// <param name="gameTime"></param>
        private void Animate(GameTime gameTime)
        {
            if (beforeExplosionTimer.IsActive)
            {
                animations.StartAnimation("NearExplode");
            }
            else
            {
                if (activationWaitTimer.IsActive)
                {
                    animations.StartAnimation("Press");
                }
                else if (countdownTimer.IsActive)
                {
                    animations.StartAnimation("Activated");
                }
                else
                {
                    animations.StartAnimation("Default");
                }
            }

            animations.Update(gameTime);
        }

        #endregion

        #region Dibujado

        /// <summary>
        /// Método que se encarga de dibujar la plataforma
        /// </summary>
        /// <param name="spritebatch"></param>
        public override void Draw(SpriteBatch spritebatch)
        {
            base.Draw(spritebatch);

            if (explosionTimer.IsActive)
            {
                explosion.Draw(spritebatch);
            }
        }

        /// <summary>
        /// Método que se encarga de dibujar la plataforma
        /// </summary>
        /// <param name="spritebatch"></param>
        public override void OnDraw(SpriteBatch spritebatch)
        {
            animations.Draw(spritebatch, CenteredDrawPosition, Color.White);

            DrawNumber(spritebatch);
        }

        /// <summary>
        /// Método que se encarga de dibujar el número de la plataforma
        /// </summary>
        /// <param name="spritebatch"></param>
        private void DrawNumber(SpriteBatch spritebatch)
        {
            if (CanDrawNumber())
            {
                // Distancias en relación al centro de la plataforma
                int xDiference;
                int yDiference;

                // Dependiendo de la dirección de la plataforma se determinan las distancias
                // del número en relación al centro de la plataforma
                if (orientation == Orientations.Up)
                {
                    xDiference = 0;
                    yDiference = 8;

                    if (activated)
                    {
                        // Si la plataforma está reproduciendo la animación de activación, entonces
                        // se realiza un pequeño movimiento después de cierto tiempo para mover el
                        // número acorde a la animación de la plataforma
                        if (activationWaitTimer.Counter > 300 && activationWaitTimer.Counter < 450)
                        {
                            yDiference = 12;
                        }
                    }
                }
                else
                {
                    yDiference = -2;

                    if (orientation == Orientations.Left)
                    {
                        xDiference = 10;

                        // Si la plataforma está reproduciendo la animación de activación, entonces
                        // se realiza un pequeño movimiento después de cierto tiempo para mover el
                        // número acorde a la animación de la plataforma
                        if (activated)
                        {
                            if (activationWaitTimer.Counter > 300 && activationWaitTimer.Counter < 450)
                            {
                                xDiference = 14;
                            }
                        }
                    }
                    else
                    {
                        xDiference = -10;

                        // Si la plataforma está reproduciendo la animación de activación, entonces
                        // se realiza un pequeño movimiento después de cierto tiempo para mover el
                        // número acorde a la animación de la plataforma
                        if (activated)
                        {
                            if (activationWaitTimer.Counter > 300 && activationWaitTimer.Counter < 450)
                            {
                                xDiference = -14;
                            }
                        }
                    }
                }

                // Se determina la posición de dibujado del número
                float x = Center.X - (NumberWidth / 2) + xDiference - Global.Viewport.X;
                float y = Center.Y - (NumberHeight / 2) + yDiference - Global.Viewport.Y;

                Vector2 drawPosition = new Vector2(x, y);

                spritebatch.Begin();

                // Finalmente se dibuja el número
                spritebatch.Draw(Resources.T2D_BombPlatformNumbersSheet, drawPosition, 
                    GetNumberRectangle(), Color.White);

                spritebatch.End();
            }
        }

        /// <summary>
        /// Método que determina si el número de conteo puede ser dibujado
        /// </summary>
        /// <returns>Regresa un bool para indicar si se puede dibujar o no el número</returns>
        private bool CanDrawNumber()
        {
            // Si la explosión está por comenzar entonces no dibujar
            if (beforeExplosionTimer.IsActive)
            {
                return false;
            }
            else
            {
                // Si la cuenta regresiva está activa, entonces se determina si se puede dibujar 
                // el número en base a cierto tiempo, es decir, para dar un efecto de flash en
                // el cambio de número
                if (countdownTimer.IsActive)
                {
                    // Valores máximos y mínimos en milisegundos que puede tener el conteo
                    int min = NumberIntervalTime;
                    int max = NumberIntervalTime * countdownTimer.StartTime;

                    // Se obtiene el tiempo en milisegundos fijo del intervalo actual, por ejemplo: 
                    // si el conteo va en 1780  y si tomamos encuenta que cada intervalo son 500 milisegundos,
                    // entonces quiere decir que 1780 se encuentra en el intervalo 1500, y este valor de 1500
                    // es el valor que se calcula en la siguente linea
                    int intervalFixedTime =
                        (int)MathHelper.Clamp(countdownTimer.Counter / NumberIntervalTime * NumberIntervalTime,
                        min, max);

                    // Una vez calculado el tiempo fijo del intervalo, entonces simplemente se hace una resta
                    // del conteo menos el tiempo fijo para saber cuanto tiempo ha pasado dentro de intervalo
                    // por ejemplo: 500 - (1780 - 1500) nos indicaría que han transcurrido 220 milisegundos en
                    // el intervalo actual
                    int intervalTime = NumberIntervalTime - (countdownTimer.Counter - intervalFixedTime);

                    // Detener el dibujado si han pasado menos de 150 milisegundos
                    if (intervalTime < 150)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Método que regresa el rectángulo de la posición de la textura del número 
        /// dentro del spriteSheet, se hace tomando en cuenta el valor actual de la cuenta 
        /// regresiva 
        /// </summary>
        /// <returns>Regresa un rectángulo con la ubicación de la textura</returns>
        private Rectangle GetNumberRectangle()
        {
            int number;

            // Si la plataforma está activada entonces se calcula el número actual tomando
            // en cuenta el valor de la cuenta regresiva
            if (activated)
            {
                if (activationWaitTimer.IsActive)
                {
                    number = countdownTimer.StartTime / NumberIntervalTime;
                }
                else
                {
                    number = countdownTimer.Counter / NumberIntervalTime;
                }
            }
            // Si la plataforma no ha sido activada entonces se regresa el número original
            // o inicial de la plataforma
            else
            {
                number = countdownTimer.StartTime / NumberIntervalTime;
            }

            return NumbersSourceRectangles[number];
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Método que se encarga de reiniciar la plataforma
        /// </summary>
        public override void ResetObject()
        {
            exploded = false;
            activated = false;

            activationWaitTimer.Stop();
            countdownTimer.Stop();
            beforeExplosionTimer.Stop();
            explosionTimer.Stop();

            animations.StartAnimation("Default");

            base.ResetObject();
        }

        #endregion
    }

    #region Clase auxiliar "Explosion"

    /// <summary>
    /// Clase para representar la explosión de la plataforma
    /// </summary>
    class Explosion : GameObject
    {
        // Referencia a la instancia de megaman
        private Megaman megaman;

        // Constantes
        public const int Damage = 25;
        private const int DefaultWidth = 85;
        private const int DefaultHeight = 85;

        /// <summary>
        /// Construcor
        /// </summary>
        /// <param name="megaman">Referencia a la instancia de megaman</param>
        public Explosion(Megaman megaman)
            : base(DefaultWidth, DefaultHeight, Color.Red)
        {
            this.megaman = megaman;

            DrawRelativeToCamera = true;
        }

        /// <summary>
        /// Método que revisa colisiones con megaman e inflige daño en caso de haber una
        /// </summary>
        public void CheckCollisionWithMegaman()
        {
            if (BoundingBox.Intersects(megaman.BoundingBox))
            {
                megaman.InflictDamage(Damage);
            }
        }
    }

    #endregion
}
