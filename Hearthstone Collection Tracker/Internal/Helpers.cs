using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hearthstone_Collection_Tracker.Internal
{
    public static class Helpers
    {
        public static int Clamp(this int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}
