using Dream.AgentClass;
using Dream.IO;

namespace Dream.Models.WinSOE
{

    public enum EConsoleOutput
    {
        TimePerYear,
        EventDistribution
    }
   
    public class Timer
    {         
        DateTime _t;
        TimeSpan _t_used;

        public Timer()
        {
            _t_used = TimeSpan.Zero;
            _t = DateTime.Now;
        }
        public void Start()
        {
            _t = DateTime.Now;
        }
        public TimeSpan Stop(bool reset=false)
        {
            _t_used += DateTime.Now - _t;
            TimeSpan tu = _t_used;
            if(reset)
                _t_used = TimeSpan.Zero;
            return tu;
        }
        public void Reset()
        {
            _t_used = TimeSpan.Zero;
            _t = DateTime.Now;
        }

        public TimeSpan TimeUsed
        {
            get { return _t_used; }
        }
    }   
    
    public class Simulation : Agent
    {
        #region Static private fields
        [ThreadStatic] static Simulation _instance;
        #endregion

        #region Private fields
        Time _time;
        Statistics _statistics;
        Agents<Household> _households;
        Agents<Agent> _tools;
        Agents<Agent> _sectors;
        Settings _settings;
        Random _random;
        int _seed = 0;
        PublicSector _publicSector;
        Forecaster _forecaster;
        double _nFirmNewTotal = 0;
        double[] _nFirmNew;
        double[][] _nFirmNewHistory;
        Dictionary<int, Firm> _firmDict;
        Agents<Firm>[] _sectorList = null;
        Firm[] _randomFirm = null;
        Household _randomHousehold;
        Investor _investor;
        DateTime _t0;
        WinFormElements _winFormElements;
        bool _baseRun;
        double _investorProfitSensitivity;
        double _shockSign = 1;
        Timer _timer = new Timer();
        Timer _t_periodStart = new Timer();
        Timer _t_update = new Timer();
        Timer _t_shopping = new Timer();
        Timer _t_periodEnd = new Timer();
        Timer _t_randomize = new Timer();
        #endregion

        public Simulation(Settings settings, Time time, WinFormElements winFormElements)
        {
            //if (_instance != null)
            //    throw new Exception("Simulation object is singleton");

            _settings = settings;
            _time = time;

#if WIN_APP

            // Hand shake with UI
            _winFormElements = winFormElements;
            if(_settings.SaveScenario)
            {
                // First backgroundworker is communicating with MainFormUI 
                if (winFormElements!=null)
                    _winFormElements.MainFormUI.Simulation = this;
            }
            else
                _winFormElements.MainFormUI.Simulation = this;
#endif

            if (_settings.RandomSeed > 0)
            {
                _random = new Random(_settings.RandomSeed);     // The one and only Random object
                Agent.RandomSeed = _settings.RandomSeed;
                _seed = _settings.RandomSeed;
            }
            else
            {
                _random = new Random();                      // We need to know the seed, even when we havent defined it
                _seed = _random.Next();
                _random = new Random(_seed);                // Overwrite _random with seeded 
                Agent.RandomSeed = _seed;
            }

            _nFirmNewHistory = new double[(1 + _settings.EndYear-_settings.StartYear)*_settings.PeriodsPerYear][];
            for(int i = 0; i < _nFirmNewHistory.Length; i++)
                _nFirmNewHistory[i] = new double[_settings.NumberOfSectors];   
           
            _settings.RandomSeed = _seed; // Save the seed in the settings so that it is saved in the json-file 


            _instance = this;

            _statistics = new Statistics();
            _publicSector = new PublicSector(); // Not used
            _forecaster = new Forecaster();     // Not used
            _investor = new Investor(_settings.InvestorInitialWealth,0);
            _households = new Agents<Household>();
            _tools = new Agents<Agent>();

            _sectors = new Agents<Agent>();

            this.AddAgent(_tools);
            this.AddAgent(_households);
            this.AddAgent(_sectors);
            this.AddAgent(_investor);
            this.AddAgent(_publicSector);

            _sectorList = new Agents<Firm>[_settings.NumberOfSectors];
            _randomFirm = new Firm[_settings.NumberOfSectors];
            for (int i = 0; i < _settings.NumberOfSectors; i++)
            {
                Agents<Firm> sector = new Agents<Firm>();
                // 'sector' is placed in 2 lists: the Agent-linked-list _sectors and the C#-list _sectorList
                _sectors += sector;         // This is for the agent tree to work
                _sectorList[i] = sector;    // This is for looking up firms

            }

            _tools += _statistics;
            _tools += _forecaster;
            
            if(settings.LoadDatabase)
            {
                throw new NotImplementedException();
                
                #region Loating database
                Console.WriteLine("LoadDatabase..");


                TabFileReader file = new TabFileReader(_settings.OutputDir + "\\db_firms.txt");

                _firmDict = new();
                
                while(file.ReadLine())
                {
                    int id = file.GetInt32("ID");
                    Firm f = new(file);
                    _firmDict.Add(id, f);

                }
                file.Close();

                file = new TabFileReader(_settings.OutputDir + "\\db_households.txt");

                while (file.ReadLine())
                    _households += new Household(file);

                file.Close();
                #endregion
            }
            else
            {
                int n_perSector = (int)(settings.NumberOfFirms / settings.NumberOfSectors);

                for (int s = 0; s < settings.NumberOfSectors; s++)
                {
                    for (int i = 0; i < n_perSector; i++)
                    {

                        List<Household> hs = new();
                        for (int j = 0; j < settings.NumberOfHouseholdsPerFirm; j++)
                        {
                            Household h = new();
                            _households += h;
                            hs.Add(h);
                        }

                        Firm f = new(hs, s); //Allocate firm
                        _sectorList[s] += f;
                        foreach (Household h in hs)
                            h.Communicate(ECommunicate.Initialize, f); // Tell households where they are employed

                    }
                }
            }

            _nFirmNew = new double[_settings.NumberOfSectors];           
            
            EventProc(Event.System.Start);

        }

        public override void EventProc(int idEvent)
        {
            switch (idEvent)
            {
                case Event.System.Start:
                    base.EventProc(idEvent);
                    // Event pump
                    do
                    {
                        _t_periodStart.Start();
                        this.EventProc(Event.System.PeriodStart);
                        _t_periodStart.Stop();

                        _t_update.Start();
                        this.EventProc(Event.Economics.Update);
                        _t_update.Stop();

                        _t_shopping.Start();
                        for (int i = 0; i < _settings.HouseholdNumberShoppingsPerPeriod; i++)
                            _households.EventProc(Event.Economics.Shopping);
                        _t_shopping.Stop();

                        _t_periodEnd.Start();
                        this.EventProc(Event.System.PeriodEnd);
                        _t_periodEnd.Stop();

                        _t_randomize.Start();
                        _households.RandomizeAgents();
                        foreach (Agent firms in _sectors)
                            firms.RandomizeAgents();
                        _t_randomize.Stop();

                        if (!_settings.SaveScenario)
                            if (testCancel())
                                break;

                    } while (_time.NextPeriod());

                    _t0 = DateTime.Now;
                    this.EventProc(Event.System.Stop);
                    break;

                case Event.System.PeriodStart:
                    //_statistics.Communicate(EStatistics.FirmNew, _nFirmNewTotal);

                    if (_time.Now % _settings.PeriodsPerYear == 0)  // Once a year
                    {

                        if(_settings.ConsoleOutput==EConsoleOutput.EventDistribution)
                        {
                            double d_periodStart = _t_periodStart.TimeUsed.TotalMilliseconds;
                            double d_update = _t_update.TimeUsed.TotalMilliseconds;
                            double d_shoppping = _t_shopping.TimeUsed.TotalMilliseconds;
                            double d_periodEnd = _t_periodEnd.TimeUsed.TotalMilliseconds;
                            double d_randomize = _t_randomize.TimeUsed.TotalMilliseconds;
                            double tot = d_periodStart + d_update + d_shoppping + d_periodEnd + d_randomize;
                            d_periodStart = 100 * d_periodStart / tot;
                            d_update = 100 * d_update / tot;
                            d_shoppping = 100 * d_shoppping / tot;
                            d_periodEnd = 100 * d_periodEnd / tot;
                            d_randomize = 100 * d_randomize / tot;

                            if (_time.Now % (25 * _settings.PeriodsPerYear) == 0)  // Every 25 years
                            {
                                Console.WriteLine("----------------------------------------------------------------");
                                Console.WriteLine("\t\tSTART\tUPDATE\tSHOP\tEND\tRAND\tdTIME");
                                Console.WriteLine("----------------------------------------------------------------");
                            }

                            Console.WriteLine("{0}\t{1}\t{2:#.#}\t{3:#.#}\t{4:#.#}\t{5:#.#}\t{6:#.#}\t{7:#.####}", _settings.Shock, _time.Now / 12,
                            d_periodStart,
                            d_update,
                            d_shoppping,
                            d_periodEnd,
                            d_randomize,
                            (DateTime.Now - _t0).TotalSeconds);

                            _t_periodStart.Reset();
                            _t_update.Reset();
                            _t_shopping.Reset();
                            _t_periodEnd.Reset();
                            _t_randomize.Reset();
                        }

                        if (_settings.ConsoleOutput == EConsoleOutput.TimePerYear)
                        {
                            Console.WriteLine("{0}, Year: {1}, Time per year: {2}", _settings.Shock, _time.Now / 12, DateTime.Now - _t0);
                        }

                        _t0 = DateTime.Now;


                    }

                    if (_time.Now==_settings.BurnInPeriod1)
                    {
                        Console.WriteLine("-------------------------------------------------------------------------------");
                        Console.WriteLine("--------------------------- END OF BURN-IN PERIOD 1 ---------------------------");
                        Console.WriteLine("-------------------------------------------------------------------------------");
                    }

                    if (_time.Now == _settings.BurnInPeriod2)
                    {
                        Console.WriteLine("===============================================================================");
                        Console.WriteLine("=========================== END OF BURN-IN PERIOD 2 ===========================");
                        Console.WriteLine("===============================================================================");
                    }

                    base.EventProc(idEvent);
                    break;

                case Event.System.PeriodEnd:
                    base.EventProc(idEvent);
                    // New households
                    for (int i = 0; i < _settings.HouseholdNewBorn; i++)
                        _households += new Household();

                    _investorProfitSensitivity = _settings.InvestorProfitSensitivity;
                    if (_time.Now < _settings.BurnInPeriod3)
                        _investorProfitSensitivity = _settings.InvestorProfitSensitivityBurnIn;

                    // After burn-in-stuff    
                    if (_time.Now == _settings.BurnInPeriod1)
                    {
                        _settings.FirmStartNewFirms = true;
                        _settings.FirmStartupPeriod = 6;
                        _settings.FirmStartupEmployment = 10;  
                    }

                    // Shock: 10% stigning i arbejdsudbud 
                    if (_time.Now == _settings.ShockPeriod)
                    {
                        if (_settings.Shock == EShock.LaborSupply)
                            for (int i = 0; i < _settings.ShockSize * _households.Count; i++)
                                _households += new Household();
                    }

                    // Investor
                    if (_time.Now == _settings.BurnInPeriod3)
                        _investor.Active = true;

                    if (_settings.FirmStartNewFirms)
                        _investor.Invest();

                    break;
                    
                    if (_settings.FirmStartNewFirms)
                    {
                        if (_time.Now < _settings.BurnInPeriod2)
                            _nFirmNewTotal = _settings.InvestorInitialInflow;
                        else
                        {                            

                            double minRealReturn = Math.Pow(1.02, 1/12) - 1;
                            double corr = Math.Pow(1.02, 1.0*_time.Now/12);

                            //double minNewFirms = 50.0;
                            double gamma = 0; //0.5
                            if (_statistics.ExpectedRealInterestRate >= minRealReturn)
                                gamma = 0.5 * Math.Pow(_statistics.ExpectedRealInterestRate - minRealReturn, 0.7);

                            int zz=0;
                            if(_time.Now>12*400)
                            {
                                zz = 0;
                            }
                                                     
                            //if(_statistics.ExpectedRealInterestRate>=minRealReturn)
                            //_nFirmNewTotal += 0.5 * _statistics.ExpectedSharpRatioTotal / _statistics.MarketPriceTotal / corr;
                            _nFirmNewTotal += _investorProfitSensitivity * _statistics.ExpectedSharpRatioTotal * _nFirmNewTotal;
                            //_nFirmNewTotal += gamma * _statistics.ExpectedSharpRatioTotal * _nFirmNewTotal;

                            //if (_statistics.ExpectedRealInterestRate < minRealReturn)
                            //    _nFirmNewTotal = 0.98 * _nFirmNewTotal;

                            if (_nFirmNewTotal<0)
                                _nFirmNewTotal = 0;

                        }
                                             
                        if (_nFirmNewTotal > 0)
                        {
                            if (_time.Now < _settings.BurnInPeriod2)
                            {
                                int n = _random.NextInteger(_nFirmNewTotal);
                                for (int i = 0; i < n; i++)
                                {
                                    // Random sector
                                    int sector = _random.Next(_settings.NumberOfSectors);                                    
                                    _sectorList[sector] += new Firm(sector);
                                }
                            }
                            else  // If multiple sectors
                            {
                                
                                double kappa = 0.0;  //0.5   !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                                double sum = 0;
                                for (int i = 0; i < _settings.NumberOfSectors; i++)
                                    sum += _nFirmNew[i] * Math.Exp(kappa * (_statistics.ExpectedSharpRatio[i] - _statistics.ExpectedSharpRatioTotal));

                                for (int i = 0; i < _settings.NumberOfSectors; i++)
                                {
                                    double dd = _nFirmNew[i];
                                    double d = _nFirmNewTotal * _nFirmNew[i] *
                                        Math.Exp(kappa * (_statistics.ExpectedSharpRatio[i] - _statistics.ExpectedSharpRatioTotal)) / sum;
                                    _nFirmNew[i] = _random.NextInteger(d);
                                    //_nFirmNew[i] = _random.NextInteger(_nFirmNewTotal);  //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                                    for (int j = 0; j < (int)_nFirmNew[i]; j++)
                                        _sectorList[i] += new Firm(i);

                                    _nFirmNewHistory[_time.Now][i] = _nFirmNew[i];
                                }
                            }
                        }
                    }                   
                    break;

                case Event.System.Stop:
                    base.EventProc(idEvent);
                    break;

                default:
                    base.EventProc(idEvent);
                    break;
            }
        }

        #region Internal methods
        #region GetRandom..
        public Household GetRandomHousehold()
        {
            if (_randomHousehold != null)
            {
                if (_households.Count == 1)
                    return _randomHousehold;
                _randomHousehold = (Household)_randomHousehold.NextAgent;
            }

            if (_randomHousehold == null)
            {
                
                // No randomization!!!!
                //_households.RandomizeAgents();
                _randomHousehold = (Household)_households.FirstAgent;
            }
            return _randomHousehold;
        }

        public Household[] GetRandomHouseholds(int n)
        {
            if (n < 1) return null;

            Household[] lst = new Household[n];
            for (int i = 0; i < n; i++)
                lst[i] = GetRandomHousehold();

            return lst;
        }

        #region GetRandomFirmsFromHouseholds
        /// <summary>
        /// Get random firm households: The firm the household use to buy goods from a given sector 
        /// </summary>
        /// <param name="n">Number of firms</param>
        /// <param name="sector">Sector</param>
        /// <returns>Array<Firm></returns>
        public Firm[] GetRandomFirmsFromHouseholdsGood(int n, int sector)
        {

            if (n < 1) return null;

            Firm[] fs = new Firm[n];

            int i = 0;
            while (i < n)
            {
                var h = GetRandomHousehold();
                if (h.FirmShopArray(sector) != null)
                {
                    fs[i] = h.FirmShopArray(sector);
                    i++;
                }
            }

            return fs;

        }
        /// <summary>
        /// Get random firm households: The firm where the household works 
        /// </summary>
        /// <param name="n">Number of firms</param>
        /// <returns>Array<Firm></returns>
        /// <returns></returns>
        public Firm[] GetRandomFirmsFromEmployedHouseholds(int n)
        {
            if (n < 1) return null;

            Firm[] fs = new Firm[n];

            int i=0;
            while(i<n)
            {
                var h = GetRandomHousehold();
                if(h.FirmEmployment!=null)
                {
                    fs[i] = h.FirmEmployment;
                    i++;
                }
            }

            return fs;
        }


        #endregion

        #region GetRandomFirm
        public Firm GetRandomFirm(int sector, bool randomize=true)
        {
            // Initialization
            if (_randomFirm[sector] == null)
                _randomFirm[sector] = (Firm)_sectorList[sector].FirstAgent;

            if (_sectorList[sector].Count == 1)
                return _randomFirm[sector];

            _randomFirm[sector] = (Firm)_randomFirm[sector].NextAgent;
            if (_randomFirm[sector] == null) // If no more firms i list
            {
                // No randomization!!!!
                //if (randomize)
                //    _sectorList[sector].RandomizeAgents();

                _randomFirm[sector] = (Firm)_sectorList[sector].FirstAgent;
            }

            return _randomFirm[sector];

        }
        
        public Firm GetRandomFirmJump(int sector)
        {
            if (_randomFirm[sector] == null)
                _randomFirm[sector] = (Firm)_sectorList[sector].FirstAgent;

            int nJumpMax = 10; //(int)(0.01 * _sectorList[sector].Count);

            if (_randomFirm[sector] != null)
            {
                if (_sectorList[sector].Count == 1)
                    return _randomFirm[sector];

                int nJump = _random.Next(nJumpMax) + 1;
                _randomFirm[sector] = (Firm)_randomFirm[sector].Jump(nJump);
            }

            return _randomFirm[sector];

        }
        public Firm GetRandomOpenFirm(int sector, int numberOfDraws)
        {

            if (_time.Now < _settings.BurnInPeriod2)
                return GetRandomFirm(sector);  // HACK to get started!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            Firm f = null;
            bool open = false;
            int i = 0;
            while (!open)
            {
                f = GetRandomFirm(sector);
                open = f.Open;
                i++;
                if (i > numberOfDraws)
                    return null;
            }

            return f;

        }

        #endregion

        #region GetRandomFirms
        public Firm[] GetRandomFirms(int n, int sector)
        {
            if (n < 1) return null;

            Firm[] lst = new Firm[n];
            for (int i = 0; i < n; i++)
                lst[i] = GetRandomFirm(sector);

            return lst;
        }
        
        public Firm[] GetRandomOpenFirms(int n, int sector, int numberOfDraws)
        {
            if (n < 1) return null;

            Firm[] lst = new Firm[n];
            for (int i = 0; i < n; i++)
                lst[i] = GetRandomOpenFirm(sector, numberOfDraws);

            return lst;
        }
        public Firm[] GetRandomFirmsAllSectors(int n) 
        {
            if (n < 1) return null;

            Firm[] lst = new Firm[n];
            for (int i = 0; i < n; i++)
            {
                int sector = _random.Next(_settings.NumberOfSectors);
                lst[i] = GetRandomFirm(sector);
            }

            return lst;
        }
        
        public Firm GetNextFirmWithGoods(double budget, int sector, int max_n)
        {
            int i = 0;

            while (i < max_n)
            {
                Firm f = GetRandomFirm(sector);

                if(f.Open)
                {
                    if ((f.Production - f.Sales) * f.Price > budget)
                        return f;

                    // Maybe i++ here????
                }

                i++;

            }

            return null;
            
        }


        #endregion
        #endregion

        public Firm GetFirmFromID(int ID)
        {
            if(_firmDict!=null)
            {
                try 
                {
                    return _firmDict[ID];
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }
        bool testCancel()
        {
#if WIN_APP
            if(_winFormElements!=null)
            {
                if (_winFormElements.MainFormUI.backgroundWorker.CancellationPending == true)
                {
                    this.EventProc(Event.System.Stop);
                    //_winFormElements.DoWorkEventArgs.Cancel = true;
                    return true;
                }
            }
#endif
            return false;

        }
        public void ShockNow(EShock shock, double shockSign=1)
        {
            _settings.Shock = shock;
            _shockSign = shockSign;
            _settings.ShockPeriod = _time.Now+1;
        }
        public void ResetShockSign()
        {
            _shockSign = 1;
        }

#endregion

        #region Public properties
        public Agents<Firm> Sector(int sector)
        {
            return _sectorList[sector];
        }
        public Settings Settings
        {
            get { return _settings; }
            set { _settings = value; }
        }
        public Investor Investor { get { return _investor; } }
        public Random Random
        {
            get { return _random; }
        }
        public int Seed
        {
            get { return _seed; }
        }
        public Time Time
        {
            get { return _time; }
        }
        public Timer Timer
        {
            get { return _timer; }
        }
        public static Simulation Instance
        {
            get { return _instance; }
        }
        public Agents<Household> Households
        {
            get { return _households; }
        }
        public Statistics Statistics
        {
            get { return _statistics; }
        }
        public PublicSector PublicSector
        {
            get { return _publicSector; }
        }
        public Forecaster Forecaster
        {
            get { return _forecaster; }
        }
        public Agents<Agent> Tools
        {
            get { return _tools; }
            //set { _tools = value; }
        }
        public WinFormElements WinFormElements
        {
            get { return _winFormElements; }
        }
        public double[][] NFirmNewHist
        {
            get { return _nFirmNewHistory; }
        }
        public double ShockSign
        {
            get { return _shockSign; }
        }
        public Agents<Firm>[] SectorList
        {
            get { return _sectorList; }

        }

        #endregion

        #region Text
        // Fase 1: Markedsfase 
        // 	 Fast antal virksomheder
        // 	 Voksende antal husholdninger (for speed)
        // 	 Initiale husholdninger har arbejde - men skal selv finde handelssted
        // 	 Nye husholdninger er aktive på arbdejds- og varemarked
        // 	 Profit betales ikke til husholdninger
        // 	
        // Fase 2:
        // 	 Virksomheder lukker hvis de giver underskud
        // 	 Konstant inflow af nye virksomheder
        // 	
        // Fase 3:
        // 	 Investor bliver aktiv: Profit betales til husholdninger (hvis positiv)
        // 	 Investor vælger antal nye virksomheder (eksogen rente)
        //
        // Fase 4:
        //   Fri kørsel: endogen rente
        #endregion


    }
}
