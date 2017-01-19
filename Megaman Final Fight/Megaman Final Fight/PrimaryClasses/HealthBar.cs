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
    public abstract class HealthBar : GameObject
    {
        private IHealthObject linkedObject;
        private Texture2D containerTexture;
        private Rectangle barSourceRectangle;
        private Vector2 barPosition;

        private Vector2 redBarPosition;
        private Vector2 redBarSize;

        private const float RedBarResizeSpeed = 0.70f;

        #region Propiedades

        protected int BarOriginalWidth
        {
            get { return Resources.T2D_HealthBar.Width; }
        }

        protected int BarOriginalHeight
        {
            get { return Resources.T2D_HealthBar.Height; }
        }

        protected Vector2 BarScale
        {
            get 
            { 
                return  new Vector2(1f, (float)linkedObject.MaxHealth / 100f); 
            }
        }

        protected virtual float ContainerBarStartYPosition
        {
            get { return Position.Y + Height - 59f; }
        }

        #endregion

        public HealthBar(IHealthObject linkedObject, Texture2D containerTexture) 
            : base(containerTexture.Width, containerTexture.Height, Color.White)
        {
            this.linkedObject = linkedObject;
            this.containerTexture = containerTexture;

            barSourceRectangle = new Rectangle(0, 0, BarOriginalWidth, BarOriginalHeight);

            redBarPosition = Vector2.Zero;
            redBarSize = new Vector2(BarOriginalWidth, BarOriginalHeight);
        }

        public void Update(GameTime gameTime)
        {
            SetBarSizeAndPosition();
            SetRedBarSizeAndPosition();
        }

        private void SetBarSizeAndPosition()
        {
            barSourceRectangle.Height = (linkedObject.Health * BarOriginalHeight) / linkedObject.MaxHealth;
            barSourceRectangle.Y = BarOriginalHeight - barSourceRectangle.Height;

            barPosition.X = Position.X + 21;
            barPosition.Y = ContainerBarStartYPosition - (barSourceRectangle.Height * BarScale.Y); 
        }

        private void SetRedBarSizeAndPosition()
        {
            if (redBarSize.Y > barSourceRectangle.Height)
            {
                redBarSize.Y -= RedBarResizeSpeed;
            }
            else
            {
                redBarSize.Y = barSourceRectangle.Height;
            }

            redBarPosition.X = Position.X + 21;
            redBarPosition.Y = ContainerBarStartYPosition - (redBarSize.Y * BarScale.Y); 
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            spritebatch.Begin();

            spritebatch.Draw(containerTexture, Position, Color.White);

            spritebatch.Draw(Resources.T2D_Pixel, redBarPosition, null,
                Color.Red, 0f, Vector2.Zero, redBarSize, SpriteEffects.None, 0f);

            spritebatch.Draw(Resources.T2D_HealthBar, barPosition, barSourceRectangle, 
                Color.White, 0f, Vector2.Zero, BarScale, SpriteEffects.None, 0f);

            spritebatch.End();
        }

        protected abstract void OnUpdate(GameTime gameTime);

        protected abstract void OnDraw(SpriteBatch spritebatch);
    }
}
