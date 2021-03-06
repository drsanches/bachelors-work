﻿using System;
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
        private const string DIRECTORY_PATH = "..\\..\\..\\..\\Formulas\\";
        //TODO: fix difficult_stuff.png
        private const string FILENAME = "..\\..\\VKR\\images\\tests\\fail-04.png";
        //private const string FILENAME = "composite_operators.png";
        private const string SYMBOLS_FILENAME = "..\\..\\..\\..\\dataset\\Symbols.json";
        Bitmap bitmap;
        Graphics g;
        Recognizer recognizer;
        string LatexCode = "";

        public Form1()
        {
            InitializeComponent();

            bitmap = new Bitmap(@DIRECTORY_PATH + FILENAME);
            g = Graphics.FromImage(bitmap);
            
            int[,] array = new int[bitmap.Width, bitmap.Height];
            for (int i = 0; i < bitmap.Width; i++)
                for (int j = 0; j < bitmap.Height; j++)
                    if (bitmap.GetPixel(i, j).GetBrightness() < 1)
                    {
                        float a = bitmap.GetPixel(i, j).GetBrightness();
                        array[i, j] = 1; 
                    }
                    else
                        array[i, j] = 0;

            Segmentation segmentation = new Segmentation();
            NeuralNetwork neuralNetworkAdapter = new NeuralNetwork();
            Structuring structuring = new Structuring(SYMBOLS_FILENAME, StructuringDelegateFactory.Create(SYMBOLS_FILENAME));
            
            recognizer = new Recognizer(segmentation, neuralNetworkAdapter, structuring);
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
                e.Graphics.DrawRectangle(new Pen(Color.Yellow), r.TopLeftX, r.TopLeftY, r.Width, r.Height);

            foreach (Rectangle r in recognizer.Recognized)
                e.Graphics.DrawString(r.Label, new Font("Arial", 16), new SolidBrush(Color.Red), r.TopLeftX, r.TopLeftY);
        }
        private void Form1_Activated(object sender, EventArgs e)
        {
            Refresh();
        }
    }
}
