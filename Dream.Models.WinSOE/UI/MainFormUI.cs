using System.ComponentModel;

#if WIN_APP
using ScottPlot;
using System.Drawing.Imaging;
using Dream.Models.WinSOE.UI;

namespace Dream.Models.WinSOE
{
    public partial class MainFormUI : Form
    {
        public static MainFormUI Instance;
        
        #region Private fields
        ScottPlot.FormsPlot[] _formsPlot;
        DateTime _t0, _t_year;
        Simulation _simulation;
        Time _time;
        Settings _settings;
        bool _tweakFormsInitialized = false;
        bool _scenariosFormInitialized = false;
        bool _histogramFormsInitialized = false;
        bool _histogramFormsVisible = false;
        string _tmpFile;
        int _scenarioRounds = 0;
        bool _chartInit = false;

        TweakForm? _tweakForm = null;
        ScenariosForm? _scenariosForm = null;
        HistogramForm? _histogramForm = null;

        public bool Busy = false;
        public bool Pause = false;
        public bool Running = false;

        public BackgroundWorker[] backgroundWorkersScenarios = new BackgroundWorker[16];
        int _nTotal = 0;

        int _total_t = 0;
        TimeSpan _timeLeftSmooth = new TimeSpan();
        int _UIChartTimeWindow0 = 0;
        double _UIChartTimeWindowShare = 1.0;
        ChartData _chartData;
        int _x_max;

        Color[] _cols = new Color[4];
        string[,] _colText = new string[4, 16];

        Color _colorDREAM;
        Color _colorMAKRO;
        Color _colorSMILE;
        Color _colorGreenREFORM;
        Color _colorREFORM;
        double[][][] _data = new double[16][][];
        bool _firstChart = true;
        bool _first = true;
        Random _random = new Random();
        int _page = 0;
        int _nPages;
        #endregion

        public MainFormUI()
        {
            InitializeComponent();
            InitializeComponentHomeMade();
            InitializePlots();
            _tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Instance = this;

        }
        private void InitializeComponentHomeMade()
        {

            for (int i = 0; i < 16; i++)
                backgroundWorkersScenarios[i] = new System.ComponentModel.BackgroundWorker();

        }
        public void runBackgroundWorkerScenarios(ArgsToWorker atw)
        {

            backgroundWorkersScenarios[atw.ID].DoWork += delegate (object? sender, DoWorkEventArgs e)
            {
                ArgsToWorker atw = (ArgsToWorker)e.Argument;
                Random r = new Random();
                int seed;
                SimulationRunner sim = null;
                WinFormElements wfe = null;

                // First worker initializes WinFormElements
                if (atw.ID == 0)
                {
                    wfe = new WinFormElements(this, e);
                    _t0 = DateTime.Now;
                }


                if (!_scenariosForm.checkBoxScenariosUseBaseRuns.Checked)
                {
                    for (int c = 0; c < atw.NumberOfCycles; c++)
                    {
                        atw.CurrentCycle = c;
                        seed = r.Next();

                        // Base run
                        sim = new SimulationRunner(saveScenario: true, winFormElements: wfe, shock: EShock.Base,
                            seed: seed, atw: atw);

                        // Shocks
                        foreach (var shock in atw.Shocks)
                            sim = new SimulationRunner(saveScenario: true, winFormElements: wfe, shock: shock,
                                seed: seed, atw: atw);

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
                                   seed: _scenariosForm.Seeds[c][atw.ID], shock: shock, atw: atw);

                    }
                }
            };

            // First backgroundworker 0 reports progress
            if (atw.ID == 0)
            {
                backgroundWorkersScenarios[0].WorkerReportsProgress = true;
                backgroundWorkersScenarios[0].ProgressChanged += delegate (object? sender, ProgressChangedEventArgs e)
                {
                    int t = e.ProgressPercentage;
                    labelPeriods.Text = t.ToString() + " / " + (t / 12).ToString();

                    Settings settings = _simulation.Settings;
                    if (_nTotal == 0)
                    {
                        _nTotal = atw.NumberOfCycles * (1 + settings.EndYear - settings.StartYear)
                                * settings.PeriodsPerYear * (1 + atw.Shocks.Count);

                        if (_scenariosForm.checkBoxScenariosUseBaseRuns.Checked)
                            _nTotal = atw.NumberOfCycles * (1 + settings.EndYear - settings.StartYear) * settings.PeriodsPerYear * atw.Shocks.Count;
                    }

                    if (t == 0)
                        _t_year = DateTime.Now;

                    if (t % settings.PeriodsPerYear == 0) // Every year
                    {
                        var time_one_year = DateTime.Now - _t_year;
                        labelTimeUsePerYear.Text = time_one_year.ToString();

                        if (_total_t < 70 * settings.PeriodsPerYear)
                        {
                            //TimeSpan timeLeft = ((_nTotal - _total_t) / settings.PeriodsPerYear) * time_one_year;
                            //_timeLeftSmooth = 0.8 * _timeLeftSmooth + 0.2 * timeLeft;

                            //scenariosForm.labelScenariosTimeLeftValue.Text = _timeLeftSmooth.ToString(@"hh\:mm");
                            _scenariosForm.labelScenariosTimeLeftValue.Text = "Waiting for enough data..";

                        }
                        else
                        {
                            TimeSpan timeLeft = (1.0 * (_nTotal - _total_t) / _total_t) * (DateTime.Now - _t0);
                            _scenariosForm.labelScenariosTimeLeftValue.Text = timeLeft.ToString(@"hh\:mm\:ss");

                        }

                        _scenariosForm.labelScenariosTimeUsedValue.Text = (DateTime.Now - _t0).ToString(@"hh\:mm\:ss");

                        _t_year = DateTime.Now;
                    }

                    ++_total_t;

                    if (_total_t % settings.PeriodsPerYear == 0)
                    {
                        int nProgress = (int)Math.Round(100 * (1.0 * _total_t / _nTotal));

                        _scenariosForm.progressBarScenario.Value = nProgress;
                    }

                };
            }

            // Run the backgroundworker
            backgroundWorkersScenarios[atw.ID].RunWorkerAsync(atw);

        }
        void OpenScenariosForm()
        {

            Running = true;
            toolStripStatusLabelMainForm.Text = "Running Scenarios..";

            //_t0 = DateTime.Now;

            if (!_scenariosFormInitialized)
            {
                _scenariosForm = new ScenariosForm(this);
                _scenariosFormInitialized = true;
            }

            if (_scenariosForm != null)
                _scenariosForm.ShowDialog();

        }
        private void InitializePlots()
        {

            _nPages = 2;
            _page = 0;

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

            for(int i = 0; i < _formsPlot.Count(); i++)
            {
                _formsPlot[i].Hide();
                _formsPlot[i].MouseMove += new MouseEventHandler(control_MouseMove);
                _formsPlot[i].MouseDown += new MouseEventHandler(control_MouseClick);
                _formsPlot[i].Name = i.ToString();
            }
        }
        void control_MouseClick(object sender, MouseEventArgs e)
        {
            FormsPlot plt = (FormsPlot)sender;

            plt.Plot.AxisAutoY();
            plt.Refresh();

            //int i = Convert.ToInt32(plt.Name);
            //double[] dataLim = calcDataLimits(i);
            //double YMin = dataLim[0];
            //double YMax = dataLim[1];

            //if (YMin > 0)
            //{
            //    plt.Plot.SetAxisLimitsY(0, 1.1*YMax);
            //    plt.Refresh();
            //}

            //if (YMax < 0)
            //{
            //    plt.Plot.SetAxisLimitsY(1.1 * YMin, 0);
            //    plt.Refresh();
            //}

            //if (YMax > 0 & YMin < 0)
            //{
            //    plt.Plot.SetAxisLimitsY(1.1 * YMin, 1.1 * YMax);
            //    plt.Refresh();
            //}


        }
        void control_MouseMove(object sender, MouseEventArgs e)
        {
            ScottPlot.FormsPlot plt = (ScottPlot.FormsPlot)sender;

            int i = Convert.ToInt32(plt.Name);
            //int i = Convert.ToInt32(plt.Name.Remove(0, 9)) - 1; // Remove "formsPlot"

            if (_colText[1, i] == null)
            {
                labelColText1.Text = "";
                labelColText2.Text = "";
                labelColText3.Text = "";
                labelColText4.Text = "";
            }
            else
            {
                //Point p = _formsPlot[i].Location;
                //labelColText1.Location = new Point(p.X + e.X + 10, p.Y + e.Y);
                //labelColText2.Location = new Point(p.X + e.X + 10, p.Y + e.Y + 20);

                labelColText1.Text = _colText[0, i];
                labelColText2.Text = _colText[1, i];
                labelColText3.Text = _colText[2, i];
                labelColText4.Text = _colText[3, i];

            }
        }
        private void MainFormUI_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true; // Makes F5 work

            toolStripStatusLabelMainForm.Text = "Ready";

            this.WindowState = FormWindowState.Maximized;

            _colorDREAM = Color.FromArgb(222, 13, 13);
            _colorMAKRO = Color.FromArgb(15, 131, 125);
            _colorSMILE = Color.FromArgb(247, 110, 0);
            _colorGreenREFORM = Color.FromArgb(49, 177, 73);
            _colorREFORM = Color.FromArgb(31, 143, 187);

            _cols[0] = _colorGreenREFORM;
            _cols[1] = _colorSMILE;
            _cols[2] = _colorREFORM;
            _cols[3] = _colorDREAM;

            int tot_width = ClientRectangle.Width;
            int tot_height = ClientRectangle.Height;

            labelMainText.Location = new Point((int)Math.Floor(0.1 * tot_width), 35);
            labelMainText.Text = "";
            labelMainText.Font = new Font(Label.DefaultFont.Name, 20, FontStyle.Bold);

            labelMainTextSub.Location = new Point((int)Math.Floor(0.1 * tot_width), 80);
            labelMainTextSub.Text = "";
            labelMainTextSub.Font = new Font(Label.DefaultFont.Name, 12);

            labelPeriods.Location = new Point((int)Math.Floor(0.9 * tot_width), 35);
            labelPeriods.Text = "";
            labelPeriods.Font = new Font(Label.DefaultFont.Name, 20, FontStyle.Bold);

            labelTimeUsePerYear.Location = new Point((int)Math.Floor(0.9 * tot_width), 35 + 45);
            labelTimeUsePerYear.Text = "";

            labelBuffer.Location = new Point((int)Math.Floor(0.9 * tot_width), 35 + 45 + 30);  //0.81 * tot_width
            labelBuffer.Hide();

            labelColText1.Text = "";
            labelColText2.Text = "";
            labelColText3.Text = "";
            labelColText4.Text = "";

            labelColText1.Font = new Font(Label.DefaultFont.Name, 10, FontStyle.Bold);
            labelColText2.Font = new Font(Label.DefaultFont.Name, 10, FontStyle.Bold);
            labelColText3.Font = new Font(Label.DefaultFont.Name, 10, FontStyle.Bold);
            labelColText4.Font = new Font(Label.DefaultFont.Name, 10, FontStyle.Bold);

            labelColText1.ForeColor = _cols[0];
            labelColText2.ForeColor = _cols[1];
            labelColText3.ForeColor = _cols[2];
            labelColText4.ForeColor = _cols[3];

            labelColText1.Location = new Point((int)Math.Floor(0.75 * tot_width), 50);
            labelColText2.Location = new Point((int)Math.Floor(0.75 * tot_width), 70);
            labelColText3.Location = new Point((int)Math.Floor(0.75 * tot_width), 90);
            labelColText4.Location = new Point((int)Math.Floor(0.75 * tot_width), 110);

            //linkLabelToPaper.Links.Add(  ("https://dreamgruppen.dk/publikationer/2022/november/copy-or-deviate-the-market-economy-as-a-self-organizing-system");

            //PSP
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
        public void MainFormUI_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    Run();
                    break;

                case Keys.F7:
                    OpenScenariosForm();
                    break;

                case Keys.D1:
                    labelMainText.Text = "Positive productivity shock";
                    _simulation.ShockNow(EShock.Productivity);
                    break;

                case Keys.D2:
                    labelMainText.Text = "Positive AR1 productivity shock";
                    _simulation.ShockNow(EShock.ProductivityAR1);
                    break;

                case Keys.D3:
                    labelMainText.Text = "Tsunami shock";
                    _simulation.ShockNow(EShock.Tsunami);
                    break;

                case Keys.Up:
                    _UIChartTimeWindowShare += 0.1;
                    _settings.UIChartTimeWindow = (int)Math.Floor(_UIChartTimeWindowShare * _UIChartTimeWindow0);
                    labelMainTextSub.Text = "Time window scaled " +
                        (Math.Round(100 * _UIChartTimeWindowShare)).ToString() + " percentage (" +
                        (_settings.UIChartTimeWindow / 12).ToString("#.#") + " years)";
                    break;

                case Keys.Down:
                    _UIChartTimeWindowShare -= 0.1;
                    if (_UIChartTimeWindowShare < 0.1)
                        _UIChartTimeWindowShare = 0.1;
                    _settings.UIChartTimeWindow = (int)Math.Floor(_UIChartTimeWindowShare * _UIChartTimeWindow0);
                    labelMainTextSub.Text = "Time window scaled " +
                        (Math.Round(100 * _UIChartTimeWindowShare)).ToString() + " percentage (" +
                        (_settings.UIChartTimeWindow / 12).ToString("#.#") + " years)";
                    break;

                case Keys.Left:
                    if (Pause)
                    {
                        _x_max -= 12 * 10;
                        plotCharts(_chartData, _settings.UIChartTimeWindow, _x_max);
                    }
                    break;

                case Keys.Right:
                    if (Pause)
                    {
                        _x_max += 12 * 10;
                        plotCharts(_chartData, _settings.UIChartTimeWindow, _x_max);
                    }
                    break;

                case Keys.Space:
                    if (Running)
                    {
                        Pause = !Pause;

                        if (Pause)
                            toolStripStatusLabelMainForm.Text = "Paused";
                        else
                            toolStripStatusLabelMainForm.Text = "Running..";
                    }
                    break;

                case Keys.X:
                    if (Running)
                        backgroundWorker.CancelAsync();
                    else
                        Application.Exit();
                    break;

                case Keys.T:
                    if (Running)
                        openTweakForm();
                    break;

                case Keys.H:
                    if (Running)
                        openHistogramForm();
                    break;


                case Keys.PageDown:
                    if (Running)
                    {
                        if (_page + 1 <= _nPages)
                        {
                            _page++;
                            plotCharts(_chartData, _settings.UIChartTimeWindow, _x_max, page: _page);
                        }
                    }
                    break;

                case Keys.PageUp:
                    if (Running)
                    {
                        if (_page - 1 >= 0)
                        {
                            _page--;
                            plotCharts(_chartData, _settings.UIChartTimeWindow, _x_max, page: _page);
                        }
                    }
                    break;

                default:
                    break;
            }

            if (e.Control)
            {
                switch (e.KeyCode)
                {
                    case Keys.D1:
                        labelMainText.Text = "Negative productivity shock";
                        _simulation.ShockNow(EShock.Productivity, -1);
                        break;

                    case Keys.D2:
                        labelMainText.Text = "Negative AR1 productivity shock";
                        _simulation.ShockNow(EShock.ProductivityAR1, -1);
                        break;

                    default:
                        break;
                }
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
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Running = false;

            toolStripStatusLabelMainForm.Text = "Finalized in " + (DateTime.Now - _t0).ToString();
            toolStripStatusLabelYear.Text = "";

            labelBuffer.Hide();
            labelTimeUsePerYear.Text = "";
            labelMainText.Text = "";
            labelPeriods.Text = "";

            ChartData chartData = (ChartData)e.Result;
            
            if(_time.Now > 12*200)
                plotCharts(chartData, x_min: 150 * 12);
            else
                plotCharts(chartData);

            labelTimeUsePerYear.Location = labelPeriods.Location;
            labelTimeUsePerYear.Text = DateTime.Now.ToString();

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
            {
                _settings.UIChartUpdateInterval += 1;
                //labelBuffer.Text = string.Format("Interval (years): {0:#.##}", 1.0*_settings.UIChartUpdateInterval/12);
                labelBuffer.Text = string.Format("Interval (months): {0}", _settings.UIChartUpdateInterval);
                labelBuffer.Show();

            }
            else
            {
                if (_random.NextDouble() < 0.1 & _settings.UIChartUpdateInterval > 1)
                    _settings.UIChartUpdateInterval -= 1;
            }

            Settings settings = _simulation.Settings;

            if (t < settings.BurnInPeriod1)
                labelMainText.Text = "Burn-in periode 1. Fixed number of firms. No firms are closed";
            else if (t < settings.BurnInPeriod2)
                labelMainText.Text = "Burn-in periode 2. Fixed number of new firms. Firms are closed if not profitable";
            else if (t < settings.BurnInPeriod3)
                labelMainText.Text = "Burn-in periode 3. Free number of new firms. Fixed interest rate";
            else
            {
                if (labelMainText.Text.Substring(0, 7) == "Burn-in")
                    labelMainText.Text = "Main run";
            }



            if (_time.Now > 12)
            {
                Busy = true;

                int window = _time.Now < 12 * 125 ? 12 * 125 : _settings.UIChartTimeWindow;
                if (_time.Now > 12 * 125 & _first)
                {
                    _first = false;
                    _settings.UIChartUpdateInterval = 1;
                    labelBuffer.Text = string.Format("Interval (months): 3");
                }

                plotCharts(_chartData, window);

            }
            Busy = false;

        }
        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
        void plotCharts(ChartData cd,int window = 0, int x_max = 0, int x_min = 0, int page=-1)
        {

            //if(page!=-1)
            //    _page = page;
            _page = 0;
            
            _chartData = cd;
            
            if (x_max == 0)
                _x_max = _time.Now;

            // Default
            int xmin = 0;
            int xmax = _time.Now;

            if (x_max > 0)
                xmax = x_max;

            if (x_min > 0)
                xmin = x_min;

            if (window > 0)
                xmin = Math.Max(0, xmax - window);

            int n_charts = 16;
            string[] title = new string[_formsPlot.Count()];

            if (_page == 0)
            {

                title[0] = "Production (detrended 2% p.a.)";
                title[1] = "New and closed firms";
                title[2] = "Stock / Production";
                title[3] = "Rate of Unemployment";
                title[4] = "Employment and Labor Supply";
                title[5] = "New jobs";
                title[6] = "Number of Firms";
                title[7] = "Sharpe Ratio";
                title[8] = "Real Wage (detrended 2% p.a.)";
                title[9] = "Good Shortage";
                title[10] = "Wage inflation";
                title[11] = "Real Wealth (detrended 2% p.a.)";
                title[12] = "Real interest rate p.a.";
                title[13] = "Interest rate p.a.";
                title[14] = "Price Inflation";
                title[15] = "Real Wage Inflation";

                for (int i = 0; i < _formsPlot.Count(); i++)
                    _data[i] = new double[1][];

                _data[0] = new double[2][];
                _data[1] = new double[2][];
                _data[4] = new double[2][];
                _data[5] = new double[4][];
                _data[7] = new double[2][];
                _data[11] = new double[3][];
                _data[12] = new double[2][];
                _data[13] = new double[2][];

                int n = _settings.UIChartUpdateInterval + 2;
                if (xmax - n < 0)
                    n = xmax;

                _data[0][0] = cd.Production[(xmax - n)..xmax]; _colText[0, 0] = "Production";
                _data[1][0] = cd.ClosedFirms[(xmax - n)..xmax]; _colText[0, 1] = "Closed firms";
                _data[2][0] = cd.Stock[(xmax - n)..xmax]; _colText[0, 2] = "Stock";
                _data[3][0] = cd.UnemploymentRate[(xmax - n)..xmax]; _colText[0, 3] = "Unemployment Rate";
                _data[4][0] = cd.Employment[(xmax - n)..xmax]; _colText[0, 4] = "Employment";
                _data[5][0] = cd.nJobFromJobAdvertise[(xmax - n)..xmax]; _colText[0, 5] = "New job from job (Adv.)";
                _data[6][0] = cd.nFirms[(xmax - n)..xmax]; _colText[0, 6] = "Number of firms";
                _data[7][0] = cd.SharpeRatio[(xmax - n)..xmax]; _colText[0, 7] = "Sharpe Ratio";
                _data[8][0] = cd.RealWage[(xmax - n)..xmax]; _colText[0, 8] = "Real Wage";
                _data[9][0] = cd.ConsumptionLoss[(xmax - n)..xmax]; _colText[0, 9] = "Good Shortage";
                _data[10][0] = cd.WageInflation[(xmax - n)..xmax]; _colText[0, 10] = "Wage Inflation";
                _data[11][0] = cd.Wealth[(xmax - n)..xmax]; _colText[0, 11] = "Wealth";
                _data[12][0] = cd.RealInterestRate[(xmax - n)..xmax]; _colText[0, 12] = "Real Interest rate";
                _data[13][0] = cd.InterestRate[(xmax - n)..xmax]; _colText[0, 13] = "Interest Rate";
                _data[14][0] = cd.Inflation[(xmax - n)..xmax]; _colText[0, 14] = "Inflation";
                _data[15][0] = cd.RealWageInflation[(xmax - n)..xmax]; _colText[0, 15] = "Real Wage Inflation";

                _data[0][1] = cd.Consumption[(xmax - n)..xmax]; _colText[1, 0] = "Consumption";
                _data[1][1] = cd.NewFirms[(xmax - n)..xmax]; _colText[1, 1] = "New firms";
                _data[4][1] = cd.LaborSupplyProductivity[(xmax - n)..xmax]; _colText[1, 4] = "Labor Supply Productivity";
                _data[5][1] = cd.nJobFromUnemploymentAdvertise[(xmax - n)..xmax]; _colText[1, 5] = "New job from unemployment (Adv.)";
                _data[7][1] = cd.ExpectedSharpeRatio[(xmax - n)..xmax]; _colText[1, 7] = "Expected Sharpe Ratio";
                _data[11][1] = cd.Inheritance[(xmax - n)..xmax]; _colText[1, 11] = "Inheritance*100";
                _data[12][1] = cd.ExpectedRealInterestRate[(xmax - n)..xmax]; _colText[1, 12] = "Expected Real InterestRate";
                _data[13][1] = cd.ExpectedInterestRate[(xmax - n)..xmax]; _colText[1, 13] = "Expected Interest Rate";

                _data[11][2] = cd.Profit[(xmax - n)..xmax]; _colText[2, 11] = "Profit*40";
                _data[5][2] = cd.nJobFromUnemployment[(xmax - n)..xmax]; _colText[2, 5] = "New job from unemployment";

                _data[5][3] = cd.nJobFromJob[(xmax - n)..xmax]; _colText[3, 5] = "New job from job";

                Settings settings = _simulation.Settings;

                for (int i = 0; i < n_charts; i++)
                {
                    //_formsPlot[i].Plot.AddVerticalLine(1.0 * settings.ShockPeriod / 12, Color.FromArgb(220,220,220), (float)1);  //, LineStyle.Dot

                    var s = _formsPlot[i].Plot.AddSignal(_data[i][0], 12);
                    s.OffsetX = 1.0 * (xmax - n) / 12;
                    s.LineColor = _cols[0];
                    s.MarkerSize = 0;
                    s.LineWidth = 0.1;

                    if (_data[i].Length > 1)
                    {
                        s = _formsPlot[i].Plot.AddSignal(_data[i][1], 12);
                        s.LineColor = _cols[1];
                        s.MarkerSize = 0;
                        s.LineWidth = 0.1;
                        s.OffsetX = 1.0 * (xmax - n) / 12;

                    }

                    if (_data[i].Length > 2)
                    {
                        s = _formsPlot[i].Plot.AddSignal(_data[i][2], 12);
                        s.LineColor = _cols[2];
                        s.MarkerSize = 0;
                        s.LineWidth = 0.1;
                        s.OffsetX = 1.0 * (xmax - n) / 12;

                    }

                    if (_data[i].Length == 4)
                    {
                        s = _formsPlot[i].Plot.AddSignal(_data[i][3], 12);
                        s.LineColor = _cols[3];
                        s.MarkerSize = 0;
                        s.LineWidth = 0.1;
                        s.OffsetX = 1.0 * (xmax - n) / 12;
                    }

                    if (_firstChart)
                    {
                        _formsPlot[i].Plot.AddHorizontalLine(0, Color.Black);
                        _formsPlot[i].Plot.Title(title[i]);

                    }

                    _formsPlot[i].Plot.SetAxisLimitsX(1.0 * xmin / 12, 1.0 * xmax / 12);

                    if (xmin < settings.BurnInPeriod3)
                    {
                        _formsPlot[i].Plot.AddVerticalLine(settings.BurnInPeriod1 / 12, _colorMAKRO, (float)0.1, LineStyle.Dot);
                        _formsPlot[i].Plot.AddVerticalLine(settings.BurnInPeriod2 / 12, _colorMAKRO, (float)0.1, LineStyle.Dot);
                        _formsPlot[i].Plot.AddVerticalLine(settings.BurnInPeriod3 / 12, _colorMAKRO, (float)0.1, LineStyle.Dot);
                    }

                    if (_simulation.Time.Now < 12 * 70)
                    {
                        if (i != 13)
                            _formsPlot[i].Plot.AxisAutoY();

                        if (i == 7) _formsPlot[i].Plot.SetAxisLimitsY(-0.8, 0.8);  // Sharpe Ratio 
                        if (i == 10) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.01);   // Wage inflation

                    }

                    int[] fixedWidth = new int[] { 2, 3, 7, 8 ,9};

                    if (_simulation.Time.Now > 12 * 70 & _simulation.Time.Now < 12 * 75)
                    {
                        if (i == 1) _formsPlot[i].Plot.SetAxisLimitsY(0, 70);         // New and closed firms
                        if (i == 2) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.5);       // Stock / Production
                        if (i == 3) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.1);        // Unemployment rate
                                                                                      //if (i == 5) _formsPlot[i].Plot.SetAxisLimitsY(0, 10);        // New jobs
                        if (i == 6) _formsPlot[i].Plot.SetAxisLimitsY(0, 1200);      // Number of firms
                        if (i == 7) _formsPlot[i].Plot.SetAxisLimitsY(-0.1, 0.1);     // Sharpe ratio
                        if (i == 8) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.5);        // Real wage
                        if (i == 9) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.5);        // Good Shortage
                        if (i == 10) _formsPlot[i].Plot.SetAxisLimitsY(-0.05, 0.1);  // Wage inflation        
                        if (i == 11) _formsPlot[i].Plot.SetAxisLimitsY(0, 100000);    // Real Wealth
                        if (i == 12) _formsPlot[i].Plot.SetAxisLimitsY(-0.05, 0.7);   // Real interest rate
                        if (i == 13) _formsPlot[i].Plot.SetAxisLimitsY(-0.05, 0.7);   // Interest rate
                        if (i == 14) _formsPlot[i].Plot.SetAxisLimitsY(-0.1, 0.1);   // Inflation
                        if (i == 15) _formsPlot[i].Plot.SetAxisLimitsY(-0.05, 0.1);   // Real wage inflation
                        if (i == 15) _formsPlot[i].Plot.AddHorizontalLine(0.02, Color.Black, (float)0.1, LineStyle.Dot);

                        //_formsPlot[i].Plot.AxisAutoY();

                    }
                    else if (_simulation.Time.Now > 12 * (125 + 5) & !fixedWidth.Contains(i))
                    {
                        _formsPlot[i].Plot.AxisAutoY();
                    }
                }

                if(_histogramFormsVisible)
                    _histogramForm.PlotHistograms(_chartData);

                _firstChart = false;
                for (int i = 0; i < n_charts; i++)
                    _formsPlot[i].Refresh();

            }

            if (_page == 1)
            {

                title[0] = "Production (detrended 2% p.a.)1";
                title[1] = "New and closed firms1";
                title[2] = "Stock / Production1";
                title[3] = "Rate of Unemployment1";
                title[4] = "Employment and Labor Supply";
                title[5] = "New jobs";
                title[6] = "Number of Firms";
                title[7] = "Sharpe Ratio";
                title[8] = "Real Wage (detrended 2% p.a.)";
                title[9] = "Good Shortage";
                title[10] = "Wage inflation";
                title[11] = "Real Wealth (detrended 2% p.a.)";
                title[12] = "Real interest rate p.a.";
                title[13] = "Interest rate p.a.";
                title[14] = "Price Inflation";
                title[15] = "Real Wage Inflation";

                for (int i = 0; i < _formsPlot.Count(); i++)
                    _data[i] = new double[1][];

                _data[0] = new double[2][];
                _data[1] = new double[2][];
                _data[4] = new double[2][];
                _data[5] = new double[4][];
                _data[7] = new double[2][];
                _data[11] = new double[3][];
                _data[12] = new double[2][];
                _data[13] = new double[2][];

                int n = _settings.UIChartUpdateInterval + 2;
                if (xmax - n < 0)
                    n = xmax;

                _data[0][0] = cd.Production[(xmax - n)..xmax]; _colText[0, 0] = "Production";
                _data[1][0] = cd.ClosedFirms[(xmax - n)..xmax]; _colText[0, 1] = "Closed firms";
                _data[2][0] = cd.Stock[(xmax - n)..xmax]; _colText[0, 2] = "Stock";
                _data[3][0] = cd.UnemploymentRate[(xmax - n)..xmax]; _colText[0, 3] = "Unemployment Rate";
                _data[4][0] = cd.Employment[(xmax - n)..xmax]; _colText[0, 4] = "Employment";
                _data[5][0] = cd.nJobFromJobAdvertise[(xmax - n)..xmax]; _colText[0, 5] = "New job from job (Adv.)";
                _data[6][0] = cd.nFirms[(xmax - n)..xmax]; _colText[0, 6] = "Number of firms";
                _data[7][0] = cd.SharpeRatio[(xmax - n)..xmax]; _colText[0, 7] = "Sharpe Ratio";
                _data[8][0] = cd.RealWage[(xmax - n)..xmax]; _colText[0, 8] = "Real Wage";
                _data[9][0] = cd.ConsumptionLoss[(xmax - n)..xmax]; _colText[0, 9] = "Good Shortage";
                _data[10][0] = cd.WageInflation[(xmax - n)..xmax]; _colText[0, 10] = "Wage Inflation";
                _data[11][0] = cd.Wealth[(xmax - n)..xmax]; _colText[0, 11] = "Wealth";
                _data[12][0] = cd.RealInterestRate[(xmax - n)..xmax]; _colText[0, 12] = "Real Interest rate";
                _data[13][0] = cd.InterestRate[(xmax - n)..xmax]; _colText[0, 13] = "Interest Rate";
                _data[14][0] = cd.Inflation[(xmax - n)..xmax]; _colText[0, 14] = "Inflation";
                _data[15][0] = cd.RealWageInflation[(xmax - n)..xmax]; _colText[0, 15] = "Real Wage Inflation";

                _data[0][1] = cd.Consumption[(xmax - n)..xmax]; _colText[1, 0] = "Consumption";
                _data[1][1] = cd.NewFirms[(xmax - n)..xmax]; _colText[1, 1] = "New firms";
                _data[4][1] = cd.LaborSupplyProductivity[(xmax - n)..xmax]; _colText[1, 4] = "Labor Supply Productivity";
                _data[5][1] = cd.nJobFromUnemploymentAdvertise[(xmax - n)..xmax]; _colText[1, 5] = "New job from unemployment (Adv.)";
                _data[7][1] = cd.ExpectedSharpeRatio[(xmax - n)..xmax]; _colText[1, 7] = "Expected Sharpe Ratio";
                _data[11][1] = cd.Inheritance[(xmax - n)..xmax]; _colText[1, 11] = "Inheritance*100";
                _data[12][1] = cd.ExpectedRealInterestRate[(xmax - n)..xmax]; _colText[1, 12] = "Expected Real InterestRate";
                _data[13][1] = cd.ExpectedInterestRate[(xmax - n)..xmax]; _colText[1, 13] = "Expected Interest Rate";

                _data[11][2] = cd.Profit[(xmax - n)..xmax]; _colText[2, 11] = "Profit*40";
                _data[5][2] = cd.nJobFromUnemployment[(xmax - n)..xmax]; _colText[2, 5] = "New job from unemployment";

                _data[5][3] = cd.nJobFromJob[(xmax - n)..xmax]; _colText[3, 5] = "New job from job";

                Settings settings = _simulation.Settings;

                for (int i = 0; i < n_charts; i++)
                {
                    var s = _formsPlot[i].Plot.AddSignal(_data[i][0], 12);
                    s.OffsetX = 1.0 * (xmax - n) / 12;
                    s.LineColor = _cols[0];
                    s.MarkerSize = 0;
                    s.LineWidth = 0.1;

                    if (_data[i].Length > 1)
                    {
                        s = _formsPlot[i].Plot.AddSignal(_data[i][1], 12);
                        s.LineColor = _cols[1];
                        s.MarkerSize = 0;
                        s.LineWidth = 0.1;
                        s.OffsetX = 1.0 * (xmax - n) / 12;

                    }

                    if (_data[i].Length > 2)
                    {
                        s = _formsPlot[i].Plot.AddSignal(_data[i][2], 12);
                        s.LineColor = _cols[2];
                        s.MarkerSize = 0;
                        s.LineWidth = 0.1;
                        s.OffsetX = 1.0 * (xmax - n) / 12;

                    }

                    if (_data[i].Length == 4)
                    {
                        s = _formsPlot[i].Plot.AddSignal(_data[i][3], 12);
                        s.LineColor = _cols[3];
                        s.MarkerSize = 0;
                        s.LineWidth = 0.1;
                        s.OffsetX = 1.0 * (xmax - n) / 12;
                    }

                    if (_firstChart)
                    {
                        _formsPlot[i].Plot.AddHorizontalLine(0, Color.Black);
                        _formsPlot[i].Plot.Title(title[i]);

                    }

                    _formsPlot[i].Plot.SetAxisLimitsX(1.0 * xmin / 12, 1.0 * xmax / 12);

                    if (xmin < settings.BurnInPeriod3)
                    {
                        _formsPlot[i].Plot.AddVerticalLine(settings.BurnInPeriod1 / 12, _colorMAKRO, (float)0.1, LineStyle.Dot);
                        _formsPlot[i].Plot.AddVerticalLine(settings.BurnInPeriod2 / 12, _colorMAKRO, (float)0.1, LineStyle.Dot);
                        _formsPlot[i].Plot.AddVerticalLine(settings.BurnInPeriod3 / 12, _colorMAKRO, (float)0.1, LineStyle.Dot);
                    }

                    if (_simulation.Time.Now < 12 * 70)
                    {
                        if (i != 13)
                            _formsPlot[i].Plot.AxisAutoY();

                        if (i == 7) _formsPlot[i].Plot.SetAxisLimitsY(-0.8, 0.8);  // Sharpe Ratio 
                        if (i == 10) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.01);   // Wage inflation

                    }

                    int[] fixedWidth = new int[] { 2, 7, 8 };

                    if (_simulation.Time.Now > 12 * 70 & _simulation.Time.Now < 12 * 75)
                    {
                        if (i == 1) _formsPlot[i].Plot.SetAxisLimitsY(0, 70);         // New and closed firms
                        if (i == 2) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.5);       // Stock / Production
                        if (i == 3) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.1);        // Unemployment rate
                                                                                      //if (i == 5) _formsPlot[i].Plot.SetAxisLimitsY(0, 10);        // New jobs
                        if (i == 6) _formsPlot[i].Plot.SetAxisLimitsY(0, 1200);      // Number of firms
                        if (i == 7) _formsPlot[i].Plot.SetAxisLimitsY(-0.1, 0.1);     // Sharpe ratio
                        if (i == 8) _formsPlot[i].Plot.SetAxisLimitsY(0, 0.5);        // Real wage
                        if (i == 10) _formsPlot[i].Plot.SetAxisLimitsY(-0.05, 0.1);  // Wage inflation        
                        if (i == 11) _formsPlot[i].Plot.SetAxisLimitsY(0, 100000);    // Real Wealth
                        if (i == 12) _formsPlot[i].Plot.SetAxisLimitsY(-0.05, 0.7);   // Real interest rate
                        if (i == 13) _formsPlot[i].Plot.SetAxisLimitsY(-0.05, 0.7);   // Interest rate
                        if (i == 14) _formsPlot[i].Plot.SetAxisLimitsY(-0.1, 0.1);   // Inflation
                        if (i == 15) _formsPlot[i].Plot.SetAxisLimitsY(-0.05, 0.1);   // Real wage inflation
                        if (i == 15) _formsPlot[i].Plot.AddHorizontalLine(0.02, Color.Black, (float)0.1, LineStyle.Dot);

                        //_formsPlot[i].Plot.AxisAutoY();

                    }
                    else if (_simulation.Time.Now > 12 * (125 + 5) & !fixedWidth.Contains(i))
                    {
                        _formsPlot[i].Plot.AxisAutoY();
                    }

                    _formsPlot[i].Plot.AddVerticalLine(1.0 * settings.ShockPeriod / 12, _colorDREAM, (float)0.1, LineStyle.Dot);

                }

                _firstChart = false;
                for (int i = 0; i < n_charts; i++)
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
                _tweakForm = new TweakForm(_simulation);
                _tweakForm.StartPosition = FormStartPosition.Manual;
                var pixBoxLocation = pictureBoxDREAM.Location;
                _tweakForm.Location = new Point(pixBoxLocation.X+145, pixBoxLocation.Y+10);
                _tweakFormsInitialized = true;

            }

            if (_tweakForm != null)
                _tweakForm.ShowDialog();


        }
        void openHistogramForm()
        {
            _histogramFormsVisible = true;

            if (!_histogramFormsInitialized)
            {
                _histogramForm = new HistogramForm();
                //_histogramForm.StartPosition = FormStartPosition.Manual;
                _histogramForm.BackColor = Color.White;
                _histogramFormsInitialized = true;
                _histogramForm.FormClosed += new FormClosedEventHandler(histogramFormClosed);

            }

            if (_histogramForm != null)
                _histogramForm.ShowDialog();

        }
        void histogramFormClosed(object sender, FormClosedEventArgs e)
        {
            _histogramFormsVisible = false;
        }
        private void tweakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openTweakForm();
        }
        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("View settings..");
        }
        public Simulation Simulation
        {
            get { return _simulation; }
            set
            {
                _simulation = value;
                _time = _simulation.Time;
                _settings = _simulation.Settings;
                _UIChartTimeWindow0 = _settings.UIChartTimeWindow;
            }
        }
        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            var sim = new SimulationRunner(false, new WinFormElements(this, e));

        }
        private void MainFormUI_Paint(object sender, PaintEventArgs e)
        {
            //e.Graphics.Clear(Color.White);
            //e.Graphics.DrawImage(_bitmapMain, 0, 0);
        }
        private void viewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            backgroundWorker.CancelAsync();

        }
        private void pictureBoxDREAM_MouseDown(object sender, MouseEventArgs e)
        {
            //if (_simulation.Time.Now > 12 * 70)
            //    for(int i = 0; i < _formsPlot.Count(); i++)
            //        control_MouseClick(_formsPlot[i], null);


        }

        public bool NeedMicroData
        {
            get { return _histogramFormsVisible; }
        }

        public ChartData ChartData
        {
            get { return _chartData; }
        }



    }
    public class ChartData
    {
        public double[] nFirms, nHouseholds, Sales, RealWage, Price, Employment, Consumption, Production,
            SharpeRatio, ExpectedSharpeRatio, UnemploymentRate, ConsumptionLoss, Stock, Wealth, LaborSupplyProductivity,
            //ProfitPerHousehold, 
            InterestRate, Profit, Inflation, RealWageInflation, ExpectedInterestRate,
            InvestorWealth, InvestorTakeOut, InvestorWealthTarget, InvestorIncome, InvestorPermanentIncome,
            RealInterestRate, ExpectedRealInterestRate, Wage, WageInflation, NewFirms, ClosedFirms, 
            nJobFromUnemployment, nJobFromJob, nJobFromUnemploymentAdvertise, nJobFromJobAdvertise, 
            Inheritance, Extra;

        public int Wait;
        public MicroData MicroData;

        public ChartData(int n)
        {
            MicroData = new MicroData();
            
            nFirms = new double[n];
            NewFirms = new double[n];
            ClosedFirms = new double[n];
            nHouseholds = new double[n];
            Sales = new double[n];
            RealWage = new double[n];
            Price = new double[n];
            Inflation = new double[n];
            Wage = new double[n];
            WageInflation = new double[n];
            Employment = new double[n];
            Consumption = new double[n];
            Production = new double[n];
            SharpeRatio = new double[n];
            ExpectedSharpeRatio = new double[n];
            UnemploymentRate = new double[n];
            ConsumptionLoss = new double[n];
            Stock = new double[n];
            Wealth = new double[n];
            LaborSupplyProductivity = new double[n];
            //ProfitPerHousehold = new double[n];
            Profit = new double[n];
            InterestRate = new double[n];
            ExpectedInterestRate = new double[n];
            RealInterestRate = new double[n];
            ExpectedRealInterestRate = new double[n];
            RealWageInflation = new double[n];
            InvestorWealth = new double[n];
            InvestorWealthTarget = new double[n];
            InvestorTakeOut = new double[n];
            InvestorIncome = new double[n];
            InvestorPermanentIncome = new double[n];
            nJobFromUnemployment = new double[n];
            nJobFromJob = new double[n];
            nJobFromUnemploymentAdvertise = new double[n];
            nJobFromJobAdvertise = new double[n];
            Inheritance = new double[n];
            Extra = new double[n];

        }
    }
    public class MicroData
    {
        public double[] Productivity, Profit, Production, Age, Employment, Price, Wage;
        public int Wait;
    }


}
#endif
