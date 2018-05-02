using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MathRecognition
{
    public class Symbol
    {
        public int TopLeftX { get { return MainRectangle.TopLeftX; } }
        public int TopLeftY { get { return MainRectangle.TopLeftY; } }
        public int Width { get { return MainRectangle.Width; } }
        public int Height { get { return MainRectangle.Height; } }
        public int MainCentreX;
        public int MainCentreY;
        public Rectangle MainRectangle;
        public List<List<Symbol>>[] Baselines;

        public Symbol(Rectangle rectangle, string symbolsFilename)
        {
            MainRectangle = rectangle;
            Baselines = new List<List<Symbol>>[5];
            MainCentreX = rectangle.GetCentreX();
            MainCentreY = rectangle.GetCentreY() + (int)(Height * getCenterYShift(rectangle.Label, symbolsFilename));
        }
        public Symbol plus(Symbol b, string symbolsFilename)
        {
            Rectangle newRectangle = new Rectangle();
            newRectangle = this.MainRectangle + b.MainRectangle;
            Symbol newSymbol = new Symbol(newRectangle, symbolsFilename);
            return newSymbol;
        }
        private double getCenterYShift(string label, string symbolsFilename)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(@symbolsFilename);
            string jsonString = file.ReadToEnd();
            file.Close();
            
            JObject fileJObject = JObject.Parse(jsonString);
            JToken element = fileJObject.GetValue("CenterHeightShift").First;
            while (element != null)
            {
                JObject elementJObject = JObject.Parse(element.ToString());
                string[] symbols = elementJObject.GetValue("Symbols").ToString().Split(' ');
                double k = double.Parse(elementJObject.GetValue("Change").ToString());

                if (Array.IndexOf(symbols, label) != -1)
                    return k;
                
                element = element.Next;
            }
            return 0;
        }
    }
}
