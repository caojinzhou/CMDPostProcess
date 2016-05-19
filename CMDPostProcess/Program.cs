using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDPostProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            //ResultCluster process = new ResultCluster();
            //process.CellIdInfoInput();
            //process.ReadData();

            ////process.FindMax();
            //process.APStatistic();
            //process.WriteData();

            STPatternStract process = new STPatternStract();
            process.CellIdInfoInput();
            process.ReadData();
            process.WriteData();
        }
    }
}
