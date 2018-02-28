using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MathRecognition
{
    public class Recognizer
    {
        public List<Rectangle> Recognized;
        public List<Rectangle> NotRecognized;
        public List<Rectangle> CanNotBeRecognized;
        private SegmentationAbstractFactory segmentation;
        private NeuralNetworkAbstractFactory neuralNetwork;
        private StructuringAbstractFactory structuring;

        public Recognizer(SegmentationAbstractFactory segmentationFactory, 
            NeuralNetworkAbstractFactory neuralNetworkFactory, StructuringAbstractFactory structuringFactory)
        {
            Recognized = new List<Rectangle>();
            NotRecognized = new List<Rectangle>();
            CanNotBeRecognized = new List<Rectangle>();
            segmentation = segmentationFactory;
            neuralNetwork = neuralNetworkFactory;
            structuring = structuringFactory;
        }
        public string Recognize(Rectangle rectangle)
        {
            NotRecognized.Add(rectangle);

            while (NotRecognized.Count > 0)
            {
                NotRecognized = makeSegmentation(NotRecognized);
                neuralNetwork.RecognizeList(NotRecognized);
                NotRecognized.Clear();

                foreach (Rectangle rect in neuralNetwork.Recognized)
                    whenRecognized(rect);

                foreach (Rectangle rect in neuralNetwork.NotRecognized)
                    NotRecognized = sumLists(NotRecognized, whenIsNotRecognized(rect));

                neuralNetwork.ClearLists();
            }

            string latexCode = structuring.getLatexCode(Recognized, CanNotBeRecognized, neuralNetwork);

            return latexCode;
        }
        private List<Rectangle> makeSegmentation(List<Rectangle> rectangles)
        {
            List<Rectangle> notRecognized = new List<Rectangle>();

            foreach (Rectangle rect in rectangles)
                notRecognized = sumLists(notRecognized, segmentation.MakeSegmentation(rect));

            return notRecognized;
        }
        private void whenRecognized(Rectangle rectangle)
        {
            Recognized.Add(rectangle);
        }
        private List<Rectangle> whenIsNotRecognized(Rectangle rectangle)
        {
            List<Rectangle> notRecognized = new List<Rectangle>();

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
                NotRecognized.Add(rectangle);
            return notRecognized;
        }
        private List<Rectangle> sumLists(List<Rectangle> a, List<Rectangle> b)
        {
            List<Rectangle> s = new List<Rectangle>();
            foreach (Rectangle rect in a)
                s.Add(rect);
            foreach (Rectangle rect in b)
                s.Add(rect);
            return s;
        }
    }
}
