#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Megaman_Final_Fight.GlobalClasses;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses
{
    /// <summary>
    /// Representa la instancia más básica del juego con propiedades básicas como posición,
    /// tamaño y métodos para dibujar el objeto en pantalla
    /// </summary>
    public abstract class GameObject
    {
        #region Campos

        public Vector2 Position;
        public int Width;
        public int Height;
        public bool DrawRelativeToCamera;

        private Color boundingBoxColor;

        #endregion

        #region Propiedades

        /// <summary>
        /// Indica si se debe de dibujar la boundingBox del objeto
        /// </summary>
        protected virtual bool DrawBoundingBox
        {
            get { return Global.DrawBoundingBoxes; }
        }

        /// <summary>
        /// Obtiene un rectágulo representando la bounding box del objeto
        /// </summary>
        public virtual Rectangle BoundingBox
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            }
        }

        /// <summary>
        /// Obtiene la posición en donde va a ser dibujado el objeto
        /// </summary>
        protected virtual Vector2 DrawPosition
        {
            get
            {
                if (DrawRelativeToCamera)
                {
                    return new Vector2(Position.X - Global.Viewport.X,
                        Position.Y - Global.Viewport.Y);
                }
                else
                {
                    return new Vector2(Position.X, Position.Y);
                }
            }
        }

        /// <summary>
        /// Obtiene la posición (de manera centrada) en donde va a ser dibujado el objeto
        /// </summary>
        protected virtual Vector2 CenteredDrawPosition
        {
            get
            {
                if (DrawRelativeToCamera)
                {
                    return new Vector2(Center.X - Global.Viewport.X, Center.Y - Global.Viewport.Y);
                }
                else
                {
                    return Center;
                }
            }
        }

        /// <summary>
        /// Regresa un vector indicando la posición central del objeto
        /// </summary>
        public virtual Vector2 Center
        {
            get 
            {
                Vector2 center = Vector2.Zero;

                center.X = Position.X + (Width / 2);
                center.Y = Position.Y + (Height / 2);

                return center; 
            }
        }

        /// <summary>
        /// Regresa la posición del lado izquerdo del objeto
        /// </summary>
        public virtual float Left
        {
            get { return Position.X; }
        }

        /// <summary>
        /// Regresa la posición del lado derecho del objeto
        /// </summary>
        public virtual float Right
        {
            get { return Position.X + Width; }
        }

        /// <summary>
        /// Regresa la posición de la parte superior del objeto
        /// </summary>
        public virtual float Top
        {
            get { return Position.Y; }
        }

        /// <summary>
        /// Regresa la posición de la parte inferior del objeto
        /// </summary>
        public virtual float Bottom
        {
            get { return Position.Y + Height; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor que inicializa el objeto
        /// </summary>
        /// <param name="width">Ancho en pixeles del objeto</param>
        /// <param name="height">Alto en pixeles del objeto</param>
        /// <param name="boundingBoxColor">Color de la bounding box en caso de ser 
        /// dibujada</param>
        public GameObject(int width, int height, Color boundingBoxColor)
        {
            // El objeto inicia por default en la posición cero
            this.Position = Vector2.Zero;

            // Inicialización de campos
            this.Width = width;
            this.Height = height;
            this.boundingBoxColor = boundingBoxColor;
            this.DrawRelativeToCamera = false;
        }

        #endregion

        #region Métodos

        /// <summary>
        /// Método que dibuja el objeto
        /// </summary>
        /// <param name="spritebatch"></param>
        public virtual void Draw(SpriteBatch spritebatch)
        {
            // Sólo dibujar la bounding box en caso de que esté habilitada la opción
            if (DrawBoundingBox)
            {
                spritebatch.Begin();

                // Se dibuja la bounding box con cierta transparencia
                spritebatch.Draw(Resources.T2D_Pixel, DrawPosition, null, boundingBoxColor * 0.5f, 
                    0f, Vector2.Zero, new Vector2(Width, Height), SpriteEffects.None, 0f);

                spritebatch.End();
            }
        }

        #endregion
    }
}
