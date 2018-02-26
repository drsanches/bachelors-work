using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MathRecognition
{
    public abstract class StructuringAbstractFactory
    {
        public StructuringAbstractFactory()
        { }
        public abstract string getLatexCode(List<Rectangle> rectangles, List<Rectangle> notRecognizedRectangles, NeuralNetworkAbstractFactory neuralNetwork);
    }
    
    public class Structuring : StructuringAbstractFactory
    {
        private StructuringDelegate structuringDelegate;
        private string symbolsFilename;

        public Structuring(string symbolsJsonFilename, StructuringDelegate structDel) : base()
        {
            symbolsFilename = symbolsJsonFilename;
            structuringDelegate = structDel;
        }
        public override string getLatexCode(List<Rectangle> rectangles, List<Rectangle> notRecognizedRectangles, NeuralNetworkAbstractFactory neuralNetwork)
        {
            List<List<Symbol>> allBaselines = Baselines.CreateBaselines(rectangles, symbolsFilename);
            structuringDelegate.Invoke(ref allBaselines, notRecognizedRectangles, neuralNetwork);
            runStructuring(ref allBaselines);
            string latexCode = getBaselineLatexCode(allBaselines[0], symbolsFilename);
            return latexCode;
        }
        private void runStructuring(ref List<List<Symbol>> baselines)
        {
            int mainBaselineIndex = Baselines.FindMainBaselineIndex(baselines);
            List<Symbol> mainBaseline = baselines[mainBaselineIndex];
            baselines.RemoveAt(mainBaselineIndex);
            Dictionary<Symbol, List<List<Symbol>>> splitedBaselines = Baselines.GetSplittedIntoGroupsBaselines(baselines, mainBaseline);
            
            Symbol[] mainBaselineArray = mainBaseline.ToArray();
            
            baselines.Clear();
            baselines.Add(new List<Symbol>());

            foreach (Symbol mainSymbol in splitedBaselines.Keys)
            {
                Symbol newSymbol = Baselines.GetSymbolWithAddedBaselines(mainSymbol, splitedBaselines[mainSymbol]);
                baselines[0].Add(newSymbol);
            }

            foreach (Symbol symbol in baselines[0])
                for (int i = 0; i < symbol.Baselines.Count(); i++)
                    if (symbol.Baselines[i] != null)
                        if (symbol.Baselines[i].Count > 0)
                            runStructuring(ref symbol.Baselines[i]);
        }
        private string getBaselineLatexCode(List<Symbol> baseline, string symbolsFilename)
        {
            string latexCode = "{";

            foreach (Symbol symbol in baseline)
                latexCode += getSymbolLatexCode(symbol, symbolsFilename);

            latexCode += "}";

            return latexCode;
        }
        private string getSymbolLatexCode(Symbol symbol, string symbolsFilename)
        {
            string latexCode = symbol.MainRectangle.label;

            switch (getSymbolType(symbol, symbolsFilename))
            {
                case "\\frac":
                    latexCode += getBaselineLatexCode(symbol.Baselines[0][0], symbolsFilename);
                    latexCode += getBaselineLatexCode(symbol.Baselines[4][0], symbolsFilename);
                    break;

                case "Simple":
                    if (symbol.Baselines[1] != null)
                    {
                        latexCode += "^";
                        latexCode += getBaselineLatexCode(symbol.Baselines[1][0], symbolsFilename);
                    }
                    else if (symbol.Baselines[3] != null)
                    {
                        latexCode += "_";
                        latexCode += getBaselineLatexCode(symbol.Baselines[3][0], symbolsFilename);
                    }
                    break;

                case "SumType":
                    if ((symbol.Baselines[0] != null) && (symbol.Baselines[4] != null))
                        latexCode += "\\limits";
                    else if ((symbol.Baselines[1] != null) && (symbol.Baselines[3] != null))
                        latexCode += "\\nolimits";
                    if (symbol.Baselines[0] != null)
                    {
                        latexCode += "^";
                        latexCode += getBaselineLatexCode(symbol.Baselines[0][0], symbolsFilename);
                    }
                    if (symbol.Baselines[4] != null)
                    {
                        latexCode += "_";
                        latexCode += getBaselineLatexCode(symbol.Baselines[4][0], symbolsFilename);
                    }
                    if (symbol.Baselines[1] != null)
                    {
                        latexCode += "^";
                        latexCode += getBaselineLatexCode(symbol.Baselines[1][0], symbolsFilename);
                    }
                    if (symbol.Baselines[3] != null)
                    {
                        latexCode += "_";
                        latexCode += getBaselineLatexCode(symbol.Baselines[3][0], symbolsFilename);
                    }
                    break;

                case "MaxType":
                    if (symbol.Baselines[4] != null)
                    {
                        latexCode += "_";
                        latexCode += getBaselineLatexCode(symbol.Baselines[4][0], symbolsFilename);
                    }
                    break;
            }

            return latexCode;
        }
        private string getSymbolType(Symbol symbol, string symbolsFilename)
        {
            if (symbol.MainRectangle.label == "\\frac")
                return "\\frac";
            
            System.IO.StreamReader file = new System.IO.StreamReader(@symbolsFilename);
            string jsonString = file.ReadToEnd();
            file.Close();

            JObject fileJObject = JObject.Parse(jsonString);
            JToken element = fileJObject.GetValue("Types");

            JObject elementJObject = JObject.Parse(element.ToString());
            string symbols = elementJObject.GetValue("SumType").ToString();
            if (Array.IndexOf(symbols.Split(' '), symbol.MainRectangle.label) != -1)
                return "SumType";

            elementJObject = JObject.Parse(element.ToString());
            symbols = elementJObject.GetValue("MaxType").ToString();
            if (Array.IndexOf(symbols.Split(' '), symbol.MainRectangle.label) != -1)
                return "MaxType";

            return "Simple";
        }

    }
}
