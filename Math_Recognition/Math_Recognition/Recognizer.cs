using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Math_Recognition
{
    public class Recognizer
    {
        public List<Rectangle> Recognized;
        public List<Rectangle> NotRecognized;
        public List<Rectangle> CanNotBeRecognized;

        public Recognizer(string filename)
        {
            Recognized = new List<Rectangle>();
            NotRecognized = new List<Rectangle>();
            CanNotBeRecognized = new List<Rectangle>();
        }
        public void Recognize(Rectangle rectangle)
        {
            NotRecognized.Add(rectangle);

            while (NotRecognized.Count > 0)
            {
                NotRecognized = MakeSegmentation(NotRecognized);

                NeuralNetwork cnn = new NeuralNetwork();
                cnn.RecognizeList(NotRecognized);
                
                NotRecognized.Clear();

                foreach (Rectangle rect in cnn.Recognized)
                    WhenRecognized(rect);

                foreach (Rectangle rect in cnn.NotRecognized)
                    NotRecognized = SumLists(NotRecognized, WhenIsNotRecognized(rect));
            } 
        }
        private List<Rectangle> SumLists(List<Rectangle> a, List<Rectangle> b)
        {
            List<Rectangle> s = new List<Rectangle>();
            foreach (Rectangle rect in a)
                s.Add(rect);
            foreach (Rectangle rect in b)
                s.Add(rect);
            return s;
        }
        public List<Rectangle> MakeSegmentation(List<Rectangle> rectangles)
        {
            Segmentation segmentation = new Segmentation();
            List<Rectangle> notRecognized = new List<Rectangle>();

            foreach (Rectangle rect in rectangles)
                notRecognized = SumLists(notRecognized, segmentation.MakeSegmentation(rect));

            return notRecognized;
        }
        public void WhenRecognized(Rectangle rectangle)
        {
            Recognized.Add(rectangle);
        }
        public List<Rectangle> WhenIsNotRecognized(Rectangle rectangle)
        {
            List<Rectangle> notRecognized = new List<Rectangle>();
            Segmentation segmentation = new Segmentation();

            if (!segmentation.IsSeparableByLines(rectangle))
            {
                if (segmentation.IsSeparableByWave(rectangle))
                {
                    List<Rectangle> partsFromWave = segmentation.WaveSegmentation(rectangle);
                    foreach (Rectangle pfw in partsFromWave)
                        notRecognized.Add(pfw);
                }
                else
                    CanNotBeRecognized.Add(rectangle);
            }
            else
                CanNotBeRecognized.Add(rectangle);
            return notRecognized;
        }
    }
}
