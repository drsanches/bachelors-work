using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MathRecognition
{
    public struct Rectangle
    {
        public int TopLeftX, TopLeftY;
        public int Width, Height;
        public int[,] Array;
        public string label;
        public Rectangle(int topLeftX, int topLeftY, int width, int height, int[,] array, int startAtArrayX, int startAtArrayY)
        {
            label = "";
            TopLeftX = topLeftX;
            TopLeftY = topLeftY;
            Width = width;
            Height = height;
            Array = new int[Width, Height];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    Array[i, j] = array[i + startAtArrayX, j + startAtArrayY];
        }
        public Rectangle(int topLeftX, int topLeftY, int width, int height)
        {
            label = "";
            TopLeftX = topLeftX;
            TopLeftY = topLeftY;
            Width = width;
            Height = height;
            Array = new int[Width, Height];
        }
        //TODO: Create mask
        public Rectangle(Bitmap bitmap)
        {
            label = "";
            TopLeftX = 0;
            TopLeftY = 0;
            Width = bitmap.Width;
            Height = bitmap.Height;
            Array = new int[Width, Height];
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                {
                    if ((int)bitmap.GetPixel(i, j).GetBrightness() <= 0.3)
                        Array[i, j] = 1;
                }
        }
        public static bool operator ==(Rectangle a, Rectangle b)
        {
            if ((a.TopLeftX == b.TopLeftX) && (a.TopLeftY == b.TopLeftY) && (a.Width == b.Width) && (a.Height == b.Height))
            {
                bool t = true;
                for (int i = 0; i < a.Width; i++)
                    for (int j = 0; j < a.Height; j++)
                        if (a.Array[i, j] != b.Array[i, j])
                        {
                            t = false;
                            break;
                        }
                return t;
            }
            else
                return false;
        }
        public static bool operator !=(Rectangle r1, Rectangle r2)
        {
            return !(r1 == r2);
        }
        public static Rectangle operator -(Rectangle a, Rectangle b)
        {
            if ((a.Width != b.Width) || (a.Height != b.Height))
                throw new Exception("Dimensions of rectangles is not matched.");

            Rectangle res = new Rectangle(a.TopLeftX, a.TopLeftY, a.Width, a.Height);

            for (int i = 0; i < a.Width; i++)
                for (int j = 0; j < a.Height; j++)
                {
                    if ((a.Array[i, j] > 0) && (b.Array[i, j] > 0))
                        res.Array[i, j] = 0;
                    else
                        res.Array[i, j] = a.Array[i, j];
                }
            return res;
        }
        public static Rectangle operator +(Rectangle a, Rectangle b)
        {
            int newWidth = 0;
            int newHeight = 0;

            if (a.TopLeftX < b.TopLeftX)
                newWidth = Math.Max(b.TopLeftX + b.Width - a.TopLeftX, a.Width);
            else
                newWidth = Math.Max(a.TopLeftX + a.Width - b.TopLeftX, b.Width);

            if (a.TopLeftY < b.TopLeftY)
                newHeight = Math.Max(b.TopLeftY + b.Height - a.TopLeftY, a.Height);
            else
                newHeight = Math.Max(a.TopLeftY + a.Height - b.TopLeftY, b.Height);

            Rectangle newRectangle = new Rectangle(Math.Min(a.TopLeftX, b.TopLeftX),
                    Math.Min(a.TopLeftY, b.TopLeftY),
                    newWidth, newHeight);

            for (int i = 0; i < newRectangle.Width; i++)
                for (int j = 0; j < newRectangle.Height; j++)
                    newRectangle.Array[i, j] = 0;

            for (int i = 0; i < a.Width; i++)
                for (int j = 0; j < a.Height; j++)
                    newRectangle.Array[Math.Abs(newRectangle.TopLeftX - a.TopLeftX) + i, Math.Abs(newRectangle.TopLeftY - a.TopLeftY) + j] += a.Array[i, j];

            for (int i = 0; i < b.Width; i++)
                for (int j = 0; j < b.Height; j++)
                    newRectangle.Array[Math.Abs(newRectangle.TopLeftX - b.TopLeftX) + i, Math.Abs(newRectangle.TopLeftY - b.TopLeftY) + j] += b.Array[i, j];

            return newRectangle;
        }
        public bool IsZero()
        {
            for (int i = 0; i < Width; i++)
                for (int j = 0; j < Height; j++)
                    if (this.Array[i, j] != 0)
                        return false;
            return true;
        }
        public Point GetCentrePoint()
        {
            Point p = new Point();
            p.X = TopLeftX + Width / 2;
            p.Y = TopLeftY + Height / 2;
            return p;
        }
    }
}
