using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathRecognition
{
    public static class Baselines
    {
        private const double CENTRE_DISPLACEMENT_COEFFICIENT = 0.1;
        private const double HIGHT_COEFFICIENT_FAULT = 0.2;

        public static List<List<Symbol>> CreateBaselines(List<Rectangle> rectangles, string symbolsFilename, 
                double centreDisplacementCoefficient = CENTRE_DISPLACEMENT_COEFFICIENT)
        {
            List<List<Symbol>> allBaselines = new List<List<Symbol>>();

            foreach (Rectangle rect in rectangles)
                AddInBaselines(ref allBaselines, rect, symbolsFilename, centreDisplacementCoefficient);

            SortBaselines(ref allBaselines);

            return allBaselines;
        }
        public static List<List<Symbol>> CreateBaselines(List<Symbol> symbols, string symbolsFilename, 
                double centreDisplacementCoefficient = CENTRE_DISPLACEMENT_COEFFICIENT)
        {
            List<List<Symbol>> allBaselines = new List<List<Symbol>>();

            foreach (Symbol symbol in symbols)
                AddInBaselines(ref allBaselines, symbol, symbolsFilename, centreDisplacementCoefficient);

            SortBaselines(ref allBaselines);

            return allBaselines;
        }
        public static void SortBaselines(ref List<List<Symbol>> baselines)
        {
            foreach (List<Symbol> baseline in baselines)
                baseline.Sort((a, b) => a.TopLeftX.CompareTo(b.TopLeftX));
            baselines.Sort((a, b) => a[0].TopLeftX.CompareTo(b[0].TopLeftX));
        }
        public static void AddInBaselines(ref List<List<Symbol>> baselines, Rectangle rectangle, 
                string symbolsFilename, double centreDisplacementCoefficient = CENTRE_DISPLACEMENT_COEFFICIENT)
        {
            Symbol newSymbol = new Symbol(rectangle, symbolsFilename);
            AddInBaselines(ref baselines, newSymbol, symbolsFilename, centreDisplacementCoefficient);
        }
        public static void AddInBaselines(ref List<List<Symbol>> baselines, Symbol symbol, 
                string symbolsFilename, double centreDisplacementCoefficient = CENTRE_DISPLACEMENT_COEFFICIENT)
        {
            bool isAdded = false;
            for (int line = 0; line < baselines.Count; line++)
            {
                foreach (Symbol element in baselines[line])
                    if (isAtOneLine(element, symbol, centreDisplacementCoefficient))
                    {
                        baselines[line].Add(symbol);
                        isAdded = true;
                        break;
                    }

                if (isAdded)
                    break;
            }

            if (!isAdded)
            {
                baselines.Add(new List<Symbol>());
                baselines.Last().Add(symbol);
            }
        }
        private static bool isAtOneLine(Symbol s1, Symbol s2, double centreDisplacementCoefficient)
        {
            int maxHeight = Math.Max(s1.Height, s2.Height);
            int y1 = s1.MainCentreY;
            int y2 = s2.MainCentreY;
            if ((y1 < y2 + maxHeight * centreDisplacementCoefficient) && (y1 > y2 - maxHeight * centreDisplacementCoefficient))
                return true;
            else
                return false;
        }
        public static double getAverageHeightCoefficient(List<Symbol> symbols)
        {
            double sum = 0;
            foreach (Symbol symbol in symbols)
                sum += symbol.HeightCoefficient;

            return sum / symbols.Count;
        }
        public static int FindMainBaselineIndex(List<List<Symbol>> baselines, double hightCoefficientFault = HIGHT_COEFFICIENT_FAULT)
        {
            int index = -1;
            double maxCoefficient = -1;

            for (int i = 0; i < baselines.Count; i++)
                if (getAverageHeightCoefficient(baselines[i]) - hightCoefficientFault > maxCoefficient)
                {
                    maxCoefficient = getAverageHeightCoefficient(baselines[i]);
                    index = i;
                }

            return index;
        }
        public static Dictionary<Symbol, List<List<Symbol>>> GetSplittedIntoGroupsBaselines(List<List<Symbol>> baselines, List<Symbol> mainBaseline)
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
                dictionary.Add(mainConnectedSymbol, getConnectedGroup(groups, connectedSymbols[mainConnectedSymbol]));

            return dictionary;
        }
        private static List<List<Symbol>> getConnectedGroup(List<List<List<Symbol>>> groups, Symbol connectedSymbol)
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
        private static bool isSplittedIntoGroups(List<List<List<Symbol>>> groups, Dictionary<Symbol, Symbol> connectedSymbols)
        {
            foreach (List<List<Symbol>> group in groups)
            {
                List<Symbol> projection = GetProjection(group);
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
        private static int findCuttingX(List<List<List<Symbol>>> groups)
        {
            int maxDistance = 0;
            int cuttingX = 0;

            foreach (List<List<Symbol>> group in groups)
            {
                List<Symbol> projection = GetProjection(group);

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
        private static void cutGroupsOnX(ref List<List<List<Symbol>>> groups, int cuttingX)
        {
            int cuttingGroupIndex = -1;
            for (int i = 0; i < groups.Count; i++)
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
        private static bool isXInGroup(int x, List<List<Symbol>> group)
        {
            List<Symbol> projection = GetProjection(group);
            if ((x > findLeftBorder(projection)) && (x < findRightBorder(projection)))
                return true;
            else
                return false;
        }
        private static List<List<List<Symbol>>> getCuttedOnXOneGroup(List<List<Symbol>> group, int cuttingX)
        {
            List<List<Symbol>> leftGroup = new List<List<Symbol>>();
            List<List<Symbol>> rightGroup = new List<List<Symbol>>();

            List<Symbol> projection = GetProjection(group);

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
        private static Dictionary<Symbol, Symbol> getConnectedSymbols(List<List<Symbol>> baselines, List<Symbol> mainBaseline)
        {
            List<Symbol> projection = GetProjection(baselines);
            Dictionary<Symbol, Symbol> connectedSymbols = new Dictionary<Symbol, Symbol>();

            foreach (Symbol mainSymbol in mainBaseline)
            {
                int leftBorder = mainSymbol.TopLeftX;
                int rightBorder = mainSymbol.TopLeftX + (int)(mainSymbol.Width * 1.5);
                int minDistace = int.MaxValue;
                Symbol minSymbol = null;

                foreach (Symbol symbol in projection)
                {
                    if (((symbol.TopLeftX > leftBorder) && (symbol.TopLeftX < rightBorder)) ||
                            ((symbol.TopLeftX + symbol.Width > leftBorder) && (symbol.TopLeftX + symbol.Width < rightBorder)))
                    {
                        int distance = getDistanceBetweenSymbols(mainSymbol, symbol);
                        
                        if (minDistace > distance)
                        {
                            minDistace = distance;
                            minSymbol = symbol;
                        }
                    }
                }
                    
                if (connectedSymbols.Values.ToList().FindIndex(x => x == minSymbol) == -1)
                    connectedSymbols.Add(mainSymbol, minSymbol);
                else
                    connectedSymbols.Add(mainSymbol, null);
            }

            return connectedSymbols;
        }
        private static int findLeftBorder(List<Symbol> baseline)
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
        private static int findRightBorder(List<Symbol> baseline)
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
        private static int getDistanceBetweenSymbols(Symbol a, Symbol b)
        {
            if (((a.TopLeftX >= b.TopLeftX) && (a.TopLeftX <= b.TopLeftX + b.Width)) ||
                    ((b.TopLeftX >= a.TopLeftX) && (b.TopLeftX <= a.TopLeftX + a.Width)))
                return 0;
            else
                return Math.Min(Math.Abs((a.TopLeftX + a.Width) - b.TopLeftX), Math.Abs((b.TopLeftX + b.Width) - a.TopLeftX));
        }
        public static Symbol GetSymbolWithAddedBaselines(Symbol symbol, List<List<Symbol>> baselines)
        {
            Symbol newSymbol = symbol;
            
            foreach (List<Symbol> baseline in baselines) 
            { 
                int index = -1;

                if (getBaselineAverageY(baseline) < symbol.TopLeftY)
                    index = 0;
                else if ((getBaselineAverageY(baseline) < symbol.MainCentreY) && (getBaselineAverageY(baseline) > symbol.TopLeftY))
                    index = 1;
                else if ((getBaselineAverageY(baseline) > symbol.MainCentreY) && (getBaselineAverageY(baseline) < symbol.TopLeftY + symbol.Height))
                    index = 3;
                else if (getBaselineAverageY(baseline) > symbol.TopLeftY + symbol.Height)
                    index = 4;

                if (index != -1)
                {
                    if (newSymbol.Baselines[index] == null)
                        newSymbol.Baselines[index] = new List<List<Symbol>>();
                    newSymbol.Baselines[index].Add(baseline);
                }
            }
            return newSymbol;
        }
        private static Dictionary<String, List<List<Symbol>>> separateByBaseline(List<List<Symbol>> baselines, int baselineIndex)
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
        private static double getBaselineAverageY(List<Symbol> baseline)
        {
            double sum = 0;
            foreach (Symbol symbol in baseline)
                sum += symbol.MainCentreY;
            return sum / baseline.Count;
        }
        public static List<Symbol> GetProjection(List<List<Symbol>> baselines)
        {
            List<Symbol> projection = new List<Symbol>();

            foreach (List<Symbol> baseline in baselines)
                foreach (Symbol symbol in baseline)
                    projection.Add(symbol);

            return projection;
        }
        public static List<Symbol> FindBottomSymbols(List<List<Symbol>> baselines, Symbol mainSymbol, int depth = int.MaxValue / 2)
        {
            List<List<Symbol>> tmpBaselines = baselines;
            List<Symbol> bottomSymbols = new List<Symbol>();

            foreach (List<Symbol> baseline in tmpBaselines)
                foreach (Symbol symbol in baseline)
                {
                    if (symbol != mainSymbol)
                    {
                        if ((symbol.MainCentreY > mainSymbol.MainCentreY) &&
                                (symbol.TopLeftY < mainSymbol.TopLeftY + mainSymbol.Height + depth) &&
                                (((symbol.TopLeftX > mainSymbol.TopLeftX) && (symbol.TopLeftX < mainSymbol.TopLeftX + mainSymbol.Width)) ||
                                ((symbol.TopLeftX + symbol.Width > mainSymbol.TopLeftX) && (symbol.TopLeftX + symbol.Width < mainSymbol.TopLeftX + mainSymbol.Width)) ||
                                ((mainSymbol.TopLeftX > symbol.TopLeftX) && (mainSymbol.TopLeftX < symbol.TopLeftX + symbol.Width)) ||
                                ((mainSymbol.TopLeftX + mainSymbol.Width > symbol.TopLeftX) && (mainSymbol.TopLeftX + mainSymbol.Width < symbol.TopLeftX + symbol.Width))))
                            bottomSymbols.Add(symbol);
                    }
                }

            return bottomSymbols;
        }
        public static List<Symbol> FindUpperSymbols(List<List<Symbol>> baselines, Symbol mainSymbol, int depth = int.MaxValue / 2)
        {
            List<List<Symbol>> tmpBaselines = baselines;
            List<Symbol> upperSymbols = new List<Symbol>();

            foreach (List<Symbol> baseline in tmpBaselines)
                foreach (Symbol symbol in baseline)
                {
                    if (symbol != mainSymbol)
                    {
                        if ((symbol.MainCentreY < mainSymbol.MainCentreY) &&
                                (symbol.TopLeftY + symbol.Height > mainSymbol.TopLeftY - depth) &&
                                (((symbol.TopLeftX > mainSymbol.TopLeftX) && (symbol.TopLeftX < mainSymbol.TopLeftX + mainSymbol.Width)) ||
                                ((symbol.TopLeftX + symbol.Width > mainSymbol.TopLeftX) && (symbol.TopLeftX + symbol.Width < mainSymbol.TopLeftX + mainSymbol.Width)) ||
                                ((mainSymbol.TopLeftX > symbol.TopLeftX) && (mainSymbol.TopLeftX < symbol.TopLeftX + symbol.Width)) ||
                                ((mainSymbol.TopLeftX + mainSymbol.Width > symbol.TopLeftX) && (mainSymbol.TopLeftX + mainSymbol.Width < symbol.TopLeftX + symbol.Width))))
                            upperSymbols.Add(symbol);
                    }
                }

            return upperSymbols;
        }
    }
}
