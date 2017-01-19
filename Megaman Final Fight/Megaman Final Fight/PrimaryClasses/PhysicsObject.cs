#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Megaman_Final_Fight.HelperClasses;

using xTile;
using xTile.Layers;
using xTile.Tiles;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses
{
    /// <summary>
    /// Clase que sirve para proporcionar características físicas como gravedad o aceleración  
    /// </summary>
    public abstract class PhysicsObject : CollisionObject
    {
        #region Campos

        public const float GroundConstantSpeedY = 1f;

        protected float maxFallSpeed;
        protected float maxHorizontalSpeed;
        protected float jumpSpeed;
        protected float gravityAcceleration;
        protected float horizontalAcceleration;

        protected bool applyGravity;

        private bool movedOnX;

        private Vector2 previousPosition;

        private bool wasOnGround;

        #endregion

        #region Delegados y eventos

        public delegate void PhysicsObjectHandler();
        public event PhysicsObjectHandler OnLanding;

        #endregion

        #region Propiedades

        /// <summary>
        /// Regresa true en caso de que el objeto esté brincando
        /// </summary>
        public virtual bool IsJumping
        {
            get { return (Speed.Y < 0f); }
        }

        /// <summary>
        /// Regresa true en caso de que el objeto esté cayendo
        /// </summary>
        public virtual bool IsFalling 
        {
            get { return (Speed.Y > 0f  && !base.HitSolidToDown); }
        }

        /// <summary>
        /// Regresa true en caso de que el objeto se esté moviendo
        /// </summary>
        public virtual bool IsMoving
        {
            get 
            { 
                if (NormalizedSpeed.X != 0f)
                {
                    return true;
                }
                else if (NormalizedSpeed.Y < 0f || NormalizedSpeed.Y > GroundConstantSpeedY)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Regresa true en caso de que el objeto esté pisando el suelo
        /// </summary>
        public virtual bool IsOnGround
        {
            get { return base.HitSolidToDown; }
        }

        /// <summary>
        /// Permite obtener o asignar(private) la dirección a la que el objeto
        /// estaba volteando anteriormente
        /// </summary>
        public Directions PreviousFacingDirection { get; private set; }

        /// <summary>
        /// Permite obtener o asignar(private) la dirección a la que el objeto
        /// está volteando actualmente
        /// </summary>
        public Directions CurrentFacingDirection { get; protected set; }

        /// <summary>
        /// Regresa la distancia que se movió el objeto en el frame actual
        /// </summary>
        public Vector2 DistanceTraveled
        {
            get
            {
                return Position - previousPosition;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que inicializa el objeto
        /// </summary>
        /// <param name="width">Ancho del personaje</param>
        /// <param name="height">Altura del personaje</param>
        /// <param name="boxColor">Color de la CollisionBox en caso de ser dibujada</param>
        /// <param name="startingDirection">Dirección a la cual el personaje volteará inicialmente</param>
        public PhysicsObject(int width, int height, Color boxColor, Directions startingDirection)
            : base(width, height, boxColor)
        {
            this.jumpSpeed = 19.2f;
            this.maxFallSpeed = 48;
            this.gravityAcceleration = 0.9f;
            this.maxHorizontalSpeed = 5.2f;
            this.horizontalAcceleration = 0.09f;

            this.CurrentFacingDirection = startingDirection;
            this.applyGravity = true;
        }

        #endregion

        #region Actualización

        public override void Update(GameTime gameTime)
        {
            AssignPreviousStates();

            ApplyGravity();

            base.Update(gameTime);

            FixSpeed();

            HandleFinalStatesOperations();
        }

        #endregion

        #region Métodos principales

        /// <summary>
        /// Método que simplemente se encarga de aplicar fuerza de gravedad al objeto
        /// </summary>
        private void ApplyGravity()
        {
            if (applyGravity)
            {
                Speed.Y = MathHelper.Clamp(Speed.Y + gravityAcceleration, -jumpSpeed, maxFallSpeed);
            }
        }

        /// <summary>
        /// Este método se encarga de arreglar la velocidad en caso de ocurrir ciertas
        /// condiciones especiales
        /// </summary>
        protected virtual void FixSpeed()
        {
            // Si el objeto está en el suelo se resetea su velcidad en Y para evitar 
            // que se acumule la velocidad y así comenzar desde cero en caso de caer
            if (IsOnGround)
            {
                Speed.Y = GroundConstantSpeedY;
            }
            else
            {
                //  Si el objeto está en el aire y choca con el techo entonces se resetea su 
                // velocidad en Y para que el objeto comience a caer en vez de quedar flotanto
                // por una pequeña cantidad de tiempo
                if (HitSolidToUp)
                {
                    Speed.Y = 0f;
                }
            }

            // Si en el frame actual no se invocaron los métodos de movimiento horizontal entonces
            // se resetea la velocidad en X del objeto
            if (!movedOnX)
            {
                Speed.X = automaticTileSpeedX;
            }

            // Reinicio de la bandera para utilizarla en el siguiente frame
            movedOnX = false;
        }

        #endregion

        #region Métodos de movimiento

        /// <summary>
        /// Método que inicia el brinco del objeto
        /// </summary>
        public virtual void Jump()
        {
            if (IsOnGround && !IsJumping)
            {
                Speed.Y = -jumpSpeed;
            }
        }

        /// <summary>
        /// Método que cancela el brinco del objeto
        /// </summary>
        public virtual void CancelJump()
        {
            if (IsJumping)
            {
                Speed.Y = 0f;
            }
        }

        public virtual void MoveRight()
        {
            CurrentFacingDirection = Directions.Right;

            if (IsOnGround)
            {
                // Movimiento con aceleración y con crecimiento exponencial de 35%
                Speed.X = MathHelper.Clamp((Speed.X + horizontalAcceleration) * 1.35f,
                    0f, maxHorizontalSpeed + automaticTileSpeedX);
            }
            else
            {
                // Movimiento con aceleración de valor inicial de 2 y con crecimiento
                // exponencial de 70%
                Speed.X = MathHelper.Clamp((Speed.X + horizontalAcceleration) * 1.7f,
                    2f, maxHorizontalSpeed + automaticTileSpeedX);
            }

            // Se enciende la bandera para indicar que el objeto se ha desplazado horizontalmente
            movedOnX = true;
        }

        public virtual void MoveLeft()
        {
            CurrentFacingDirection = Directions.Left;

            if (IsOnGround)
            {
                // Movimiento con aceleración y con crecimiento exponencial de 35%
                Speed.X = MathHelper.Clamp((Speed.X - horizontalAcceleration) * 1.35f,
                    -maxHorizontalSpeed + automaticTileSpeedX, 0f);
            }
            else
            {
                // Movimiento con aceleración de valor inicial de -2 y con crecimiento
                // exponencial de 70%
                Speed.X = MathHelper.Clamp((Speed.X - horizontalAcceleration) * 1.7f,
                    -maxHorizontalSpeed + automaticTileSpeedX, -2f);
            }

            // Se enciende la bandera para indicar que el objeto se ha desplazado horizontalmente
            movedOnX = true;
        }

        #endregion

        #region Manejo de estados

        /// <summary>
        /// Método que se encarga de guardar estados anteriores
        /// </summary>
        private void AssignPreviousStates()
        {
            wasOnGround = IsOnGround;

            previousPosition = Position;

            PreviousFacingDirection = CurrentFacingDirection;
        }

        /// <summary>
        /// Método que se encarga de realizar acciones en relación entre los estados
        /// anteriores y los estados actuales
        /// </summary>
        private void HandleFinalStatesOperations()
        {
            // Invocación de evento al tocar el piso
            if (!wasOnGround && IsOnGround)
            {
                if (OnLanding != null)
                {
                    OnLanding.Invoke();
                }
            }
        }

        #endregion
    }
}
