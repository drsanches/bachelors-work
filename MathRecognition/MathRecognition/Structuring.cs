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
        public abstract string getLatexCode(List<Rectangle> rectangles);
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
        public override string getLatexCode(List<Rectangle> rectangles)
        {
            List<List<Symbol>> allBaselines = Baselines.CreateBaselines(rectangles, symbolsFilename);
            structuringDelegate.Invoke(ref allBaselines);
            runStructuring(ref allBaselines);
            string latexCode = getBaselineLatexCode(allBaselines[0]);
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
                        if (symbol.Baselines[i].Count > 1)
                            runStructuring(ref symbol.Baselines[i]);
        }
        private string getBaselineLatexCode(List<Symbol> baseline)
        {
            string latexCode = "{";

            foreach (Symbol symbol in baseline)
                latexCode += getSymbolLatexCode(symbol);

            latexCode += "}";

            return latexCode;
        }
        private string getSymbolLatexCode(Symbol symbol)
        {
            string latexCode = symbol.MainRectangle.label;

            if (symbol.MainRectangle.label == "\\frac")
            {
                latexCode += getBaselineLatexCode(symbol.Baselines[0][0]) + getBaselineLatexCode(symbol.Baselines[4][0]);
            }
            else
                for (int i = 0; i < symbol.Baselines.Count(); i++)
                    if (symbol.Baselines[i] != null)
                    {
                        switch (i)
                        {
                            case 1:
                                latexCode += "^";
                                break;
                            case 3:
                                latexCode += "_";
                                break;
                        }

                        if (symbol.Baselines[i] != null)
                            latexCode += getBaselineLatexCode(symbol.Baselines[i][0]);
                    }

            return latexCode;
        }
    }
}
