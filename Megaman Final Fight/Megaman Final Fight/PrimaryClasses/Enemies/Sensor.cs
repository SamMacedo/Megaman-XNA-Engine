#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Megaman_Final_Fight.GlobalClasses;
using Megaman_Final_Fight.HelperClasses;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using AnimationSystem;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses.Enemies
{
    class Sensor : Enemy
    {
        private const int DefaultWidth = 50;
        private const int DefaultHeight = 20;
        private const float NormalSpeed = 2f;
        private const float FastSpeed = 7f;

        private Directions startingDirection;
        private AnimationSet animations;
        private Timer freezedTimer;

        public override int MaxHealth
        {
            get { return 0; }
        }

        protected override int BodyDamage
        {
            get { return 15; }
        }

        protected override bool HasActiveShots
        {
            get { return false; }
        }

        public override bool UpdateIfOnRoom
        {
            get { return true; }
        }

        private bool MegamanIsOnSameGroundLevel
        {
            get
            {
                return megaman.IsOnGround && (int)megaman.Bottom == (int)this.Bottom;
            }
        }

        #region Constructor

        public Sensor(Megaman megaman, Vector2 initialPosition, Directions startingDirection)
            : base(megaman, DefaultWidth, DefaultHeight, Color.Pink, initialPosition, startingDirection)
        {
            if (startingDirection == Directions.Left)
            {
                this.startingDirection = Directions.Left;
            }
            else
            {
                this.startingDirection = Directions.Right;
            }

            IsInvinsible = true;
            AlwaysFaceMegaman = false;

            freezedTimer = new Timer(2000);

            LoadContent();
            ResetObject();
        }

        #endregion

        private void LoadContent()
        {
            animations = Resources.ANIM_ENEM_Sensor.CreateCopy();
        }

        public override void OnUpdate(GameTime gameTime)
        {
            freezedTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            HandleMovement();
            Animate(gameTime);
        }

        private void HandleMovement()
        {
            float speed;

            if (!freezedTimer.IsActive)
            {
                if (CurrentFacingDirection == Directions.Left)
                {
                    if (HitSolidToLeft)
                    {
                        CurrentFacingDirection = Directions.Right;
                    }
                    else
                    {
                        if (!HasWallAtOffset(new Vector2(-Width, 1f)))
                        {
                            CurrentFacingDirection = Directions.Right;
                        }
                    }
                }
                else
                {
                    if (HitSolidToRight)
                    {
                        CurrentFacingDirection = Directions.Left;
                    }
                    else
                    {
                        if (!HasWallAtOffset(new Vector2(Width, 1f)))
                        {
                            CurrentFacingDirection = Directions.Left;
                        }
                    }
                }

                if (MegamanIsOnSameGroundLevel)
                {
                    speed = FastSpeed;
                }
                else
                {
                    speed = NormalSpeed;
                }

                if (CurrentFacingDirection == Directions.Left)
                {
                    Speed.X = -speed;
                }
                else
                {
                    Speed.X = speed;
                }
            }
        }

        protected override void CheckCollisionWithMegamanWeapon()
        {
            if (megaman.CurrentWeapon.Intersects(BoundingBox, MegamanWeapon.CollisionActions.Deactivate))
            {
                damageTimer.Restart();

                freezedTimer.Restart();

                Resources.SFX_Enemy_Hit.Play();
            }
        }

        private void Animate(GameTime gameTime)
        {
            if (freezedTimer.IsActive)
            {
                animations.StartAnimation("Idle");
            }
            else
            {
                animations.StartAnimation("Alert");
            }

            animations.Update(gameTime);
        }

        public override void OnDraw(SpriteBatch spritebatch)
        {
            animations.Draw(spritebatch, CenteredDrawPosition, Color.White * Opacity);
        }

        public override void ResetObject()
        {
            animations.StartAnimation("Idle");

            freezedTimer.Stop();

            base.ResetObject();
        }

        #region Métodos sin implementación

        public override void UpdateShots(GameTime gameTime)
        {
        }

        protected override void CheckShotsCollision()
        {
        }

        public override void DrawShots(SpriteBatch spritebatch)
        {
        }

        public override void ResetShots()
        {
        }

        #endregion
    }
}
