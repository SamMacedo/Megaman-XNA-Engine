#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Megaman_Final_Fight.GlobalClasses;
using Megaman_Final_Fight.HelperClasses;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses
{
    /// <summary>
    /// Clase que sirve como base para proporcionar movilidad a los proyectiles
    /// </summary>
    public abstract class Projectile : GameObject
    {
        #region Campos

        /// <summary>
        /// Velocidad de movimiento del proyectil
        /// </summary>
        public float Speed;

        /// <summary>
        /// Determina si el proyectil puede actualizarse y dibujarse
        /// </summary>
        public bool Active;

        /// <summary>
        /// Daño que inflije el proyectil
        /// </summary>
        public int Damage;

        /// <summary>
        /// Indica si el proyectil puede colisionar con otro objeto
        /// </summary>
        public bool CanCollide;

        /// <summary>
        /// Indica si la textura debe de dibujarse volteada horizontalmente
        /// </summary>
        public bool FlipTexture;

        // Variables auxiliares para la detección de colisiones
        public bool HandleCollisions;
        private bool invokedCollidedWithLevel;
        private bool collided;
        private Vector2 tilePosition;

        /// <summary>
        /// Indica el lugar en donde ha ocurrido una colisión por ejemplo:
        /// X =  1  (colisión a la derecha)
        /// X = -1  (colisión a la izquierda)
        /// Y =  1  (colisión hacia abajo)
        /// Y = -1  (colisión hacia arriba)
        /// </summary>
        private Vector2 collisionSides;

        /// <summary>
        /// Posibles direcciones de colisión del proyectil con el nivel 
        /// </summary>
        public enum Directions
        {
            Right,
            Left,
            Up,
            Down
        }

        #endregion

        #region Delegados y eventos

        public delegate void CollisionEventHandler(Vector2 collisionSides);
        public event CollisionEventHandler CollidedWithLevel;

        #endregion

        #region Propiedades

        /// <summary>
        /// Ángulo del proyectil, en caso de asignación lo convierte a radianes 
        /// y en caso de obtener se convierte a grados
        /// </summary>
        public float Angle
        {
            set { angle = MathHelper.ToRadians(value); }
            get { return MathHelper.ToDegrees(angle); }
        }
        private float angle;

        /// <summary>
        /// Regresa la velocidad de movimiento de los componentes X y Y del proyectil
        /// </summary>
        public virtual Vector2 ComponentSpeed
        {
            get
            {
                float speedX = Speed * (float)Math.Cos(angle);
                float speedY = Speed * -(float)Math.Sin(angle);

                return new Vector2(speedX, speedY);
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que inicializa el proyectil
        /// </summary>
        /// <param name="speed">Velocidad de desplazamiento del proyectil</param>
        /// <param name="damage">Daño que inflije el proyectil</param>
        /// <param name="width">Ancho del proyectil</param>
        /// <param name="height">Alto del proyectil</param>
        /// <param name="boxColor">Color de la caja de colisiones</param>
        public Projectile(float speed, int damage, int width, int height, Color boxColor)
            : base(width, height, boxColor)
        {
            this.Speed = speed;
            this.Damage = damage;

            this.Active = false;
            this.CanCollide = true;

            this.HandleCollisions = false;

            this.tilePosition = Vector2.Zero;

            base.DrawRelativeToCamera = true;
        }

        #endregion

        #region Actualización

        /// <summary>
        /// Método que actualiza el movimiento del disparo
        /// </summary>
        public virtual void Update()
        {
            if (Active)
            {
                HandleMovement();

                // Si el proyectil deja de ser visible entonces se destruye
                if (Position.X > (Global.Viewport.Location.X + Global.Viewport.Width) || 
                    Position.X + Width < Global.Viewport.Location.X)
                {
                    Deactivate();
                }
                else if(Position.Y + Height > Global.Viewport.Location.Y + Global.Viewport.Height || 
                    Position.Y < Global.Viewport.Location.Y)
                {
                    Deactivate();
                }
            }
        }

        /// <summary>
        /// Método que se encarga de manejar el movimiento del proyectil
        /// </summary>
        protected virtual void HandleMovement()
        {
            // Limpieza de variables
            collisionSides = Vector2.Zero;
            invokedCollidedWithLevel = false;

            // Manejo de colisiones con el nivel
            if (HandleCollisions)
            {
                if (ComponentSpeed.X != 0f)
                {
                    CheckHorizontalCollisions();
                }
                if (ComponentSpeed.Y != 0f)
                {
                    CheckVerticalCollisions();
                }
            }
            // Movimiento de acuerdo al valor del ángulo 
            else
            {
                Position.X += ComponentSpeed.X;
                Position.Y += ComponentSpeed.Y;
            }
        }

        #endregion

        #region Dibujado

        /// <summary>
        /// Método que dibuja el disparo
        /// </summary>
        /// <param name="spritebatch">Objeto requerido para dibujar</param>
        /// <param name="Texture">Textura que se va a dibujar</param>
        public virtual void Draw(SpriteBatch spritebatch, Texture2D Texture)
        {
            if (Active)
            {
                spritebatch.Begin();

                // Si el objeto está en dirección opuesta entonces se dibuja volteado horizontalmente
                if (FlipTexture)
                {
                    spritebatch.Draw(Texture, CenteredDrawPosition, null, Color.White, 0f, 
                        new Vector2(Texture.Width / 2, Texture.Height / 2), 1f, 
                        SpriteEffects.FlipHorizontally, 1f);
                }
                // Si no, se dibuja normalmente
                else
                {
                    spritebatch.Draw(Texture, CenteredDrawPosition, null, Color.White, 0f, 
                        new Vector2(Texture.Width / 2, Texture.Height / 2), 1f, 
                        SpriteEffects.None, 1f);
                }

                spritebatch.End();

                base.Draw(spritebatch);
            }
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Método que activa (dispara el proyectil) en las coordenadas y dirección indicadas
        /// </summary>
        /// <param name="position">Posición en donde va a ser activado el proyectil</param>
        /// <param name="angle">Angulo de trayectoria</param>
        public virtual void Activate(Vector2 position, float angle)
        {
            Active = true;
            CanCollide = true;

            // Se posiciona centrado el proyectil
            position.X -= Width / 2;
            position.Y -= Height / 2;
            Position = position;

            Angle = angle;
        }

        /// <summary>
        /// Método que desactiva el proyectil
        /// </summary>
        public virtual void Deactivate()
        {
            Active = false;
        }

        /// <summary>
        /// Método que determina si el proyectil intersecta con otro objeto
        /// </summary>
        /// <param name="other"></param>
        /// <returns>true en caso de ocurrir una colisión</returns>
        public virtual bool Intersects(Rectangle other)
        {
            if (Active)
            {
                if (CanCollide)
                {
                    if (BoundingBox.Intersects(other))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Manejo de colisiones con el nivel

        private void CheckHorizontalCollisions()
        {
            // Se obtiene la posición del tile que nos servira más adelante a la hora de asignar la
            // posición del proyectil en caso de ocurrir una colisión
            GetTilePosition(true);

            collided = HasWallAtOffset(new Vector2(ComponentSpeed.X, 0f));

            AssignPosition(true);
        }

        private void CheckVerticalCollisions()
        {
            // Se obtiene la posición del tile que nos servira más adelante a la hora de asignar la 
            // posición del proyectil en caso de ocurrir una colisión
            GetTilePosition(false);

            collided = HasWallAtOffset(new Vector2(0f, ComponentSpeed.Y));

            AssignPosition(false);
        }

        /// <summary>
        /// Método que obtiene la posición del tile que intersecta con la nextPosition
        /// </summary>
        /// <param name="horizontal">true si se trata de movimiento horizontal</param>
        private void GetTilePosition(bool horizontal)
        {
            // Vector auxiliar para almacenar la posición en la que el objeto
            // estaría en caso de no ocurrir una colisión
            Vector2 nextPosition = Position + ComponentSpeed;

            if (horizontal)
            {
                // Lado derecho
                if (ComponentSpeed.X > 0f)
                {
                    tilePosition.X = (((int)nextPosition.X + Width) / 64) * 64;
                }
                // Lado izquierdo
                else
                {
                    tilePosition.X = ((int)nextPosition.X - 1) / 64 * 64;
                }
            }
            else
            {
                // Parte inferior
                if (ComponentSpeed.Y > 0f)
                {
                    tilePosition.Y = ((int)(nextPosition.Y + Height) / 64) * 64;
                }
                // Parte superior
                else
                {
                    tilePosition.Y = ((int)nextPosition.Y - 1) / 64 * 64;
                }
            }
        }

        /// <summary>
        /// Método que asigna la posición nueva al proyectil
        /// </summary>
        /// <param name="horizontal">true si se trata de movimiento horizontal</param>
        private void AssignPosition(bool horizontal)
        {
            float speedX = ComponentSpeed.X;
            float speedY = ComponentSpeed.Y;

            if (horizontal)
            {
                // Lado derecho
                if (speedX > 0f)
                {
                    if (collided)
                    {
                        Position.X = tilePosition.X - Width;
                        collisionSides.X = 1;
                    }
                    else
                    {
                        Position.X += speedX;
                    }
                }
                // Lado izquierdo
                else
                {
                    if (collided)
                    {
                        Position.X = tilePosition.X + 64;
                        collisionSides.X = -1;
                    }
                    else
                    {
                        Position.X += speedX;
                    }
                }
            }
            else
            {
                // Parte inferior
                if (speedY > 0f)
                {
                    if (collided)
                    {
                        Position.Y = tilePosition.Y - Height;
                        collisionSides.Y = 1;
                    }
                    else
                    {
                        Position.Y += speedY;
                    }
                }
                // Parte superior
                else
                {
                    if (collided)
                    {
                        Position.Y = tilePosition.Y + 64;
                        collisionSides.Y = -1;
                    }
                    else
                    {
                        Position.Y += speedY;
                    }
                }
            }

            // Invocación de evento
            if (collided && !invokedCollidedWithLevel)
            {
                if (CollidedWithLevel != null)
                {
                    CollidedWithLevel.Invoke(collisionSides);
                    invokedCollidedWithLevel = true;
                }
            }
        }

        #endregion

        #region Offset

        /// <summary>
        /// Método que revisa colisiones en la dirección y distancia indicada en el offset 
        /// (únicamente en una sola dirección)
        /// </summary>
        /// <param name="offset">Vector para indicar la dirección y magnitud de la revisión
        /// de colisiones, por ejemplo: si offset.X == 2f entonces se realizará la revisión
        /// a 2 pixeles de distancia del lado derecho del proyectil</param>
        /// <returns></returns>
        public bool HasWallAtOffset(Vector2 offset)
        {
            int totalIncrements;
            int increment;
            int type;

            int x;
            int y;

            // Horizontal
            if (offset.X != 0f)
            {
                // Se obtienen las veces que se hará la revisión de colisiones
                totalIncrements = (Height / 32) + 1;

                for (int i = 0; i <= totalIncrements; i++)
                {
                    // Se calcula la posición del incremento para hacer las comparaciones
                    increment = (int)MathHelper.Clamp(i * 32, 0, Height - 1);

                    // Lado derecho
                    if (offset.X > 0f)
                    {
                        x = (int)(Position.X + Width + offset.X);
                        y = (int)(Position.Y + offset.Y + increment);
                    }
                    // Lado izquierdo
                    else
                    {
                        x = (int)(Position.X + offset.X);
                        y = (int)(Position.Y + offset.Y + increment);
                    }

                    // Se obtiene el tipo de tile
                    type = GameTiles.CheckTile(Global.LevelLayer, x, y);

                    if (type == GameTiles.Solid
                        || type == GameTiles.LeftMovingTile || type == GameTiles.RightMovingTile)
                    {
                        return true;
                    }
                }
            }

            // Vertical
            if (offset.Y != 0f)
            {
                // Se obtienen las veces que se hará la revisión de colisiones
                totalIncrements = (Width / 32) + 1;

                for (int i = 0; i <= totalIncrements; i++)
                {
                    // Se calcula la posición del incremento para hacer las comparaciones
                    increment = (int)MathHelper.Clamp(i * 32, 0, Width - 1);

                    // Parte superior
                    if (offset.Y < 0f)
                    {
                        x = (int)(Position.X + offset.X + increment);
                        y = (int)(Position.Y + offset.Y);
                    }
                    // Parte inferior
                    else
                    {
                        x = (int)(Position.X + offset.X + increment);
                        y = (int)(Position.Y + Height + offset.Y);
                    }

                    // Se obtiene el tipo de tile
                    type = GameTiles.CheckTile(Global.LevelLayer, x, y);

                    // Si es un sóido entonces ha ocurrido una colisión
                    if (type == GameTiles.Solid
                        || type == GameTiles.LeftMovingTile || type == GameTiles.RightMovingTile)
                    {
                        return true;
                    }
                    // Si es un tope de escalera y el objeto se encuentra encima de ella entonces
                    // ha ocurrido una colisión
                    else if (type == GameTiles.Ladder)
                    {
                        if (GameTiles.IsTopLadderTile(x, y))
                        {
                            if ((Position.Y + Height) <= (y / GameTiles.TileHeight * GameTiles.TileHeight))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        #endregion
    }
}
