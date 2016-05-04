using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace FbPlot
{


    public partial class Form1 : Form
    {
        private string _logfolder;
        private ChatTypePicker _chartSetter;
        public string LogFolder
        {
            get { return _logfolder; }
            set
            {
                _logfolder = value;
                FillNamesCombo(LogFolder + "\\" + _namesfile);
                tbLogFolder.Text = LogFolder;
            }
        }

        private string _namesfile = "idlookup.txt";

        private FriendData _activefriend;

        public FriendData Activefriend
        {
            get { return _activefriend; }
            set
            {
                _activefriend = value;
                Properties.Settings.Default.LogPath = _logfolder;
                RefreshGraph();
            }
        }

        public Form1()
        {
            InitializeComponent();
            //FillNamesCombo(LogFolder+"\\"+namesfile);
            LogFolder = Properties.Settings.Default.LogPath;
            _chartSetter = new ChatTypePicker(this);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            LogFolder = tbLogFolder.Text;
        }

        private void FillNamesCombo(string filename)
        {
            cbNames.Items.Clear();
            string line;
            // Read the file and display it line by line.
            try
            {
                System.IO.StreamReader file =
                    new System.IO.StreamReader(filename);
                while ((line = file.ReadLine()) != null)
                {
                    string[] d = line.Split('=');
                    ComboboxItem i = new ComboboxItem
                    {
                        Text = d[1],
                        Value = d[0]
                    };
                    cbNames.Items.Add(i);
                }

                file.Close();
            }
            catch (Exception ex)
            {

            }
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            DialogResult res = openFileDialog1.ShowDialog();

            if (res == DialogResult.OK)
            {
                LogFolder = Path.GetDirectoryName(openFileDialog1.FileName);
                //FillNamesCombo(LogFolder + "\\" + namesfile);
            }
        }

        private void cbNames_SelectedIndexChanged(object sender, EventArgs e)
        {
            Activefriend = new FriendData((ComboboxItem) cbNames.Items[cbNames.SelectedIndex], LogFolder);
        }

        private void RefreshGraph()
        {
            chart1.Series["status"].Points.Clear();
            chart1.Series["vc"].Points.Clear();
            chart1.Series["a"].Points.Clear();

            chart1.ChartAreas[0].AxisX.Minimum = Activefriend.times[0].ToOADate();
            chart1.ChartAreas[0].AxisX.Maximum = Activefriend.times[Activefriend.times.Count() - 1].ToOADate();

            //chart1.Series["status"].XValueType = ChartValueType.DateTime;
            ////chart1.Series[0].XValueType = ChartValueType.DateTime;
            chart1.ChartAreas[0].AxisX.LabelStyle.Format = "MM-dd HH:mm";
            chart1.ChartAreas[0].AxisX.ScaleView.MinSize = 0.0001;
            //chart1.ChartAreas[0].AxisX.Interval = 1;
            chart1.ChartAreas[0].AxisX.IntervalType = DateTimeIntervalType.Minutes;
            //chart1.ChartAreas[0].AxisX.IntervalOffset = 1;

            for (int i = 0; i < Activefriend.times.Count(); i++)
            {
                chart1.Series["status"].Points.AddXY(Activefriend.times[i], Activefriend.states[i]);
            }
            for (int i = 0; i < Activefriend.jsons.Count; i++)
            {
                DateTime t = FriendData.UnixTimeStampToDateTime(Activefriend.jsons[i].la);
                chart1.Series["vc"].Points.AddXY(t, Activefriend.jsons[i].vc/2);
                chart1.Series["a"].Points.AddXY(t, Activefriend.jsons[i].a);
            }


        }

        private void chart1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs) e;
            if (me.Button == MouseButtons.Right)
            {
                chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(0);
                chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(0);
            }
            //if (me.Button == MouseButtons.Left)
            //{
            //    Point mousePoint = new Point(me.X, me.Y);

            //    chart1.ChartAreas[0].CursorX.SetCursorPixelPosition(mousePoint, true);
            //    chart1.ChartAreas[0].CursorY.SetCursorPixelPosition(mousePoint, true);
            //}
        }

        public SeriesChartType GetChartType(string ser)
        {
            return chart1.Series[ser].ChartType;
        }

        public void SetChartType(string ser, SeriesChartType typ)
        {
            SeriesChartType saved = chart1.Series[ser].ChartType;
            try
            {
                chart1.Series[ser].ChartType = typ;
                chart1.Refresh();
            }
            catch (Exception ex)
            {
                chart1.Series[ser].ChartType = saved;
                //throw;
            }
        }

        public void SetChartException(bool suppress)
        {
            chart1.SuppressExceptions = suppress;
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _chartSetter.Show();
        }

        private Point? _prevPosition = null;
        private ToolTip _tooltip = new ToolTip();

        public static double PointDist(Point p1, Point p2)
        {
            int dx = p1.X - p2.X;
            int dy = p1.Y - p2.Y;

            double distance = Math.Sqrt(dx * dx + dy * dy);

            return (int)Math.Round(distance, MidpointRounding.AwayFromZero);
        }
        void chart1_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.Location;
            if (_prevPosition.HasValue && PointDist(pos , _prevPosition.Value) < 5)
                return;
            _tooltip.RemoveAll();
            _prevPosition = pos;
            var results = chart1.HitTest(pos.X, pos.Y, false,
                                            ChartElementType.DataPoint);
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {   
                        
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (2 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 3 &&  Math.Abs(pos.Y - pointYPixel) < 3)
                        {
                            _tooltip.Show("Time=" + DateTime.FromOADate(prop.XValue) + "\n value=" + prop.YValues[0]  , this.chart1,
                                            pos.X, pos.Y - 15);
                        }
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }
    }

    public class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }


}

