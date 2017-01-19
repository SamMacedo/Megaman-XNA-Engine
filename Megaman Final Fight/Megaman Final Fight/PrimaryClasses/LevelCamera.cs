#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using Megaman_Final_Fight.GlobalClasses;
using Megaman_Final_Fight.HelperClasses;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses
{
    public class LevelCamera
    {
        #region Campos

        // Referencia a la instancia de megaman
        private Megaman megaman;

        // Posición de la cámara
        public Vector2 Position;

        // Ancho de la cámara
        private int width;

        // Alto de la cámara
        private int height;

        // Velocidad de movimiento de la cámara
        private Vector2 speed;

        public enum TransitionDirections
        {
            Up,
            Down,
            Left,
            Right
        };

        private TransitionDirections transitionDirection;

        public bool IsOnTransition;

        private Vector2 finalTransitionPosition;

        private Vector2 cameraTransitionSpeed = new Vector2(10f, 10f);
        private Vector2 megamanTransitionSpeed = new Vector2(1.75f, 1.75f);

        #endregion

        #region Delegados y eventos

        public delegate void CameraTransitionEventHander(TransitionDirections transitionDirection);
        public event CameraTransitionEventHander OnTransitionStart;
        public event CameraTransitionEventHander OnTransitionFinish;

        #endregion

        #region Propiedades

        /// <summary>
        /// Regresa la posición central de la cámara
        /// </summary>
        private Vector2 Center
        {
            get { return new Vector2(Position.X + (width / 2), Position.Y + (height / 2)); }
        }

        /// <summary>
        /// Regresa la posición del lado izquerdo de la cámara
        /// </summary>
        public virtual float Left
        {
            get { return Position.X; }
        }

        /// <summary>
        /// Regresa la posición del lado derecho de la cámara
        /// </summary>
        public virtual float Right
        {
            get { return Position.X + width; }
        }

        /// <summary>
        /// Regresa la posición de la parte superior de la cámara
        /// </summary>
        public virtual float Top
        {
            get { return Position.Y; }
        }

        /// <summary>
        /// Regresa la posición de la parte inferior de la cámara
        /// </summary>
        public virtual float Bottom
        {
            get { return Position.Y + height; }
        }

        #endregion

        #region Constructor

        public LevelCamera(Megaman megaman)
        {
            this.megaman = megaman;
            this.Position = Vector2.Zero;

            this.width = Global.Viewport.Width;
            this.height = Global.Viewport.Height;
        }

        #endregion

        #region Actualización

        public void Update()
        {
            LimitMegamanPositionToCamera();

            CheckRoomLimits();

            if (IsOnTransition)
            {
                HandleTransition();
            }
            else
            {
                FollowMegaman();
                CheckForTransitionTiles();
            }

            Global.Viewport.X = (int)Position.X;
            Global.Viewport.Y = (int)Position.Y;
        }

        #endregion

        #region Seguimiento y limitado de megaman

        /// <summary>
        /// Método que se encarga de asegurar de que la posición de megaman no salga
        /// de los límites de la cámara
        /// </summary>
        private void LimitMegamanPositionToCamera()
        {
            // Izquierda
            if (megaman.Left < this.Left)
            {
                megaman.Position.X = this.Left;
            }
            // Derecha
            if (megaman.Right > this.Right)
            {
                megaman.Position.X = this.Right - megaman.Width;
            }
            // Arriba
            if (megaman.Top < this.Top)
            {
                megaman.Position.Y = this.Top;

                megaman.CancelJump();
            }
            // Abajo
            if (megaman.Bottom > this.Bottom)
            {
                megaman.Position.Y = this.Bottom - megaman.Height;
            }
        }

        /// <summary>
        /// Método que se encarga de seguir la posición de megaman con la cámara
        /// </summary>
        private void FollowMegaman()
        {
            // Reinicio de la velocidad 
            speed = Vector2.Zero;

            // Movimiento hacia la derecha
            if (megaman.DistanceTraveled.X > 0f)
            {
                if (megaman.Center.X >= this.Center.X)
                {
                    speed.X += megaman.DistanceTraveled.X;
                }
            }
            // Movimiento hacia la izquierda
            else if (megaman.DistanceTraveled.X < 0f)
            {
                if (megaman.Center.X <= this.Center.X)
                {
                    speed.X += megaman.DistanceTraveled.X;
                }
            }

            // Movimiento hacia arriba
            if (megaman.DistanceTraveled.Y < 0f)
            {
                if (megaman.Center.Y <= this.Center.Y)
                {
                    speed.Y += megaman.DistanceTraveled.Y;
                }
            }
            // Movimiento hacia abajo
            else if (megaman.DistanceTraveled.Y > 0f)
            {
                if (megaman.Center.Y >= this.Center.Y)
                {
                    speed.Y += megaman.DistanceTraveled.Y;
                }
            }

            Position += speed;

            LimitCameraToRoomLimits();
        }

        #endregion

        #region Transición de cámara

        /// <summary>
        /// Método que se encarga de revisar si megaman toca un tile de transición de cámara
        /// y en caso de ser así se inicia el proceso de transición de cámara
        /// </summary>
        private void CheckForTransitionTiles()
        {
            int x;
            int y;

            // Revisión a la derecha de megaman
            if (megaman.DistanceTraveled.X > 0f)
            {
                x = (int)megaman.Right - 1;
                y = (int)megaman.Center.Y;

                if (GameTiles.CheckTile(Global.CameraLayer, x, y) == GameTiles.RightCameraTransitionTile)
                {
                    StartTransition(TransitionDirections.Right);
                }
            }
            // Revisión a la izquierda de megaman
            else if (megaman.DistanceTraveled.X < 0f)
            {
                x = (int)megaman.Left;
                y = (int)megaman.Center.Y;

                if (GameTiles.CheckTile(Global.CameraLayer, x, y) == GameTiles.LeftCameraTransitionTile)
                {
                    StartTransition(TransitionDirections.Left);
                }
            }

            // Revisión abajo de megaman
            if (megaman.DistanceTraveled.Y > 0f)
            {
                x = (int)megaman.Center.X;
                y = (int)megaman.Center.Y;

                if (GameTiles.CheckTile(Global.CameraLayer, x, y) == GameTiles.DownCameraTransitionTile)
                {
                    StartTransition(TransitionDirections.Down);
                }
            }
            // Revisión arriba de megaman
            else if (megaman.DistanceTraveled.Y < 0f)
            {
                x = (int)megaman.Center.X;
                y = (int)megaman.Center.Y;

                if (GameTiles.CheckTile(Global.CameraLayer, x, y) == GameTiles.UpCameraTransitionTile)
                {
                    StartTransition(TransitionDirections.Up);
                }
            }
        }

        /// <summary>
        /// Método que se encarga de iniciar una nueva transición de cámara
        /// </summary>
        /// <param name="direction">Dirección de la transición de cámara</param>
        private void StartTransition(TransitionDirections direction)
        {
            // Aquí se determina la posición final que tendrá la cámara al finalizar
            // la transición
            switch (direction)
            {
                case TransitionDirections.Right:
                    finalTransitionPosition = new Vector2(Right, Position.Y);
                    break;
                case TransitionDirections.Left:
                    finalTransitionPosition = new Vector2(Left - width, Position.Y);
                    break;
                case TransitionDirections.Down:
                    finalTransitionPosition = new Vector2(Position.X, Bottom);
                    break;
                case TransitionDirections.Up:
                    finalTransitionPosition = new Vector2(Position.X, Top - height);
                    break;
            }

            // Invocación del evento
            if (OnTransitionStart != null)
            {
                OnTransitionStart.Invoke(direction);
            }

            transitionDirection = direction;
            IsOnTransition = true;
        }

        /// <summary>
        /// Método que se encarga de manejar tanto el movimiento de la cámara como del 
        /// movimiento de megaman durante la transición
        /// </summary>
        private void HandleTransition()
        {
            // Transición hacia la derecha
            if (transitionDirection == TransitionDirections.Right)
            {
                // Si la posición no alcanza la posición final entonces se continúa 
                // el proceso de movimiento
                if (Position.X <= finalTransitionPosition.X)
                {
                    megaman.Position.X += megamanTransitionSpeed.X;
                    Position.X += cameraTransitionSpeed.X;
                }
                else
                {
                    FinishTransition();
                }
            }
            // Transición hacia la izquierda
            else if (transitionDirection == TransitionDirections.Left)
            {
                // Si la posición no alcanza la posición final entonces se continúa 
                // el proceso de movimiento
                if (Position.X >= finalTransitionPosition.X)
                {
                    megaman.Position.X -= megamanTransitionSpeed.X;
                    Position.X -= cameraTransitionSpeed.X;
                }
                else
                {
                    FinishTransition();
                }
            }
            // Transición hacia abajo
            else if (transitionDirection == TransitionDirections.Down)
            {
                // Si la posición no alcanza la posición final entonces se continúa 
                // el proceso de movimiento
                if (Position.Y <= finalTransitionPosition.Y)
                {
                    megaman.Position.Y += megamanTransitionSpeed.Y;
                    Position.Y += cameraTransitionSpeed.Y;
                }
                else
                {
                    FinishTransition();
                }
            }
            // Transición hacia arriba
            else if (transitionDirection == TransitionDirections.Up)
            {
                // Si la posición no alcanza la posición final entonces se continúa 
                // el proceso de movimiento
                if (Position.Y >= finalTransitionPosition.Y)
                {
                    megaman.Position.Y -= megamanTransitionSpeed.Y;
                    Position.Y -= cameraTransitionSpeed.Y;
                }
                else
                {
                    FinishTransition();
                }
            }
        }

        /// <summary>
        /// Método que se encarga de finalizar la transición de cámara
        /// </summary>
        private void FinishTransition()
        {
            IsOnTransition = false;
            Position = finalTransitionPosition;

            CheckRoomLimits();

            // Invocación del evento
            if (OnTransitionFinish != null)
            {
                OnTransitionFinish.Invoke(transitionDirection);
            }
        }

        #endregion

        #region Obtención de límites del cuarto

        /// <summary>
        /// Método que se encarga de revisar los límites del cuarto al que entra megaman
        /// al finalizar una transición de cámara
        /// </summary>
        private void CheckRoomLimits()
        {
            // Variables auxiliares revisión de tiles
            int posX;
            int posY;
            bool found;

            // Variables auxiliares para la creación del rectángulo que indica los 
            // límites del cuarto
            int roomX = 0;
            int roomY = 0;
            int roomWidth = 0;
            int roomHeight = 0;


            // Obtención de la posición en X del cuarto
            posX = (int)Center.X;
            posY = (int)Center.Y;
            found = false;

            // Aquí se revisa tile por tile hasta encontrar un límite, en el momento
            // de encontrarlo entonces se asigna la variable del límite correspondiente
            // y se finaliza el ciclo
            do
            {
                if (CheckIfTileIsRoomLimit(posX, posY))
                {
                    roomX = posX / GameTiles.TileWidth * GameTiles.TileWidth;
                    found = true;
                }
                else
                {
                    posX -= GameTiles.TileWidth;
                }
            }
            while (!found);


            // Obtención de la posición en Y del cuarto
            posX = (int)Center.X;
            posY = (int)Center.Y;
            found = false;

            // Aquí se revisa tile por tile hasta encontrar un límite, en el momento
            // de encontrarlo entonces se asigna la variable del límite correspondiente
            // y se finaliza el ciclo
            do
            {
                if (CheckIfTileIsRoomLimit(posX, posY))
                {
                    roomY = posY / GameTiles.TileHeight * GameTiles.TileHeight;
                    found = true;
                }
                else
                {
                    posY -= GameTiles.TileHeight;
                }
            }
            while (!found);


            // Obtención del ancho del cuarto
            posX = (int)Center.X;
            posY = (int)Center.Y;
            found = false;

            // Aquí se revisa tile por tile hasta encontrar un límite, en el momento
            // de encontrarlo entonces se asigna la variable del límite correspondiente
            // y se finaliza el ciclo
            do
            {
                if (CheckIfTileIsRoomLimit(posX, posY))
                {
                    // Se resta la posición en X para encontrar el ancho del cuarto
                    roomWidth = (posX / GameTiles.TileWidth * GameTiles.TileWidth + GameTiles.TileWidth) - roomX;
                    found = true;
                }
                else
                {
                    posX += GameTiles.TileWidth;
                }
            }
            while (!found);


            // Obtención del alto del cuarto
            posX = (int)Center.X;
            posY = (int)Center.Y;
            found = false;

            // Aquí se revisa tile por tile hasta encontrar un límite, en el momento
            // de encontrarlo entonces se asigna la variable del límite correspondiente
            // y se finaliza el ciclo
            do
            {
                if (CheckIfTileIsRoomLimit(posX, posY))
                {
                    // Se resta la posición en Y para encontrar el alto del cuarto
                    roomHeight = (posY / GameTiles.TileHeight * GameTiles.TileHeight + GameTiles.TileHeight) - roomY;
                    found = true;
                }
                else
                {
                    posY += GameTiles.TileHeight;
                }
            }
            while (!found);

            // Finalmente se crea el rectángulo con los límites encontrados
            Global.RoomRectangle = new Rectangle(roomX, roomY, roomWidth, roomHeight);
        }

        /// <summary>
        /// Método que revisa si el tile en la posición indicada es un límite de cámara
        /// </summary>
        /// <param name="x">Posición en X</param>
        /// <param name="y">Posición en Y</param>
        /// <returns>Regresa true en caso de ser un tile límite de cámara</returns>
        private bool CheckIfTileIsRoomLimit(int x, int y)
        {
            int type = GameTiles.CheckTile(Global.CameraLayer, x, y);

            return type == GameTiles.CameraLimit
                || type == GameTiles.UpCameraTransitionTile
                || type == GameTiles.DownCameraTransitionTile
                || type == GameTiles.LeftCameraTransitionTile
                || type == GameTiles.RightCameraTransitionTile;
        }

        #endregion

        #region Métodos auxiliares

        /// <summary>
        /// Método que reinicia la posición de la cámara en el último checkpoint
        /// </summary>
        public void ResetToCheckpoint()
        {
            // Se posiciona la cámara de manera centrada en el checkpoint
            Position.X = CheckpointManager.LastCheckpoint.X - (width / 2);
            Position.Y = CheckpointManager.LastCheckpoint.Y - (height / 2);

            CheckRoomLimits();
            LimitCameraToRoomLimits();
        }

        /// <summary>
        /// Método que simplemente se encarga de evitar que la cámara rebase los límites
        /// del cuarto
        /// </summary>
        private void LimitCameraToRoomLimits()
        {
            Position.X = MathHelper.Clamp(Position.X, Global.RoomRectangle.X,
                Global.RoomRectangle.Right - width);

            Position.Y = MathHelper.Clamp(Position.Y, Global.RoomRectangle.Y,
                Global.RoomRectangle.Bottom - height);
        }

        #endregion
    }
}
