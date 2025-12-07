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
            float Magnitude = (float)Math.Sqrt(Math.Pow(vector.X, 2)+Math.Pow(vector.Y, 2));
            if (Magnitude!=0)
            {
                return new Vector2(vector.X / Magnitude, vector.Y / Magnitude + 0);
            }
            else
            {
                return new Vector2(vector.X, vector.Y);
            }
            
        }
    }
}
