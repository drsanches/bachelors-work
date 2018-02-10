using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MathRecognition
{
    public delegate void StructuringDelegate(ref List<List<Symbol>> baselines);

    public static class StructuringDelegateCreator
    {
        public static StructuringDelegate CreateDelegate()
        {
            StructuringDelegate structuringDelegate = doNothing;

            structuringDelegate += checkAllDotsForIJ;
            structuringDelegate += checkAllLines;

            return structuringDelegate;
        }
        private static void doNothing(ref List<List<Symbol>> baselines)
        { }
        //TODO: Write code of functions
        private static void checkAllDotsForIJ(ref List<List<Symbol>> baselines)
        {
            MessageBox.Show("Check all dots");
        }
        private static void checkAllLines(ref List<List<Symbol>> baselines)
        { 
            //Equal, minus, \frac
            MessageBox.Show("Check all lines");
        }
    }
}
