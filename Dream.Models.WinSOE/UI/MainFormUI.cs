using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Runtime.Serialization;

using ScottPlot;
using ScottPlot.Control;
using ScottPlot.Renderable;
using System.Drawing.Imaging;
using Dream.Models.WinSOE.UI;

namespace Dream.Models.WinSOE
{

    public partial class MainFormUI : Form
    {

        ScottPlot.FormsPlot[] _formsPlot;
        DateTime _t0, _t_year;
        Simulation _simulation;
        Time _time;
        bool _tweakFormsInitialized = false;
        bool _scenariosFormInitialized = false;
        string _tmpFile;
        int _scenarioRounds = 0;


        TweakForm? tweakForm = null;
        ScenariosForm? scenariosForm = null;

        public bool Busy = false;
        public bool Pause = false;
        public bool Running = false;

        public MainFormUI()
        {
            InitializeComponent();
            InitializeComponentHomeMade();
            InitializePlots();
            _tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        }

        public BackgroundWorker[] backgroundWorkersScenarios = new BackgroundWorker[16];
        int _nTotal = 0;
        private void InitializeComponentHomeMade()
        {

            for (int i = 0; i < 16; i++)
                backgroundWorkersScenarios[i] = new System.ComponentModel.BackgroundWorker();

        }

        int _total_t = 0;
        TimeSpan _timeLeftSmooth = new TimeSpan();
        public void runBackgroundWorkerScenarios(ArgsToWorker atw)
        {

            backgroundWorkersScenarios[atw.ID].DoWork += delegate (object? sender, DoWorkEventArgs e)
            {
                ArgsToWorker atw = (ArgsToWorker)e.Argument;
                Random r = new Random();
                int seed;
                SimulationRunner sim = null;
                WinFormElements wfe = null;
                
                if(atw.ID==0)
                {
                    wfe = new WinFormElements(this, e);
                    _t0 = DateTime.Now;
                }
                

                if (!scenariosForm.checkBoxScenariosUseBaseRuns.Checked)
                {
                    for (int c = 0; c < atw.NumberOfCycles; c++)
                    {
                        atw.CurrentCycle = c;
                        seed = r.Next();

                        // Base run
                        sim = new SimulationRunner(saveScenario: true, winFormElements: wfe,
                                seed: seed, shock: EShock.Nothing, atw: atw);

                        // Shocks
                        foreach (var shock in atw.Shocks)
                            sim = new SimulationRunner(saveScenario: true, winFormElements: wfe,
                                 seed: seed, shock: shock, atw: atw);

                    }
                }
                else  // If Use Base Runs:
                {
                    for (int c = 0; c < atw.NumberOfCycles; c++)
                    {
                        atw.CurrentCycle = c;

                        // Shocks
                        foreach (var shock in atw.Shocks)
                            sim = new SimulationRunner(saveScenario: true, winFormElements: wfe,
                                   seed: scenariosForm.Seeds[c][atw.ID], shock: shock, atw: atw);

                    }
                }
            };

            // First backgroundworker reports progress
            if (atw.ID == 0)  
            {
                backgroundWorkersScenarios[0].WorkerReportsProgress = true;
                backgroundWorkersScenarios[0].ProgressChanged += delegate (object? sender, ProgressChangedEventArgs e)
                {
                    int t = e.ProgressPercentage;
                    labelPeriods.Text = t.ToString() + " / " + (t / 12).ToString();

                    Settings settings = _simulation.Settings;
                    if(_nTotal==0)
                    {
                        _nTotal = atw.NumberOfCycles * (1 + settings.EndYear - settings.StartYear) * settings.PeriodsPerYear * (1 + atw.Shocks.Count);
                        if(scenariosForm.checkBoxScenariosUseBaseRuns.Checked)
                            _nTotal = atw.NumberOfCycles * (1 + settings.EndYear - settings.StartYear) * settings.PeriodsPerYear * atw.Shocks.Count;
                    }

                    if (t == 0)
                        _t_year = DateTime.Now;

                    if (t % settings.PeriodsPerYear == 0)
                    {
                        var time_one_year = DateTime.Now - _t_year;
                        labelTimeUsePerYear.Text = time_one_year.ToString();
                        
                        if(_total_t < 70 * settings.PeriodsPerYear)
                        {
                            //TimeSpan timeLeft = ((_nTotal - _total_t) / settings.PeriodsPerYear) * time_one_year;
                            //_timeLeftSmooth = 0.8 * _timeLeftSmooth + 0.2 * timeLeft;

                            //scenariosForm.labelScenariosTimeLeftValue.Text = _timeLeftSmooth.ToString(@"hh\:mm");
                            scenariosForm.labelScenariosTimeLeftValue.Text = "Waiting for enough data..";

                        }
                        else
                        {
                            TimeSpan timeLeft = ((_nTotal - _total_t) / _total_t) * (DateTime.Now - _t0);
                            scenariosForm.labelScenariosTimeLeftValue.Text = timeLeft.ToString(@"hh\:mm");

                        }

                        scenariosForm.labelScenariosTimeUsedValue.Text = (DateTime.Now - _t0).ToString(@"hh\:mm\:ss");

                        _t_year = DateTime.Now;
                    }

                    ++_total_t;
                    
                    if(_total_t % settings.PeriodsPerYear == 0)
                    {
                        int nProgress = (int)Math.Round(100 * (1.0 * _total_t / _nTotal));

                        scenariosForm.progressBarScenario.Value = nProgress;
                    }

                };
            }

            backgroundWorkersScenarios[atw.ID].RunWorkerAsync(atw);

        }
       
        void OpenScenariosForm()
        {

            Running = true;
            toolStripStatusLabelMainForm.Text = "Running Scenarios..";

            //_t0 = DateTime.Now;

            if (!_scenariosFormInitialized)
            {
                scenariosForm = new ScenariosForm(this);
                _scenariosFormInitialized = true;
            }

            if (scenariosForm != null)
                scenariosForm.ShowDialog();

        }

        private void InitializePlots()
        {
            _formsPlot = new ScottPlot.FormsPlot[16];
            _formsPlot[0] = formsPlot1;
            _formsPlot[1] = formsPlot2;
            _formsPlot[2] = formsPlot3;
            _formsPlot[3] = formsPlot4;
            _formsPlot[4] = formsPlot5;
            _formsPlot[5] = formsPlot6;
            _formsPlot[6] = formsPlot7;
            _formsPlot[7] = formsPlot8;
            _formsPlot[8] = formsPlot9;
            _formsPlot[9] = formsPlot10;
            _formsPlot[10] = formsPlot11;
            _formsPlot[11] = formsPlot12;
            _formsPlot[12] = formsPlot13;
            _formsPlot[13] = formsPlot14;
            _formsPlot[14] = formsPlot15;
            _formsPlot[15] = formsPlot16;

            foreach (var plt in _formsPlot)
                plt.Hide();

        }

        private void MainFormUI_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true; // Makes F5 work

            toolStripStatusLabelMainForm.Text = "Ready";

            this.WindowState = FormWindowState.Maximized;

            int tot_width = ClientRectangle.Width;
            int tot_height = ClientRectangle.Height;

            labelMainText.Location = new Point((int)Math.Floor(0.1 * tot_width), 35);
            labelMainText.Text = "";
            labelMainText.Font = new Font(Label.DefaultFont.Name, 20, FontStyle.Bold);


            labelPeriods.Location = new Point((int)Math.Floor(0.9 * tot_width), 35);
            labelPeriods.Text = "";
            labelPeriods.Font = new Font(Label.DefaultFont.Name, 20, FontStyle.Bold);

            labelTimeUsePerYear.Location = new Point((int)Math.Floor(0.9 * tot_width), 35 + 45);
            labelTimeUsePerYear.Text = "";

            labelBuffer.Location = new Point((int)Math.Floor(0.81 * tot_width), 35 + 2 * 45);
            labelBuffer.Hide();

            //linkLabelToPaper.Links.Add(  ("https://dreamgruppen.dk/publikationer/2022/november/copy-or-deviate-the-market-economy-as-a-self-organizing-system");

            double m_share = 0.06;
            double t_share = 0.15;
            double b_share = 0.02;

            int heigth = (int)Math.Floor((1 - t_share - b_share) * tot_height / 4);
            int width = (int)Math.Floor((1 - 2 * m_share) * tot_width / 4);
            int top = (int)Math.Floor(t_share * tot_height);
            int margin = (int)Math.Floor(m_share * tot_width);

            foreach (var plt in _formsPlot)
            {
                plt.Plot.XLabel("Year");
                plt.Size = new Size(width, heigth);
                plt.Show();
            }

            _formsPlot[0].Location = new Point(margin + 0 * width, top + 0 * heigth);
            _formsPlot[1].Location = new Point(margin + 0 * width, top + 1 * heigth);
            _formsPlot[2].Location = new Point(margin + 0 * width, top + 2 * heigth);
            _formsPlot[3].Location = new Point(margin + 0 * width, top + 3 * heigth);

            _formsPlot[4].Location = new Point(margin + 1 * width, top + 0 * heigth);
            _formsPlot[5].Location = new Point(margin + 1 * width, top + 1 * heigth);
            _formsPlot[6].Location = new Point(margin + 1 * width, top + 2 * heigth);
            _formsPlot[7].Location = new Point(margin + 1 * width, top + 3 * heigth);

            _formsPlot[8].Location = new Point(margin + 2 * width, top + 0 * heigth);
            _formsPlot[9].Location = new Point(margin + 2 * width, top + 1 * heigth);
            _formsPlot[10].Location = new Point(margin + 2 * width, top + 2 * heigth);
            _formsPlot[11].Location = new Point(margin + 2 * width, top + 3 * heigth);

            _formsPlot[12].Location = new Point(margin + 3 * width, top + 0 * heigth);
            _formsPlot[13].Location = new Point(margin + 3 * width, top + 1 * heigth);
            _formsPlot[14].Location = new Point(margin + 3 * width, top + 2 * heigth);
            _formsPlot[15].Location = new Point(margin + 3 * width, top + 3 * heigth);



        }

        void Run()
        {
            Running = true;
            toolStripStatusLabelMainForm.Text = "Running..";
            tweakToolStripMenuItem.Enabled = true;

            _t0 = DateTime.Now;
            backgroundWorker.RunWorkerAsync();

        }

        private void MainFormUI_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
                Run();

            if (e.KeyCode == Keys.F7)
                OpenScenariosForm();


            if (e.KeyCode == Keys.Escape)
            {
                backgroundWorker.CancelAsync();
                toolStripStatusLabelMainForm.Text = "Stopped";
            }

            if (Running)
            {
                if (e.KeyCode == Keys.Space)
                {
                    Pause = !Pause;

                    if (Pause)
                        toolStripStatusLabelMainForm.Text = "Paused";
                    else
                    {
                        toolStripStatusLabelMainForm.Text = "Running..";


                    }

                }

                if (e.Control && e.KeyCode == Keys.T)  // Ctrl + T
                    openTweakForm();

            }



        }

        private void runModelF5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Run();
        }

        private void runChokF6ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var sim = new SimulationRunner(false, new WinFormElements(this, e));
        }

        // Not used
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            var sim = new SimulationRunner(false, new WinFormElements(this, e));

        }


        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toolStripStatusLabelMainForm.Text = "Finalized in " + (DateTime.Now - _t0).ToString();
            toolStripStatusLabelYear.Text = "";

            labelBuffer.Hide();
            labelTimeUsePerYear.Text = "";
            labelMainText.Text = "";
            labelPeriods.Text = "";

            ChartData chartData = (ChartData)e.Result;
            plotCharts(chartData);

            using (Bitmap bmp = new Bitmap(this.Width, this.Height))
            {
                this.DrawToBitmap(bmp, new Rectangle(Point.Empty, bmp.Size));
                bmp.Save(_simulation.Settings.ROutputDir + "\\Graphics\\" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + "sceenshot.bmp", ImageFormat.Bmp); // make sure path exists!
            }

            //IFormatter formatter = new BinaryFormatter();
            //Stream stream = new FileStream(_tmpFile, FileMode.Create, FileAccess.Write);
            //formatter.Serialize(stream, chartData);
            //stream.Close();

        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }


        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            int t = e.ProgressPercentage;
            labelPeriods.Text = t.ToString() + " / " + (t / 12).ToString();

            if (t == 0)
                _t_year = DateTime.Now;

            if (t % 12 == 0)
            {
                labelTimeUsePerYear.Text = (DateTime.Now - _t_year).ToString();
                _t_year = DateTime.Now;
            }

            if (e.UserState == null)
                return;

            ChartData _chartData = (ChartData)e.UserState;

            if (_chartData.Wait > 0)
                labelBuffer.Show();
            else
                labelBuffer.Hide();

            Settings settings = _simulation.Settings;

            if (t < settings.BurnInPeriod1)
                labelMainText.Text = "Burn-in periode 1. Fixed number of new firms. No firms are closed";
            else if (t < settings.BurnInPeriod2)
                labelMainText.Text = "Burn-in periode 2. Fixed number of new firms. Firms are closed if not profitable";
            else if (t < settings.BurnInPeriod3)
                labelMainText.Text = "Burn-in periode 3. Investor builds up buffer-stock wealth";
            else
                labelMainText.Text = "Main run";



            if (_time.Now > 12)
            {
                Busy = true;
                plotCharts(_chartData, 12 * 15);

            }
            Busy = false;

        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        void plotCharts(ChartData cd, int window = 0)
        {

            Color colorDREAM = Color.FromArgb(222, 13, 13);
            Color colorMAKRO = Color.FromArgb(15, 131, 125);
            Color colorSMILE = Color.FromArgb(247, 110, 0);
            Color colorGreenREFORM = Color.FromArgb(49, 177, 73);
            Color colorREFORM = Color.FromArgb(31, 143, 187);

            int n_charts = 16;
            string[] title = new string[_formsPlot.Count()];
            title[0] = "Production (detrended 2% p.a.)";
            title[1] = "Investor Take-out (wage deflated)";
            title[2] = "Investor Reserves (wage deflated)";
            title[3] = "Rate of Unemployment";
            title[4] = "Employment and Labor Supply";
            title[5] = "Consumption (detrended 2% p.a.)";
            title[6] = "Number of Firms";
            title[7] = "Sharpe Ratio";
            title[8] = "Real Wage (detrended 2% p.a.)";
            title[9] = "Consumption Loss";
            title[10] = "Stock / Production";
            title[11] = "Wealth / Income";
            title[12] = "Market Price";
            //title[13] = "Profit share in Income";
            title[13] = "Profit per Wealth unit p.a.";
            title[14] = "Price Inflation";
            title[15] = "Real Wage Inflation";

            double[][][] y = new double[_formsPlot.Count()][][];
            for (int i = 0; i < _formsPlot.Count(); i++)
                y[i] = new double[1][];

            y[1] = new double[3][];
            y[2] = new double[2][];
            y[4] = new double[2][];
            y[13] = new double[2][];

            y[0][0] = cd.Production;
            y[1][0] = cd.InvestorIncome;
            y[2][0] = cd.InvestorWealth;
            y[3][0] = cd.UnemploymentRate;
            y[4][0] = cd.Employment;
            y[5][0] = cd.Consumption;
            y[6][0] = cd.nFirms;
            y[7][0] = cd.SharpeRatio;
            y[8][0] = cd.RealWage;
            y[9][0] = cd.ConsumptionLoss;
            y[10][0] = cd.Stock;
            y[11][0] = cd.Wealth;
            y[12][0] = cd.Price;
            //y[13][0] = cd.ProfitShare;
            y[13][0] = cd.ProfitPerWealthUnit;
            y[14][0] = cd.Inflation;
            y[15][0] = cd.RealWageInflation;

            y[1][1] = cd.InvestorTakeOut;
            y[2][1] = cd.InvestorWealthTarget;
            y[4][1] = cd.LaborSupplyProductivity;
            y[13][1] = cd.InterestRate;

            y[1][2] = cd.InvestorPermanentIncome;

            Settings settings = _simulation.Settings;

            for (int i = 0; i < n_charts; i++)
            {
                var s = _formsPlot[i].Plot.AddSignal(y[i][0], 12);
                s.MarkerSize = 0;
                s.LineColor = colorGreenREFORM;
                s.LineWidth = 0.1;

                if (y[i].Length > 1)
                {
                    s = _formsPlot[i].Plot.AddSignal(y[i][1], 12);
                    s.MarkerSize = 0;
                    s.LineColor = colorSMILE;
                    s.LineWidth = 0.1;
                }

                if (y[i].Length == 3)
                {
                    s = _formsPlot[i].Plot.AddSignal(y[i][2], 12);
                    s.MarkerSize = 0;
                    s.LineColor = colorREFORM;
                    s.LineWidth = 0.1;
                }

                _formsPlot[i].Plot.AddHorizontalLine(0, Color.Black);
                _formsPlot[i].Plot.Title(title[i]);

                int x_min = 0;
                if (window > 0)
                    x_min = Math.Max(0, _time.Now - window);
                _formsPlot[i].Plot.SetAxisLimitsX(x_min / 12, _time.Now / 12);

                if (x_min < settings.BurnInPeriod3)
                {
                    _formsPlot[i].Plot.AddVerticalLine(settings.BurnInPeriod1 / 12, Color.Black, (float)0.1, LineStyle.Dot);
                    _formsPlot[i].Plot.AddVerticalLine(settings.BurnInPeriod2 / 12, Color.Black, (float)0.1, LineStyle.Dot);
                    _formsPlot[i].Plot.AddVerticalLine(settings.BurnInPeriod3 / 12, Color.Black, (float)0.1, LineStyle.Dot);
                }

                if(i!=13)
                    _formsPlot[i].Plot.AxisAutoY();

                if (i == 7) _formsPlot[i].Plot.SetAxisLimitsY(-0.3, 0.3);
                if (i == 10) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.01);
                if (_simulation.Time.Now > 12 * 70)
                {
                    if (i == 1) _formsPlot[i].Plot.SetAxisLimitsY(-2000, 10000);  // Invester income
                    if (i == 7) _formsPlot[i].Plot.SetAxisLimitsY(-0.15, 0.15);
                    if (i == 12) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.5);  // Price
                    //if (i == 13) _formsPlot[i].Plot.SetAxisLimitsY(-0.2, 0.5);
                    if (i == 14) _formsPlot[i].Plot.SetAxisLimitsY(-0.15, 0.15);
                    if (i == 15) _formsPlot[i].Plot.SetAxisLimitsY(-0.05, 0.1);
                    if (i == 15) _formsPlot[i].Plot.AddHorizontalLine(0.02, Color.Black, (float)0.1, LineStyle.Dot);
                }

                //_formsPlot[i].Render(); 
                _formsPlot[i].Refresh();
            }

        }

        private void labelPeriods_MouseHover(object sender, EventArgs e)
        {
            toolTipLabelPeriods.Show("Month / Year", labelPeriods);
        }

        private void labelTimeUsePerYear_MouseHover(object sender, EventArgs e)
        {
            toolTipLabelTimeUsePerYear.Show("Time use per year", labelTimeUsePerYear);
        }

        private void linkLabelToPaper_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        void openTweakForm()
        {
            if (!_tweakFormsInitialized)
            {
                tweakForm = new TweakForm(_simulation);
                _tweakFormsInitialized = true;
            }

            if (tweakForm != null)
                tweakForm.ShowDialog();


        }

        private void tweakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openTweakForm();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("View settings..");
        }


        #region backgroundWorkerSenarios_DoWork0..15
        private void backgroundWorkerSenarios_DoWork0(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork1(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }


        private void backgroundWorkerSenarios_DoWork2(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork3(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork4(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork5(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork6(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork7(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork8(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork9(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork10(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork11(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork12(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork13(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork14(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }

        private void backgroundWorkerSenarios_DoWork15(object sender, DoWorkEventArgs e)
        {
            ArgsToWorker atw = (ArgsToWorker)e.Argument;
            Random r = new Random();
            int seed;

            for (int i = 0; i < atw.NumberOfCycles; i++)
            {
                atw.CurrentCycle = i;
                seed = r.Next();

                SimulationRunner sim = null;
                // Base run
                sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: EShock.Nothing, atw: atw);

                // Shocks
                foreach (var shock in atw.Shocks)
                    sim = new SimulationRunner(saveScenario: true, winFormElements: new WinFormElements(this, e),
                    seed: seed, shock: shock, atw: atw);

            }
        }



        #endregion

        public Simulation Simulation
        {
            get { return _simulation; }
            set
            {
                _simulation = value;
                _time = _simulation.Time;
            }
        }


    }

    /// <summary>
    /// Class for sending data from backgroundworker to UI
    /// </summary>
    [Serializable]
    public class ChartData
    {
        public double[] nFirms, nHouseholds, Sales, RealWage, Price, Employment, Consumption, Production,
            SharpeRatio, UnemploymentRate, ConsumptionLoss, Stock, Wealth, LaborSupplyProductivity,
            ProfitPerHousehold, ProfitPerWealthUnit, ProfitShare, Inflation, RealWageInflation, InterestRate,
            InvestorWealth, InvestorTakeOut, InvestorWealthTarget, InvestorIncome, InvestorPermanentIncome;

        public int Wait;

        public ChartData(int n)
        {

            
            nFirms = new double[n];
            nHouseholds = new double[n];
            Sales = new double[n];
            RealWage = new double[n];
            Price = new double[n];
            Employment = new double[n];
            Consumption = new double[n];
            Production = new double[n];
            SharpeRatio = new double[n];
            UnemploymentRate = new double[n];
            ConsumptionLoss = new double[n];
            Stock = new double[n];
            Wealth = new double[n];
            LaborSupplyProductivity = new double[n];
            ProfitPerHousehold = new double[n];
            ProfitPerWealthUnit = new double[n];
            ProfitShare = new double[n];
            Inflation = new double[n];
            RealWageInflation = new double[n];
            InvestorWealth = new double[n];
            InvestorWealthTarget = new double[n];
            InvestorTakeOut = new double[n];
            InvestorIncome = new double[n];
            InvestorPermanentIncome = new double[n];
            InterestRate = new double[n];

        }
    }
}
