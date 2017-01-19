#region Directivas de uso

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace Megaman_Final_Fight.PrimaryClasses
{
    public interface IHealthObject
    {
        int MaxHealth { get; }
        int Health { get; set; }
    }
}
