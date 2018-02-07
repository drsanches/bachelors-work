using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Math_Recognition
{
    class Symbol
    {
        int TopLeftX;
        int TopLeftY;
        int Width;
        int Height;
        public int RectangleIndex;
        public List<Symbol>[] Inside;

        public Symbol(int rectangleIndex, Rectangle rect)
        {
            TopLeftX = rect.TopLeftX;
            TopLeftY = rect.TopLeftY;
            Width = rect.Width;
            Height = rect.Height;
            RectangleIndex = rectangleIndex;
            Inside = new List<Symbol>[5];
        }

    }
}
