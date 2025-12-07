using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monogame_test1.Classes.OwnMathForVectors
{
    
    public static class OwnMath
    {
        public static Vector2 Normalize(Vector2 vector)
        {
            if (vector != Vector2.Zero)
            {
                vector.Normalize();
            }
            return vector;
        }
    }
}
