using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics; 
using System.IO;
using System.Windows.Forms;
using System.Threading;

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
        public void ClearLists()
        {
            Recognized.Clear();
            NotRecognized.Clear();
        }
    }

    public class NeuralNetwork : NeuralNetworkAbstractFactory
    {
        private string PYTHON_SCRIPT_FILEPATH = Properties.Resources.PYTHON_SCRIPT_FILEPATH;
        private string TEMP_DIRECTORY_PATH = Properties.Resources.TEMP_DIRECTORY_PATH;
        //TODO: Here may be multithreading bug caused by adding into one list.
        public int processesCount = 4;

        public NeuralNetwork() : base()
        { }
        public override void RecognizeList(List<Rectangle> notRecognized)
        {
            List<Rectangle>[] lists = cutListOfRectangles(notRecognized);
            Thread[] threads = new Thread[lists.Length];
            
            for (int i = 0; i < processesCount; i++)
            {
                threads[i] = new Thread(new ParameterizedThreadStart(threadFunction));
                threads[i].Name = "Thread-" + i.ToString();
                threads[i].Start(lists[i]);
            }

            for (int i = 0; i < processesCount; i++)
                threads[i].Join();

            Recognized.Sort((a, b) => a.TopLeftX.CompareTo(b.TopLeftX));
        }
        private void threadFunction(Object obj)
        {
            RecognizeListForOneProcess((List<Rectangle>)obj);
        }
        private List<Rectangle>[] cutListOfRectangles(List<Rectangle> notRecognized)
        {
            List<Rectangle>[] lists = new List<Rectangle>[processesCount];
            
            for (int i = 0; i < lists.Length; i++)
                lists[i] = new List<Rectangle>();

            for (int i = 0; i < notRecognized.Count; i++)
                lists[i % processesCount].Add(notRecognized[i]);

            return lists;
        }
        private void RecognizeListForOneProcess(List<Rectangle> notRecognized)
        {
            string[] arrayPaths = createArrayFiles(notRecognized, TEMP_DIRECTORY_PATH);
            string[] results = recognizeAll(arrayPaths);


            for (int i = 0; i < notRecognized.Count; i++)
            {
                if (results[i].Equals("Error"))
                    NotRecognized.Add(notRecognized[i]);
                else
                {
                    Rectangle newRectangle = notRecognized[i];
                    newRectangle.label = results[i];
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
        private string[] recognizeAll(string[] arrayPaths)
        {
            string arrayPathsArg = "";
            foreach (string arrayPath in arrayPaths)
                arrayPathsArg += "\"" + arrayPath + "\" ";

            Process p = new Process(); 
            p.StartInfo.FileName = "python.exe";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true; //DO NOT TOUCH THIS LINE! NOTHING CANT WORK WITHOUT IT!
            p.StartInfo.UseShellExecute = false; //Make sure we can read the output from stdout
            p.StartInfo.Arguments = "\"" + PYTHON_SCRIPT_FILEPATH + "\" " + arrayPathsArg; 
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
