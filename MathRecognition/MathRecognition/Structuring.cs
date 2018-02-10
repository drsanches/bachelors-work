using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRecognition
{
    public abstract class StructuringAbstractFactory
    {
        public StructuringAbstractFactory()
        { }
        public abstract void Run(List<Rectangle> rectangles);
    }
    
    public class Structuring : StructuringAbstractFactory
    {
        private StructuringDelegate structuringDelegate;
        private const string SYMBOLS_FILENAME = "..\\..\\..\\..\\dataset\\Symbols.json";
        private const double CENTRE_DISPLACEMENT_COEFFICIENT = 0.1;
        private List<List<Symbol>> baselines;

        public Structuring(StructuringDelegate structDel) : base()
        {
            structuringDelegate = structDel;
        }
        public override void Run(List<Rectangle> rectangles)
        {
            baselines = new List<List<Symbol>>();

            foreach (Rectangle rect in rectangles)
                addInBaselines(rect);

            structuringDelegate.Invoke(ref baselines);
        }
        private void addInBaselines(Rectangle rectangle)
        {
            Symbol newSymbol = new Symbol(rectangle, SYMBOLS_FILENAME);

            bool isAdded = false;
            for (int line = 0; line < baselines.Count; line++)
            {
                foreach (Symbol element in baselines[line])
                    if (isAtOneLine(element, newSymbol))
                    {
                        baselines[line].Add(newSymbol);
                        isAdded = true;
                        break;
                    }

                if (isAdded)
                    break;
            }

            if (!isAdded)
            {
                baselines.Add(new List<Symbol>());
                baselines.Last().Add(newSymbol);
            }
        }
        private bool isAtOneLine(Symbol s1, Symbol s2)
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
