using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace MathRecognition
{
    public delegate void StructuringDelegate(ref List<List<Symbol>> baselines, List<Rectangle> notRecognizedRectangles, INeuralNetwork neuralNetwork);

    public static class StructuringDelegateFactory
    {
        private const double LINE_FAULT = 0.1;
        private static string symbolsFilename;

        public static StructuringDelegate Create(string symbolsJsonFilename)
        {
            symbolsFilename = symbolsJsonFilename;
            StructuringDelegate structuringDelegate = doNothing;

            structuringDelegate += checkAllDotsForIJ;
            structuringDelegate += checkAllEquals;
            structuringDelegate += checkAllSqrts;
            structuringDelegate += checkAllFracs;
            structuringDelegate += checkAllCompositeOperators;

            return structuringDelegate;
        }
        private static void doNothing(ref List<List<Symbol>> baselines, List<Rectangle> notRecognizedRectangles, INeuralNetwork neuralNetwork)
        { }
        private static void checkAllDotsForIJ(ref List<List<Symbol>> baselines, List<Rectangle> notRecognizedRectangles, INeuralNetwork neuralNetwork)
        {
            List<Symbol> deletingSymbols = new List<Symbol>();
            foreach (Rectangle rectangle in notRecognizedRectangles)
            {
                Symbol newSymbol = new Symbol(rectangle, symbolsFilename);
                BaselinesMethods.AddInBaselines(ref baselines, newSymbol, symbolsFilename);
                deletingSymbols.Add(newSymbol);
            }

            foreach (List<Symbol> baseline in baselines)
                foreach (Symbol symbol in baseline)
                    if (symbol.MainRectangle.label == ".")
                    {
                        List<Symbol> sqrtBottomSymbols = BaselinesMethods.FindBottomSymbols(baselines, symbol, symbol.Height * 5);
                        List<Symbol> bottomSymbols = new List<Symbol>();
                        
                        foreach (Symbol sqrtBottomSymbol in sqrtBottomSymbols)
                            if (sqrtBottomSymbol.MainRectangle.label != "\\sqrt")
                                bottomSymbols.Add(sqrtBottomSymbol);

                        if (bottomSymbols.Count == 1)
                        {
                            Symbol bottomSymbol = bottomSymbols.First();
                            List<Rectangle> list = new List<Rectangle>();
                            list.Add(symbol.MainRectangle + bottomSymbol.MainRectangle);
                            neuralNetwork.RecognizeList(list);
                            if (neuralNetwork.GetRecognizedList().Count == 1)
                            {
                                deletingSymbols.Add(symbol);
                                deletingSymbols.Add(bottomSymbol);
                                BaselinesMethods.AddInBaselines(ref baselines, neuralNetwork.GetRecognizedList().First(), symbolsFilename);
                            }
                        }
                    }

            foreach (Symbol deletingSymbol in deletingSymbols)
                foreach (List<Symbol> baseline in baselines)
                    baseline.RemoveAll(x => x == deletingSymbol);
            baselines.RemoveAll(x => x.Count == 0);

            BaselinesMethods.SortBaselines(ref baselines);
        }
        private static void checkAllEquals(ref List<List<Symbol>> baselines, List<Rectangle> notRecognizedRectangles, INeuralNetwork neuralNetwork)
        {
            Dictionary<Symbol, Symbol> equals = getAllPartsOfEquals(baselines);

            foreach (List<Symbol> baseline in baselines)
            {
                foreach (Symbol deletingSymbol in equals.Keys)
                    baseline.Remove(deletingSymbol);

                foreach (Symbol deletingSymbol in equals.Values)
                    baseline.Remove(deletingSymbol);
            }

            baselines.RemoveAll(x => x.Count == 0);

            foreach (Symbol key in equals.Keys)
            {
                Rectangle newRectangle = key.MainRectangle + equals[key].MainRectangle;
                newRectangle.label = "=";
                BaselinesMethods.AddInBaselines(ref baselines, newRectangle, symbolsFilename);
            }

            BaselinesMethods.SortBaselines(ref baselines);
        }
        private static Dictionary<Symbol, Symbol> getAllPartsOfEquals(List<List<Symbol>> baselines)
        {
            List<List<Symbol>> tmpBaselines = baselines;
            Dictionary<Symbol, Symbol> equals = new Dictionary<Symbol, Symbol>();

            foreach (List<Symbol> baseline in tmpBaselines)
                foreach (Symbol symbol in baseline)
                {
                    if (symbol.MainRectangle.label == "-")
                    {
                        List<Symbol> bottomSymbols = BaselinesMethods.FindBottomSymbols(tmpBaselines, symbol, (int)(symbol.Width / 2));
                        List<Symbol> upperSymbols = BaselinesMethods.FindUpperSymbols(tmpBaselines, symbol, (int)(symbol.Width / 2));
                        if ((bottomSymbols.Count == 1) && (upperSymbols.Count == 0))
                        {
                            Symbol bottomSymbol = bottomSymbols[0];
                            if ((bottomSymbol.MainRectangle.label == "-") &&
                                    (Math.Abs(bottomSymbol.Width - symbol.Width) < Math.Max(bottomSymbol.Width, symbol.Width) * LINE_FAULT) &&
                                    (equals.Keys.ToList().FindIndex(x => x == bottomSymbol) == -1))
                                equals.Add(symbol, bottomSymbol);
                        }
                        if ((bottomSymbols.Count == 0) && (upperSymbols.Count == 1))
                        {
                            Symbol upperSymbol = upperSymbols[0];
                            if ((upperSymbol.MainRectangle.label == "-") &&
                                    (Math.Abs(upperSymbol.Width - symbol.Width) < Math.Max(upperSymbol.Width, symbol.Width) * LINE_FAULT) &&
                                    (equals.Keys.ToList().FindIndex(x => x == upperSymbol) == -1))
                                equals.Add(symbol, upperSymbol);
                        }
                    }
                }

            return equals;
        }
        private static void checkAllSqrts(ref List<List<Symbol>> baselines, List<Rectangle> notRecognizedRectangles, INeuralNetwork neuralNetwork)
        {
            Symbol sqrt = getLeastEmptySqrt(baselines);
            if (sqrt != null)
            {
                List<Symbol> innerSymbols = BaselinesMethods.FindInnerSymbols(baselines, sqrt);

                sqrt.Baselines[2] = BaselinesMethods.CreateBaselines(innerSymbols, symbolsFilename);

                foreach (List<Symbol> baseline in baselines)
                    foreach (Symbol innerSymbol in innerSymbols)
                        baseline.Remove(innerSymbol);
                
                baselines.RemoveAll(x => x.Count == 0);

                checkAllSqrts(ref baselines, notRecognizedRectangles, neuralNetwork);
            }
        }
        private static Symbol getLeastEmptySqrt(List<List<Symbol>> baselines)
        {
            Symbol leastEmptySqrt = null;

            foreach (List<Symbol> baseline in baselines)
                foreach (Symbol symbol in baseline)
                {
                    if ((symbol.MainRectangle.label == "\\sqrt") && (symbol.Baselines[2] == null))
                    {
                        if (leastEmptySqrt != null)
                        {
                            if (symbol.Width < leastEmptySqrt.Width)
                                leastEmptySqrt = symbol;
                        }
                        else
                        {
                            leastEmptySqrt = symbol;
                        }
                    }
                }

            return leastEmptySqrt;
        }
        private static void checkAllFracs(ref List<List<Symbol>> baselines, List<Rectangle> notRecognizedRectangles, INeuralNetwork neuralNetwork)
        {
            Symbol frac = getBiggestEmptyFrac(baselines);
            if (frac != null)
            {
                List<Symbol> bottomSymbols = BaselinesMethods.FindBottomSymbols(baselines, frac);
                List<Symbol> upperSymbols = BaselinesMethods.FindUpperSymbols(baselines, frac);

                frac.Baselines[0] = BaselinesMethods.CreateBaselines(upperSymbols, symbolsFilename);
                frac.Baselines[4] = BaselinesMethods.CreateBaselines(bottomSymbols, symbolsFilename);
                frac.MainRectangle.label = "\\frac";

                foreach (List<Symbol> baseline in baselines)
                {
                    foreach (Symbol upperSymbol in upperSymbols)
                        baseline.Remove(upperSymbol);

                    foreach (Symbol bottomSymbol in bottomSymbols)
                        baseline.Remove(bottomSymbol);
                }
                baselines.RemoveAll(x => x.Count == 0);

                checkAllFracs(ref frac.Baselines[0], notRecognizedRectangles, neuralNetwork);
                checkAllFracs(ref frac.Baselines[4], notRecognizedRectangles, neuralNetwork);
                checkAllFracs(ref baselines, notRecognizedRectangles, neuralNetwork);
            }
            else
            {
                foreach (List<Symbol> baseline in baselines)
                    foreach (Symbol symbol in baseline)
                        for (int i = 0; i < symbol.Baselines.Length; i++)
                            if (symbol.Baselines[i] != null)
                            checkAllFracs(ref symbol.Baselines[i], notRecognizedRectangles, neuralNetwork);
            }
        }
        private static Symbol getBiggestEmptyFrac(List<List<Symbol>> baselines)
        {
            Symbol biggestFrac = null;

            foreach (List<Symbol> baseline in baselines)
                foreach (Symbol symbol in baseline)
                {
                    if (isFrac(baselines, symbol))
                    {
                        if (biggestFrac != null)
                        {
                            if (symbol.Width > biggestFrac.Width)
                                biggestFrac = symbol;
                        }
                        else
                        {
                            biggestFrac = symbol;
                        }
                    }
                }
            
            return biggestFrac;
        }
        private static bool isFrac(List<List<Symbol>> baselines, Symbol symbol)
        { 
            if (symbol.MainRectangle.label == "-")
            {
                List<Symbol> bottomSymbols = BaselinesMethods.FindBottomSymbols(baselines, symbol, symbol.Width * 2);
                List<Symbol> upperSymbols = BaselinesMethods.FindUpperSymbols(baselines, symbol, symbol.Width * 2);

                return (bottomSymbols.Count != 0) && (upperSymbols.Count != 0);
            }
            else 
                return false;
        }
        private static void checkAllCompositeOperators(ref List<List<Symbol>> baselines, List<Rectangle> notRecognizedRectangles, INeuralNetwork neuralNetwork)
        {
            string[] operatorsCodes = getAllOperators();
            
            foreach (string operatorCode in operatorsCodes)
            {
                List<Symbol> deletingSymbols = new List<Symbol>();
                List<List<Symbol>> softBaselines = BaselinesMethods.CreateBaselines(BaselinesMethods.GetProjection(baselines), symbolsFilename, 0.4);

                foreach (List<Symbol> baseline in softBaselines)
                {
                    string stringOfBaseline = getStringOfBaseline(baseline);
                    int operatorLength = operatorCode.Length - 1;
                    int startIndex = stringOfBaseline.IndexOf(operatorCode.Substring(1, operatorLength), StringComparison.CurrentCultureIgnoreCase);

                    while (startIndex != -1)
                    {
                        List<Symbol> symbolsBaseline = getSymbolsBaseline(baseline);
                        Symbol newSymbol = symbolsBaseline[startIndex];
                        deletingSymbols.Add(symbolsBaseline[startIndex]); 

                        for (int j = 1; j < operatorLength; j++)
                        {
                            Rectangle newRectangle = newSymbol.MainRectangle + symbolsBaseline[startIndex + j].MainRectangle;
                            newRectangle.label = operatorCode;
                            newSymbol = new Symbol(newRectangle, symbolsFilename);
                            deletingSymbols.Add(symbolsBaseline[startIndex + j]);
                        }

                        BaselinesMethods.AddInBaselines(ref baselines, newSymbol, symbolsFilename);

                        string stringForInsert = new String(' ', operatorLength);
                        stringOfBaseline = stringOfBaseline.Remove(startIndex, operatorLength);
                        stringOfBaseline = stringOfBaseline.Insert(startIndex, stringForInsert);

                        if (stringOfBaseline.IndexOf(operatorCode.Substring(1, operatorLength), StringComparison.CurrentCultureIgnoreCase) != -1)
                            startIndex += stringOfBaseline.IndexOf(operatorCode.Substring(1, operatorLength), StringComparison.CurrentCultureIgnoreCase);
                        else
                            startIndex = -1;
                    }
                }

                foreach (Symbol deletingSymbol in deletingSymbols)
                    foreach (List<Symbol> baseline in baselines)
                        baseline.Remove(deletingSymbol);
                baselines.RemoveAll(x => x.Count == 0);

                BaselinesMethods.SortBaselines(ref baselines);
            }
        }
        private static List<Symbol> getSymbolsBaseline(List<Symbol> baseline)
        {
            List<Symbol> symbolsBaseline = new List<Symbol>();

            foreach (Symbol symbol in baseline)
                if (symbol.MainRectangle.label[0] != '\\')
                    symbolsBaseline.Add(symbol);

            return symbolsBaseline;
        }
        private static string[] getAllOperators()
        {
            System.IO.StreamReader file = new System.IO.StreamReader(symbolsFilename);
            string jsonString = file.ReadToEnd();
            file.Close();

            JObject fileJObject = JObject.Parse(jsonString);
            string[] operatorsCodes = fileJObject.GetValue("CompositeOperators").ToString().Split(' ');

            return operatorsCodes;
        }
        private static string getStringOfBaseline(List<Symbol> baseline)
        { 
            string stringOfBaseline = "";

            foreach (Symbol symbol in baseline)
                if (symbol.MainRectangle.label[0] != '\\')
                    stringOfBaseline += symbol.MainRectangle.label;

            return stringOfBaseline;
        }
    }
}