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
    /// Clase representante de la plataforma de movimiento horizontal deslizante sobre un riel
    /// </summary>
    public sealed class SliderPlatform : MovingPlatform
    {
        #region Campos

        // Animaciones de la plataforma
        private AnimationSet animations;

        // Objetos de las dos plataformas sólidas
        private SolidPlatform leftSolidPlatform;
        private SolidPlatform rightSolidPlatform;

        // Contadores de tiempo
        private Timer movementTimer;
        private Timer idleTimer;

        // Bandera para indicar si la plataforma está en retroceso
        private bool moveBackwards;

        // Posibles direcciones de movimiento de la plataforma
        public enum Directions
        {
            Left,
            Right
        };
        public Directions Direction;

        // Velocidad de movimiento de la plataforma
        private const float DefaultMoveSpeed = 3f;

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
        /// Indica si la plataforma se encuentra visible dentro de la cámara
        /// </summary>
        public override bool IsOnScreen
        {
            get
            {
                int x = (int)leftSolidPlatform.Left;
                int y = (int)Math.Min(leftSolidPlatform.Top, rightSolidPlatform.Top);
                int width = (int)(rightSolidPlatform.Right - leftSolidPlatform.Left);
                int height = (int)Bottom - y;

                Rectangle rectangle = new Rectangle(x, y, width, height);

                return Global.ScreenContainsRectangle(rectangle);
            }
        }

        /// <summary>
        /// Indica si la plataforma se encuentra en el mismo cuarto que megaman
        /// </summary>
        public override bool IsOnRoom
        {
            get
            {
                int x = (int)leftSolidPlatform.Left;
                int y = (int)Math.Min(leftSolidPlatform.Top, rightSolidPlatform.Top);
                int width = (int)(rightSolidPlatform.Right - leftSolidPlatform.Left);
                int height = (int)Bottom - y;

                Rectangle rectangle = new Rectangle(x, y, width, height);

                return Global.RoomContainsRectangle(rectangle);
            }
        }

        #endregion

        #region Constructor e inicialización

        /// <summary>
        /// Constructor que inicializa la plataforma
        /// </summary>
        /// <param name="megaman">Referencia a la instancia de megaman</param>
        /// <param name="initialPosition">Posición inicial de la plataforma</param>
        /// <param name="direction">Dirección de movimiento de la plataforma</param>
        public SliderPlatform(Megaman megaman, Vector2 initialPosition, Directions direction)
            : base(megaman, initialPosition, 40, 70, Color.GreenYellow)
        {
            this.Direction = direction;

            movementTimer = new Timer(800);
            idleTimer = new Timer(4000);

            LoadContent();
            InitializeSolidPlatforms();
            ResetObject();

            // Suscripción a eventos
            this.movementTimer.TimerReachedZero += new Timer.TimerStopedHandler(movementTimer_TimerReachedZero);
            this.idleTimer.TimerReachedZero += new Timer.TimerStopedHandler(idleTimer_TimerReachedZero);
        }

        /// <summary>
        /// Método que carga las animaciones de la plataforma
        /// </summary>
        public void LoadContent()
        {
            animations = Resources.ANIM_SliderPlatform.CreateCopy();
        }

        /// <summary>
        /// Método que inicializa las dos plataformas sólidas
        /// </summary>
        private void InitializeSolidPlatforms()
        {
            // Vector auxiliar para determinar las posiciones
            Vector2 position = Vector2.Zero;

            // Dependiendo de la dirección de movimiento de la plataforma se inicializan las
            // plataformas sólidas, una arriba y otra abajo
            if (Direction == Directions.Right)
            {
                // Plataforma izquierda
                position.X = Left - SolidPlatform.DistanceOnXToSlider - SolidPlatform.DefaultWidth;
                position.Y = Center.Y - SolidPlatform.DefaultHeight;
                leftSolidPlatform = new SolidPlatform(megaman, this, position);

                // Plataforma derecha
                position.X = Right + SolidPlatform.DistanceOnXToSlider;
                position.Y = Center.Y - SolidPlatform.MaxDistanceOnYToSlider;
                rightSolidPlatform = new SolidPlatform(megaman, this, position);
            }
            else
            {
                // Plataforma izquierda
                position.X = Left - SolidPlatform.DistanceOnXToSlider - SolidPlatform.DefaultWidth;
                position.Y = Center.Y - SolidPlatform.MaxDistanceOnYToSlider;
                leftSolidPlatform = new SolidPlatform(megaman, this, position);

                // Plataforma derecha
                position.X = Right + SolidPlatform.DistanceOnXToSlider;
                position.Y = Center.Y - SolidPlatform.DefaultHeight;
                rightSolidPlatform = new SolidPlatform(megaman, this, position);
            }
        }

        #endregion

        #region Colisiones

        /// <summary>
        /// Método que revisa colisiones con las dos plataformas sólidas
        /// </summary>
        /// <param name="other">Objeto con el cual se revisarán colisiones</param>
        /// <param name="collisionSides">Vector para modificar e indicar los lados en donde
        /// ocurrió la colisión en caso de ocurrir</param>
        public override void CheckCollisions(CollisionObject other, ref Vector2 collisionSides)
        {
            leftSolidPlatform.CheckCollisions(other, ref collisionSides);
            rightSolidPlatform.CheckCollisions(other, ref collisionSides);
        }

        #endregion

        #region Actualización

        /// <summary>
        /// Método que actualiza la plataforma
        /// </summary>
        /// <param name="gameTime"></param>
        public override void OnUpdate(GameTime gameTime)
        {
            // Actualización de los contadores de tiempo
            movementTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            idleTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            // Se activan las plataformas y se inicializa el movimiento en caso
            // de que megaman esté encima de alguna de ellas
            if (leftSolidPlatform.HasMegamanAbove)
            {
                if (!leftSolidPlatform.Active)
                {
                    leftSolidPlatform.Activate();
                    rightSolidPlatform.Deactivate();

                    StartMovement();
                }
            }
            else if (rightSolidPlatform.HasMegamanAbove)
            {
                if (!rightSolidPlatform.Active)
                {
                    leftSolidPlatform.Deactivate();
                    rightSolidPlatform.Activate();

                    StartMovement();
                }
            }

            HandleMovement();

            // Actualización de las plataformas
            leftSolidPlatform.OnUpdate(gameTime);
            rightSolidPlatform.OnUpdate(gameTime);

            Animate(gameTime);
        }

        #endregion

        #region Métodos de movimiento

        /// <summary>
        /// Métoto que inicializa el movimiento de la plataforma
        /// </summary>
        private void StartMovement()
        {
            // Se inicia el contador de tiempo para controlar el movimiento y además
            // se detiene el 2do contador de tiempo el cual sirve para controlar
            // el tiempo que la plataforma permanece inactiva
            movementTimer.Restart();
            idleTimer.Stop();

            moveBackwards = false;
        }

        /// <summary>
        /// Método que se encarga de realizar el movimiento de la plataforma
        /// </summary>
        private void HandleMovement()
        {
            Speed.X = 0f;

            // Avance
            if (!moveBackwards)
            {
                HandleNormalMovement();
            }
            // Retroceso
            else
            {
                HandleBackwardMovement();
            }
        }

        /// <summary>
        /// Método que se encarga de realizar el movimiento de avance de la plataforma
        /// </summary>
        private void HandleNormalMovement()
        {
            // Sólo mover si está activo el contador de tiempo
            if (movementTimer.IsActive)
            {
                // Se realiza el movimiento de acuerdo a la dirección, primero se revisa
                // si la nueva posición estará dentro de los rieles y en caso de ser así
                // se realiza el movimiento
                if (Direction == Directions.Right)
                {
                    if (CheckForRailTiles(DefaultMoveSpeed))
                    {
                        Speed.X = DefaultMoveSpeed;
                    }
                }
                else
                {
                    if (CheckForRailTiles(-DefaultMoveSpeed))
                    {
                        Speed.X = -DefaultMoveSpeed;
                    }
                }
            }
        }

        /// <summary>
        /// Método que se encarga de realizar el movimiento de retroceso de la plataforma
        /// </summary>
        private void HandleBackwardMovement()
        {
            // Se realiza el movimiento de acuerdo a la dirección, primero se revisa
            // si la nueva posición estará dentro de los rieles y en caso de ser así
            // se realiza el movimiento, si no entonces quiere decir que la plataforma
            // ha llegado al inicio de los rieles y se termina el movimiento de
            // retroceso
            if (Direction == Directions.Right)
            {
                if (CheckForRailTiles(-DefaultMoveSpeed))
                {
                    Speed.X = -DefaultMoveSpeed;
                }
                else
                {
                    moveBackwards = false;
                }
            }
            else
            {
                if (CheckForRailTiles(DefaultMoveSpeed))
                {
                    Speed.X = DefaultMoveSpeed;
                }
                else
                {
                    moveBackwards = false;
                }
            }
        }

        /// <summary>
        /// Método que se encarga de revisar si la plataforma está en los rieles
        /// </summary>
        /// <param name="offset">Incremente de posición/velocidad</param>
        /// <returns>regresa true en caso de estar dentro de los rieles</returns>
        private bool CheckForRailTiles(float offset)
        {
            int x;
            int y;

            // Derecha de la plataforma
            if (offset > 0f)
            {
                x = (int)(rightSolidPlatform.Right + offset);
            }
            // Izquierda de la plataforma
            else
            {
                x = (int)(leftSolidPlatform.Left + offset);
            }
            y = (int)Center.Y;

            // Se obtiene el tipo de tile
            int type = GameTiles.CheckTile(Global.MovingPlatformsLayer, x, y);

            // Cualquiera de estos tres tipos de tiles son considerados como rieles
            return (type == GameTiles.SliderPlatform_Rail
                || type == GameTiles.SliderPlatform_Left || type == GameTiles.SliderPlatform_Right);
        }

        #endregion

        #region Suscripciones a eventos

        /// <summary>
        /// En cuanto finaliza el movimiento se inicia el contador de inactividad
        /// </summary>
        void movementTimer_TimerReachedZero()
        {
            idleTimer.Restart();
        }

        /// <summary>
        /// Si el contador de inactividad termina entonces se inicia el movimiento
        /// de retroceso
        /// </summary>
        void idleTimer_TimerReachedZero()
        {
            moveBackwards = true;
        }

        #endregion

        #region Animación

        /// <summary>
        /// Método que se encarga de animar la plataforma
        /// </summary>
        /// <param name="gameTime"></param>
        private void Animate(GameTime gameTime)
        {
            animations.Update(gameTime);

            if (Direction == Directions.Right)
            {
                if (moveBackwards)
                {
                    animations.StartAnimation("Moving Left");
                }
                else
                {
                    if (idleTimer.IsActive && idleTimer.Counter < 1500)
                    {
                        animations.StartAnimation("Flashing Left");
                    }
                    else
                    {
                        if (movementTimer.IsActive)
                        {
                            animations.StartAnimation("Moving Right");
                        }
                        else
                        {
                            animations.StartAnimation("Idle Right");
                        }
                    }
                }
            }
            else
            {
                if (moveBackwards)
                {
                    animations.StartAnimation("Moving Right");
                }
                else
                {
                    if (idleTimer.IsActive && idleTimer.Counter < 1500)
                    {
                        animations.StartAnimation("Flashing Right");
                    }
                    else
                    {
                        if (movementTimer.IsActive)
                        {
                            animations.StartAnimation("Moving Left");
                        }
                        else
                        {
                            animations.StartAnimation("Idle Left");
                        }
                    }
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

            leftSolidPlatform.OnDraw(spritebatch);
            rightSolidPlatform.OnDraw(spritebatch);
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Métpdo que reinicia la plataforma
        /// </summary>
        public override void ResetObject()
        {
            // Reinicio de las dos plataformas sólidas
            leftSolidPlatform.ResetObject();
            rightSolidPlatform.ResetObject();

            // Se activa una de las dos plataformas sólidas dependiendo
            // de la dirección del a plataforma central
            if (Direction == Directions.Right)
            {
                leftSolidPlatform.Activate();
                rightSolidPlatform.Deactivate();
            }
            else
            {
                leftSolidPlatform.Deactivate();
                rightSolidPlatform.Activate();
            }

            // Reinicio de la animación
            animations.StartAnimation("Default");
            animations.RestartAnimation();

            // Se detienen los contadores de tiempo
            movementTimer.Stop();
            idleTimer.Stop();

            moveBackwards = false;

            base.ResetObject();
        }

        #endregion
    }

    #region Clase auxiliar SolidPlatform

    /// <summary>
    /// Clase auxiliar para instanciar las dos plataformas sólidas
    /// </summary>
    class SolidPlatform : MovingPlatform
    {
        #region Campos

        // Referencia a la plataforma central
        private SliderPlatform sliderPlatform;

        // Indica si está activa la plataforma la cual se moverá hacia abajo
        // en caso de estarlo
        private bool active;

        // Valores constantes
        public const int DefaultWidth = 80;
        public const int DefaultHeight = 20;
        public const float MaxDistanceOnYToSlider = 100f;
        public const float DistanceOnXToSlider = 58f;
        private const float MoveSpeedY = 3f;

        private Vector2[] ballPositions;

        #endregion

        #region Propiedades

        /// <summary>
        /// Indica si la plataforma está activa
        /// </summary>
        public bool Active { get { return active; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que inicializa la plataforma
        /// </summary>
        /// <param name="megaman">Referencia a la instancia de megaman</param>
        /// <param name="sliderPlatform">Referencia a la instancia de la plataforma central</param>
        /// <param name="initialPosition">Posición inicial de la plataforma</param>
        public SolidPlatform(Megaman megaman, SliderPlatform sliderPlatform, 
            Vector2 initialPosition)
            : base(megaman, initialPosition, DefaultWidth, DefaultHeight, Color.Yellow)
        {
            this.sliderPlatform = sliderPlatform;

            jumpOverPlatform = true;

            ballPositions = new Vector2[4];

            for (int i = 0; i < ballPositions.Length; i++)
            {
                ballPositions[i] = Vector2.Zero;
            }

            ResetObject();
        }

        #endregion

        #region Actualización

        /// <summary>
        /// Método que actualiza la plataforma
        /// </summary>
        /// <param name="gameTime"></param>
        public override void OnUpdate(GameTime gameTime)
        {
            // Movimiento hacia abajo en caso de estar activa, en caso contrario
            // se realiza movimiento hacia arriba
            if (active)
            {
                Speed.Y = MoveSpeedY;
            }
            else
            {
                Speed.Y = -MoveSpeedY;
            }

            // Si la plataforma sobrepasa algún límite, ya sea el inferior o el
            // superior, entonces se detiene su movimiento
            if (Bottom + Speed.Y >= sliderPlatform.Center.Y)
            {
                Speed.Y = 0f;
            }
            else if (Top + Speed.Y <= sliderPlatform.Center.Y - MaxDistanceOnYToSlider)
            {
                Speed.Y = 0f;
            }

            // La plataforma se mueve en X junto con la plataforma central
            Speed.X = sliderPlatform.Speed.X;

            Position += Speed;
        }

        #endregion

        #region Posicionado de las bolas

        /// <summary>
        /// Método que calcula las posiciones de las bolas de las plataformas sólidas
        /// </summary>
        private void CalculateBallPositions()
        {
            // Se deja una distancia extra en relación a la plataforma central
            float extraDistanceX = 25f;

            // Se calcula la posición en Y a partir de la cual se comienza a dibujar
            // la cadena de bolas
            float yPosition = sliderPlatform.Center.Y - 7f;

            // Se calculan los catetos opuestos(Y) y adyacentes(X)
            float incrementX = Center.X + 2 - sliderPlatform.Center.X;
            float incrementY = yPosition - Center.Y - 5f;

            // Se calcula el signo en X de la cadena, esto sirve para saber si debemos de
            // colocar las bolas a la derecha o izquierda
            float sign = incrementX / Math.Abs(incrementX);

            // Se disminuye la distancia en X para que las bolas no queden en el puro centro
            incrementX -= sign * extraDistanceX;

            // Se calcula el ángulo que hay entre la plataforma central y la plataforma sólida
            float angle = (float)Math.Atan2(incrementY, incrementX);

            // Se calcula la hipotenusa o la distancia que hay entre la plataforma central
            // y la plataforma sólida
            float hypotenuse = incrementX / (float)Math.Cos(angle);

            // Se obtienen las divisiones para colocar las bolas de manera equitativa en distancia
            // a lo largo de la hipotenusa
            float ballDivisions = 1f / (ballPositions.Length - 1);

            // Variable auxiliar para almacenar la distancia de cada bola
            float distance;

            // Finalmente con los datos obtenidos anteriormente se asignan las posiciones
            // a las bolas
            for (int i = 0; i < ballPositions.Length; i++)
            {
                distance = MathHelper.Lerp(0f, hypotenuse, i * ballDivisions);

                // Posición en X
                ballPositions[i].X = sliderPlatform.Center.X + (sign * extraDistanceX) 
                    + distance * (float)Math.Cos(angle) 
                    - Global.Viewport.X - (Resources.T2D_SliderPlatformBall.Width / 2);

                // Posición en Y
                ballPositions[i].Y = yPosition - distance * (float)Math.Sin(angle) 
                    - Global.Viewport.Y - (Resources.T2D_SliderPlatformBall.Height / 2);
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
            spritebatch.Begin();

            // Se dibuja la plataforma
            spritebatch.Draw(Resources.T2D_SliderSolidPlatform, DrawPosition, Color.White);

            CalculateBallPositions();

            // Se dibujan las bolas
            foreach (Vector2 ballposition in ballPositions)
            {
                spritebatch.Draw(Resources.T2D_SliderPlatformBall, ballposition, Color.White);
            }

            // Sólo dibujar la bounding box en caso de que esté habilitada la opción
            if (DrawBoundingBox)
            {
                // Se dibuja la bounding box con cierta transparencia
                spritebatch.Draw(Resources.T2D_Pixel, DrawPosition, null, Color.Yellow * 0.5f,
                    0f, Vector2.Zero, new Vector2(Width, Height), SpriteEffects.None, 0f);
            }

            spritebatch.End();
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Método que activa la plataforma
        /// </summary>
        public void Activate()
        {
            active = true;
        }

        /// <summary>
        /// Método que desactiva la plataforma
        /// </summary>
        public void Deactivate()
        {
            active = false;
        }

        #endregion
    }

    #endregion
}
