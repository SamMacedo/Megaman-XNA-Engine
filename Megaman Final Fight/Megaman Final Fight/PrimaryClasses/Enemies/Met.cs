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
    public sealed class Met : Enemy
    {
        private AnimationSet animations;
        private Timer coveredTimer;
        private Timer uncoveredTimer;
        private Timer beforeShootTimer;
        private Timer bounceAnimationTimer;
        private Shot[] shots;

        private const int DefaultWidth = 54;
        private const int CoveredHeight = 44;
        private const int UncoveredHeight = 57;
        private const int VisualReach = 300;

        public override int MaxHealth
        {
            get { return 20; }
        }

        protected override int BodyDamage
        {
            get { return 10; }
        }

        protected override bool HasActiveShots
        {
            get 
            {
                foreach (Shot shot in shots)
                {
                    if (shot.Active)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public Met(Megaman megaman, Vector2 initialPosition)
            : base(megaman, DefaultWidth, CoveredHeight, Color.Yellow, initialPosition, Directions.Left)
        {
            coveredTimer = new Timer(1500);
            uncoveredTimer = new Timer(1500);
            beforeShootTimer = new Timer(500);
            bounceAnimationTimer = new Timer(200);

            this.CollidedWithMegamanWeapon += new MegamanWeaponCollisionHandler(Met_CollidedWithMegamanWeapon);
            this.beforeShootTimer.TimerReachedZero += new Timer.TimerStopedHandler(beforeShootTimer_TimerReachedZero);
            this.uncoveredTimer.TimerReachedZero += new Timer.TimerStopedHandler(UncoveredTimer_TimerReachedZero);

            LoadContent();
            InitializeShots();
            ResetObject();
        }

        void beforeShootTimer_TimerReachedZero()
        {
            Shoot();
        }

        void UncoveredTimer_TimerReachedZero()
        {
            Cover();
        }

        private void InitializeShots()
        {
            shots = new Shot[3];

            for (int i = 0; i < 3; i++)
            {
                shots[i] = new Shot();
            }
        }

        void Met_CollidedWithMegamanWeapon()
        {
            if (!uncoveredTimer.IsActive)
            {
                coveredTimer.Restart();
                bounceAnimationTimer.Restart();
            }
        }

        private void LoadContent()
        {
            animations = Resources.ANIM_ENEM_Met.CreateCopy();
        }

        public override void OnUpdate(GameTime gameTime)
        {
            coveredTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            uncoveredTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            beforeShootTimer.Update(gameTime.ElapsedGameTime.Milliseconds);
            bounceAnimationTimer.Update(gameTime.ElapsedGameTime.Milliseconds);

            HandleUncovering();

            Animate(gameTime);
        }

        private void HandleUncovering()
        {
            if (!coveredTimer.IsActive && !uncoveredTimer.IsActive)
            {
                if (DistanceToMegaman <= VisualReach)
                {
                    Uncover();
                }
            }
        }

        private void Cover()
        {
            coveredTimer.Start();
            IsInvinsible = true;

            Position.Y = Bottom - CoveredHeight;
            Height = CoveredHeight;
        }

        private void Uncover()
        {
            uncoveredTimer.Start();
            beforeShootTimer.Start();
            IsInvinsible = false;

            Position.Y = Bottom - UncoveredHeight;
            Height = UncoveredHeight;
        }

        public override void UpdateShots(GameTime gameTime)
        {
            foreach (Shot shot in shots)
            {
                shot.Update();
            }
        }

        private void Shoot()
        {
            Vector2 position;

            if (CurrentFacingDirection == Directions.Left)
            {
                position = new Vector2(Center.X - 15, Center.Y);

                shots[0].Activate(position, 135);
                shots[1].Activate(position, 180);
                shots[2].Activate(position, 225);
            }
            else
            {
                position = new Vector2(Center.X + 15, Center.Y);

                shots[0].Activate(position, 45);
                shots[1].Activate(position, 0);
                shots[2].Activate(position, 315);
            }

            Resources.SFX_Enemy_Shot1.Play();
        }

        protected override void CheckShotsCollision()
        {
            foreach (Shot shot in shots)
            {
                if (shot.Intersects(megaman.BoundingBox) && megaman.CanInflictDamage)
                {
                    megaman.InflictDamage(shot.Damage);
                    shot.Deactivate();
                }
            }
        }

        private void Animate(GameTime gameTime)
        {
            if (bounceAnimationTimer.IsActive)
            {
                if (animations.CurrentAnimation == "Bounce" && animations.CurrentAnimationFinished)
                {
                    animations.RestartAnimation();
                }
                else
                {
                    animations.StartAnimation("Bounce");
                }
            }
            else
            {
                if (!uncoveredTimer.IsActive)
                {
                    animations.StartAnimation("Covered");
                }
                else
                {
                    animations.StartAnimation("Uncovered");
                }
            }

            animations.Update(gameTime);
        }

        public override void OnDraw(SpriteBatch spritebatch)
        {
            animations.Flipped = Flipped;

            animations.Draw(spritebatch, CenteredDrawPosition, Color.White * Opacity);
        }

        public override void DrawShots(SpriteBatch spritebatch)
        {
            foreach (Shot shot in shots)
            {
                shot.Draw(spritebatch);
            }
        }

        public override void ResetObject()
        {
            coveredTimer.Stop();
            uncoveredTimer.Stop();
            beforeShootTimer.Stop();
            bounceAnimationTimer.Stop();

            animations.StartAnimation("Covered");
            IsInvinsible = true;
            Height = CoveredHeight;

            base.ResetObject();
        }

        public override void ResetShots()
        {
            foreach (Shot shot in shots)
            {
                shot.Deactivate();
            }
        }
    }

    class Shot : Projectile
    {
        public Shot()
            : base(7f, 5, 20, 20, Color.Orange)
        {
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            base.Draw(spritebatch, Resources.T2D_Met_Shot);
        }
    }
}
