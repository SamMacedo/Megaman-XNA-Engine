#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

using Megaman_Final_Fight.GlobalClasses;
using Megaman_Final_Fight.HelperClasses;
using Megaman_Final_Fight.PrimaryClasses.MegamanWeapons;
using Megaman_Final_Fight.PrimaryClasses.HealthBars;

using AnimationSystem;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses
{
    /// <summary>
    /// Clase representante del objeto principal que controla el jugador
    /// </summary>
    public sealed class Megaman : PhysicsObject, IHealthObject
    {
        #region Campos

        // Set de animaciones de megaman
        private AnimationSet animations;

        private float opacity;
        private int healthAfterRecover;

        // Banderas
        private bool canQuitLadder;
        private bool canJumpSlide;
        private bool onSlideDamage;
        private bool spawnLanded;

        // Constantes de tamaño
        private const int OriginalWidth = 54;
        private const int OriginalHeight = 88;
        private const int SlideWidth = 64;
        private const int SlideHeight = 64;

        // Constantes de movimiento
        private const float DefaultSpeed = 5.2f;
        private const float DefaultAcceleration = 0.09f;
        private const float SlideSpeed = 10f;
        private const float SlideAcceleration = 10f;
        private const float LadderClimbSpeed = 5f;
        private const float DamageRecoilSpeed = 1.25f;

        // Contadores de tiempo
        private Timer slideTimer;
        private Timer damageTimer;
        private Timer invinsibilityTimer;
        private Timer deathTimer;

        // Armas de megaman
        public MegamanWeapon CurrentWeapon;
        private BusterCannon busterCannon;

        // Camara
        public LevelCamera Camera;

        // Barra de vida
        private MegamanHealthBar healthBar;

        SoundEffectInstance sfx_recoverHealthLoop;

        #endregion

        #region Propiedades

        /// <summary>
        /// Indica si megaman está realizando la animación de aparición
        /// </summary>
        public bool IsOnSpawn { get; private set; }

        /// <summary>
        /// Indica si megaman se encuentra deslizandose
        /// </summary>
        public bool IsSliding { get; private set; }

        /// <summary>
        /// Indica si megaman está en una escalera
        /// </summary>
        public bool IsOnLadder { get; private set; }

        /// <summary>
        /// Indica si megaman está recuperando vida
        /// </summary>
        public bool RecoveringHealth { get; private set; }

        /// <summary>
        /// Indica si megaman ha muerto
        /// </summary>
        public bool IsDeath 
        {
            get { return deathTimer.IsActive; } 
        }

        /// <summary>
        /// Indica si megaman está reproduciendo la animación de daño
        /// </summary>
        public bool IsOnDamage
        {
            get { return damageTimer.IsActive; }  
        }

        /// <summary>
        /// Indica si megaman tiene un tope de escalera a sus pies
        /// </summary>
        private bool HasLadderToBottom
        {
            get
            {
                // Se obtienen las coordenadas del punto medio de la parte inferior de megaman
                int x = (int)(Position.X + (Width / 2));
                int y = (int)(Position.Y + Height);

                // Se obtiene el tipo de tile y se determina si es un tope de escalera
                int type = GameTiles.CheckTile(Global.LevelLayer, x, y);

                if (type == GameTiles.Ladder)
                {
                    return GameTiles.IsTopLadderTile(x, y);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Indica si megaman tiene una escalera en su centro
        /// </summary>
        private bool HasLadderToCenter
        {
            get
            {
                // Se obtienen las coordenadas del punto central de megaman
                int x = (int)(Position.X + (Width / 2));
                int y = (int)(Position.Y + (Height / 2));

                return GameTiles.CheckTile(Global.LevelLayer, x, y) == GameTiles.Ladder;
            }
        }

        /// <summary>
        /// Inica si megaman se encuentra agarrado a una escalera y está a unos cuantos pixeles
        /// (tomando como referencia su centro) de llegar al tope de la escalera
        /// </summary>
        public bool AlmostOnTopOfLadder
        {
            get
            {
                // Se obtienen las coordenadas de 10 pixeles arriba del punto central de megaman
                int x = (int)(Position.X + (Width / 2));
                int y = (int)(Position.Y + (Height / 2)) - 10;

                return IsOnLadder && GameTiles.CheckTile(Global.LevelLayer, x, y) != GameTiles.Ladder;
            }
        }

        /// <summary>
        /// Obtiene la posición de dibujado de los disparos tomando en cuenta la dirreción
        /// a la que está volteando megaman y su estado actual
        /// </summary>
        public Vector2 ShotDrawPosition
        {
            get
            {
                float x;
                float y;

                // Se acomoda la posición en X deacuerdo a la dirección a la que está
                // volteando megaman
                if (CurrentFacingDirection == CollisionObject.Directions.Right)
                {
                    x = Position.X + Width + 20;
                }
                else
                {
                    x = Position.X - 20;
                }

                // De acuerdo al estado de megaman se acomoda la posición en Y
                if (IsOnGround)
                {
                    if (IsMoving)
                    {
                        y = Position.Y + 40;
                    }
                    else
                    {
                        y = Position.Y + 35;
                    }
                }
                else
                {
                    if (IsJumping)
                    {
                        y = Position.Y + 20;
                    }
                    else
                    {
                        y = Position.Y + 40;
                    }
                }

                return new Vector2(x, y);
            }
        }

        /// <summary>
        /// Obtiene el ángulo de la direción a la que megaman está volteando
        /// </summary>
        public float FacingAngle
        {
            get
            {
                if (CurrentFacingDirection == Directions.Right)
                {
                    return 0f;
                }
                else
                {
                    return 180f;
                }
            }
        }

        /// <summary>
        /// Indica si se puede infligir daño a megaman
        /// </summary>
        public bool CanInflictDamage
        {
            get 
            {
                return !Camera.IsOnTransition && !invinsibilityTimer.IsActive
                && !deathTimer.IsActive;
            }
        }

        #endregion

        #region Constructor

        public Megaman()
            : base(OriginalWidth, OriginalHeight, Color.Blue, Directions.Right)
        {
            IsOnSpawn = false;
            IsSliding = false;
            IsOnLadder = false;

            canQuitLadder = false;
            canJumpSlide = false;

            slideTimer = new Timer(400);
            damageTimer = new Timer(500);
            invinsibilityTimer = new Timer(1500);
            deathTimer = new Timer(3000);

            busterCannon = new BusterCannon(this);
            CurrentWeapon = busterCannon;

            base.maxHorizontalSpeed = DefaultSpeed;
            base.horizontalAcceleration = DefaultAcceleration;

            Camera = new LevelCamera(this);
            healthBar = new MegamanHealthBar(this);

            sfx_recoverHealthLoop = Resources.SFX_Megaman_Health_Recover.CreateInstance();
            sfx_recoverHealthLoop.IsLooped = true;

            // Suscripciónes a eventos
            CurrentWeapon.BeforeShoot += new MegamanWeapon.MegamanWeaponHandler(currentWeapon_BeforeShoot);
            Camera.OnTransitionStart += new LevelCamera.CameraTransitionEventHander(camera_OnTransitionStart);
            invinsibilityTimer.TimerStoped += new Timer.TimerStopedHandler(invinsibilityTimer_TimerStoped);
            deathTimer.TimerReachedZero += new Timer.TimerStopedHandler(deathTimer_TimerReachedZero);
            base.OnLanding += new PhysicsObjectHandler(Megaman_OnLanding);

            Reset();
        }

        #endregion

        #region Delegados y eventos

        public delegate void MegamanEventHandler();

        /// <summary>
        /// Evento que ocurre al iniciar el spawn de megaman
        /// </summary>
        public event MegamanEventHandler OnSpawnStart;

        #endregion

        #region Suscripciones a eventos

        /// <summary>
        /// Evento que ocurre antes de realizar un disparo
        /// </summary>
        /// <param name="weapon"></param>
        void currentWeapon_BeforeShoot(MegamanWeapon weapon)
        {
            // Cambio de dirección en la escalera dependiendo del botón presionado
            if (IsOnLadder)
            {
                if (InputManager.ButtonDown(InputManager.Buttons.Left))
                {
                    CurrentFacingDirection = Directions.Left;
                }
                else if (InputManager.ButtonDown(InputManager.Buttons.Right))
                {
                    CurrentFacingDirection = Directions.Right;
                }
            }
        }

        /// <summary>
        /// Evento que ocurre al iniciar una transición de cámara
        /// </summary>
        void camera_OnTransitionStart(LevelCamera.TransitionDirections transitionDirection)
        {
            // Se desactivan todos los disparos activos
            CurrentWeapon.DeactivateAllShots();

            // Cancelar deslizamiento si es transición hacia abajo
            if (transitionDirection == LevelCamera.TransitionDirections.Down)
            {
                CancelSlide();
            }
        }

        /// <summary>
        /// Evento que se ejecuta al caer megaman en el suelo
        /// </summary>
        void Megaman_OnLanding()
        {
            if (!spawnLanded)
            {
                spawnLanded = true;
                Resources.SFX_Megaman_Spawn_Land.Play();
            }
            else
            {
                Resources.SFX_Megaman_Land.Play();
            }
        }

        /// <summary>
        /// Evento que ocurre al detener el contador de invensibilidad
        /// </summary>
        void invinsibilityTimer_TimerStoped()
        {
            opacity = 1f;
        }

        /// <summary>
        /// Evento que ocurre al llegar a cero el contador de muerte de megaman
        /// </summary>
        void deathTimer_TimerReachedZero()
        {
            StartSpawn();
        }

        #endregion

        #region Carga de contenido

        /// <summary>
        /// Método que carga el contenido del objeto
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent(ContentManager content)
        {
            // Se carga el set de animaciones
            animations = content.Load<AnimationSet>(@"Graphics\Animations\Megaman\Megaman");
        }

        #endregion

        #region Actualización

        public override void Update(GameTime gameTime)
        {
            if (IsOnSpawn)
            {
                HandleSpawn();
            }
            else if (RecoveringHealth)
            {
                HandleHealthRecover();
            }
            else
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Space))
                {
                    InflictDamage(50);
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    RecoverHealth(50);
                }

                if (!Camera.IsOnTransition)
                {
                    HandleInput(gameTime);

                    CheckpointManager.SearchNewCheckpoints(this);
                }

                CurrentWeapon.Update(gameTime);
            }

            if (!Camera.IsOnTransition && !RecoveringHealth)
            {
                base.Update(gameTime);
            }

            Camera.Update();

            healthBar.Update(gameTime);

            Animate(gameTime);
        }

        #endregion

        #region Manejo del spawn

        /// <summary>
        /// Método que inicia el Spawn de Megaman
        /// </summary>
        public void StartSpawn()
        {
            Reset();

            // Reinicio de la posición de la cámara
            Camera.ResetToCheckpoint();

            // Se coloca a megaman en la parte superior de la pantalla alineado en X
            // con la posición inicial del mapa
            Position.X = CheckpointManager.LastCheckpoint.X - (Width / 2) + 32;
            Position.Y = Camera.Position.Y;

            IsOnSpawn = true;

            // Invocación de evento
            if (OnSpawnStart != null)
            {
                OnSpawnStart.Invoke();
            }
        }

        /// <summary>
        /// Método que se encarga de actualizar el movimiento de megaman en caso de estar
        /// spawneando y a la vez de cancelarlo en caso de alcanzar la posición inicial del
        /// mapa
        /// </summary>
        private void HandleSpawn()
        {
            // Si megaman todavia no llega a la posición inicial en Y entonces continuar
            // bajando
            if (Position.Y + Height < CheckpointManager.LastCheckpoint.Y)
            {
                Speed.Y += 0.5f;
            }
            else
            {
                FinishSpawn();
            }
        }

        /// <summary>
        /// Método que finaliza el spawn de megaman
        /// </summary>
        private void FinishSpawn()
        {
            IsOnSpawn = false;

            base.applyGravity = true;
            base.HandleCollisions = true;
        }

        /// <summary>
        /// Método que reinicia a megaman
        /// </summary>
        private void Reset()
        {
            StopMovement();

            CurrentWeapon.DeactivateAllShots();

            damageTimer.Stop();
            invinsibilityTimer.Stop();
            deathTimer.Stop();

            Health = MaxHealth;
            RecoveringHealth = false;
            opacity = 1f;

            spawnLanded = false;
        }

        /// <summary>
        /// Método que detiene el movimiento de megaman
        /// </summary>
        private void StopMovement()
        {
            Speed = Vector2.Zero;

            CancelJump();
            CancelSlide();
            QuitLadder(false);

            base.applyGravity = false;
            base.HandleCollisions = false;
        }

        #endregion

        #region Manejo del input

        /// <summary>
        /// Método que se encarga de capturar el input introducido por el jugador
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            // Actualización de contadores de tiempo
            slideTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            damageTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            invinsibilityTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            deathTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            if (!deathTimer.IsActive)
            {
                if (!damageTimer.IsActive)
                {
                    if (!IsOnLadder)
                    {
                        if (InputManager.ButtonDown(InputManager.Buttons.Jump))
                        {
                            if (canJumpSlide)
                            {
                                if (InputManager.ButtonDown(InputManager.Buttons.Down))
                                {
                                    Slide();
                                }
                                else
                                {
                                    if (IsOnGround)
                                    {
                                        CancelSlide();
                                        Jump();
                                    }
                                }

                                canJumpSlide = false;
                            }
                        }
                        else
                        {
                            CancelJump();

                            canJumpSlide = true;
                        }

                        if (!IsSliding)
                        {
                            if (InputManager.ButtonDown(InputManager.Buttons.Right))
                            {
                                MoveRight();
                            }
                            else if (InputManager.ButtonDown(InputManager.Buttons.Left))
                            {
                                MoveLeft();
                            }

                            // Revisión para verificar si se puede subir a una escalera
                            if (HasLadderToBottom)
                            {
                                if (InputManager.ButtonDown(InputManager.Buttons.Down))
                                {
                                    GrabToLadder(true);
                                }
                            }
                            else if (HasLadderToCenter)
                            {
                                if (InputManager.ButtonDown(InputManager.Buttons.Down))
                                {
                                    if (!IsOnGround)
                                    {
                                        GrabToLadder(false);
                                    }
                                }

                                if (InputManager.ButtonDown(InputManager.Buttons.Up))
                                {
                                    GrabToLadder(false);
                                }
                            }
                        }
                        else
                        {
                            HandleSlide();
                        }
                    }
                    else
                    {
                        HandleLadderMovement();
                    }
                }
                else
                {
                    HandleDamageRecoil();
                }

                if (invinsibilityTimer.IsActive)
                {
                    HandleInvinsibilityOpacity(gameTime);
                }
            }
        }

        #endregion

        #region Manejo del slide

        /// <summary>
        /// Método que inicia el slide de megaman
        /// </summary>
        private void Slide()
        {
            if (IsOnGround && !IsSliding)
            {
                // Se inicia el timer que para limitar el tiempo de duración del slide
                slideTimer.Start();

                // Se modifica el tamaño de megaman a uno mas pequeño
                Position.Y = Bottom - SlideHeight;
                Height = SlideHeight;

                // Cambio en la velocidad de movimiento a una velocidad más rápida
                base.maxHorizontalSpeed = SlideSpeed;
                base.horizontalAcceleration = SlideAcceleration;

                IsSliding = true;
            }
        }

        /// <summary>
        /// Método que cancela el slide de megaman
        /// </summary>
        private void CancelSlide()
        {
            if (IsSliding)
            {
                // Sólo cancelar si megaman no tiene una pared encima
                if (!HasWallAtOffset(new Vector2(0f, -1f)))
                {
                    // Se detiene el timer que limita el tiempo de duración del slide
                    slideTimer.Stop();

                    // Se modifica el tamaño de megaman a su estado original
                    Position.Y = Bottom - OriginalHeight;
                    Height = OriginalHeight;

                    // Cambio en la velocidad de movimiento a su estado original
                    base.maxHorizontalSpeed = DefaultSpeed;
                    base.horizontalAcceleration = DefaultAcceleration;

                    IsSliding = false;
                }
            }
        }

        /// <summary>
        /// Método que se encarga de manejar el movimiento automático del slide de megaman, así 
        /// también de detenerlo en caso de ser necesario
        /// </summary>
        private void HandleSlide()
        {
            if (IsSliding)
            {
                // Detener si tiene una pared enfrente
                if ((CurrentFacingDirection == Directions.Left && HasWallAtOffset(new Vector2(-1f, 0f)) 
                    || (CurrentFacingDirection == Directions.Right && HasWallAtOffset(new Vector2(1f, 0f)))))
                {
                    CancelSlide();
                }
                // Detener si ha finalizado el tiempo de duración del slide
                else if (!slideTimer.IsActive)
                {
                    CancelSlide();
                }
                // Detener si ha comenzado a caer
                else if (IsFalling)
                {
                    CancelSlide();
                }

                // Se realiza el moviminento de acuerdo a la dirección a la que está volteando
                if (CurrentFacingDirection == Directions.Right)
                {
                    // Cancelar slide en caso de cambiar la dirección
                    if (InputManager.ButtonDown(InputManager.Buttons.Left))
                    {
                        CurrentFacingDirection = Directions.Left;

                        CancelSlide();
                    }
                    else
                    {
                        MoveRight();
                    }
                }
                else
                {
                    // Cancelar slide en caso de cambiar la dirección
                    if (InputManager.ButtonDown(InputManager.Buttons.Right))
                    {
                        CurrentFacingDirection = Directions.Right;

                        CancelSlide();
                    }
                    else
                    {
                        MoveLeft();
                    }
                }
            }
        }

        #endregion

        #region Manejo de escaleras

        /// <summary>
        /// Método que se encarga de iniciar el agarre de escalera
        /// </summary>
        /// <param name="top">true para indicar si se va a realizar la subida desde
        /// el tope de la escelara</param>
        private void GrabToLadder(bool top)
        {
            // Se detiene el movimiendo de caída por gravedad 
            base.applyGravity = false;

            // Si se va a subir desde el tope de la escalera entonces se posiciona a
            // megaman en Y deacuerdo al tope escalera
            if (top)
            {
                Position.Y += (Height / 2);
            }

            // Se centra a megaman en la escalera
            Position.X = (int)(Position.X + (Width / 2)) / 64 * 64 + 32 - (Width / 2);

            // Reinicio de la velocidad para evitar problemas de posicionamiento
            Speed = Vector2.Zero;

            IsOnLadder = true;
            canQuitLadder = false;
        }

        /// <summary>
        /// Método que se encarga de hacer que megaman deje de agarrase de la escalera
        /// </summary>
        /// <param name="top">true para indicar si megaman va a dejar la escalera debido
        /// a que ha llegado al tope</param>
        private void QuitLadder(bool top)
        {
            // Se habilita el movimiendo de caída por gravedad 
            base.applyGravity = true;

            // Si se va a salir de la escalera debido a que megaman ha alcanzado el tope
            // entonces se posiciona a megaman en la parte superior de la escalera
            if (top)
            {
                Position.Y = (int)Position.Y / 64 * 64 + 64 - Height;
            }

            // Reinicio de la velocidad para evitar problemas de posicionamiento
            Speed = Vector2.Zero;

            IsOnLadder = false;
        }

        /// <summary>
        /// Método que se encarga de manejar el movimiento cuando megaman está agarrado
        /// de una escalera
        /// </summary>
        private void HandleLadderMovement()
        {
            // Reinicio de la velocidad en Y
            Speed.Y = 0f;

            // Salir de la escalera si se presiona el botón de brincar y no se están
            // presionando los botones de arriba ni abajo
            if (InputManager.ButtonDown(InputManager.Buttons.Jump)
                  && !InputManager.ButtonDown(InputManager.Buttons.Up)
                  && !InputManager.ButtonDown(InputManager.Buttons.Down))
            {
                if (canQuitLadder )
                {
                    QuitLadder(false);
                    return;
                }
            }
            else
            {
                canQuitLadder = true;
            }

            if (!CurrentWeapon.ShotAnimationTimer.IsActive)
            {
                // Movimiento hacia arriba
                if (InputManager.ButtonDown(InputManager.Buttons.Up))
                {
                    Speed.Y = -LadderClimbSpeed;
                }
                // Movimiento hacia abajo
                else if (InputManager.ButtonDown(InputManager.Buttons.Down))
                {
                    Speed.Y = LadderClimbSpeed;
                }
            }

            // Salir de la escalera si la posición central de megaman no está sobre un tile escalera
            if (!HasLadderToCenter)
            {
                if (AlmostOnTopOfLadder)
                {
                    QuitLadder(true);
                }
                else
                {
                    QuitLadder(false);
                }
            }
            // Salir de la escalera en caso de que megaman toque el piso
            else if (IsOnGround)
            {
                QuitLadder(false);
            }
        }

        #endregion

        #region Manejo del daño

        /// <summary>
        /// Método que se encarga de infligir daño a megaman
        /// </summary>
        /// <param name="damage"></param>
        public void InflictDamage(int damage)
        {
            if (CanInflictDamage)
            {
                Health = (int)MathHelper.Clamp(Health - damage, 0, MaxHealth);

                // Comparacion para saber si megaman ha muerto
                if (Health == 0)
                {
                    deathTimer.Start();
                    StopMovement();

                    Resources.SFX_Megaman_Death.Play();

                    return;
                }

                // Inicio de contadores de tiempo
                damageTimer.Start();
                invinsibilityTimer.Start();

                onSlideDamage = false;

                // Cancelar brinco y agarre de escalera
                if (IsJumping)
                {
                    CancelJump();
                }
                else if (IsOnLadder)
                {
                    QuitLadder(false);
                }
                // Se determina si se debe de reproducir la animación de daño en slide
                else if (IsSliding)
                {
                    if (!HasWallAtOffset(new Vector2(0f, -1f)))
                    {
                        CancelSlide();
                    }
                    else
                    {
                        onSlideDamage = true;
                    }
                }

                Resources.SFX_Megaman_Hit.Play();
            }
        }

        /// <summary>
        /// Método que se encarga de realizar el movimiento de retroceso de megaman
        /// al recibir daño
        /// </summary>
        private void HandleDamageRecoil()
        {
            // Movimiento de retroceso deacuerdo a la dirección 
            // a la que está volteando megaman
            if (CurrentFacingDirection == Directions.Right)
            {
                Speed.X = -DamageRecoilSpeed;
            }
            else
            {
                Speed.X = DamageRecoilSpeed;
            }
        }

        /// <summary>
        /// Método que se encarga de realizar el efecto flash mientras megaman está invensible
        /// </summary>
        /// <param name="gametime"></param>
        private void HandleInvinsibilityOpacity(GameTime gametime)
        {
            opacity = (1f + (float)Math.Sin(gametime.TotalGameTime.TotalMilliseconds / 25)) / 2f;
        }

        #endregion

        #region Manejo de la recuperación de vida

        /// <summary>
        /// Método que se encarga de inicializar la recuperación de vida de megaman
        /// </summary>
        /// <param name="increment">Cantidad de vida que se va a añadir</param>
        public void RecoverHealth(int increment)
        {
            if (!RecoveringHealth && Health < MaxHealth && !deathTimer.IsActive)
            {
                if (!Camera.IsOnTransition)
                {
                    healthAfterRecover = (int)MathHelper.Clamp(Health + increment, 0, MaxHealth);
                    RecoveringHealth = true;

                    damageTimer.Stop();
                    invinsibilityTimer.Stop();

                    sfx_recoverHealthLoop.Play();
                }
            }
        }

        /// <summary>
        /// Método que se encarga de manejar el incremento de vida de megaman
        /// </summary>
        private void HandleHealthRecover()
        {
            Health += 1;

            if (Health >= healthAfterRecover)
            {
                Health = healthAfterRecover;
                RecoveringHealth = false;

                sfx_recoverHealthLoop.Stop(true);
            }
        }

        #endregion

        #region Animación

        /// <summary>
        /// Método que se encarga de animar a megaman de acuerdo a la situación en la 
        /// que se encuentra
        /// </summary>
        /// <param name="gametime"></param>
        private void Animate(GameTime gametime)
        {
            // Animaciones de daño
            if (damageTimer.IsActive)
            {
                if (!onSlideDamage)
                {
                    animations.StartAnimation("DamageNormal");
                }
                else
                {
                    animations.StartAnimation("DamageSlide");
                }

                animations.Update(gametime);
            }
            // Animación de spawn
            else
            {
                if (IsOnSpawn)
                {
                    animations.StartAnimation("Spawn");

                    animations.Update(gametime);
                }
                else
                {
                    // Animaciones de disparo
                    if (CurrentWeapon.ShotAnimationTimer.IsActive)
                    {
                        if (IsOnLadder)
                        {
                            if (AlmostOnTopOfLadder)
                            {
                                animations.StartAnimation("LadderShoot");
                            }
                            else
                            {
                                animations.StartAnimation("LadderShoot");
                            }

                            animations.Update(gametime);
                        }
                        else
                        {
                            if (IsOnGround)
                            {
                                if (!IsSliding)
                                {
                                    if (IsMoving)
                                    {
                                        if (animations.CurrentAnimation == "Walk")
                                        {
                                            animations.TransitionToAnimation("WalkShoot");
                                        }
                                        else
                                        {
                                            animations.StartAnimation("WalkShoot");
                                        }
                                    }
                                    else
                                    {
                                        animations.StartAnimation("Shoot");
                                    }
                                }
                                else
                                {
                                    animations.StartAnimation("Slide");
                                }
                            }
                            else
                            {
                                animations.StartAnimation("JumpShoot");
                            }


                            animations.Update(gametime);
                        }
                    }
                    // Animaciones normales
                    else
                    {
                        if (IsSliding)
                        {
                            animations.StartAnimation("Slide");

                            animations.Update(gametime);
                        }
                        else if (IsOnLadder)
                        {
                            if (AlmostOnTopOfLadder)
                            {
                                animations.StartAnimation("LadderTop");
                            }
                            else
                            {
                                animations.StartAnimation("Ladder");
                            }

                            if (Speed.Y != 0f)
                            {
                                animations.Update(gametime);
                            }
                        }
                        else
                        {
                            if (IsOnGround)
                            {
                                if (IsMoving)
                                {
                                    if (Math.Abs(NormalizedSpeed.X) < maxHorizontalSpeed / 2f)
                                    {
                                        animations.StartAnimation("Step");
                                    }
                                    else
                                    {
                                        if (animations.CurrentAnimation == "WalkShoot")
                                        {
                                            animations.TransitionToAnimation("Walk");
                                        }
                                        else
                                        {
                                            animations.StartAnimation("Walk");
                                        }
                                    }
                                }
                                else
                                {
                                    animations.StartAnimation("Idle");
                                }
                            }
                            else
                            {
                                animations.StartAnimation("Jump");
                            }


                            animations.Update(gametime);
                        }
                    }
                }
            }
        }

        #endregion

        #region Dibujado

        /// <summary>
        /// Método que se encarga de dibujar a megaman
        /// </summary>
        /// <param name="spritebatch"></param>
        public override void Draw(SpriteBatch spritebatch)
        {
            if (!deathTimer.IsActive)
            {
                // Se determina el sentido de dibujado de megaman tomando en cuenta
                // la dirección hacia donde está volteando
                if (CurrentFacingDirection == Directions.Right)
                {
                    animations.Flipped = false;
                }
                else
                {
                    animations.Flipped = true;
                }

                animations.Draw(spritebatch, CenteredDrawPosition, Color.White * opacity);

                DrawFlashEffect(spritebatch);
            }

            CurrentWeapon.Draw(spritebatch);

            healthBar.Draw(spritebatch);

            base.Draw(spritebatch);
        }

        /// <summary>
        /// Método que dibuja el efecto flash blanco de megaman cuando es dañado
        /// </summary>
        /// <param name="spritebatch"></param>
        private void DrawFlashEffect(SpriteBatch spritebatch)
        {
            if (damageTimer.IsActive)
            {
                spritebatch.Begin();

                // Se calcula la posición centrada de dibujo
                Vector2 position = Vector2.Zero;
                position.X = Center.X - Resources.T2D_Megaman_DamageFlash.Width / 2 - Global.Viewport.X;
                position.Y = Center.Y - Resources.T2D_Megaman_DamageFlash.Height / 2 - Global.Viewport.Y;

                // Finalmente se dibuja la textura con el 70% de opacidad
                spritebatch.Draw(Resources.T2D_Megaman_DamageFlash, position, Color.White * opacity * 0.7f);

                spritebatch.End();
            }
        }

        #endregion

        #region Implementación de IHealthObject

        /// <summary>
        /// Vida máxima de megaman
        /// </summary>
        public int MaxHealth
        {
            get { return 100; }
        }

        /// <summary>
        /// Vida actual de megaman
        /// </summary>
        public int Health { get; set; }

        #endregion
    }
}
