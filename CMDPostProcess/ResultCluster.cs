using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDPostProcess
{
    class STData
    {
        public DateTime intimeindex;
        public DateTime outtimeindex;
        public int userid;
        public int cellid;
        public int NumST;
        public string ActiType;

        public STData()
        {

        }

        public STData(DateTime p0, DateTime p1, int p2, int p3,int p5,string p6)
        {
            // TODO: Complete member initialization
            intimeindex = p0;
            outtimeindex = p1;
            userid = p2;
            cellid = p3;
            NumST = p5;
            ActiType = p6;
        }
    }

    class ResultCluster
    {
        //private Double[,,] Result = new Double[9,5952,24];
        private double[,] TimeCategory = new double[24, 9];
        private Dictionary<int, int[]> TowerTime = new Dictionary<int, int[]>();
        private Dictionary<int, int[]> TowerCategory = new Dictionary<int, int[]>();
        private Dictionary<int, int[]> RegionTime = new Dictionary<int, int[]>();
        private Dictionary<int, int[]> RegionCategory = new Dictionary<int, int[]>();

        //初始化

        //private Double[,] ResultPro = new Double[9, 5952];
        private Dictionary<int, List<STData>> ImportData = new Dictionary<int, List<STData>>();
        //private int[] MaxCat = new int[5952];
        //private double[] Sum = new double[5952];
        enum Activity { Shopping, Eating, Transportation, work, Social, home, study, Recreation, Entertainment };  // 隐藏状态（活动）
        int Totalcount = 0;
        int TotalUser = 0;
        //每个用户总活动数，唯一活动数。
        private Dictionary<int, int[]> UserStati = new Dictionary<int, int[]>();
        private Dictionary<int, int> ATStati = new Dictionary<int, int>();
        private Dictionary<int, int> ATStatiDis = new Dictionary<int, int>();
        int UserH = 0;
        int UserW = 0;
        int UserO = 0;

        Dictionary<int, string[]> CellInfo = new Dictionary<int, string[]>();



        public void ReadData()
        {

            //分163个文件读
            DirectoryInfo dir = new System.IO.DirectoryInfo("D:\\201604_CMProcess\\HMMResult\\versionTest5");
            int m = 0;
            foreach (FileInfo fi in dir.GetFiles())
            {
                StreamReader sr = new StreamReader(fi.FullName);
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        //文件顺序：timeindex, userid, stationid，已排序
                        string[] strArr = line.Split('\t');
                        int userid=Convert.ToInt32(strArr[0]);
                        string actitype = Convert.ToString(strArr[1]);
                        DateTime indt = DateTime.ParseExact(strArr[2], "yyyy/M/dd H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime outdt = DateTime.ParseExact(strArr[3], "yyyy/M/dd H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        int numst = Convert.ToInt32(strArr[4]);
                        int cellid = Convert.ToInt32(strArr[5]);

                        if (ImportData.ContainsKey(userid))
                        {
                            ImportData[userid].Add(new STData(indt,outdt, userid, cellid,numst,actitype));
                        }
                        else
                        {
                            ImportData.Add(userid, new List<STData>() { new STData(indt, outdt, userid, cellid, numst, actitype) });
                            TotalUser++;
                        }
                        Totalcount++;
                    }
                    MPDataProcessing();

                    Console.WriteLine(m + "");
                    m++;
                    ImportData.Clear();
            }

        }



        public void MPDataProcessing()
        {
            foreach (var TT in ImportData)
            {
                var q = from w in TT.Value select w.cellid;
                List<int> ActivityTypes = new List<int>();
                if(UserStati.ContainsKey(TT.Key))
                {
                    UserStati[TT.Key][0] = TT.Value.Count();
                    UserStati[TT.Key][1] = q.Distinct().Count();
                }
                else
                {
                    UserStati.Add(TT.Key, new int[2]);
                    UserStati[TT.Key][0] = TT.Value.Count();
                    UserStati[TT.Key][1] = q.Distinct().Count();
                }

                foreach(STData vv in TT.Value)
                {
                    int type=(int)Enum.Parse(typeof(Activity), vv.ActiType, true);
                    ActivityTypes.Add(type);

                    int tower = vv.cellid;
                    int RegionId = Convert.ToInt32(CellInfo[tower][2]);
                    int intimestamp = vv.intimeindex.Hour;
                    int outtimestamp = vv.outtimeindex.Hour;
                    //int timestamp = (vv.intimeindex.Hour + vv.outtimeindex.Hour) > 24 ? (int)(vv.intimeindex.Hour + vv.outtimeindex.Hour - 24) / 2 : (int)(vv.intimeindex.Hour + vv.outtimeindex.Hour) / 2;
                    //前一晚23点的问题。
                    int starttime;
                    if (intimestamp == 23)
                    {
                        TimeCategory[23, type]++;
                        if (TowerTime.ContainsKey(tower))
                        {
                            TowerTime[tower][23]++;
                        }
                        else
                        {
                            TowerTime.Add(tower, new int[24]);
                            TowerTime[tower][23]++;
                        }
                        if (RegionTime.ContainsKey(RegionId))
                        {
                            RegionTime[RegionId][23]++;
                        }
                        else
                        {
                            RegionTime.Add(RegionId, new int[24]);
                            RegionTime[RegionId][23]++;
                        }
                        starttime = 0;

                    }
                    else
                    {
                        starttime = intimestamp;
                    }

                    for (int timestamp = starttime; timestamp <= outtimestamp; timestamp++)
                    {
                        TimeCategory[timestamp, type]++;
                        if(TowerTime.ContainsKey(tower))
                        {
                            TowerTime[tower][timestamp]++;
                        }
                        else
                        {
                            TowerTime.Add(tower, new int[24]);
                            TowerTime[tower][timestamp]++;
                        }

                        if (RegionTime.ContainsKey(RegionId))
                        {
                            RegionTime[RegionId][timestamp]++;
                        }
                        else
                        {
                            RegionTime.Add(RegionId, new int[24]);
                            RegionTime[RegionId][timestamp]++;
                        }
                    }

                    if (TowerCategory.ContainsKey(tower))
                    {
                        TowerCategory[tower][type]++;
                    }
                    else
                    {
                        TowerCategory.Add(tower, new int[9]);
                        TowerCategory[tower][type]++;
                    }

                    if (RegionCategory.ContainsKey(RegionId))
                    {
                        RegionCategory[RegionId][type]++;
                    }
                    else
                    {
                        RegionCategory.Add(RegionId, new int[9]);
                        RegionCategory[RegionId][type]++;
                    }
                }

                if (ActivityTypes.Contains(5))
                    UserH++;
                if (ActivityTypes.Contains(3))
                    UserW++;
                if (!ActivityTypes.Contains(5) && !ActivityTypes.Contains(3))
                    UserO++;
            }
           // Console.WriteLine("aa");
        }


        //public void FindMax()
        //{
        //    Double[, ] TempResult;
        //    //每一个位置有个特征向量，找到最大向量值
        //    for (int i = 0; i < 5952; i++)
        //    {
        //        TempResult = new Double[8, 5952];
        //        for (int j = 0; j < 8; j++)
        //        {
        //            for (int m = 0; m < 24; m++)
        //            {
        //                Sum[i] += Result[j, i, m];
        //                TempResult[j, i] += Result[j, i, m];
        //            }
        //        }


        //        for (int j = 0; j < 8; j++)
        //        {
        //            if(Sum[i]!=0)
        //                ResultPro[j, i] = TempResult[j, i] / Sum[i];
        //        }

        //        MaxCat[i]=0;
        //        double MaxValue = ResultPro[0, i];
        //        for (int j = 1; j < 8; j++)
        //        {
        //            if (ResultPro[j, i] > MaxValue)
        //            {
        //                MaxCat[i]=j;
        //                MaxValue = ResultPro[j, i];
        //            }
        //        }

        //        ///if(Sum[i]<100||MaxValue<0.55)
        //        //{
        //            //MaxCat[i] = -1;
        //        //}
        //    }
        //}

        public void APStatistic()
        {
            //分别输出每个用户的数据列表,统计概率再输出到一个表
            foreach (var st in UserStati)
            {
                //字段：userid，活动点个数，distinct点个数
                //swtti.WriteLine("{0}\t{1}", st.Key, st.Value);
                if (ATStati.ContainsKey(st.Value[0]))
                {
                    ATStati[st.Value[0]]++;
                }
                else
                {
                    ATStati.Add(st.Value[0], 1);
                }

                if (ATStatiDis.ContainsKey(st.Value[1]))
                {
                    ATStatiDis[st.Value[1]]++;
                }
                else
                {
                    ATStatiDis.Add(st.Value[1], 1);
                }
            }
        }

        public void WriteData()
        {
            string directoryPath = @"D:\\201604_CMProcess\\HMMstatistical\\versionTest5";//定义一个路径变量            

            if (!Directory.Exists(directoryPath))//如果路径不存在
            {
                Directory.CreateDirectory(directoryPath);//创建一个路径的文件夹
            }
            //TimeCategory
            StreamWriter swTC=new StreamWriter(Path.Combine(directoryPath, "TimeCategory.txt")); ;
            for (int i = 0; i < 24; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    swTC.Write("{0}\t", TimeCategory[i,j]);
                }
                swTC.Write("\r\n");
            }
            //TowerTime
            StreamWriter swTT = new StreamWriter(Path.Combine(directoryPath, "TowerTime.txt")); ;
            foreach(var TT in TowerTime)
            {
                swTT.Write("{0}\t{1}\t{2}\t{3}\t{4}\t", TT.Key, CellInfo[TT.Key][0], CellInfo[TT.Key][1], CellInfo[TT.Key][2], CellInfo[TT.Key][3]);
                for (int j = 0; j < 24; j++)
                {
                    swTT.Write("{0}\t", TT.Value[j]);
                }
                swTT.Write("\r\n");
            }
            //TowerCategory
            StreamWriter swTtC = new StreamWriter(Path.Combine(directoryPath, "TowerCategory.txt")); ;
            foreach (var TT in TowerCategory)
            {
                swTtC.Write("{0}\t{1}\t{2}\t{3}\t{4}\t", TT.Key, CellInfo[TT.Key][0], CellInfo[TT.Key][1], CellInfo[TT.Key][2], CellInfo[TT.Key][3]);
                for (int j = 0; j < 9; j++)
                {
                    swTtC.Write("{0}\t", TT.Value[j]);
                }
                swTtC.Write("\r\n");
            }
            //RegionTime
            StreamWriter swRT = new StreamWriter(Path.Combine(directoryPath, "RegionTime.txt")); ;
            foreach (var TT in RegionTime)
            {
                swRT.Write("{0}\t", TT.Key);
                for (int j = 0; j < 24; j++)
                {
                    swRT.Write("{0}\t", TT.Value[j]);
                }
                swRT.Write("\r\n");
            }
            //RegionCategory
            StreamWriter swRC = new StreamWriter(Path.Combine(directoryPath, "RegionCategory.txt")); ;
            foreach (var TT in RegionCategory)
            {
                swRC.Write("{0}\t", TT.Key);
                for (int j = 0; j < 9; j++)
                {
                    swRC.Write("{0}\t", TT.Value[j]);
                }
                swRC.Write("\r\n");
            }

            StreamWriter swLog = new StreamWriter(Path.Combine(directoryPath, "Log.txt")); ;
            swLog.WriteLine(TotalUser+"\t"+UserH + "\t" + UserW + "\t" + UserO);

            //活动点，唯一活动点个数统计
            StreamWriter swAT = new StreamWriter(Path.Combine(directoryPath, "ActivityPointCount.txt")); ;
            foreach (var mm in ATStati)
            {
                swAT.WriteLine("{0}\t{1}", mm.Key, mm.Value);
            }

            StreamWriter swATD = new StreamWriter(Path.Combine(directoryPath, "DistinctActivityPointCount.txt")); ;
            foreach (var mm in ATStatiDis)
            {
                swATD.WriteLine("{0}\t{1}", mm.Key, mm.Value);
            }

            swLog.Flush();
            swLog.Close();
            swRC.Flush();
            swRC.Close();
            swRT.Flush();
            swRT.Close();
            swTC.Flush();
            swTC.Close();
            swTT.Flush();
            swTT.Close();
            swTtC.Flush();
            swTtC.Close();
            swAT.Flush();
            swAT.Close();
            swATD.Flush();
            swATD.Close();
        }


        public void CellIdInfoInput()
        {
            //基站id数据
            try
            {

                using (StreamReader sr = new StreamReader("D:\\201604_CMProcess\\GridCellInfo.txt"))
                {

                    String line;
                    int i = 0;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] strArr = line.Split('\t');
                        //字段：lat，lon，regionid，regionname
                        String[] LatLon = new string[] { strArr[2], strArr[3], strArr[5], strArr[4] };

                        CellInfo.Add(Convert.ToInt32(strArr[0]), LatLon);
                        i++;
                    }
                    Console.WriteLine(i + "  cell id has been read");

                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
    }
}
