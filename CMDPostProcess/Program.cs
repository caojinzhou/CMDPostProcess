﻿using System;
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
            ResultCluster process = new ResultCluster();
            process.StationIdInfoInput();
            process.ReadData();
            //process.MPDataProcessing();
            //process.FindMax();
            process.APStatistic();
            process.WriteData();
        }
    }
}