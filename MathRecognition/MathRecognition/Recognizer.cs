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
        private SegmentationAbstractFactory segmentation;
        private NeuralNetworkAbstractFactory neuralNetwork;
        private StructuringAbstractFactory structuring;

        public Recognizer(SegmentationAbstractFactory segmentationFactory, 
            NeuralNetworkAbstractFactory neuralNetworkFactory, StructuringAbstractFactory structuringFactory)
        {
            Recognized = new List<Rectangle>();
            NotRecognized = new List<Rectangle>();
            segmentation = segmentationFactory;
            neuralNetwork = neuralNetworkFactory;
            structuring = structuringFactory;
        }
        public string Recognize(Rectangle rectangle)
        {
            NotRecognized = segmentation.MakeSegmentation(rectangle);
            neuralNetwork.RecognizeList(NotRecognized);

            Recognized = new List<Rectangle>(neuralNetwork.Recognized);
            NotRecognized = new List<Rectangle>(neuralNetwork.NotRecognized);
            
            string latexCode = structuring.getLatexCode(neuralNetwork.Recognized, neuralNetwork.NotRecognized, neuralNetwork);

            return latexCode;
        }
    }
}
