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
        public int TopLeftX;
        public int TopLeftY;
        public int Width;
        public int Height;
        public int MainCentreX;
        public int MainCentreY;
        public double HeightCoefficient;
        public Rectangle MainRectangle;
        public List<List<Symbol>>[] Baselines;

        public Symbol(Rectangle rectangle, string symbolsFilename)
        {
            TopLeftX = rectangle.TopLeftX;
            TopLeftY = rectangle.TopLeftY;
            Width = rectangle.Width;
            Height = rectangle.Height;
            MainRectangle = rectangle;
            Baselines = new List<List<Symbol>>[5];
            MainCentreX = rectangle.GetCentrePoint().X;
            MainCentreY = rectangle.GetCentrePoint().Y + (int)(Height * getCenterYShift(rectangle.label, symbolsFilename));
            HeightCoefficient = Height / (double)getRelativeHeignt(rectangle, symbolsFilename);
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
        private int getRelativeHeignt(Rectangle rectangle, string symbolsFilename)
        {
            System.IO.StreamReader file = new System.IO.StreamReader(@symbolsFilename);
            string jsonString = file.ReadToEnd();
            file.Close();

            JObject fileJObject = JObject.Parse(jsonString);
            JToken element = fileJObject.GetValue("SymbolsHeights").First;
            while (element != null)
            {
                JObject elementJObject = JObject.Parse(element.ToString());
                string symbol = elementJObject.GetValue("Symbol").ToString();

                if (symbol == rectangle.label)
                {
                    int relativeHeight = int.Parse(elementJObject.GetValue("Height").ToString());
                    return relativeHeight;
                }

                element = element.Next;
            }
            return rectangle.Height;
        }
    }
}
