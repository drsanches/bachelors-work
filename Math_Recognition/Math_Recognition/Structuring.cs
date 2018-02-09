using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Math_Recognition
{
    public class Structuring
    {
        const double SEARCH_TOP = 0.4; // Not for \frac and 0 positions (e.g. vectors)
        const double SEARCH_DOWN = 0.4; // 
        const double SEARCH_RIGHT = 1;
        const double SEARCH_LEFT = 1;

        const string SYMBOLS_FILENAME = "..\\..\\..\\..\\dataset\\Symbols.json";

        const double CENTRE_DISPLACEMENT_COEFFICIENT = 0.1;
        
        List<List<Symbol>> Baselines;

        public Structuring()
        {
        }
        public void Run(List<Rectangle> rectangles)
        {
            Baselines = new List<List<Symbol>>();

            foreach (Rectangle rect in rectangles)
                AddInBaselines(rect);
        }
        private void AddInBaselines(Rectangle rectangle)
        {
            Symbol newSymbol = new Symbol(rectangle, SYMBOLS_FILENAME);

            bool isAdded = false;
            for (int line = 0; line < Baselines.Count; line++)
            {
                foreach (Symbol element in Baselines[line])
                    if (IsAtOneLine(element, newSymbol))
                    {
                        Baselines[line].Add(newSymbol);
                        isAdded = true;
                        break;
                    }

                if (isAdded)
                    break;
            }

            if (!isAdded)
            {
                Baselines.Add(new List<Symbol>());
                Baselines.Last().Add(newSymbol);
            }
        }
        private bool IsAtOneLine(Symbol s1, Symbol s2)
        {
            int maxHeight = Math.Max(s1.Height, s2.Height);
            int y1 = s1.MainCentreY;
            int y2 = s2.MainCentreY;
            if ((y1 < y2 + maxHeight * CENTRE_DISPLACEMENT_COEFFICIENT) && (y1 > y2 - maxHeight * CENTRE_DISPLACEMENT_COEFFICIENT))
                return true;
            else
                return false;
        }
    }
}
