using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMDPostProcess
{
    class STPatternStract
    {
        private Dictionary<int, List<STData>> ImportData = new Dictionary<int, List<STData>>();
        int Totalcount = 0;
        int TotalUser = 0;
        Dictionary<int, string[]> CellInfo = new Dictionary<int, string[]>();
        enum Activity { Shopping, Eating, Transportation, work, Social, home, study, Recreation, Entertainment };  // 隐藏状态（活动）
        List<Dictionary<int, int[]>> CategoryTimeTower = new List<Dictionary<int, int[]>>();


        public void ReadData()
        {

            //分163个文件读
            DirectoryInfo dir = new System.IO.DirectoryInfo("D:\\201604_CMProcess\\HMMResult\\versionTest9");
            int m = 0;
            foreach (FileInfo fi in dir.GetFiles())
            {
                StreamReader sr = new StreamReader(fi.FullName);
                String line;
                while ((line = sr.ReadLine()) != null)
                {
                    //文件顺序：timeindex, userid, stationid，已排序
                    string[] strArr = line.Split('\t');
                    int userid = Convert.ToInt32(strArr[0]);
                    string actitype = Convert.ToString(strArr[1]);
                    DateTime indt = DateTime.ParseExact(strArr[2], "yyyy/M/dd H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime outdt = DateTime.ParseExact(strArr[3], "yyyy/M/dd H:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    int numst = Convert.ToInt32(strArr[4]);
                    int cellid = Convert.ToInt32(strArr[5]);

                    if (ImportData.ContainsKey(userid))
                    {
                        ImportData[userid].Add(new STData(indt, outdt, userid, cellid, numst, actitype));
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
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int[]> temp = new Dictionary<int, int[]>();
                CategoryTimeTower.Add(temp);
            }

            foreach (var TT in ImportData)
            {
                var q = from w in TT.Value select w.cellid;

                foreach (STData vv in TT.Value)
                {
                    int type = (int)Enum.Parse(typeof(Activity), vv.ActiType, true);

                    int tower = vv.cellid;
                    int RegionId = Convert.ToInt32(CellInfo[tower][2]);
                    int intimestamp = vv.intimeindex.Hour;
                    int outtimestamp = vv.outtimeindex.Hour;
                    //int timestamp = (vv.intimeindex.Hour + vv.outtimeindex.Hour) > 24 ? (int)(vv.intimeindex.Hour + vv.outtimeindex.Hour - 24) / 2 : (int)(vv.intimeindex.Hour + vv.outtimeindex.Hour) / 2;
                    //前一晚23点的问题。
                    int starttime;
                    if (intimestamp == 23)
                    {

                        if(CategoryTimeTower[type].ContainsKey(tower))
                        {
                            CategoryTimeTower[type][tower][23]++;
                        }
                        else
                        {
                            CategoryTimeTower[type].Add(tower, new int[24]);
                            CategoryTimeTower[type][tower][23]++;
                        }
                        starttime = 0;

                    }
                    else
                    {
                        starttime = intimestamp;
                    }

                    for (int timestamp = starttime; timestamp <= outtimestamp; timestamp++)
                    {

                        if (CategoryTimeTower[type].ContainsKey(tower))
                        {
                            CategoryTimeTower[type][tower][timestamp]++;
                        }
                        else
                        {
                            CategoryTimeTower[type].Add(tower, new int[24]);
                            CategoryTimeTower[type][tower][timestamp]++;
                        }
                    }
                }

            }
            // Console.WriteLine("aa");
        }

        public void WriteData()
        {
            string directoryPath = @"D:\\201604_CMProcess\\HMMstatistical\\versionTest9\\CategoryTimeTower";//定义一个路径变量            

            if (!Directory.Exists(directoryPath))//如果路径不存在
            {
                Directory.CreateDirectory(directoryPath);//创建一个路径的文件夹
            }

            //CategoryTowerTime
            for(int i=0;i< 9;i++)
            {
                string activity = Enum.GetName(typeof(Activity), i);
                StreamWriter swCTT = new StreamWriter(Path.Combine(directoryPath, activity+"TowerTime.txt"));
                StreamWriter swCTT2 = new StreamWriter(Path.Combine(directoryPath, activity + "TowerTimeall.txt"));

                foreach (var TT in CategoryTimeTower[i])
                {
                    swCTT.Write("{0}\t{1}\t{2}\t{3}\t{4}\t", TT.Key, CellInfo[TT.Key][0], CellInfo[TT.Key][1], CellInfo[TT.Key][2], CellInfo[TT.Key][3]);
                    for (int j = 0; j < 24; j++)
                    {
                        swCTT.Write("{0}\t", TT.Value[j]);
                        swCTT2.Write("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", TT.Key, CellInfo[TT.Key][0], CellInfo[TT.Key][1], CellInfo[TT.Key][2], CellInfo[TT.Key][3],j, TT.Value[j]);
                        swCTT2.Write("\r\n");
                    }
                    swCTT.Write("\r\n");
                }

                swCTT.Flush();
                swCTT.Close();
                swCTT2.Flush();
                swCTT2.Close();
            }
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
