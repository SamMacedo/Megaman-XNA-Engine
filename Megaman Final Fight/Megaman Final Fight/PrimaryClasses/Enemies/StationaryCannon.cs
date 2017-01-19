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

namespace Megaman_Final_Fight.PrimaryClasses.Enemies
{
    public sealed class StationaryCannon : Enemy
    {
        private const int DefaultWidth = 144;
        private const int DefaultHeight = 80;

        private Barrel barrel;

        public override int MaxHealth
        {
            get { return 30; }
        }

        protected override int BodyDamage
        {
            get { return 15; }
        }

        protected override bool HasActiveShots
        {
            get { return true; }
        }

        public StationaryCannon(Megaman megaman, Vector2 initialPosition )
            : base(megaman, DefaultWidth, DefaultHeight, Color.Orange, initialPosition, Directions.Right)
        {
            barrel = new Barrel(this, megaman);

            ResetObject();
        }

        public override void OnUpdate(GameTime gameTime)
        {
            barrel.Update(gameTime);
        }

        public override void OnDraw(SpriteBatch spritebatch)
        {
            //barrel.Draw(spritebatch);

            spritebatch.Begin();
            spritebatch.Draw(Resources.T2D_StationaryCannon_Base, DrawPosition, Color.White * Opacity);
            spritebatch.End();

            barrel.Draw(spritebatch);
        }

        public override void UpdateShots(GameTime gameTime)
        {
            barrel.UpdateBall();
        }

        public override void DrawShots(SpriteBatch spritebatch)
        {
            barrel.DrawBall(spritebatch);
        }

        public override void ResetShots()
        {
        }

        public override void ResetObject()
        {
            barrel.ResetObject();

            base.ResetObject();
        }

        protected override void CheckShotsCollision()
        {
        }
    }

    #region Clase Auxiliar "Barrel"

    class Barrel : GameObject
    {
        private const int DefaultWidth = 100;
        private const int DefaultHeight = 42;

        private float angle;
        private StationaryCannon cannon;
        private Megaman megaman;
        private Ball ball;

        private Timer beforeShootTimer;

        public Barrel(StationaryCannon cannon, Megaman megaman)
            : base(DefaultWidth, DefaultHeight, Color.OrangeRed)
        {
            DrawRelativeToCamera = true;

            this.cannon = cannon;
            this.megaman = megaman;

            this.ball = new Ball();

            this.beforeShootTimer = new Timer(1000);
            this.beforeShootTimer.TimerReachedZero += new Timer.TimerStopedHandler(beforeShootTimer_TimerReachedZero);
        }

        void beforeShootTimer_TimerReachedZero()
        {
            Shoot();
        }

        public void Update(GameTime gameTime)
        {
            beforeShootTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            HandleRotation();
        }

        private void HandleRotation()
        {
            float distanceX = megaman.Center.X - Center.X;

            if (distanceX < 0f)
            {
                angle = (float)Math.PI - (float)Math.Asin(((distanceX * ball.Gravity) / Math.Pow(ball.Speed, 2))) / 2f; 
            }
            else
            {
                angle = -(float)Math.Asin(((distanceX * ball.Gravity) / Math.Pow(ball.Speed, 2))) / 2f; 
            }

            Debug.WindowText = angle.ToString();
        }

        private void Shoot()
        {
            ball.Activate(cannon.Center, MathHelper.ToDegrees(angle));

            beforeShootTimer.Restart();
        }

        public void UpdateBall()
        {
            ball.Update();
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            if (false)
            {
                spritebatch.Begin();
                spritebatch.Draw(Resources.T2D_StationaryCannon_Barrel, DrawPosition, null, Color.White,
                    angle, new Vector2(10f, Height / 2), 1f, SpriteEffects.None, 0f);
                spritebatch.End();
            }
            else
            {
                spritebatch.Begin();
                spritebatch.Draw(Resources.T2D_StationaryCannon_Barrel, DrawPosition, null, Color.White,
                    angle, new Vector2(10f, Height / 2), 1f, SpriteEffects.None, 0f);
                spritebatch.End();
            }

            base.Draw(spritebatch);
        }

        public void DrawBall(SpriteBatch spritebatch)
        {
            ball.Draw(spritebatch);
        }

        public void ResetObject()
        {
            Position.X = cannon.Center.X;
            Position.Y = cannon.Center.Y + 15f;

            this.beforeShootTimer.Restart();
        }
    }

    #endregion

    #region Clase Auxiliar "Ball"

    class Ball : Projectile
    {
        public float Gravity = 0.25f;
        public float MaxFallSpeed = 48f;
        public float SpeedX;
        public float SpeedY;

        public override Vector2 ComponentSpeed
        {
            get
            {
                return new Vector2(SpeedX, SpeedY);
            }
        }

        public Ball()
            : base(16f, 20, 32, 32, Color.Red)
        {
            HandleCollisions = true;

            SpeedX = 0f;
            SpeedY = 0f;

            this.CollidedWithLevel += new CollisionEventHandler(Ball_CollidedWithLevel);
        }

        public override void Update()
        {
            SpeedY = MathHelper.Clamp(SpeedY + Gravity, -MaxFallSpeed, MaxFallSpeed);

            base.Update();
        }

        public override void Activate(Vector2 position, float angle)
        {
            SpeedX = Speed * (float)Math.Cos(MathHelper.ToRadians(angle));
            SpeedY = Speed * (float)Math.Sin(MathHelper.ToRadians(angle));

            base.Activate(position, angle);
        }

        void Ball_CollidedWithLevel(Vector2 collisionSides)
        {
            Deactivate();
        }
    }

    #endregion
}
