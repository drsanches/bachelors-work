using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Math_Recognition
{
    class Symbol
    {
        public int RectangleIndex;
        public List<Symbol>[] Inside;

        public Symbol(int rectangleIndex)
        {
            RectangleIndex = rectangleIndex;
            Inside = new List<Symbol>[5];
        }

    }
}
