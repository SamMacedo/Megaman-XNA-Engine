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
    /// Clase abtracta que sirve para proporcionar característcas a las plataformas movibles
    /// </summary>
    public abstract class MovingPlatform : GameObject, IScreenObject
    {
        #region Campos

        // Velocidad de movimiento de la plataforma
        public Vector2 Speed;

        // Indica si la plataforma solo tiene colisiones encima de ella
        protected bool jumpOverPlatform;

        // Referencia a la instancia de megaman
        protected Megaman megaman;

        // Banderas para indicar el lugar de la plataforma en donde ocurrió
        // la colisión con megaman
        private bool hasMegamanAbove;
        private bool hasMegamanToLeft;
        private bool hasMegamanToRight;

        // Bandera para evitar llamadas continuas al método Reset
        private bool invokedReset;

        #endregion

        #region Propiedades

        /// <summary>
        /// Indica si megaman está encima de la plataforma
        /// </summary>
        public bool HasMegamanAbove
        {
            get { return hasMegamanAbove; }
        }

        /// <summary>
        /// Indica si megaman chocó con el dalo izquierdo de la plataforma
        /// </summary>
        public bool HasMegamanToLeft
        {
            get { return hasMegamanToLeft; } 
        }

        /// <summary>
        /// Indica si megaman chocó con el dalo derecho de la plataforma
        /// </summary>
        public bool HasMegamanToRight
        {
            get { return hasMegamanToRight; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que inicializa la plataforma
        /// </summary>
        /// <param name="megaman">Referencia a megaman</param>
        /// <param name="initialPosition">Posición inicial</param>
        /// <param name="width">Ancho de la plataforma</param>
        /// <param name="height">Altura de la plataforma</param>
        /// <param name="boundingboxColor">Color de la boundingBox de la plataforma</param>
        public MovingPlatform(Megaman megaman, Vector2 initialPosition, int width, int height, Color boundingboxColor)
            : base(width, height, boundingboxColor)
        {
            this.megaman = megaman;

            Speed = Vector2.Zero;

            InitialPosition = initialPosition;
            Position = initialPosition;

            DrawRelativeToCamera = true;
            jumpOverPlatform = false;
            invokedReset = false;

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

        #endregion

        #region Colisiones

        /// <summary>
        /// Método que se encarga de revisar colisiones y resolverlas en caso de ocurrir
        /// </summary>
        /// <param name="other">Objeto con el cual se revisarán colisiones</param>
        /// <param name="collisionSides">Vector para modificar e indicar los lados en donde
        /// ocurrió la colisión en caso de ocurrir</param>
        public virtual void CheckCollisions(CollisionObject other, ref Vector2 collisionSides)
        {
            // Reinicio de banderas
            hasMegamanAbove = false;
            hasMegamanToLeft = false;
            hasMegamanToRight = false;

            if (CanUpdate)
            {
                // Variables auxiliares
                Rectangle resultingRectangle;
                Rectangle otherRectangle;
                Rectangle thisRectangle;

                // Revisión horizontal de colisiones
                if (!jumpOverPlatform)
                {
                    // Se obtienen los rectángulos de los objetos, se restan las velocidadades en Y debido
                    // a que queremos revisar colisiones en la posición original/anterior de movimiento, 
                    // de esta manera prácticamente estaremos revisando colisiones solamente en el eje X 
                    // y no en el eje Y.
                    otherRectangle = new Rectangle((int)other.Position.X, (int)(other.Position.Y - other.Speed.Y),
                        other.Width, other.Height - 1);

                    thisRectangle = new Rectangle((int)Position.X, (int)(Position.Y - Speed.Y),
                        Width, Height - 1);

                    // Se obtiene el rectángulo resultante de la colisión
                    resultingRectangle = Rectangle.Intersect(thisRectangle, otherRectangle);

                    // Si ha ocurrido una colisión entonces resolver posicionamiento del otro objeto
                    if (resultingRectangle != Rectangle.Empty)
                    {
                        // Colisión en el lado izquierdo del otro objeto
                        if (resultingRectangle.Center.X < other.Center.X)
                        {
                            other.Position.X = this.Right;
                            collisionSides.X = 1f;

                            if (other is Megaman)
                            {
                                hasMegamanToRight = true;
                            }
                        }
                        // Colisión en el lado derecho del otro objeto
                        else
                        {
                            other.Position.X = this.Left - other.Width;
                            collisionSides.X = -1f;

                            if (other is Megaman)
                            {
                                hasMegamanToLeft = true;
                            }
                        }
                    }
                }

                // Revisión vertical de colisiones
                // Primero se obtiene el rectángulo resultante de la colisión, en caso de que la
                // plataforma esté moviéndose hacia abajo entonces se revisan colisiones en la posición
                // original (se resta la velocidad tanto en X como en Y) de la plataforma, esto se hace 
                // para en caso de haber una colisión entonces el otro objeto es movido hacia abajo junto 
                // con la plataforma
                if (Speed.Y > 0f)
                {
                    resultingRectangle = Rectangle.Intersect(new Rectangle((int)(Position.X - Speed.X),
                        (int)(Position.Y - Speed.Y), Width, Height), other.BoundingBox);
                }
                // En caso de que la plataforma no se esté moviendo hacia abajo entonces se revisan
                // colisiones en la posicion actual en Y de la plataforma
                else
                {
                    resultingRectangle = Rectangle.Intersect(new Rectangle((int)(Position.X - Speed.X),
                        (int)Position.Y, Width, Height), other.BoundingBox);
                }

                // Si ha ocurrido una colisión entonces resolver posicionamiento del otro objeto
                if (resultingRectangle != Rectangle.Empty)
                {
                    // Colisión en la parte inferior del otro objeto
                    if (resultingRectangle.Center.Y > other.Center.Y)
                    {
                        // Si el otro objeto está arriba de la plataforma entonces ha ocurrido
                        // una colisión
                        if ((other.Bottom - other.Speed.Y - 1) <= (this.Top - Speed.Y))
                        {
                            // Si el otro objeto no choca con un tile entonces se posiciona encima
                            // de la plataforma
                            if (!other.HasWallAtOffset(new Vector2(0, Speed.Y)))
                            {
                                other.Position.Y = this.Top - other.Height;
                            }
                            // Si no entonces se coloca encima del tile
                            else
                            {
                                other.Position.Y = (int)(other.Bottom + Speed.Y) / GameTiles.TileHeight *
                                    GameTiles.TileHeight - other.Height;
                            }

                            collisionSides.Y = 1f;

                            // Se mueve el otro objeto junto a la plataforma en el eje X
                            if (Speed.X != 0f)
                            {
                                // Si el otro objeto no tiene una tile a los lados entonces se mueve
                                // en X junto a la plataforma
                                if (!other.HasWallAtOffset(new Vector2(Speed.X, 0f)))
                                {
                                    other.Position.X += Speed.X;
                                }
                                // Si no entonces se posiciona pegado al tile
                                else
                                {
                                    // Izquierda
                                    if (Speed.X < 0f)
                                    {
                                        other.Position.X = (int)(other.Left + Speed.X) / GameTiles.TileWidth *
                                            GameTiles.TileWidth + GameTiles.TileWidth;
                                    }
                                    // Derecha
                                    else
                                    {
                                        other.Position.X = (int)(other.Right + Speed.X) / GameTiles.TileWidth *
                                            GameTiles.TileWidth - other.Width;
                                    }
                                }
                            }

                            if (other is Megaman)
                            {
                                hasMegamanAbove = true;
                            }
                        }
                    }
                    // Colisión en la parte superior del otro objeto
                    else if (!jumpOverPlatform)
                    {
                        other.Position.Y = this.Bottom;
                        collisionSides.Y = -1f;
                    }
                }
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
        }


        /// <summary>
        /// Evento para reiniciar la plataforma en cuanto megaman aparece de nuevo en el nivel
        /// </summary>
        void megaman_OnSpawnStart()
        {
            if (!invokedReset)
            {
                ResetObject();
            }
        }

        #endregion

        #region Implementación de IScreenObject

        /// <summary>
        /// Posición inicial de la plataforma
        /// </summary>
        public Vector2 InitialPosition { get; set; }

        /// <summary>
        /// Indica si la plataforma ha salido de la pantalla
        /// </summary>
        public bool ExitedScreen { get; set; }

        /// <summary>
        /// Indica si la plataforma debe de actualizarse si está en el mismo cuarto que megaman
        /// </summary>
        public virtual bool UpdateIfOnRoom
        {
            get { return true; }
        }

        /// <summary>
        /// Indica si la plataforma se encuentra visible dentro de la cámara
        /// </summary>
        public virtual bool IsOnScreen
        {
            get
            {
                return Global.ScreenContainsRectangle(BoundingBox);
            }
        }

        /// <summary>
        /// Indica si la plataforma en su posición inicial está dentro de la cámara
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
        /// Indica si la plataforma está dentro del mismo cuarto que megaman
        /// </summary>
        public virtual bool IsOnRoom
        {
            get
            {
                return Global.RoomContainsRectangle(BoundingBox);
            }
        }

        /// <summary>
        /// Indica si la plataforma puede actualizarse
        /// </summary>
        public virtual bool CanUpdate
        {
            get 
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

        /// <summary>
        /// Indica si la plataforma puede dibujarse
        /// </summary>
        public virtual bool CanDraw
        {
            get 
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

        /// <summary>
        /// Método que actualiza la plataforma
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            if (CanUpdate)
            {
                OnUpdate(gameTime);

                Position += Speed;

                invokedReset = false;

                if (!IsOnScreen)
                {
                    ExitedScreen = true;
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
        }

        /// <summary>
        /// Método que dibuja la plataforma
        /// </summary>
        /// <param name="spritebatch"></param>
        public override void Draw(SpriteBatch spritebatch)
        {
            if (CanDraw)
            {
                OnDraw(spritebatch);

                base.Draw(spritebatch);
            }
        }

        /// <summary>
        /// Método que resetea la plataforma
        /// </summary>
        public virtual void ResetObject()
        {
            Position = InitialPosition;

            ExitedScreen = false;

            invokedReset = true;
        }

        #endregion
    }
}
