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
    public interface INeuralNetwork
    {
        void RecognizeList(List<Rectangle> notRecognized);
        List<Rectangle> GetRecognizedList();
        List<Rectangle> GetNotRecognizedList();
    }

    public class NeuralNetwork : INeuralNetwork
    {
        private string PYTHON_SCRIPT_FILEPATH = Properties.Resources.PYTHON_SCRIPT_FILEPATH;
        private string TEMP_DIRECTORY_PATH = Properties.Resources.TEMP_DIRECTORY_PATH;
        private int PROCESSES_MAX_NUMBER = int.Parse(Properties.Resources.NEURAL_NETWORK_PROCESSES_MAX_NUMBER);
        private List<Rectangle> RecognizedRectangles;
        private List<Rectangle> NotRecognizedRectangles;
        private static object locker = new object();
        
        public NeuralNetwork() 
        {
            NotRecognizedRectangles = new List<Rectangle>();
            RecognizedRectangles = new List<Rectangle>();
        }
        public void RecognizeList(List<Rectangle> notRecognized)
        {
            ClearLists();
            List<Rectangle>[] lists = cutListOfRectangles(notRecognized);
            Thread[] threads = new Thread[lists.Length];
            
            for (int i = 0; i < threads.Count(); i++)
            {
                threads[i] = new Thread(new ParameterizedThreadStart(threadFunction));
                threads[i].Name = "Thread-" + i.ToString();
                threads[i].Start(lists[i]);
            }

            for (int i = 0; i < threads.Count(); i++)
                threads[i].Join();

            RecognizedRectangles.Sort((a, b) => a.TopLeftX.CompareTo(b.TopLeftX));
        }
        public List<Rectangle> GetRecognizedList()
        {
            return RecognizedRectangles;
        }
        public List<Rectangle> GetNotRecognizedList()
        {
            return NotRecognizedRectangles;
        }
        public void ClearLists()
        {
            RecognizedRectangles.Clear();
            NotRecognizedRectangles.Clear();
        }
        private List<Rectangle>[] cutListOfRectangles(List<Rectangle> notRecognized)
        {
            int newProcessesCount = Math.Min(notRecognized.Count, PROCESSES_MAX_NUMBER);
            
            List<Rectangle>[] lists = new List<Rectangle>[newProcessesCount];
            
            for (int i = 0; i < lists.Length; i++)
                lists[i] = new List<Rectangle>();

            for (int i = 0; i < notRecognized.Count; i++)
                lists[i % PROCESSES_MAX_NUMBER].Add(notRecognized[i]);

            return lists;
        }
        private void threadFunction(Object obj)
        {
            recognizeListForOneProcess((List<Rectangle>)obj);
        }
        private void recognizeListForOneProcess(List<Rectangle> notRecognized)
        {
            string[] arrayPaths = createArrayFiles(notRecognized, TEMP_DIRECTORY_PATH);
            string[] results = recognizeAll(arrayPaths);


            for (int i = 0; i < notRecognized.Count; i++)
            {
                if (results[i].Equals("Error"))
                    NotRecognizedRectangles.Add(notRecognized[i]);
                else
                {
                    Rectangle newRectangle = notRecognized[i];
                    newRectangle.Label = results[i];
                    
                    lock (locker)
                    {
                        RecognizedRectangles.Add(newRectangle);
                    }
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
                try
                {
                    File.Delete(filepath);
                }
                catch { }
        }
    }
}
