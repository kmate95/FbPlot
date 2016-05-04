using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace FbPlot
{
    public class JsonLine
    {
        public int a;
        public string s;
        public int vc;
        public long la;
    public static JsonLine ParseStrangeLine(string line)
    {
        return JsonConvert.DeserializeObject<JsonLine>(line);
    }
    }

}
