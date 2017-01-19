#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Megaman_Final_Fight.GlobalClasses;
using Megaman_Final_Fight.HelperClasses;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses
{
    /// <summary>
    /// Clase que sirve como base para proporcionar la funcionalidad básica a los enemigos
    /// </summary>
    public abstract class Enemy : PhysicsObject, IHealthObject, IScreenObject
    {
        #region Campos

        // Referencia a la instancia de megaman
        protected Megaman megaman;

        // Bandera para evitar llamadas continuas al método Reset
        private bool invokedReset;

        private bool alive;

        protected Timer damageTimer;

        #endregion

        #region Propiedades

        protected abstract int BodyDamage { get; }

        protected abstract bool HasActiveShots { get; }

        protected bool IsInvinsible { get; set; }

        protected bool AlwaysFaceMegaman { get; set; }

        public float AngleBetweenMegaman
        {
            get
            {
                float x = megaman.Center.X - Center.X;
                float y = megaman.Center.Y - Center.Y;

                return (float)Math.Atan2(y, x);
            }
        }

        public float DistanceToMegaman
        {
            get
            {
                float x;
                float y;
                float hypotenuse;

                x = Center.X - megaman.Center.X;
                y = Center.Y - megaman.Center.Y;

                hypotenuse = (float)Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

                return hypotenuse;
            }
        }

        protected bool Flipped
        {
            get { return CurrentFacingDirection == Directions.Left; }
        }

        protected float Opacity { get; private set; }

        private bool CanUpdateShots
        {
            get
            {
                return !megaman.Camera.IsOnTransition && !megaman.IsOnSpawn
                    && !megaman.RecoveringHealth;
            }
        }

        #endregion

        #region Delegados y eventos

        protected delegate void MegamanWeaponCollisionHandler();

        protected event MegamanWeaponCollisionHandler CollidedWithMegamanWeapon;

        #endregion

        #region Constructor

        public Enemy(Megaman megaman, int width, int height, Color boxColor, Vector2 initialPosition, 
            Directions startingDirection)
            : base(width, height, boxColor, startingDirection)
        {
            this.megaman = megaman;

            // Se calcula la posición central de manera que el enemigo quede centrado en el tile
            float x = initialPosition.X - (width / 2) + GameTiles.TileWidth / 2;
            float y = initialPosition.Y + GameTiles.TileHeight - height;

            InitialPosition = new Vector2(x, y);
            Position = InitialPosition;

            DrawRelativeToCamera = true;
            invokedReset = false;
            IsInvinsible = true;
            AlwaysFaceMegaman = true;

            damageTimer = new Timer(75);

            // Suscripciones a eventos
            megaman.Camera.OnTransitionFinish += new LevelCamera.CameraTransitionEventHander(Camera_OnTransitionFinish);
            megaman.OnSpawnStart += new Megaman.MegamanEventHandler(megaman_OnSpawnStart);
        }

        #endregion

        #region Métodos

        /// <summary>
        /// Método que deben de implementar las clases derivadas para realizar su propia
        /// lógica de actualización
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void OnUpdate(GameTime gameTime);

        /// <summary>
        /// Método que deben de implementar las clases derivadas para realizar su propia
        /// lógica de dibujado
        /// </summary>
        /// <param name="spritebatch"></param>
        public abstract void OnDraw(SpriteBatch spritebatch);

        /// <summary>
        /// Método que deben de implementar las clases derivadas para realizar su propia
        /// lógica de actualización de disparos
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void UpdateShots(GameTime gameTime);

        /// <summary>
        /// Método que deben de implementar las clases derivadas para realizar su propia
        /// lógica de dibujado de disparos
        /// </summary>
        /// <param name="spritebatch"></param>
        public abstract void DrawShots(SpriteBatch spritebatch);

        /// <summary>
        /// Método que deben de implementar las clases derivadas para realizar su propia
        /// lógica de reinicio de disparos
        /// </summary>
        /// <param name="spritebatch"></param>
        public abstract void ResetShots();

        /// <summary>
        /// Método que deben de implementar las clases derivadas para realizar su propia
        /// lógica de revisión de colisiones con los disparos
        /// </summary>
        /// <param name="spritebatch"></param>
        protected abstract void CheckShotsCollision();

        /// <summary>
        /// Método que revisa colisiones con megaman e inflije daño en caso de ocurrir alguna
        /// </summary>
        protected virtual void CheckCollisionWithMegaman()
        {
            if (BoundingBox.Intersects(megaman.BoundingBox))
            {
                megaman.InflictDamage(BodyDamage);
            }
        }

        protected virtual void CheckCollisionWithMegamanWeapon()
        {
            bool collided = false;

            if (IsInvinsible)
            {
                if (megaman.CurrentWeapon.Intersects(BoundingBox, MegamanWeapon.CollisionActions.Reflect))
                {
                    collided = true;

                    Resources.SFX_Enemy_Reflect.Play();
                }
            }
            else
            {
                if (megaman.CurrentWeapon.Intersects(BoundingBox, MegamanWeapon.CollisionActions.Deactivate))
                {
                    InflictDamage(megaman.CurrentWeapon.Damage);

                    damageTimer.Restart();

                    collided = true;

                    Resources.SFX_Enemy_Hit.Play();
                }
            }

            if (collided)
            {
                if (CollidedWithMegamanWeapon != null)
                {
                    CollidedWithMegamanWeapon.Invoke();
                }
            }
        }

        protected void InflictDamage(int damage)
        {
            Health = (int)MathHelper.Clamp(Health - damage, 0, MaxHealth);

            if (Health == 0)
            {
                alive = false;
            }
        }

        protected void HandleDirection()
        {
            if (AlwaysFaceMegaman)
            {
                if (Center.X > megaman.Center.X)
                {
                    CurrentFacingDirection = Directions.Left;
                }
                else
                {
                    CurrentFacingDirection = Directions.Right;
                }
            }
        }

        private void HandleDamageOpacity(GameTime gameTime)
        {
            damageTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            if (damageTimer.IsActive)
            {
                Opacity = 1f - (float)Math.Sin(damageTimer.ElapsedTimePercentage * Math.PI);
            }
            else
            {
                Opacity = 1f;
            }
        }

        #endregion

        #region Suscripciones a eventos

        /// <summary>
        /// Evento para detectar el momento en que termina una transición de cámara
        /// </summary>
        /// <param name="transitionDirection"></param>
        void Camera_OnTransitionFinish(LevelCamera.TransitionDirections transitionDirection)
        {
            if (!invokedReset)
            {
                ResetObject();
            }

            ResetShots();
        }

        /// <summary>
        /// Evento para reiniciar el enemigo en cuanto megaman aparece de nuevo en el nivel
        /// </summary>
        void megaman_OnSpawnStart()
        {
            if (!invokedReset)
            {
                ResetObject();
            }

            ResetShots();
        }

        #endregion

        #region Implementación de IScreenObject

        /// <summary>
        /// Posición inicial del enemigo
        /// </summary>
        public Vector2 InitialPosition { get; set; }

        /// <summary>
        /// Indica si el enemigo ha salido de la pantalla
        /// </summary>
        public bool ExitedScreen { get; set; }

        /// <summary>
        /// Indica si el enemigo debe de actualizarse si está en el mismo cuarto que megaman
        /// </summary>
        public virtual bool UpdateIfOnRoom
        {
            get { return false; }
        }

        /// <summary>
        /// Indica si el enemigo se encuentra visible dentro de la cámara
        /// </summary>
        public virtual bool IsOnScreen
        {
            get
            {
                return Global.ScreenContainsRectangle(BoundingBox);
            }
        }

        /// <summary>
        /// Indica si el enemigo en su posición inicial está dentro de la cámara
        /// </summary>
        public virtual bool IsOnScreenOnInitialPosition
        {
            get
            {
                // Se obtienen las coordenadas de la posición inicial
                int x = (int)InitialPosition.X;
                int y = (int)InitialPosition.Y;

                Rectangle rectangle = new Rectangle(x, y, Width, Height);

                return Global.ScreenContainsRectangle(rectangle);
            }
        }

        /// <summary>
        /// Indica si el enemigo está dentro del mismo cuarto que megaman
        /// </summary>
        public virtual bool IsOnRoom
        {
            get
            {
                return Global.RoomContainsRectangle(BoundingBox);
            }
        }

        /// <summary>
        /// Indica si el enemigo puede actualizarse
        /// </summary>
        public bool CanUpdate
        {
            get
            {
                if (!alive)
                {
                    return false;
                }
                else
                {
                    if (IsOnScreen)
                    {
                        return !megaman.Camera.IsOnTransition && !megaman.IsOnSpawn
                            && !megaman.RecoveringHealth;
                    }
                    else
                    {
                        return UpdateIfOnRoom && IsOnRoom;
                    }
                }
            }
        }

        /// <summary>
        /// Indica si el enemigo puede dibujarse
        /// </summary>
        public bool CanDraw
        {
            get
            {
                if (!alive)
                {
                    return false;
                }
                else
                {
                    if (IsOnScreen)
                    {
                        if (!UpdateIfOnRoom && ExitedScreen)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Método que actualiza el enemigo
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (CanUpdate)
            {
                HandleDirection();

                OnUpdate(gameTime);

                base.Update(gameTime);

                CheckCollisionWithMegamanWeapon();

                CheckCollisionWithMegaman();

                HandleDamageOpacity(gameTime);

                invokedReset = false;

                if (!IsOnScreen)
                {
                    ExitedScreen = true;
                }
                else
                {
                    ExitedScreen = false;
                }
            }
            else
            {
                if (!UpdateIfOnRoom)
                {
                    if (!IsOnScreenOnInitialPosition && !megaman.Camera.IsOnTransition)
                    {
                        if (!invokedReset)
                        {
                            ResetObject();
                        }
                    }
                }
            }

            if (HasActiveShots)
            {
                if (CanUpdateShots)
                {
                    UpdateShots(gameTime);

                    CheckShotsCollision();
                }
            }
            else
            {
                ResetShots();
            }
        }

        /// <summary>
        /// Método que dibuja el enemigo
        /// </summary>
        /// <param name="spritebatch"></param>
        public override void Draw(SpriteBatch spritebatch)
        {
            if (CanDraw)
            {
                OnDraw(spritebatch);

                base.Draw(spritebatch);
            }

            if (HasActiveShots)
            {
                DrawShots(spritebatch);
            }
        }

        /// <summary>
        /// Método que resetea el enemigo
        /// </summary>
        public virtual void ResetObject()
        {
            Position = InitialPosition;

            ExitedScreen = false;

            invokedReset = true;

            Health = MaxHealth;

            alive = true;

            Opacity = 1f;

            damageTimer.Stop();
        }

        #endregion

        #region Implementación de IHealthObject

        public abstract int MaxHealth { get; }

        public int Health { get; set; }

        #endregion
    }
}
