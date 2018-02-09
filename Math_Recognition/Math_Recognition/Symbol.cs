using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Math_Recognition
{
    class Symbol
    {
        public int TopLeftX;
        public int TopLeftY;
        public int Width;
        public int Height;
        public int MainCentreX;
        public int MainCentreY;
        public Rectangle MainRectangle;
        public List<Symbol>[] Inside;

        public Symbol(Rectangle rect, string symbolsFilename)
        {
            TopLeftX = rect.TopLeftX;
            TopLeftY = rect.TopLeftY;
            Width = rect.Width;
            Height = rect.Height;
            MainRectangle = rect;
            Inside = new List<Symbol>[5];
            MainCentreX = rect.GetCentrePoint().X;
            MainCentreY = rect.GetCentrePoint().Y + (int)(Height * getCenterYShift(rect.label, symbolsFilename));
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
