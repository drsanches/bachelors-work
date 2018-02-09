using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace Math_Recognition
{
    //TODO: cuts a little from below
    public class Segmentation
    {
        public Segmentation()
        {            
        }
        public List<Rectangle> MakeSegmentation(Rectangle rectangle)
        {
            Rectangle newRectangle = FindFormula(rectangle);
            List<Rectangle> newRectangles = CutByLines(newRectangle, VerticalLines(newRectangle), HorizontalLines(newRectangle));

            for (int i = newRectangles.Count - 1; i >= 0 ; i--)
            {
                newRectangle = FindFormula(newRectangles[i]);
                newRectangles.RemoveAt(i);
                newRectangles.Add(newRectangle);
            }
            return newRectangles;
        }
        public Rectangle FindFormula(Rectangle rectangle)
        {
            int x1 = rectangle.Width - 1;
            int y1 = rectangle.Height - 1;
            int x2 = 0;
            int y2 = 0;
            
            for (int i = 0; i < rectangle.Width; i++)
            for (int j = 0; j < rectangle.Height; j++)
            {
                if (rectangle.Array[i, j] > 0)
                {
                    if (i < x1)
                        x1 = i;
                }
            }

            for (int j = 0; j < rectangle.Height; j++)
            for (int i = 0; i < rectangle.Width; i++)
            {
                if (rectangle.Array[i, j] > 0)
                {
                    if (j < y1)
                        y1 = j; 
                }
            }

            for (int i = rectangle.Width - 1; i >= 0; i--)
            for (int j = rectangle.Height - 1; j >= 0 ; j--)
            {
                if (rectangle.Array[i, j] > 0)
                {
                    if (i > x2)
                        x2 = i;
                }
            }

            for (int j = rectangle.Height - 1; j >= 0; j--)
            for (int i = rectangle.Width - 1; i >= 0; i--)
            {
                if (rectangle.Array[i, j] > 0)
                {
                    if (j > y2)
                        y2 = j;
                }
            }

            return new Rectangle(rectangle.TopLeftX + x1, rectangle.TopLeftY + y1, 
                                 x2 - x1 + 1, y2 - y1 + 1, rectangle.Array, x1, y1);
        }
        public List<int> HorizontalLines(Rectangle rectangle)
        {
            List<int> lines = new List<int>();

            bool empty_line = true;
            bool begin = false;

            for (int j = 0; j < rectangle.Height; j++)
            {
                empty_line = true;
                for (int i = 0; i < rectangle.Width; i++)
                {
                    if (rectangle.Array[i, j] > 0)
                    {
                        empty_line = false;
                        if (!begin)
                        {
                            begin = true;
                            lines.Add(j);
                        }
                    }
                }

                if ((empty_line) && (begin))
                {
                    begin = false;
                    lines.Add(j);
                }
            }
            lines.Add(rectangle.Height);
            return lines;
        }
        public List<int> VerticalLines(Rectangle rectangle)
        {
            List<int> lines = new List<int>();

            bool empty_line = true;
            bool begin = false;

            for (int i = 0; i < rectangle.Width; i++)
            {
                empty_line = true;
                for (int j = 0; j < rectangle.Height; j++)
                {
                    if (rectangle.Array[i, j] > 0)
                    {
                        empty_line = false;
                        if (!begin)
                        {
                            begin = true;
                            lines.Add(i);
                        }
                    }
                }

                if ((empty_line) && (begin))
                {
                    begin = false;
                    lines.Add(i);
                }
            }
            lines.Add(rectangle.Width);
            return lines;
        }
        public List<Rectangle> CutByLines(Rectangle rectangle, List<int> verticalLines, List<int> horizontalLines)
        {
            int v = verticalLines.Count() - 1;
            int h = horizontalLines.Count() - 1;

            List<Rectangle> newRectangles = new List<Rectangle>();
            for (int i = 0; i < v; i++)
            for (int j = 0; j < h; j++)
            {
                int topLeftX = rectangle.TopLeftX + verticalLines[i];
                int topLeftY = rectangle.TopLeftY + horizontalLines[j];
                int width = verticalLines[i + 1] - verticalLines[i];
                int height = horizontalLines[j + 1] - horizontalLines[j];
                newRectangles.Add(new Rectangle(topLeftX, topLeftY, width, height, rectangle.Array, 
                                                topLeftX - rectangle.TopLeftX, topLeftY - rectangle.TopLeftY));
            }

            DeleteEmptyRectangles(newRectangles);

            return newRectangles;
        }
        public void DeleteEmptyRectangles(List<Rectangle> rectangles)
        {
            for (int i = rectangles.Count - 1; i >= 0; i--)
                if (rectangles[i].IsZero())
                    rectangles.RemoveAt(i);
        }
        public List<Rectangle> WaveSegmentation(Rectangle rect)
        {
            List<Rectangle> partsOfRectangles = new List<Rectangle>();

            for (int i = 0; i < rect.Width; i++)
            {
                bool is_break = false;
                for (int j = 0; j < rect.Height; j++)
                {
                    if (rect.Array[i, j] > 0)
                    {
                        int[,] a = Wave(rect, i, j);

                        Rectangle r = new Rectangle(rect.TopLeftX, rect.TopLeftY, rect.Width, rect.Height, a, 0, 0);
                        partsOfRectangles.Add(r);

                        Rectangle newRectangle = rect - r;

                        if ((r != newRectangle) && (!newRectangle.IsZero()))
                            partsOfRectangles.Add(newRectangle);

                        is_break = true;
                        break;

                    }
                }
                if (is_break)
                    break;
            }
            return partsOfRectangles;
        }
        int[,] Wave(Rectangle rect, int x1, int y1)
        {
            int[,] a = (int[,])rect.Array.Clone();

            //PrintArray(new Rectangle(rectangle.TopLeftX, rectangle.TopLeftY, rectangle.X2, rectangle.Y2, Array, rectangle.TopLeftX, rectangle.TopLeftY));
            
            int k = 2;
            a[x1, y1] = k;
            bool isEnd = false;
            while (!isEnd)
            {
                isEnd = true;
                k++;
                for (int i = 0; i < rect.Width; i++)
                for (int j = 0; j < rect.Height; j++)
                    if (a[i, j] == k - 1)
                    {
                        isEnd = false;
                        if (i + 1 < rect.Width)
                            if (a[i + 1, j] == 1)
                                a[i + 1, j] = k;
                        if (i - 1 >= 0)
                            if (a[i - 1, j] == 1)
                                a[i - 1, j] = k;
                        if (j + 1 < rect.Height)
                            if (a[i, j + 1] == 1)
                                a[i, j + 1] = k;
                        if (j - 1 >= 0)
                            if (a[i, j - 1] == 1)
                                a[i, j - 1] = k;
                    }
            }

            //PrintArray(new Rectangle(rectangle.TopLeftX, rectangle.TopLeftY, rectangle.X2, rectangle.Y2, Array, rectangle.TopLeftX, rectangle.TopLeftY));

            for (int i = 0; i < rect.Width; i++)
            for (int j = 0; j < rect.Height; j++)
                if (a[i, j] >= 2)
                    a[i, j] = rect.Array[i, j];
                else 
                    a[i, j] = 0;

            //PrintArray(new Rectangle(rectangle.TopLeftX, rectangle.TopLeftY, rectangle.X2, rectangle.Y2, Array, rectangle.TopLeftX, rectangle.TopLeftY));
            
            return a;
        }
        public bool IsSeparableByLines(Rectangle rectangle)
        {
            return (VerticalLines(rectangle).Count > 2) && (HorizontalLines(rectangle).Count > 2);
        }
        public bool IsSeparableByWave(Rectangle rectangle)
        {
            List<Rectangle> partsFromWave = WaveSegmentation(rectangle);
            if (partsFromWave.Count > 1)
                return true;
            else
                return false;
        }

        public void PrintArray(Rectangle rect)
        {
            string s = "";
            for (int i = rect.Width - 1; i >= 0; i--)
            {
                for (int j = 0; j < rect.Height; j++)
                {
                    if (rect.Array[i, j] == 0)
                        s += '0';
                    else
                        s += '1';
                }
                s += '\n';
            }

            MessageBox.Show(s);
        }
    }
}
