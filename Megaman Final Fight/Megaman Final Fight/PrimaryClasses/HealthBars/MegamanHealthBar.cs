#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Megaman_Final_Fight.GlobalClasses;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses.HealthBars
{
    public sealed class MegamanHealthBar : HealthBar
    {
        public MegamanHealthBar(Megaman megaman)
            : base(megaman, Resources.T2D_MegamanHealthBarContainer)
        {
            float x;
            float y;

            x = Global.GameSafeArea.Left + 30;
            y = Global.GameSafeArea.Center.Y - (Height / 2) - 150;

            Position.X = x;
            Position.Y = y;
        }

        protected override void OnUpdate(GameTime gameTime)
        {
        }

        protected override void OnDraw(SpriteBatch spritebatch)
        {
        }
    }
}
