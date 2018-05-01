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
        private ISegmentation segmentation;
        private INeuralNetwork neuralNetwork;
        private IStructuring structuring;

        public Recognizer(ISegmentation segmentationStrategy, INeuralNetwork neuralNetworkStrategy, IStructuring structuringStrategy)
        {
            Recognized = new List<Rectangle>();
            NotRecognized = new List<Rectangle>();
            segmentation = segmentationStrategy;
            neuralNetwork = neuralNetworkStrategy;
            structuring = structuringStrategy;
        }
        public string Recognize(Rectangle rectangle)
        {
            NotRecognized = segmentation.MakeSegmentation(rectangle);
            neuralNetwork.RecognizeList(NotRecognized);

            Recognized = new List<Rectangle>(neuralNetwork.GetRecognizedList());
            NotRecognized = new List<Rectangle>(neuralNetwork.GetNotRecognizedList());
            
            string latexCode = structuring.GetLatexCode(neuralNetwork.GetRecognizedList(), neuralNetwork.GetNotRecognizedList(), neuralNetwork);

            return latexCode;
        }
    }
}
