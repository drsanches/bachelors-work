using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Math_Recognition
{
    public partial class Form1 : Form
    {
        const string filename = "..\\..\\..\\..\\Formulas\\Example5.png";
        Bitmap bitmap;
        Graphics g;
        public Recognizer recognizer;


        public Form1()
        {
            bitmap = new Bitmap(@filename);
            g = Graphics.FromImage(bitmap);

            int[,] array = new int[bitmap.Width, bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            for (int j = 0; j < bitmap.Height; j++)
            {
                if ((int)bitmap.GetPixel(i, j).GetBrightness() <= 0.3)
                    array[i, j] = 1;
            }

            recognizer = new Recognizer(filename);
            recognizer.Recognize(new Rectangle(0, 0, bitmap.Width - 1, bitmap.Height - 1, array, 0, 0));

            InitializeComponent();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bitmap, 0, 0);
            this.Height = bitmap.Height + 30;
            this.Width = bitmap.Width + 5;

            foreach (Rectangle r in recognizer.Recognized)
            {
                e.Graphics.DrawRectangle(new Pen(Color.Green), r.TopLeftX, r.TopLeftY, r.Width, r.Height);
            }

            foreach (Rectangle r in recognizer.NotRecognized)
            {
                e.Graphics.DrawRectangle(new Pen(Color.Yellow), r.TopLeftX, r.TopLeftY, r.Width, r.Height);
            }

            foreach (Rectangle r in recognizer.CanNotBeRecognized)
            {
                e.Graphics.DrawRectangle(new Pen(Color.Red), r.TopLeftX, r.TopLeftY, r.Width, r.Height);
            }

            //e.Graphics.DrawLine(new Pen(Color.Blue), results.X2, results.TopLeftY, results.X2, results.Y2);
            //e.Graphics.DrawLine(new Pen(Color.Blue), results.TopLeftX, results.TopLeftY, results.TopLeftX, results.Y2);
            //e.Graphics.DrawLine(new Pen(Color.Blue), results.TopLeftX, results.TopLeftY, results.X2, results.TopLeftY);
            //e.Graphics.DrawLine(new Pen(Color.Blue), results.TopLeftX, results.Y2, results.X2, results.Y2);

            //e.Graphics.DrawRectangle(new Pen(Color.Blue), rectangle.TopLeftX - 1, rectangle.TopLeftY - 1, rectangle.X2 - rectangle.TopLeftX + 2, rectangle.Y2 - rectangle.TopLeftY + 2);

            //foreach (int w in HorizontalLines)
            //    e.Graphics.DrawLine(new Pen(Color.Red), 0, w, bitmap.Width, w);

            //foreach (int h in VerticalLines)
            //    e.Graphics.DrawLine(new Pen(Color.Red), h, 0, h, bitmap.Height);

        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            Refresh();
        }
    }
}
