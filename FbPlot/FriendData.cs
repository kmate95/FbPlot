using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FbPlot
{
    public class FriendData
    {
        public FriendData(ComboboxItem data, string logpath)
        {
            userid = long.Parse((string)data.Value);
            name = data.Text;
            LoadFromFile(logpath);
        }



        public long userid;
        public string name;
        public  List<DateTime> times;
        public List<int> states;
        public List<JsonLine> jsons;
        public int starttime;
        



        private void LoadFromFile(string logpath)
        {
            string filename = logpath + "\\" + userid.ToString() + ".txt";
            int counter = 0;
            
            // Read the file and display it line by line.
            
            string[] lines = File.ReadAllLines(filename);
            times=new List<DateTime>(lines.Count());
            states = new List<int>(lines.Count());
            jsons = new List<JsonLine>();
            string[] d;

            for (int i = 0; i < lines.Count(); i++)
            {
                try
                {
                     d = lines[i].Split('|');
                    if (d[0].Length < 5)
                    {
                        continue;
                    }
                    double unix = double.Parse(d[0], CultureInfo.InvariantCulture);
                    times.Add( UnixTimeStampToDateTime(unix));
                    if (d[1].Length == 1)
                    {
                        states.Add(int.Parse(d[1]));
                    }
                    else
                    {
                        jsons.Add(JsonLine.ParseStrangeLine(d[1]));
                        states.Add(-1);
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            

            //starttime = times.Min().;
        }
        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
