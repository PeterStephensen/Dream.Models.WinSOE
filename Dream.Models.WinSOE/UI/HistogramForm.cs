using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dream.Models.WinSOE.UI
{
    public partial class HistogramForm : Form
    {
        #region Private fields
        ScottPlot.FormsPlot[] _formsPlot;
        #endregion


        public HistogramForm()
        {
            InitializeComponent();

            this.KeyPreview = true;

            labelMicroDataPeriod.Text = "";

            Paint += new PaintEventHandler(histograms_Paint);

            _formsPlot = new ScottPlot.FormsPlot[9];
            for (int i = 0; i < _formsPlot.Length; i++)
            {
                _formsPlot[i] = new ScottPlot.FormsPlot();
                _formsPlot[i].Hide();
                Controls.Add(_formsPlot[i]);
            }
            setUpPage();
        }

        private void histograms_Paint(object sender, PaintEventArgs e)
        {
            setUpPage();
        }

        void setUpPage()
        {

            int tot_width = ClientRectangle.Width;
            int tot_height = ClientRectangle.Height;

            double m_share = 0.02;
            double t_share = 0.02;
            double b_share = 0.02;

            int heigth = (int)Math.Floor((1 - t_share - b_share) * tot_height / 3);
            int width = (int)Math.Floor((1 - 2 * m_share) * tot_width / 3);
            int top = (int)Math.Floor(t_share * tot_height);
            int margin = (int)Math.Floor(m_share * tot_width);

            foreach (var plt in _formsPlot)
            {
                //plt.Plot.XLabel("Year");
                plt.Size = new Size(width, heigth);
                plt.Show();
            }

            _formsPlot[0].Location = new Point(margin + 0 * width, top + 0 * heigth);
            _formsPlot[1].Location = new Point(margin + 0 * width, top + 1 * heigth);
            _formsPlot[2].Location = new Point(margin + 0 * width, top + 2 * heigth);

            _formsPlot[3].Location = new Point(margin + 1 * width, top + 0 * heigth);
            _formsPlot[4].Location = new Point(margin + 1 * width, top + 1 * heigth);
            _formsPlot[5].Location = new Point(margin + 1 * width, top + 2 * heigth);

            _formsPlot[6].Location = new Point(margin + 2 * width, top + 0 * heigth);
            _formsPlot[7].Location = new Point(margin + 2 * width, top + 1 * heigth);
            _formsPlot[8].Location = new Point(margin + 2 * width, top + 2 * heigth);


            //_formsPlot[i].Location = new Point(10, 10 + i * 100);
            //_formsPlot[i].Size = new Size(400, 80);
            //_formsPlot[i].BackColor = Color.White;
            //_formsPlot[i].Margin = new Padding(0);
            //_formsPlot[i].Padding = new Padding(0);

            //Graphics g = this.CreateGraphics();
            //Pen pen = new Pen(Color.Black, 1);
            //g.DrawLine(pen, 10, 10, 100, 100);
            //g.Dispose();

        }

        double[] log(double[] x)
        {
            double[] y = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
                y[i] = Math.Log(x[i]);
            return y;
        }
        
        public void PlotHistograms(ChartData chartData)
        {

            double mn, mx;
            int nBins = 50;


            //----------------------------------------------
            double[] data = chartData.HistogramData.Productivity;
            if (data == null || data.Length == 0)
                return;


            //mx = data.Max();
            mx = 8;
            ScottPlot.Statistics.Histogram hist = new(min: 0, max: mx, binCount: nBins);
            hist.AddRange(data);

            _formsPlot[0].Plot.Clear();
            _formsPlot[0].Plot.Title("Productivity (detrended 2% per year)");
            var bar = _formsPlot[0].Plot.AddBar(values: hist.Counts, positions: hist.Bins);
            bar.BarWidth = 0.7 * mx / nBins;

            //----------------------------------------------
            data = chartData.HistogramData.Profit;
            if (data == null || data.Length == 0)
                return;

            //mn = data.Min();
            //mx = data.Max();
            mn = -50;
            mx = 75;
            hist = new(min: mn, max: mx, binCount: nBins);
            hist.AddRange(data);

            _formsPlot[1].Plot.Clear();
            _formsPlot[1].Plot.Title("Real Profits (detrended 2% per year)");
            bar = _formsPlot[1].Plot.AddBar(values: hist.Counts, positions: hist.Bins);
            bar.BarWidth = 0.7 * mx / nBins;


            //----------------------------------------------
            double[] data1 = chartData.HistogramData.Productivity;
            if (data1 == null || data1.Length == 0)
                return;

            double[] data2 = chartData.HistogramData.Profit;
            if (data2 == null || data2.Length == 0)
                return;

            if(data1.Length != data2.Length)
                throw new Exception("Data1 and Data2 have different lengths");

            _formsPlot[2].Plot.Clear();
            _formsPlot[2].Plot.AddScatter(data1, data2, lineWidth: 0, markerSize: 3);
            _formsPlot[2].Plot.SetAxisLimits(0, 8, -50, 75);
            _formsPlot[2].Plot.XLabel("Productivity");
            _formsPlot[2].Plot.YLabel("Profit");

            //----------------------------------------------
            data = chartData.HistogramData.Production;
            if (data == null || data.Length == 0)
                return;

            //mn = data.Min();
            //mx = data.Max();
            mn = 0;
            mx = 50;
            hist = new(min: mn, max: mx, binCount: nBins);
            hist.AddRange(data);

            _formsPlot[3].Plot.Clear();
            _formsPlot[3].Plot.Title("Production (detrended 2% per year)");
            bar = _formsPlot[3].Plot.AddBar(values: hist.Counts, positions: hist.Bins);
            bar.BarWidth = 0.7 * mx / nBins;

            //----------------------------------------------
            data1 = chartData.HistogramData.Productivity;
            if (data1 == null || data1.Length == 0)
                return;

            data2 = chartData.HistogramData.Production;
            if (data2 == null || data2.Length == 0)
                return;

            _formsPlot[4].Plot.Clear();
            _formsPlot[4].Plot.AddScatter(data1, data2, lineWidth: 0, markerSize: 3);
            _formsPlot[4].Plot.SetAxisLimits(0, 8, 0, 75);
            _formsPlot[4].Plot.XLabel("Productivity");
            _formsPlot[4].Plot.YLabel("Production");

            //----------------------------------------------
            data = chartData.HistogramData.Age;
            if (data == null || data.Length == 0)
                return;

            mn = data.Min();
            mx = data.Max();
            //mn = -50;
            //mx = 75;
            if(mn<mx)
            {
                hist = new(min: mn, max: mx, binCount: nBins);
                hist.AddRange(data);

                _formsPlot[5].Plot.Clear();
                _formsPlot[5].Plot.Title("Age");
                bar = _formsPlot[5].Plot.AddBar(values: hist.Counts, positions: hist.Bins);
                bar.BarWidth = 0.7 * mx / nBins;

            }

            //----------------------------------------------
            data1 = chartData.HistogramData.Productivity;
            if (data1 == null || data1.Length == 0)
                return;

            data2 = chartData.HistogramData.Age;
            if (data2 == null || data2.Length == 0)
                return;

            _formsPlot[6].Plot.Clear();
            _formsPlot[6].Plot.AddScatter(data1, data2, lineWidth: 0, markerSize: 3);
            _formsPlot[6].Plot.SetAxisLimits(0, 8, 0, 75);
            _formsPlot[6].Plot.XLabel("Productivity");
            _formsPlot[6].Plot.YLabel("Age");
            //----------------------------------------------
            data1 = chartData.HistogramData.Productivity;
            if (data1 == null || data1.Length == 0)
                return;

            data2 = chartData.HistogramData.Employment;
            if (data2 == null || data2.Length == 0)
                return;

            _formsPlot[7].Plot.Clear();
            _formsPlot[7].Plot.AddScatter(data1, data2, lineWidth: 0, markerSize: 3);
            _formsPlot[7].Plot.SetAxisLimits(0, 8, 0, 150);
            _formsPlot[7].Plot.XLabel("Productivity");
            _formsPlot[7].Plot.YLabel("Employment");

            //----------------------------------------------

            _formsPlot[0].Render();
            _formsPlot[1].Render();
            _formsPlot[2].Render();
            _formsPlot[3].Render();
            _formsPlot[4].Render();
            _formsPlot[5].Render();
            _formsPlot[6].Render();
            _formsPlot[7].Render();

            int now = MainFormUI.Instance.Simulation.Time.Now;
            Text = "Micro data: month/year = " + now.ToString() + "/" + (now/12).ToString();
            MainFormUI.Instance.Focus();

        }

        private void HistogramForm_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.H)
            {
                if(this.Visible)
                {
                    this.Hide();
                    MainFormUI.Instance.Show();
                    Thread.Sleep(10);
                    MainFormUI.Instance.Invalidate();
                }
                else
                    this.Show();
                return;
            }

            MainFormUI.Instance.MainFormUI_KeyUp(sender, e);

        }
    }
}
