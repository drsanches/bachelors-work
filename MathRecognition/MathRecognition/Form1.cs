using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MathRecognition
{
    public partial class Form1 : Form
    {
        const string filename = "..\\..\\..\\..\\Formulas\\symbol_types4.png";
        private const string SYMBOLS_FILENAME = "..\\..\\..\\..\\dataset\\Symbols.json";
        Bitmap bitmap;
        Graphics g;
        Recognizer recognizer;
        string LatexCode = "";

        public Form1()
        {
            InitializeComponent();

            bitmap = new Bitmap(@filename);
            g = Graphics.FromImage(bitmap);

            int[,] array = new int[bitmap.Width, bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
            for (int j = 0; j < bitmap.Height; j++)
            {
                if ((int)bitmap.GetPixel(i, j).GetBrightness() <= 0.2)
                    array[i, j] = 1;
            }

            Segmentation segmentation = new Segmentation();
            NeuralNetwork cnn = new NeuralNetwork();
            Structuring structuring = new Structuring(SYMBOLS_FILENAME, StructuringDelegateCreator.CreateDelegate(SYMBOLS_FILENAME));
            
            recognizer = new Recognizer(segmentation, cnn, structuring);
            LatexCode = recognizer.Recognize(new Rectangle(0, 0, bitmap.Width - 1, bitmap.Height - 1, array, 0, 0));
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bitmap, 0, 0);

            textBox1.SetBounds(0, bitmap.Height, bitmap.Width, textBox1.Height);
            textBox1.Text = LatexCode;

            this.Height = bitmap.Height + 30 + textBox1.Height;
            this.Width = bitmap.Width + 5;
            

            foreach (Rectangle r in recognizer.Recognized)
            {
                e.Graphics.DrawRectangle(new Pen(Color.Green), r.TopLeftX, r.TopLeftY, r.Width, r.Height);
                //e.Graphics.DrawEllipse(new Pen(Color.Red, 2), r.GetCentrePoint().X - 1, r.GetCentrePoint().Y - 1, 2, 2);
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
