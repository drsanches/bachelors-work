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
            foreach (Rectangle rect in notRecognized)
            {
                //For normal work
                string arrayPath = createArrayFile(rect, TEMP_DIRECTORY_PATH);
                string result = recognizeOne(rect, arrayPath);

                //For debug
                //string result = recognizeOne(rectangle, "..\\..\\..\\..\\cnn\\tools\\1.txt");

                if (result.Equals("Error"))
                    NotRecognized.Add(rect);
                else
                {
                    Rectangle newRectangle = rect;
                    newRectangle.label = result;
                    Recognized.Add(newRectangle);
                }

                deleteArrayFile(arrayPath);
            }
        }
        private string createArrayFile(Rectangle rectangle, string tempDirectoryPath)
        {
            string arrayPath = tempDirectoryPath + Guid.NewGuid().ToString() + ".txt";

            using (StreamWriter arrayFile = new StreamWriter(arrayPath))
            {
                for (int h = 0; h < rectangle.Height; h++)
                {
                    for (int w = 0; w < rectangle.Width; w++)
                        arrayFile.Write(rectangle.Array[w, h].ToString() + " ");
                    arrayFile.Write("\n");
                }
            }
            return arrayPath;
        }
        private string recognizeOne(Rectangle rect, string arrayPath)
        {
            Process p = new Process(); 
            p.StartInfo.FileName = "python.exe";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true; // DO NOT TOUCH THIS LINE! NOTHING CANT WORK WITHOUT IT!
            p.StartInfo.UseShellExecute = false; // make sure we can read the output from stdout
            p.StartInfo.Arguments = "\"" + PYTHON_SCRIPT_DIRECTORY_PATH + PYTHON_SCRIPT_NAME + "\" \"" + arrayPath + "\""; 
            p.Start();
            StreamReader s = p.StandardOutput;
            String output = s.ReadToEnd();
            string[] results = output.Split(new char[] { ' ' }); 
            p.WaitForExit();
            p.Close();

            return results[0];
        }
        private void deleteArrayFile(string filepath)
        {
            File.Delete(filepath);
        }
    }
}
