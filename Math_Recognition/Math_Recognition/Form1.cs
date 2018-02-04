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
        const string filename = "..\\..\\..\\..\\Formulas\\digits.png";
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

            foreach (Rectangle r in recognizer.Recognized)
            {
                e.Graphics.DrawString(r.label, new Font("Arial", 16), new SolidBrush(Color.Red), r.TopLeftX, r.TopLeftY);
            }
        }

        private void Form1_Activated(object sender, EventArgs e)
        {
            Refresh();
        }
    }
}
