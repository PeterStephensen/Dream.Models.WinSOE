﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Dream.AgentClass;
using Dream.IO;

namespace Dream.Models.WinSOE
{
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
        Dictionary<int, Firm> _firmDict;
        Agents<Firm>[] _sectorList = null;
        Firm[] _randomFirm = null;
        Household _randomHousehold;
        Investor _investor;
        DateTime _t0;
        WinFormElements _winFormElements;
        bool _baseRun;
        #endregion

        #region Constructors
        //public Simulation(Settings settings, Time time, WinFormElements winFormElements, bool baseRun)
        public Simulation(Settings settings, Time time, WinFormElements winFormElements)
        {
            _settings = settings;
            _time = time;

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

            //if (_settings.SaveScenario & _settings.Shock!=EShock.Nothing)
            //{
            //    string scnPath = _settings.ROutputDir + "\\scenario_info.txt";
            //    using (StreamReader sr = File.OpenText(scnPath))
            //    {
            //        sr.ReadLine();                          // Read first line
            //        _seed = Int32.Parse(sr.ReadLine());  // Seed on second line

            //        _random = new(_seed);                    // Overwrite _random with seeded 
            //        Agent.RandomSeed = _seed;

            //    }
            //}
            
            _settings.RandomSeed = _seed; // Save the seed in the settings so that it is saved in the json-file 

            //if (_instance != null)
            //    throw new Exception("Simulation object is singleton");

            _instance = this;

            _statistics = new Statistics();
            _publicSector = new PublicSector(); // Not used
            _forecaster = new Forecaster();     // Not used
            _investor = new Investor(5000,0);
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


                TabFileReader file = new TabFileReader(_settings.ROutputDir + "\\db_firms.txt");

                _firmDict = new();
                
                while(file.ReadLine())
                {
                    int id = file.GetInt32("ID");
                    Firm f = new(file);
                    _firmDict.Add(id, f);

                }
                file.Close();

                file = new TabFileReader(_settings.ROutputDir + "\\db_households.txt");

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
        #endregion

        #region EventProc()
        public override void EventProc(int idEvent)
        {
            switch (idEvent)
            {
                case Event.System.Start:
                    #region Event.System.Start
                    base.EventProc(idEvent);
                    // Event pump
                    do
                    {
                        this.EventProc(Event.System.PeriodStart);
                        this.EventProc(Event.Economics.Update);
                        for (int i = 0; i < _settings.HouseholdNumberShoppingsPerPeriod; i++)
                            _households.EventProc(Event.Economics.Shopping);
                        this.EventProc(Event.System.PeriodEnd);

                        _households.RandomizeAgents();
                        foreach (Agent firms in _sectors)
                            firms.RandomizeAgents();

                        if (testCancel())
                            break;

                    } while (_time.NextPeriod());

                    _t0 = DateTime.Now;
                    this.EventProc(Event.System.Stop);
                    break;
                    #endregion

                case Event.System.PeriodStart:
                    #region Event.System.PeriodStart


                    _statistics.Communicate(EStatistics.FirmNew, _nFirmNewTotal);
                    
                    if (_time.Now % _settings.PeriodsPerYear == 0)  // Once a year
                    {
                        Console.WriteLine("***************************************** Time per year: {0}", DateTime.Now - _t0);
                        _t0 = DateTime.Now;
                    }

                    if(_time.Now==_settings.BurnInPeriod1)
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
                    #endregion

                case Event.System.PeriodEnd:
                    #region Event.System.PeriodEnd
                    base.EventProc(idEvent);
                    // New households
                    for (int i = 0; i < _settings.HouseholdNewBorn; i++)
                        _households += new Household();


                    if(_time.Now==_settings.BurnInPeriod2)
                    {
                        _investor.Active = true;
                        _settings.HouseholdProfitShare = 0;
                    }

                    if (_time.Now > _settings.BurnInPeriod2 & _time.Now < _settings.BurnInPeriod3)
                        _settings.HouseholdProfitShare += 1.0 / (_settings.BurnInPeriod3 - _settings.BurnInPeriod2);


                    // Shock: 10% stigning i arbejdsudbud 
                    if (_time.Now == _settings.ShockPeriod)
                    {
                        if(_settings.Shock==EShock.LaborSupply)
                            for (int i = 0; i < 0.1 * _households.Count; i++)
                                _households += new Household();
                    }

                    // After burn-in-stuff    
                    if (_time.Now == _settings.BurnInPeriod1)
                    {
                        //_settings.FirmDefaultProbability = 1.0 / (40 * 12);  // Expectet default age: 40 years
                        _settings.FirmDefaultProbability = 0; //!!!!!!!!!!!!!!!!!!!!
                        _settings.FirmStartNewFirms = true;
                        _settings.FirmStartupPeriod = 6;
                        _settings.FirmStartupEmployment = 10;  
                    }
                    
                    //PSP
                    //if (_time.Now == _settings.BurnInPeriod2)
                    //{
                    //    _settings.FirmWageMarkup = 0.1;
                    //    _settings.FirmPriceMarkdown = 0.1;

                    //}


                    // Investor
                    if (_settings.FirmStartNewFirms)
                    {

                        if (_time.Now < _settings.BurnInPeriod2)
                            _nFirmNewTotal = _settings.InvestorInitialInflow;
                        else
                        {
                            _nFirmNewTotal += _settings.InvestorProfitSensitivity * _statistics.PublicExpectedSharpRatioTotal * _nFirmNewTotal;
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
                                //int z = 0;
                                //if (_time.Now > 12 * 30)
                                //        z++;

                                double kappa = 0.5;

                                double sum = 0;
                                for (int i = 0; i < _settings.NumberOfSectors; i++)
                                    sum += _nFirmNew[i] * Math.Exp(kappa * (_statistics.PublicExpectedSharpRatio[i] - _statistics.PublicExpectedSharpRatioTotal));

                                for (int i = 0; i < _settings.NumberOfSectors; i++)
                                {
                                    double d = _nFirmNewTotal * _nFirmNew[i] *
                                        Math.Exp(kappa * (_statistics.PublicExpectedSharpRatio[i] - _statistics.PublicExpectedSharpRatioTotal)) / sum;
                                    _nFirmNew[i] = _random.NextInteger(d);
                                    for (int j = 0; j < (int)_nFirmNew[i]; j++)
                                        _sectorList[i] += new Firm(i);
                                }                               
                            }
                        }
                    }                   
                    break;
                    #endregion

                case Event.System.Stop:
                    base.EventProc(idEvent);
                    break;

                default:
                    base.EventProc(idEvent);
                    break;
            }
        }
        #endregion

        #region Internal methods
        #region GetRandom..
        #region GetRandomHousehold
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

        #endregion

        #region GetRandomHouseholds
        public Household[] GetRandomHouseholds(int n)
        {
            if (n < 1) return null;

            Household[] lst = new Household[n];
            for (int i = 0; i < n; i++)
                lst[i] = GetRandomHousehold();

            return lst;
        }

        #endregion

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
        public Firm[] GetRandomFirmsFromHouseholdsEmployment(int n)
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

        #region GetFirmFromID()
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
        #endregion

        #region testCancel()
        bool testCancel()
        {
            if(_winFormElements!=null)
            {
                if (_winFormElements.MainFormUI.backgroundWorker.CancellationPending == true)
                {
                    this.EventProc(Event.System.Stop);
                    //_winFormElements.DoWorkEventArgs.Cancel = true;
                    return true;
                }
            }
            return false;

        }
        #endregion
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
        /// <summary>
        /// Seed used to initialize the Random object
        /// </summary>
        public int Seed
        {
            get { return _seed; }
        }

        public Time Time
        {
            get { return _time; }
        }


        public static Simulation Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// Groups
        /// </summary>
        public Agents<Household> Households
        {
            get { return _households; }
        }

        //public Agents<Firm> Firms
        //{
        //    get { return _firms; }
        //}


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
        #endregion

    }
}
