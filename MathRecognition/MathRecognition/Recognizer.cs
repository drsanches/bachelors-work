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
            NotRecognized = segmentation.MakeFullSegmentation(rectangle);
            neuralNetwork.RecognizeList(NotRecognized);

            Recognized = neuralNetwork.Recognized;
            CanNotBeRecognized = neuralNetwork.NotRecognized;
            
            string latexCode = structuring.getLatexCode(Recognized, CanNotBeRecognized, neuralNetwork);

            return latexCode;
        }
    }
}
