using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FbPlot
{
    public partial class ChatTypePicker : Form
    {
        private Form1 parent;
        private bool _loading;
        public ChatTypePicker(object frm)
        {
            InitializeComponent();
            parent = (Form1) frm;
            cbName.SelectedIndex = 0;
        }
        
        private void ChatTypePicker_Load(object sender, EventArgs e)
        {
            //int a = 0;
            _loading = true;

            List<string> Types = System.Enum.GetNames(typeof (SeriesChartType)).ToList();
            List<string> resultList = Types.Where(x => x.Contains("Bar")).ToList();
            resultList.Add("Radar");
            resultList.Add("Renko");
            string[] res = Types.Except(resultList).ToArray();
            cbType.DataSource = res;

            _loading = false;
        }
        private SeriesChartType SelectedType()
        {
            return (SeriesChartType)Enum.Parse(typeof(SeriesChartType), cbType.Items[cbType.SelectedIndex].ToString());
        }
        private string SelectedName()
        {
            return cbName.Items[cbName.SelectedIndex].ToString();
        }
        private void GetActiveType()
        {
            SeriesChartType t = parent.GetChartType(SelectedName());
            for (int i = 0; i < cbType.Items.Count; i++)
            {
                if ((SeriesChartType)Enum.Parse(typeof(SeriesChartType), cbType.Items[i].ToString() )== t)
                {
                    cbType.SelectedIndex = i;
                    break;
                }
            }
        }

        private void cbName_SelectedIndexChanged(object sender, EventArgs e)
        {
            GetActiveType();
            SetError(false);
        }

        private void cbType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SeriesChartType saved =  parent.GetChartType(SelectedName());
            SetError(false);
            if (_loading)
            {
                return;
            }
            try
            {
                parent.SetChartType(SelectedName(),SelectedType());
            }
            catch (Exception ex)
            {
                SetError(true, ex.Message);
                parent.SetChartType(SelectedName(), saved);
                GetActiveType();
            }
        }

        private void SetError(bool on,  string message = "")
        {
            labError.Visible = on;
            labError.Tag = message;
        }
        private void ChatTypePicker_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                parent.SetChartException(false);
                this.Hide();
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            parent.SetChartException(false);
            this.Hide();
        }

        private void labError_Click(object sender, EventArgs e)
        {
            MessageBox.Show((string)labError.Tag, "Can't set this type", MessageBoxButtons.OK) ;
        }

        private void ChatTypePicker_Shown(object sender, EventArgs e)
        {
            _loading = true;
            GetActiveType();
            _loading = false;
            parent.SetChartException(true);
        }
    }
}
