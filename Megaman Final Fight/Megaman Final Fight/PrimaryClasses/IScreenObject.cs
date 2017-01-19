#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses
{
    public interface IScreenObject
    {
        Vector2 InitialPosition { get; set; }
        bool ExitedScreen { get; set; }

        bool UpdateIfOnRoom { get; }
        bool IsOnScreen { get; }
        bool IsOnScreenOnInitialPosition { get; }
        bool IsOnRoom { get; }

        bool CanUpdate { get; }
        bool CanDraw { get; }

        void Update(GameTime gametime);
        void Draw(SpriteBatch spritebatch);
        void ResetObject();
    }
}
