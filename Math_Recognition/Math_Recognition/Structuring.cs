using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Math_Recognition
{
    public class Structuring
    {
        const double CENTRE_DISPLACEMENT_COEFFICIENT = 0.2;

        List<List<Symbol>> Baselines;
        List<Rectangle> Rectangles;

        public Structuring()
        {
            Baselines = new List<List<Symbol>>();
            Rectangles = new List<Rectangle>();
        }
        public void Run(List<Rectangle> rectangles)
        {
            Baselines = new List<List<Symbol>>();
            Rectangles = rectangles;

            foreach (Rectangle rect in Rectangles)
                AddInBaselines(rect);
        }
        private void AddInBaselines(Rectangle rectangle)
        {
            int rectIndex = Rectangles.FindIndex(x => x.GetCentrePoint() == rectangle.GetCentrePoint());
            bool isAdded = false;
            for (int line = 0; line < Baselines.Count; line++)
            {
                foreach (Symbol s in Baselines[line])
                    if (IsAtOneLine(Rectangles[s.RectangleIndex], rectangle)) 
                    { 
                        Baselines[line].Add(new Symbol(rectIndex));
                        isAdded = true;
                        break;
                    }

                if (isAdded)
                    break;
            }

            if (!isAdded)
            {
                Baselines.Add(new List<Symbol>());
                Baselines.Last().Add(new Symbol(rectIndex));
            }
        }
        private bool IsAtOneLine(Rectangle rect1, Rectangle rect2)
        {
            int maxHeight = Math.Max(rect1.Height, rect2.Height);
            int y1 = rect1.GetCentrePoint().Y;
            int y2 = rect2.GetCentrePoint().Y;
            if ((y1 < y2 + maxHeight * CENTRE_DISPLACEMENT_COEFFICIENT) && (y1 > y2 - maxHeight * CENTRE_DISPLACEMENT_COEFFICIENT))
                return true;
            else
                return false;
        }
    }
}
