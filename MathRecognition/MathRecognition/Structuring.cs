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

        public Structuring(StructuringDelegate structDel) : base()
        {
            structuringDelegate = structDel;
        }
        public override void Run(List<Rectangle> rectangles)
        {
            List<List<Symbol>> allBaselines = new List<List<Symbol>>();

            foreach (Rectangle rect in rectangles)
                addInBaselines(ref allBaselines, rect);

            //structuringDelegate.Invoke(ref baselines);

            runStructuring(ref allBaselines);

            string latexCode = getLatexCode(allBaselines[0]);
        }
        private void addInBaselines(ref List<List<Symbol>> baselines, Rectangle rectangle)
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
        private int findMainBaselineIndex(List<List<Symbol>> baselines)
        {
            int index = -1;
            double maxCoefficient = 0;
            
            for (int i = 0; i < baselines.Count; i++)
                if (getAverageHeightCoefficient(baselines[i]) > maxCoefficient)
                { 
                    maxCoefficient = getAverageHeightCoefficient(baselines[i]);
                    index = i;
                }

            return index;
        }
        private double getAverageHeightCoefficient(List<Symbol> symbols)
        {
            double sum = 0;
            foreach (Symbol symbol in symbols)
                sum += symbol.HeightCoefficient;

            return sum / symbols.Count;
        }
        private void runStructuring(ref List<List<Symbol>> baselines)
        {
            int mainBaselineIndex = findMainBaselineIndex(baselines);
            List<Symbol> mainBaseline = baselines[mainBaselineIndex];
            baselines.RemoveAt(mainBaselineIndex);
            Dictionary<Symbol, List<List<Symbol>>> splitedBaselines = getSplittedIntoGroupsBaselines(baselines, mainBaseline);
            
            Symbol[] mainBaselineArray = mainBaseline.ToArray();
            
            baselines.Clear();
            baselines.Add(new List<Symbol>());

            foreach (Symbol mainSymbol in splitedBaselines.Keys)
            {
                Symbol newSymbol = getSymbolWithAddedBaselines(mainSymbol, splitedBaselines[mainSymbol]);
                baselines[0].Add(newSymbol);
            }

            //TODO: SORT
            baselines[0].Sort((a, b) => a.TopLeftX.CompareTo(b.TopLeftX));

            foreach (Symbol symbol in baselines[0])
                for (int i = 0; i < symbol.Baselines.Count(); i++)
                    if (symbol.Baselines[i] != null)
                        if (symbol.Baselines[i].Count > 1)
                            runStructuring(ref symbol.Baselines[i]);
        }
        private Dictionary<Symbol, List<List<Symbol>>> getSplittedIntoGroupsBaselines(List<List<Symbol>> baselines, List<Symbol> mainBaseline)
        {
            List<List<Symbol>> newBaselines = baselines;
            Dictionary<Symbol, Symbol> connectedSymbols = getConnectedSymbols(newBaselines, mainBaseline);
            
            List<List<List<Symbol>>> groups = new List<List<List<Symbol>>>();
            groups.Add(newBaselines);

            while (!isSplittedIntoGroups(groups, connectedSymbols))
            {
                int cuttingX = findCuttingX(groups);
                cutGroupsOnX(ref groups, cuttingX);
            }

            Dictionary<Symbol, List<List<Symbol>>> dictionary = new Dictionary<Symbol, List<List<Symbol>>>();

            foreach (Symbol mainConnectedSymbol in connectedSymbols.Keys)
            {
                dictionary.Add(mainConnectedSymbol, getConnectedGroup(groups, connectedSymbols[mainConnectedSymbol]));
            }

            return dictionary;
        }
        private List<List<Symbol>> getConnectedGroup(List<List<List<Symbol>>> groups, Symbol connectedSymbol)
        {
            List<List<Symbol>> connectedBaselines = new List<List<Symbol>>();
            
            foreach (List<List<Symbol>> group in groups)
            {
                bool isHere = false;

                foreach (List<Symbol> baseline in group)
                    if (baseline.FindIndex(x => x == connectedSymbol) != -1)
                    {
                        isHere = true;
                        break;
                    }

                if (isHere)
                    foreach (List<Symbol> baseline in group)
                        connectedBaselines.Add(baseline);
            }

            return connectedBaselines;
        }
        private bool isSplittedIntoGroups(List<List<List<Symbol>>> groups, Dictionary<Symbol, Symbol> connectedSymbols)
        {
            foreach (List<List<Symbol>> group in groups)
            { 
                List<Symbol> projection = getProjection(group);
                int count = 0;

                foreach (Symbol connectedSymbol in connectedSymbols.Values)
                {
                    if (projection.FindIndex(x => x == connectedSymbol) != -1)
                        count++;
                    if (count >= 2)
                        return false;
                }
            }

            return true;
        }
        private int findCuttingX(List<List<List<Symbol>>> groups)
        {
            int maxDistance = 0;
            int cuttingX = 0;

            foreach (List<List<Symbol>> group in groups)
            {
                List<Symbol> projection = getProjection(group);

                for (int i = 0; i < projection.Count - 1; i++)
                {
                    int distance = getDistanceBetweenSymbols(projection[i], projection[i + 1]);
                    if (distance > maxDistance)
                    {
                        maxDistance = distance;

                        //TODO: test this line (should sort projection by MainCentreX)
                        cuttingX = Math.Max(projection[i].TopLeftX, projection[i + 1].TopLeftX) - (int)(distance / 2);
                    }
                }
            }

            return cuttingX;
        }
        private void cutGroupsOnX(ref List<List<List<Symbol>>> groups, int cuttingX)
        {
            int cuttingGroupIndex = -1;
            for (int i = 0; i <groups.Count; i++)
                if (isXInGroup(cuttingX, groups[i]))
                {
                    cuttingGroupIndex = i;
                    break;
                }

            if (cuttingGroupIndex != -1)
            {
                List<List<Symbol>> cuttingGroup = groups[cuttingGroupIndex];
                groups.RemoveAt(cuttingGroupIndex);

                List<List<List<Symbol>>> cuttedGroup = getCuttedOnXOneGroup(cuttingGroup, cuttingX);

                foreach (List<List<Symbol>> group in cuttedGroup)
                    if (group.Count != 0)
                        groups.Add(group);
            }
        }
        private bool isXInGroup(int x, List<List<Symbol>> group)
        { 
            List<Symbol> projection = getProjection(group);
            if ((x > findLeftBorder(projection)) && (x < findRightBorder(projection)))
                return true;
            else
                return false;
        }
        private List<List<List<Symbol>>> getCuttedOnXOneGroup(List<List<Symbol>> group, int cuttingX)
        {
            List<List<Symbol>> leftGroup = new List<List<Symbol>>();
            List<List<Symbol>> rightGroup = new List<List<Symbol>>();

            List<Symbol> projection = getProjection(group);

            foreach (List<Symbol> baseline in group)
            {
                List<Symbol> leftBaseline = new List<Symbol>();
                List<Symbol> rightBaseline = new List<Symbol>();

                foreach (Symbol symbol in baseline)
                {
                    if (symbol.TopLeftX > cuttingX)
                        rightBaseline.Add(symbol);
                    else
                        leftBaseline.Add(symbol);
                }

                if (leftBaseline.Count != 0)
                    leftGroup.Add(leftBaseline);
                if (rightBaseline.Count != 0)
                    rightGroup.Add(rightBaseline);
            }

            List<List<List<Symbol>>> cuttedGroups = new List<List<List<Symbol>>>();
            cuttedGroups.Add(leftGroup);
            cuttedGroups.Add(rightGroup);
            return cuttedGroups;
        }
        private Dictionary<Symbol, Symbol> getConnectedSymbols(List<List<Symbol>> baselines, List<Symbol> mainBaseline)
        {
            List<Symbol> projection = getProjection(baselines);
            Dictionary<Symbol, Symbol> connectedSymbols = new Dictionary<Symbol, Symbol>();

            foreach (Symbol mainSymbol in mainBaseline)
            {
                int minDistance = int.MaxValue;
                Symbol connectedSymbol = null;
                
                foreach (Symbol symbol in projection)
                {
                    int distance = getDistanceBetweenSymbols(mainSymbol, symbol);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        connectedSymbol = symbol;
                    }
                }

                if (connectedSymbol != null)
                    connectedSymbols.Add(mainSymbol, connectedSymbol);
            }

            return connectedSymbols;
        }
        private int findLeftBorder(List<Symbol> baseline)
        {
            if (baseline.Count == 0)
                return 0;

            int leftBorder = baseline[0].TopLeftX;

            if (baseline.Count > 1)
                for (int i = 1; i < baseline.Count; i++)
                    if (baseline[i].TopLeftX < leftBorder)
                        leftBorder = baseline[i].TopLeftX;

            return leftBorder;
        }
        private int findRightBorder(List<Symbol> baseline)
        {
            if (baseline.Count == 0)
                return 0;

            int rightBorder = baseline[0].TopLeftX + baseline[0].Width;

            if (baseline.Count > 1)
                for (int i = 1; i < baseline.Count; i++)
                    if (baseline[i].TopLeftX + baseline[i].Width > rightBorder)
                        rightBorder = baseline[i].TopLeftX + baseline[i].Width;

            return rightBorder;
        }
        private int getDistanceBetweenSymbols(Symbol a, Symbol b)
        {
            if (((a.TopLeftX >= b.TopLeftX) && (a.TopLeftX <= b.TopLeftX + b.Width)) ||
                    ((b.TopLeftX >= a.TopLeftX) && (b.TopLeftX <= a.TopLeftX + a.Width)))
                return 0;
            else
                return Math.Min(Math.Abs((a.TopLeftX + a.Width) - b.TopLeftX), Math.Abs((b.TopLeftX + b.Width) - a.TopLeftX));
        }
        private Symbol getSymbolWithAddedBaselines(Symbol symbol, List<List<Symbol>> baselines)
        {
            Symbol newSymbol = symbol;

            foreach (List<Symbol> baseline in baselines)
                if (getBaselineAverageY(baseline) < symbol.MainCentreY)
                {
                    if (newSymbol.Baselines[1] == null)
                        newSymbol.Baselines[1] = new List<List<Symbol>>();

                    newSymbol.Baselines[1].Add(baseline);
                }
                else
                {
                    if (newSymbol.Baselines[3] == null)
                        newSymbol.Baselines[3] = new List<List<Symbol>>();

                    newSymbol.Baselines[3].Add(baseline);
                }

            return newSymbol;
        }
        private Dictionary<String, List<List<Symbol>>> separateByBaseline(List<List<Symbol>> baselines, int baselineIndex)
        {
            List<List<Symbol>> up = new List<List<Symbol>>();
            List<List<Symbol>> down = new List<List<Symbol>>();
            double mainBaselineAverageY = getBaselineAverageY(baselines[baselineIndex]);

            for (int i = 0; i < baselines.Count; i++)
                if (i != baselineIndex)
                    if (getBaselineAverageY(baselines[i]) < mainBaselineAverageY)
                        up.Add(baselines[i]);
                    else
                        down.Add(baselines[i]);

            Dictionary<String, List<List<Symbol>>> response = new Dictionary<string, List<List<Symbol>>>();
            response.Add("up", up);
            response.Add("down", down);
            return response;
        }
        private double getBaselineAverageY(List<Symbol> baseline)
        {
            double sum = 0;
            foreach (Symbol symbol in baseline)
                sum += symbol.MainCentreY;
            return sum / baseline.Count;
        }
        private List<Symbol> getProjection(List<List<Symbol>> baselines)
        {
            List<Symbol> projection = new List<Symbol>();

            foreach (List<Symbol> baseline in baselines)
                foreach (Symbol symbol in baseline)
                    projection.Add(symbol);

            return projection;
        }

        private string getLatexCode(List<Symbol> baseline)
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

                    if (symbol.Baselines[1] != null)
                        latexCode += getLatexCode(symbol.Baselines[1][0]);
                }

            return latexCode;
        }
    }
}
