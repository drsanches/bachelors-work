using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MathRecognition
{
    public delegate void StructuringDelegate(ref List<List<Symbol>> baselines);

    public static class StructuringDelegateCreator
    {
        private const double LINE_FAULT = 0.1;
        private static string jsonFilename;

        public static StructuringDelegate CreateDelegate(string symbolsFilename)
        {
            jsonFilename = symbolsFilename;
            StructuringDelegate structuringDelegate = doNothing;

            //structuringDelegate += checkAllDotsForIJ;
            structuringDelegate += checkAllEquals;
            structuringDelegate += checkAllFracs;

            return structuringDelegate;
        }
        private static void doNothing(ref List<List<Symbol>> baselines)
        { }
        private static void checkAllDotsForIJ(ref List<List<Symbol>> baselines)
        {
            //TODO: Write code of functions
            MessageBox.Show("Check all dots");
        }
        private static void checkAllEquals(ref List<List<Symbol>> baselines)
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
                Baselines.AddInBaselines(ref baselines, newRectangle, jsonFilename);
            }

            Baselines.SortBaselines(ref baselines);
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
                        List<Symbol> bottomSymbols = Baselines.FindBottomSymbols(tmpBaselines, symbol, (int)(symbol.Width / 2));
                        List<Symbol> upperSymbols = Baselines.FindUpperSymbols(tmpBaselines, symbol, (int)(symbol.Width / 2));
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
        private static void checkAllFracs(ref List<List<Symbol>> baselines)
        {
            Symbol frac = getBiggestFrac(baselines);
            if (frac != null)
            {
                List<Symbol> bottomSymbols = Baselines.FindBottomSymbols(baselines, frac);
                List<Symbol> upperSymbols = Baselines.FindUpperSymbols(baselines, frac);

                frac.Baselines[0] = Baselines.CreateBaselines(upperSymbols, jsonFilename);
                frac.Baselines[4] = Baselines.CreateBaselines(bottomSymbols, jsonFilename);
                frac.MainRectangle.label = "\\frac";

                foreach (List<Symbol> baseline in baselines)
                {
                    foreach (Symbol upperSymbol in upperSymbols)
                        baseline.Remove(upperSymbol);
                    
                    foreach (Symbol bottomSymbol in bottomSymbols)
                        baseline.Remove(bottomSymbol);
                }
                baselines.RemoveAll(x => x.Count == 0);

                checkAllFracs(ref frac.Baselines[0]);
                checkAllFracs(ref frac.Baselines[4]);
            }
        }
        private static Symbol getBiggestFrac(List<List<Symbol>> baselines)
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

                List<Symbol> bottomSymbols = Baselines.FindBottomSymbols(baselines, symbol, (int)(symbol.Width / 2));
                List<Symbol> upperSymbols = Baselines.FindUpperSymbols(baselines, symbol, (int)(symbol.Width / 2));

                return (bottomSymbols.Count != 0) && (upperSymbols.Count != 0);
            }
            else 
                return false;
        }
    }
}