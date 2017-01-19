#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using xTile;
using xTile.Layers;
using xTile.Tiles;

using Megaman_Final_Fight.GlobalClasses;
using Megaman_Final_Fight.HelperClasses;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses
{
    /// <summary>
    /// Clase que proporciona funcionalidad para permitir detección de colisiones con el mapa
    /// </summary>
    public abstract class CollisionObject : GameObject
    {
        #region Campos

        public Vector2 Speed;
        protected float automaticTileSpeedX;
        private const float DefaultAutomaticTileSpeedX = 4f;

        // Variables auxiliares para la detección de colisiones
        public bool HandleCollisions;
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

        public enum Directions
        {
            Right,
            Left,
            Up,
            Down
        }

        #endregion

        #region Propiedades

        /// <summary>
        /// Indica si el objeto ha chocado con un sólido en su parte superior
        /// </summary>
        protected bool HitSolidToUp
        {
            get { return collisionSides.Y == -1; }
        }

        /// <summary>
        /// Indica si el objeto ha chocado con un sólido en su parte inferior
        /// </summary>
        protected bool HitSolidToDown
        {
            get { return collisionSides.Y == 1; }
        }

        /// <summary>
        /// Indica si el objeto ha chocado con un sólido en su lado derecho
        /// </summary>
        protected bool HitSolidToRight
        {
            get { return collisionSides.X == 1; }
        }

        /// <summary>
        /// Indica si el objeto ha chocado con un sólido en su lado izquierdo
        /// </summary>
        protected bool HitSolidToLeft
        {
            get { return collisionSides.X == -1; }
        }

        /// <summary>
        /// Regresa la velocidad de movimiento del objeto sin tomar en cuenta la velocidad
        /// influida por el tile automático
        /// </summary>
        public Vector2 NormalizedSpeed
        {
            get
            {
                float x = Speed.X - automaticTileSpeedX;
                float y = Speed.Y;

                return new Vector2(x, y);
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que inicializa el objeto
        /// </summary>
        /// <param name="Width">Ancho del personaje</param>
        /// <param name="Height">Altura del personaje</param>
        /// <param name="BoxColor">Color de la CollisionBox en caso de ser dibujada</param>
        public CollisionObject(int width, int height, Color boxColor)
            : base(width, height, boxColor)
        {
            this.Speed = Vector2.Zero;
            this.automaticTileSpeedX = 0f;
            this.HandleCollisions = true;
            base.DrawRelativeToCamera = true;
        }

        #endregion

        #region Actualización

        public virtual void Update(GameTime gameTime)
        {
            // Limpieza de variables
            collisionSides = Vector2.Zero;
            automaticTileSpeedX = 0f;

            if (HandleCollisions)
            {
                // Sólo checar colisiones en caso de que el objeto se encuentre en movimiento
                if (Speed.X != 0f)
                {
                    CheckHorizontalCollisions();
                }
                if (Speed.Y != 0f)
                {
                    CheckVerticalCollisions();
                }

                PlatformCollision();
            }
            else
            {
                Position += Speed;
            }
        }

        #endregion

        #region Colisiones

        private void CheckHorizontalCollisions()
        {
            // Se obtiene la posición del tile que nos servira más adelante a la hora de asignar la
            // posición del personaje en caso de ocurrir una colisión
            GetTilePosition(true);

            collided = HasWallAtOffset(new Vector2(Speed.X, 0f));

            AssignPosition(true);
        }

        private void CheckVerticalCollisions()
        {
            // Se obtiene la posición del tile que nos servira más adelante a la hora de asignar la 
            // posición del personaje en caso de ocurrir una colisión
            GetTilePosition(false);

            collided = HasWallAtOffset(new Vector2(0f, Speed.Y));

            AssignPosition(false);
        }

        /// <summary>
        /// Método que asigna la posición nueva al personaje
        /// </summary>
        /// <param name="horizontal">true si se trata de movimiento horizontal</param>
        private void AssignPosition(bool horizontal)
        {
            if (horizontal)
            {
                // Lado derecho
                if (Speed.X > 0f)
                {
                    if (collided)
                    {
                        Position.X = tilePosition.X - Width;
                        collisionSides.X = 1;
                    }
                    else
                    {
                        Position.X += Speed.X;
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
                        Position.X += Speed.X;
                    }
                }
            }
            else
            {
                // Parte inferior
                if (Speed.Y > 0f)
                {
                    if (collided)
                    {
                        Position.Y = tilePosition.Y - Height;
                        collisionSides.Y = 1;
                    }
                    else
                    {
                        Position.Y += Speed.Y;
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
                        Position.Y += Speed.Y;
                    }
                }
            }
        }

        #endregion

        #region Colisiones Con Plataformas

        private void PlatformCollision()
        {
            foreach (MovingPlatform m in Game1.MovingPlatforms)
            {
                m.CheckCollisions(this, ref collisionSides);
            }
        }

        #endregion

        #region Posición de tiles

        /// <summary>
        /// Método que obtiene la posición del tile que intersecta con la nextPosition
        /// </summary>
        /// <param name="horizontal">true si se trata de movimiento horizontal</param>
        private void GetTilePosition(bool horizontal)
        {
            // Vector auxiliar para almacenar la posición en la que el objeto
            // estaría en caso de no ocurrir una colisión
            Vector2 nextPosition = Position + Speed;

            if (horizontal)
            {
                // Lado derecho
                if (Speed.X > 0f)
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
                if (Speed.Y > 0f)
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

        #endregion

        #region Offset

        /// <summary>
        /// Método que revisa colisiones en la dirección y distancia indicada en el offset 
        /// (únicamente en una sola dirección)
        /// </summary>
        /// <param name="offset">Vector para indicar la dirección y magnitud de la revisión
        /// de colisiones, por ejemplo: si offset.X == 2f entonces se realizará la revisión
        /// a 2 pixeles de distancia del lado derecho del personaje</param>
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
                    if (type == GameTiles.Solid)
                    {
                        return true;
                    }
                    // Si es un tile de arrastre automático y el objeto se encuentra encima de él
                    // entonces se aplica movimiento
                    else if (type == GameTiles.LeftMovingTile || type == GameTiles.RightMovingTile)
                    {
                        if ((Position.Y + Height) <= (y / GameTiles.TileHeight * GameTiles.TileHeight))
                        {
                            if (type == GameTiles.LeftMovingTile)
                            {
                                automaticTileSpeedX = -DefaultAutomaticTileSpeedX;
                            }
                            else
                            {
                                automaticTileSpeedX = DefaultAutomaticTileSpeedX;
                            }
                        }

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
