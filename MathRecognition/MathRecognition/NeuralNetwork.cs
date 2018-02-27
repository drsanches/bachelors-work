using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; 
using System.IO;
using System.Windows.Forms;

namespace MathRecognition
{
    public abstract class NeuralNetworkAbstractFactory
    {
        public List<Rectangle> NotRecognized;
        public List<Rectangle> Recognized;
        public NeuralNetworkAbstractFactory() 
        {
            NotRecognized = new List<Rectangle>();
            Recognized = new List<Rectangle>();
        }
        public abstract void RecognizeList(List<Rectangle> notRecognized);
    }

    public class NeuralNetwork : NeuralNetworkAbstractFactory
    {
        private const string PYTHON_SCRIPT_DIRECTORY_PATH = "..\\..\\..\\..\\cnn\\";
        private const string PYTHON_SCRIPT_NAME = "runnable_cnn.py";
        private const string TEMP_DIRECTORY_PATH = "..\\..\\..\\..\\temp\\";

        public NeuralNetwork() : base()
        { }
        public override void RecognizeList(List<Rectangle> notRecognized)
        {
            string[] arrayPaths = createArrayFiles(notRecognized, TEMP_DIRECTORY_PATH);
            string[] results = recognizeOne(arrayPaths);


            for (int i = 0; i < notRecognized.Count; i++)
            {
                if (results[i].Equals("Error"))
                    NotRecognized.Add(notRecognized[i]);
                else
                {
                    Rectangle newRectangle = notRecognized[i];
                    newRectangle.label = results[i];

                    //TODO: Do something with this
                    if (newRectangle.label == "-")
                    {
                        Segmentation segmentation = new Segmentation();
                        if (segmentation.IsSeparableByLines(newRectangle))
                        {
                            newRectangle.label = "";
                            NotRecognized.Add(newRectangle);
                        }
                        else
                            Recognized.Add(newRectangle);
                    }
                    else
                        Recognized.Add(newRectangle);
                }

                deleteArrayFiles(arrayPaths);
            }
        }
        private string[] createArrayFiles(List<Rectangle> rectangles, string tempDirectoryPath)
        {
            string[] arrayPaths = new string[rectangles.Count];

            for (int i = 0; i < rectangles.Count; i++)
            {
                arrayPaths[i] = tempDirectoryPath + Guid.NewGuid().ToString() + ".txt";
                
                using (StreamWriter arrayFile = new StreamWriter(arrayPaths[i]))
                {
                    for (int h = 0; h < rectangles[i].Height; h++)
                    {
                        for (int w = 0; w < rectangles[i].Width; w++)
                            arrayFile.Write(rectangles[i].Array[w, h].ToString() + " ");
                        arrayFile.Write("\n");
                    }
                }
            }

            return arrayPaths;
        }
        private string[] recognizeOne(string[] arrayPaths)
        {
            string arrayPathsArg = "";
            foreach (string arrayPath in arrayPaths)
                arrayPathsArg += "\"" + arrayPath + "\" ";

            Process p = new Process(); 
            p.StartInfo.FileName = "python.exe";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true; // DO NOT TOUCH THIS LINE! NOTHING CANT WORK WITHOUT IT!
            p.StartInfo.UseShellExecute = false; // make sure we can read the output from stdout
            p.StartInfo.Arguments = "\"" + PYTHON_SCRIPT_DIRECTORY_PATH + PYTHON_SCRIPT_NAME + "\" " + arrayPathsArg; 
            p.Start();
            StreamReader s = p.StandardOutput;
            String output = s.ReadToEnd();
            string[] results = output.Split(' '); 
            p.WaitForExit();
            p.Close();

            return results;
        }
        private void deleteArrayFiles(string[] filepaths)
        {
            foreach (string filepath in filepaths)
                File.Delete(filepath);
        }
    }
}
