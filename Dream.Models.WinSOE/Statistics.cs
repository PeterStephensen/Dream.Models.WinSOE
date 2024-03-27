using System.Diagnostics;

using Dream.AgentClass;
using Dream.IO;

namespace Dream.Models.WinSOE
{

    #region FirmInfo Class
    /// <summary>
    /// This class contains information to the Statistics object
    /// </summary>
    public class FirmInfo
    {
        public int Age { get; set; }
        public int Sector { get; set; }
        public double Profit { get; set; }

        public FirmInfo(Firm firm)
        {
            Age = firm.Age;
            Sector = firm.Sector;
            Profit = firm.Profit;
        }
    }
    #endregion

    public class Statistics : Agent
    {

        #region Private fields
        Simulation _simulation;
        Settings _settings;
        Time _time;
        double[] _marketPrice, _marketWage;
        double[] _employment, _sales, _production;
        double _marketWageTotal = 0, _marketWageTotal0 = 0, _realwageInflation = 0;
        double _marketPriceTotal = 0, _marketPriceTotal0=0, _inflation=0;
        double _expectedInflation = 0, _expectedRealwageInflation = 0;
        double _realInterestRate=0, _expectedRealInterestRate=0;
        double _profitPerHousehold, _expProfit, _interestRate;
        double _avrProductivity = 0;
        StreamWriter _fileFirmReport;
        StreamWriter _fileHouseholdReport;
        StreamWriter _fileDBHouseholds;
        StreamWriter _fileDBFirms;
        StreamWriter _fileDBStatistics;
        StreamWriter _fileMacro;
        StreamWriter _fileSectors;
        StreamWriter _fileFirmApplications;
        double[] _sectorProductivity;
        double _expectedInterestRate;
        double _meanValue = 0;
        double _discountedProfits = 0;
        int _nFirmCloseNatural = 0, _nFirmCloseNegativeProfit = 0, _nFirmCloseTooBig = 0, _nFirmCloseZeroEmployment=0;
        double _nFirmNew = 0;
        double _expDiscountedProfits = 0;
        double _sharpeRatioTotal = 0;
        double _sigmaRiskTotal = 0;
        double _expSharpeRatioTotal = 0;
        double[] _sharpeRatio;
        double[] _sigmaRisk;
        double[] _expSharpeRatio;
        double _yr_consumption = 0; 
        int _yr_employment = 0;
        double _totalEmployment = 0;
        double _totalSales = 0;
        double _totalPotensialSales = 0;
        double _totalProduction = 0;
        int _scenario_id = 0;
        string _runName = "Base";
        double _laborSupplyProductivity = 0;  // Measured in productivity units
        int _n_laborSupply = 0;   // Measured in heads
        int _n_unemployed = 0;     // Measured in heads
        int _n_couldNotFindSupplier=0;
        List<FirmInfo> _firmInfo = new List<FirmInfo>();
        int _nChangeShopInSearchForShop = 0;
        int _nChangeShopInBuyFromShopNull = 0;
        int _nChangeShopInBuyFromShopLookingForGoods = 0;
        int _nCouldNotFindFirmWithGoods = 0;
        int _nBuyFromShop = 0;
        int _nSuccesfullTrade = 0;
        int _nZeroBudget = 0;
        int _nSuccesfullTradeNonZero = 0;
        double[] _priceMedian;
        double _wageMedian = 0;
        double _vb = 0; // Used en console output
        double _wage_lag = 0, _price_lag = 0;
        double _stock, _wealth;
        double _totalProfit;
        double _expextedWage = 1;
        double _expextedPrice = 1;
        double _growthPerPeriod;
        int _nJobFromUnemployment;
        int _nFromJob;
        int _nFromUnemploymentAdvertise;
        int _nFromJobAdvertise;
        int _nFired;
        double _shockSizeAbs = 0;
        double _macroProductivity0 = 0;
        double _inheritence = 0;

        int _nextUIChartUpdateTime = 0;
        int[,] _nFirmNewHistory;
        int[] _nFirmNewTotalHistory;

#if WIN_APP
        // Chart output
        ChartData _chartData;
        MicroData _histogramData;
#endif

        #endregion

        public Statistics()
        {
            _simulation = Simulation.Instance;
            _settings = _simulation.Settings;
            _time = _simulation.Time;

            _marketPrice = new double[_settings.NumberOfSectors];
            _marketWage = new double[_settings.NumberOfSectors];
            _employment = new double[_settings.NumberOfSectors];
            _sales = new double[_settings.NumberOfSectors];
            _production = new double[_settings.NumberOfSectors];
            _sectorProductivity = new double[_settings.NumberOfSectors];
            _sigmaRisk = new double[_settings.NumberOfSectors];
            _sharpeRatio = new double[_settings.NumberOfSectors];
            _expSharpeRatio = new double[_settings.NumberOfSectors];
            _priceMedian = new double[_settings.NumberOfSectors];

            for (int i = 0; i < _settings.NumberOfSectors; i++)
            {
                _marketPrice[i] = _settings.StatisticsInitialMarketPrice;
                _priceMedian[i] = _settings.StatisticsInitialMarketPrice;
                _marketWage[i] = _settings.StatisticsInitialMarketWage;
                _sectorProductivity[i] = 1.0;
            }
            _marketPriceTotal = _settings.StatisticsInitialMarketPrice;
            _marketWageTotal = _settings.StatisticsInitialMarketWage;
            _wageMedian = _settings.StatisticsInitialMarketWage;
            _expectedInterestRate = _settings.StatisticsInitialInterestRate;
            _expectedRealInterestRate = _settings.StatisticsInitialInterestRate;
            _growthPerPeriod = Math.Pow(1 + _settings.FirmProductivityGrowth, 1.0 / _settings.PeriodsPerYear) - 1;

            _nFirmNewTotalHistory = new int[(1 + _settings.EndYear - _settings.StartYear) * _settings.PeriodsPerYear];

            //var options = new JsonSerializerOptions { WriteIndented = true };
            //string sJson = JsonSerializer.Serialize(_settings, options);
            //File.WriteAllText(_settings.ROutputDir + "\\Settings.json", sJson);

            if (_settings.LoadDatabase)
            {
                TabFileReader file = new TabFileReader(_settings.ROutputDir + "\\db_statistics.txt");
                file.ReadLine();

                //_marketPrice = file.GetDouble("marketPrice");
                //_marketWage = file.GetDouble("marketWage");
                _expSharpeRatioTotal = file.GetDouble("expSharpeRatio");
                _sharpeRatioTotal = file.GetDouble("expSharpeRatio");

                file.Close();

            }

        }
        
        public override void EventProc(int idEvent)
        {
            
            switch (idEvent)
            {
                case Event.System.Start:
                    OpenFiles();
#if WIN_APP
                    _chartData = new ChartData(1 + _time.EndPeriod - _time.StartPeriod);
                    _histogramData = new MicroData();
#endif
                    break;

                case Event.System.PeriodStart:
                    if (_time.Now == _settings.StatisticsWritePeriode)
                    {

                        string path = _settings.ROutputDir + "\\db_households.txt";
                        if (File.Exists(path)) File.Delete(path);
                        _fileDBHouseholds = File.CreateText(path);
                        _fileDBHouseholds.WriteLine("ID\tAge\tFirmEmploymentID\tFirmShopID\tProductivity");

                        path = _settings.ROutputDir + "\\db_firms.txt";
                        if (File.Exists(path)) File.Delete(path);
                        _fileDBFirms = File.CreateText(path);
                        _fileDBFirms.WriteLine("ID\tAge\tphi0\texpPrice\texpWage\texpQuitters\texpApplications\texpPotentialSales\texpSales\tw\tp\tSales\tProfit");

                        path = _settings.ROutputDir + "\\db_statistics.txt";
                        if (File.Exists(path)) File.Delete(path);
                        _fileDBStatistics = File.CreateText(path);
                        _fileDBStatistics.WriteLine("expSharpeRatio\tmacroProductivity\tmarketPrice\tmarketWage");

                    }

                    if (_time.Now == _settings.StatisticsWritePeriode + 1)
                    {
                        if (_fileDBHouseholds != null)
                            _fileDBHouseholds.Close();

                        if (_fileDBFirms != null)
                            _fileDBFirms.Close();

                        if (_fileDBStatistics != null)
                            _fileDBStatistics.Close();
                    }

                    //Calculate Profit Per Household and Sharpe Ratios
                    _profitPerHousehold = 0;
                    double discountedProfitsTotal = 0;
                    double[] discountedProfits = new double[_settings.NumberOfSectors];
                    int[] nFirms = new int[_settings.NumberOfSectors];
                    _totalProfit = 0;

                    // Growth corrected real interest rate
                    double r = (1 + _expectedRealInterestRate) / (1 + _growthPerPeriod) - 1.0;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    //double r = _expectedRealInterestRate;
                    //double r = _expectedInterestRate;

                    //_nFirmNewTotalHistory
                    //double[] NFirmNewHistTotal = new double[(_settings.EndYear - _settings.StartYear)*_settings.PeriodsPerYear+1];
                    int t = _time.Now;
                    _nFirmNewTotalHistory[t] = 0;
                    if(t>0)
                        for (int i = 0; i < _settings.NumberOfSectors; i++)
                            _nFirmNewTotalHistory[t] += (int)_simulation.NFirmNewHist[t-1][i];

                    if (_nFirmNewTotalHistory[t] <= 0)
                        _nFirmNewTotalHistory[t] = _settings.InvestorInitialInflow;

                    //int zz = 0;
                    //if(_time.Now>12*60)
                    //    zz++;


                    foreach (var fi in _firmInfo)
                    {
                        _totalProfit += fi.Profit;

                        discountedProfitsTotal += (fi.Profit / Math.Pow(1 + r, fi.Age)) / _nFirmNewTotalHistory[_time.Now - fi.Age];
                        discountedProfits[fi.Sector] += (fi.Profit / Math.Pow(1 + r, fi.Age)) / _simulation.NFirmNewHist[_time.Now - fi.Age][fi.Sector];
                        //discountedProfitsTotal += (fi.Profit / Math.Pow(1 + r, fi.Age));
                        //discountedProfits[fi.Sector] += (fi.Profit / Math.Pow(1 + r, fi.Age));

                        nFirms[fi.Sector]++; 
                    }

                    double dpTotal = discountedProfitsTotal / _firmInfo.Count;
                    //double dpTotal = discountedProfitsTotal;

                    double[] dp = new double[_settings.NumberOfSectors];
                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                        dp[i] = discountedProfits[i] / nFirms[i];
                        //dp[i] = discountedProfits[i];

                    _sigmaRiskTotal = 0;
                    for (int i = 0; i < _settings.NumberOfSectors; i++) 
                        _sigmaRisk[i] = 0;
                    
                    foreach (var fi in _firmInfo)
                    {
                        if (_nFirmNewTotalHistory[_time.Now - fi.Age] > 0)
                        {
                            _sigmaRiskTotal += Math.Pow((fi.Profit / Math.Pow(1 + r, fi.Age)) / _nFirmNewTotalHistory[_time.Now - fi.Age] - dpTotal, 2);
                            _sigmaRisk[fi.Sector] += Math.Pow((fi.Profit / Math.Pow(1 + r, fi.Age)) / _simulation.NFirmNewHist[_time.Now - fi.Age][fi.Sector] - dp[fi.Sector], 2);

                            //_sigmaRiskTotal += Math.Pow((fi.Profit / Math.Pow(1 + r, fi.Age)) - dpTotal, 2);
                            //_sigmaRisk[fi.Sector] += Math.Pow((fi.Profit / Math.Pow(1 + r, fi.Age)) - dp[fi.Sector], 2);
                        }
                    }

                    _sigmaRiskTotal = Math.Sqrt(_sigmaRiskTotal / _firmInfo.Count);
                    for (int i = 0; i < _settings.NumberOfSectors; i++) _sigmaRisk[i] = Math.Sqrt(_sigmaRisk[i] / nFirms[i]);

                    _sharpeRatioTotal = _sigmaRiskTotal > 0 ? dpTotal / _sigmaRiskTotal : 0;
                    _expSharpeRatioTotal = _settings.StatisticsExpectedSharpeRatioSmooth * _expSharpeRatioTotal + (1 - _settings.StatisticsExpectedSharpeRatioSmooth) * _sharpeRatioTotal;

                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                    {
                        _sharpeRatio[i] = _sigmaRisk[i] > 0 ? dp[i] / _sigmaRisk[i] : 0;
                        _expSharpeRatio[i] = _settings.StatisticsExpectedSharpeRatioSmooth * _expSharpeRatio[i] + (1 - _settings.StatisticsExpectedSharpeRatioSmooth) * _sharpeRatio[i];
                    }

                    if (_time.Now > _settings.BurnInPeriod2)
                    {
                        _simulation.Investor.Iterate();
                        _profitPerHousehold = _simulation.Investor.TakeOut / _simulation.Households.Count;

                        double totWealth = 0;
                        foreach (Household h in _simulation.Households)
                            totWealth += h.Wealth;

                        if (totWealth > 0)
                            _interestRate = _simulation.Investor.TakeOut / totWealth;

                        if(_time.Now<_settings.BurnInPeriod3)
                            _interestRate = _settings.StatisticsInitialInterestRate;
                        //if(_interestRate<0)  
                        //    _interestRate = 0;   

                        // Simplification: Exogeneous interes rate
                        if (_settings.SimplificationInterestRate)
                            _interestRate = _settings.StatisticsInitialInterestRate;

                        //-----------------------------------------------------------------------------------------------
                        double smooth = 0.99;  //0.99
                        if (_time.Now > _settings.BurnInPeriod3)
                        {
                            _expectedInterestRate = smooth * _expectedInterestRate + (1 - smooth) * _interestRate;
                            _expectedInflation = smooth * _expectedInflation + (1 - smooth) * _inflation;
                            _expectedRealwageInflation = smooth * _expectedRealwageInflation + (1 - smooth) * _realwageInflation;
                            _expectedRealInterestRate = smooth * _expectedRealInterestRate + (1 - smooth) * _realInterestRate;

                            if (_expectedInterestRate<0)   //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! 
                                _expectedInterestRate = 0;

                        }
                        //-----------------------------------------------------------------------------------------------
                    }

                    //PSP
                    //for (int i = 0; i < _settings.NumberOfSectors; i++)
                    //    _expSharpeRatio[i] += 1 * Math.Pow(0.99, _time.Now);

                    //_expSharpeRatioTotal += 5 * Math.Pow(0.99, _time.Now);

                    //int z = 0;
                    //if (_time.Now > 12 * 30)
                    //    z++;

                    //_totalProfitFromDefaults = 0;
                    //_profitPerHousehold = 0;
                    _n_couldNotFindSupplier = 0;
                    _firmInfo = new List<FirmInfo>();

                    //_totalProfit = 0;

                    //_nFirmCloseNatural = 0;
                    //_nFirmCloseTooBig = 0;
                    //_nFirmCloseNegativeProfit = 0;
                    //_nFirmCloseZeroEmployment = 0;
                    //_nFirmNew = 0;
                    _nChangeShopInSearchForShop = 0;
                    _nChangeShopInBuyFromShopNull = 0;
                    _nChangeShopInBuyFromShopLookingForGoods = 0;
                    _nCouldNotFindFirmWithGoods = 0;
                    _nBuyFromShop = 0;
                    _nSuccesfullTrade = 0;
                    _nZeroBudget = 0;
                    _nSuccesfullTradeNonZero = 0;
                    
                    _nJobFromUnemployment = 0;
                    _nFromJob = 0;
                    _nFromUnemploymentAdvertise = 0;
                    _nFromJobAdvertise = 0;
                    _nFired = 0;

                    _inheritence = 0;
                    break;

                case Event.System.PeriodEnd:
                    #region Calculations
                    if (_time.Now == _settings.StatisticsWritePeriode)
                        Write();

                    // Profit income to households. What comes from defaults and deaths during Update
                    //_profitPerHousehold += _totalProfitFromDefaults / _simulation.Households.Count;  

                    // TWO LOOPS over firms !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    double totalRevenues = 0;
                    _stock = 0;
                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                    {
                        double meanWage = 0;
                        double meanPrice = 0;
                        //double discountedProfits = 0;
                        _employment[i] = 0;
                        _sales[i] = 0;

                        foreach (Firm f in _simulation.Sector(i))
                        {
                            meanWage += f.Wage * f.Employment;
                            meanPrice += f.Price * f.Sales;
                            _employment[i] += f.Employment;
                            _sales[i] += f.Sales;
                            totalRevenues += f.Price * f.Sales;
                            _production[i] += f.Production;
                            _stock += f.Stock;
                        }
                        
                        if (meanWage > 0)
                            _marketWage[i] = meanWage / _employment[i];

                        if (meanPrice > 0 & _sales[i] > 0)
                            _marketPrice[i] = meanPrice / _sales[i];
                    }
                    
                    _meanValue = 0;
                    _discountedProfits = 0;
                    _totalSales = 0;
                    _totalPotensialSales = 0;
                    _totalEmployment = 0;
                    _totalProduction = 0;
                    //_totalProfit = _totalProfitFromDefaults; 
                    //double totProfit = 0;
                    double mean_age = 0;
                    double tot_vacancies = 0;
                    double meanWageTot = 0;
                    double meanPriceTot = 0;
                    double nEmployment = 0;
                    //double potentialSales = 0;
                    int no = 0;
                    int ok = 0;
                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                        foreach (Firm f in _simulation.Sector(i))
                        {
                            meanWageTot += f.Wage * f.Employment;
                            meanPriceTot += f.Price * f.Sales;
                            _totalEmployment += f.Employment;
                            nEmployment += f.NumberOfEmployed; 
                            _totalSales += f.Sales;                              // Hmm..
                            _totalPotensialSales += f.PotentialSales;            // Hmm..
                            _totalProduction += f.Production;                    // Hmm..
                            _meanValue += f.Value;
                            mean_age += f.Age;
                            tot_vacancies += f.Vacancies;
                            _discountedProfits += f.Profit / Math.Pow(1+_expectedInterestRate, f.Age);
                            no += f.NumberOfNo;
                            ok += f.NumberOfOK;
                        }
                   
                    _avrProductivity = _totalEmployment / nEmployment;
                    //double firmRejectionRate = (double)no / (no + ok);
                    //double potentilaSalesRate = _totalPotensialSales / _totalSales;
                    // Calculation of profitPerHousehold
                    //_profitPerHousehold = _totalProfit / _simulation.Households.Count;  // Profit income to households
                    //_totalProfitFromDefaults = 0;

                    int n_firms = 0;
                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                        n_firms += _simulation.Sector(i).Count;

                    //double m_pi = _discountedProfits /n_firms;
                    //_sigmaRiskTotal = 0;

                    //for (int i = 0; i < _settings.NumberOfSectors; i++)
                    //    foreach (Firm f in _simulation.Sector(i))
                    //        _sigmaRiskTotal += Math.Pow(f.Profit / Math.Pow(1 + _interestRate, f.Age) - m_pi, 2);
                    //_sigmaRiskTotal = Math.Sqrt(_sigmaRiskTotal / n_firms);
                    //_sharpeRatioTotal = _sigmaRiskTotal > 0 ? m_pi / _sigmaRiskTotal : 0;

                    _expDiscountedProfits = 0.99 * _expDiscountedProfits + (1 - 0.99) * _discountedProfits; // Bruges ikke
                    //_expSharpeRatioTotal = _settings.StatisticsExpectedSharpeRatioSmooth * _expSharpeRatioTotal + (1 - _settings.StatisticsExpectedSharpeRatioSmooth) * _sharpeRatioTotal;
                    mean_age /= n_firms;
                    _meanValue /= n_firms;
                    //_expProfit = totProfit / n_firms;

                    if (meanWageTot > 0)
                        _marketWageTotal = meanWageTot / _totalEmployment;

                    if (_marketWageTotal0 > 0)
                        _realwageInflation = (_marketWageTotal/_marketPriceTotal) / (_marketWageTotal0/ _marketPriceTotal0) - 1;


                    //----------
                    int nUnemp = 0;
                    int laborSupply = 0;
                    _n_laborSupply = 0;
                    _laborSupplyProductivity = 0;
                    _n_unemployed = 0;
                    double totalConsumption = 0;
                    int h_no = 0;
                    int h_ok = 0;
                    double consValue = 0, consBudget = 0;

                    List<double> wages= new();
                    List<double>[] prices = new List<double>[_settings.NumberOfSectors];
                    for(int i=0;i< _settings.NumberOfSectors;i++)
                        prices[i] = new List<double>();

                    double wage_income=0;
                    _wealth = 0;
                    foreach (Household h in _simulation.Households)
                    {
                        if (h.Age < _settings.HouseholdPensionAge)
                        {
                            nUnemp += h.Unemployed ? 1 : 0;
                            laborSupply++;
                            _n_laborSupply++;
                            _laborSupplyProductivity += h.Productivity;
                            _n_unemployed += h.Unemployed ? 1 : 0;
                            if (h.FirmEmployment != null)
                            {
                                wages.Add(h.FirmEmployment.Wage);
                                wage_income += h.Productivity * h.FirmEmployment.Wage;
                            }
                        }

                        _wealth += h.Wealth;
                        h_no += h.No;
                        h_ok += h.Ok;
                        consValue += h.ConsumptionValue;
                        consBudget += h.ConsumptionBudget;
                        totalConsumption += h.ConsumptionSector0;

                        for (int i = 0; i < _settings.NumberOfSectors; i++)
                            if (h.FirmShopArray(i) != null)
                                prices[i].Add(h.FirmShopArray(i).Price);

                    }
                    //double h_rejectionRate = (double)h_no / (h_no + h_ok);
                    double consLoss = 1.0 - consValue / consBudget;
                    // Calculate median wage
                    if (wages.Count > 0)
                        _wageMedian = wages.Median();
                        //_wageMedian = wages.Average();  //!!!!!!!!!!!!!!!!!!!!!!

                    // Calculate median prices
                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                        _priceMedian[i] = prices[i].Median();
                        //_priceMedian[i] = prices[i].Average();  //!!!!!!!!!!!!!!!!!!!!!


                    if (_time.Now > _settings.FirmPriceMechanismStart)
                    {
                        // Calculate median price
                        //if (prices.Count > 0)
                        //    _marketPrice = prices.Median();

                        if (meanPriceTot > 0 & _totalSales > 0)
                            _marketPriceTotal = meanPriceTot / _totalSales;

                        if (_marketPriceTotal0 > 0)
                            _inflation = _marketPriceTotal / _marketPriceTotal0 - 1;

                        _marketPriceTotal0 = _marketPriceTotal;
                        _marketWageTotal0 = _marketWageTotal;

                        _realInterestRate = (1+_interestRate)/(1+_inflation) - 1;

                        //_discountedProfits /= _marketPrice;
                    }

                    if ((_time.Now + 1) % _settings.PeriodsPerYear == 0)
                    {
                        // REMOVE THIS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        _yr_consumption = 0;
                        _yr_employment = 0;
                        foreach (Household h in _simulation.Households)
                        {
                            _yr_consumption += h.YearConsumption;
                            _yr_employment += h.YearEmployment;
                        }
                    }
                    #endregion

                    #region Graphics
                    // Graphics
                    double[] price = new double[_settings.NumberOfSectors];
                    double[] wage = new double[_settings.NumberOfSectors];
                    double[] employment = new double[_settings.NumberOfSectors];
                    double[] production = new double[_settings.NumberOfSectors];
                    //double[] sales = new double[_settings.NumberOfSectors];
                    //double[] potensialSales = new double[_settings.NumberOfSectors];
                    int[] nFirm = new int[_settings.NumberOfSectors];

                    if (_settings.StatisticsGraphicsPlotInterval > 0 & (_time.Now > _settings.StatisticsGraphicsStartPeriod))
                        if (_time.Now % _settings.StatisticsGraphicsPlotInterval == 0) // Once a year
                        {
                            double tot_opt_l = 0;// Calculate total optimal employment  
                            double prod_avr = 0; // Calculate average productivity
                            using (StreamWriter sw = File.CreateText(_settings.ROutputDir + "\\data_firms.txt"))
                            {

                                sw.WriteLine("Productivity\tOptimalEmployment\tOptimalProduction\tEmployment\tProfit\tSales\tAge\tOptimalProfit\t" +
                                    "NumberOfEmployed\tVacancies\tApplications\tQuitters\tSector");
                                for (int i = 0; i < _settings.NumberOfSectors; i++)
                                {
                                    price[i] = 0;
                                    wage[i] = 0;
                                    employment[i] = 0;
                                    production[i] = 0;
                                    //sales[i] = 0;
                                    //potensialSales[i] = 0;
                                    nFirm[i] = _simulation.Sector(i).Count;
                                    foreach (Firm f in _simulation.Sector(i))
                                    {
                                        double disc = 1.0 / (1 + _expectedInterestRate);
                                        double discProfit = f.Profit * Math.Pow(disc, f.Age) / _marketPrice[f.Sector];

                                        sw.WriteLineTab(f.Productivity, f.OptimalEmployment, f.OptimalProduction, f.Employment, f.Profit,
                                            f.Sales, f.Age, f.OptimalProduction, f.NumberOfEmployed, f.Vacancies, f.JobApplications, f.JobQuitters,
                                            f.Sector);

                                        prod_avr += Math.Pow(f.Productivity, 1 / (1 - _settings.FirmAlpha));
                                        tot_opt_l += f.OptimalEmployment;

                                        price[i] += f.Price;
                                        wage[i] += f.Wage;
                                        employment[i] += f.Employment;
                                        production[i] += f.Production;
                                        //sales[i] += f.Sales;
                                        //potensialSales[i] += f.PotentialSales;
                                    }
                                    price[i] /= nFirm[i];
                                    wage[i] /= nFirm[i];
                                }
                            }

                            //sw.WriteLine("Time\tSector\tPrice\tWage\tEmployment\tProduction\tSales\tExpShapeRatio\tnFirm");

                            using (StreamWriter sw = File.AppendText(_settings.ROutputDir + "\\sector_year.txt"))
                            {
                                for (int i = 0; i < _settings.NumberOfSectors; i++)
                                    sw.WriteLineTab(_time.Now, i, price[i], wage[i], employment[i], production[i], 
                                        _sales[i], nFirm[i], _expSharpeRatio[i], _expSharpeRatioTotal);
                                //sw.Flush();
                            }

                            prod_avr /= n_firms;
                            prod_avr = Math.Pow(prod_avr, 1 - _settings.FirmAlpha);
                            double P_star = 0;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                            //sw.WriteLine("Year\tn_Households\tavr_productivity\tnUnemployed\tnOptimalEmplotment\tP_star\tnEmployment\tnVacancies\tWage\tPrice\t" +
                            //"Sales\tProfitPerHousehold\tnFirms\tProfitPerFirm\tMeanAge\tMeanValue\tnFirmCloseNatural\tnFirmCloseNegativeProfit\tnFirmCloseTooBig\t" +
                            //"nFirmNew\tDiscountedProfits\tExpDiscountedProfits\tSharpeRatio\tExpSharpRatio\tLaborSupply\tYearConsumption\tYearEmployment");

                            using (StreamWriter sw = File.AppendText(_settings.ROutputDir + "\\data_year.txt"))
                            {

                                
                                sw.WriteLineTab(1.0 * _settings.StartYear + 1.0 * _time.Now / _settings.PeriodsPerYear, _time.Now,
                                    _simulation.Households.Count, prod_avr, nUnemp, tot_opt_l, P_star, _totalEmployment, 
                                    tot_vacancies, _marketWageTotal, _marketPriceTotal, _totalSales, _profitPerHousehold,
                                    n_firms, _expProfit, mean_age, _meanValue, _nFirmCloseNatural, 
                                    _nFirmCloseZeroEmployment, _nFirmCloseTooBig, _nFirmNew, _discountedProfits, 
                                    _expDiscountedProfits, _sharpeRatioTotal, _expSharpeRatioTotal, laborSupply, _yr_consumption, _yr_employment, 
                                    _totalPotensialSales, _totalProduction, totalConsumption, consValue, consBudget,
                                    _nChangeShopInSearchForShop, _nChangeShopInBuyFromShopNull, _nChangeShopInBuyFromShopLookingForGoods, 
                                    _nCouldNotFindFirmWithGoods, _nBuyFromShop, _nSuccesfullTrade, _nZeroBudget, _nSuccesfullTradeNonZero);
                                //sw.Flush();

                            }

                            using (StreamWriter sw = File.CreateText(_settings.ROutputDir + "\\data_households.txt"))
                            {
                                sw.WriteLine("UnemplDuration\tProductivity\tAge\tConsumptionValue\tConsumptionBudget\tPrice\tWage\tIncome");
                                foreach (Household h in _simulation.Households)
                                {
                                    double w = h.FirmEmployment!=null ? h.FirmEmployment.Wage : 0;
                                    if(_simulation.Random.NextEvent(0.1))
                                        sw.WriteLineTab(h.UnemploymentDuration, h.Productivity, h.Age, h.ConsumptionValue, h.ConsumptionBudget, h.CES_Price, w, h.Income);
                                }
                            }

                            //RunRScript("..\\..\\..\\R\\graphs.R");
                            //Console.WriteLine("Running R..");
                            RunRScript(_settings.ROutputDir + "\\graphs.R");

                        }
                    #endregion

                    #region Chart output
#if WIN_APP
                    double gg = Math.Pow(1 + 0.02, 1.0 / 12) - 1;
                    double corr = Math.Pow(1 + gg, _time.Now);
                    double smooth2 = 0.99;
                    _expextedWage = smooth2 * _expextedWage + (1 - smooth2) * _marketWageTotal;
                    _expextedPrice = smooth2 * _expextedPrice + (1 - smooth2) * _marketPriceTotal;

                    _chartData.nFirms[_time.Now] = n_firms;
                    _chartData.NewFirms[_time.Now] = _nFirmNew;
                    _chartData.ClosedFirms[_time.Now] = _nFirmCloseNatural + _nFirmCloseNegativeProfit + _nFirmCloseTooBig + _nFirmCloseZeroEmployment;
                    _chartData.nHouseholds[_time.Now] = _simulation.Households.Count;
                    _chartData.Sales[_time.Now] = _totalSales / corr;
                    _chartData.RealWage[_time.Now] = (_marketWageTotal / _marketPriceTotal) / corr;
                    _chartData.Price[_time.Now] = _marketPriceTotal;
                    _chartData.Wage[_time.Now] = _marketWageTotal;
                    _chartData.Consumption[_time.Now] = totalConsumption / corr;
                    _chartData.Production[_time.Now] = _totalProduction / corr;
                    _chartData.SharpeRatio[_time.Now] = _sharpeRatioTotal;
                    _chartData.ExpectedSharpeRatio[_time.Now] = _expSharpeRatioTotal;
                    _chartData.UnemploymentRate[_time.Now] = 1.0 * _n_unemployed / _n_laborSupply;
                    _chartData.ConsumptionLoss[_time.Now] = consLoss;
                    _chartData.Stock[_time.Now] = _stock / _totalProduction;
                    //_chartData.Wealth[_time.Now] = _wealth / consBudget;
                    //_chartData.Inheritance[_time.Now] = 100*_inheritence / consBudget;
                    //_chartData.Wealth[_time.Now] = _wealth / (_expextedWage*_n_laborSupply);
                    //_chartData.Inheritance[_time.Now] = 100 * _inheritence / (_expextedWage * _n_laborSupply);
                    _chartData.Wealth[_time.Now] = _wealth / _expextedPrice / corr;
                    _chartData.Inheritance[_time.Now] = 100 * _inheritence / _expextedPrice / corr;

                    _chartData.Employment[_time.Now] = _n_laborSupply - _n_unemployed;
                    _chartData.LaborSupplyProductivity[_time.Now] = _n_laborSupply;
                    //_chartData.ProfitPerHousehold[_time.Now] = _profitPerHousehold;

                    _chartData.InterestRate[_time.Now] = Math.Pow(1 + _interestRate, _settings.PeriodsPerYear) - 1;
                    _chartData.ExpectedInterestRate[_time.Now] = Math.Pow(1 + _expectedInterestRate, _settings.PeriodsPerYear) - 1;

                    _chartData.RealInterestRate[_time.Now] = Math.Pow(1 + _realInterestRate, _settings.PeriodsPerYear) - 1;
                    _chartData.ExpectedRealInterestRate[_time.Now] = Math.Pow(1 + _expectedRealInterestRate, _settings.PeriodsPerYear) - 1;

                    _chartData.Profit[_time.Now] = 40*_totalProfit / _expextedPrice / corr;

                    _chartData.Inflation[_time.Now] = Math.Pow(1 + _inflation, _settings.PeriodsPerYear) - 1;
                    //_chartData.RealWageInflation[_time.Now] = _realWageInflation;

                    if (_time.Now > 2)
                    {
                        //_chartData.Inflation[_time.Now] =
                        //    Math.Pow(_chartData.Price[_time.Now] / _chartData.Price[_time.Now - 1], 12) - 1;

                        _chartData.RealWageInflation[_time.Now] =
                            Math.Pow(_chartData.RealWage[_time.Now] / (_chartData.RealWage[_time.Now - 1] / (1 + gg)), 12) - 1;

                        _chartData.WageInflation[_time.Now] =
                            Math.Pow(_chartData.Wage[_time.Now] / (_chartData.Wage[_time.Now - 1]), 12) - 1;


                    }


                    _chartData.InvestorTakeOut[_time.Now] = _simulation.Investor.TakeOut / _expextedWage;
                    
                    double investorIncome = _simulation.Investor.Income / _expextedWage;                    
                    _chartData.InvestorIncome[_time.Now] = Double.IsNaN(investorIncome) ? 0 : investorIncome;                   
                    
                    _chartData.InvestorPermanentIncome[_time.Now] = _simulation.Investor.PermanentIncome / _expextedWage;
                    _chartData.InvestorWealth[_time.Now] = _simulation.Investor.Wealth / _expextedWage;
                    _chartData.InvestorWealthTarget[_time.Now] = _simulation.Investor.WealthTarget / _expextedWage;

                    _chartData.nJobFromUnemployment[_time.Now] = _nJobFromUnemployment;
                    _chartData.nJobFromJob[_time.Now] = _nFromJob;
                    _chartData.nJobFromUnemploymentAdvertise[_time.Now] = _nFromUnemploymentAdvertise;
                    _chartData.nJobFromJobAdvertise[_time.Now] = _nFromJobAdvertise;

                    //_chartData.Extra[_time.Now] = 0;
                    _chartData.Extra[_time.Now] = _settings.InvestorProfitSensitivity;

                    // Reporting progress
                    MainFormUI mainFormUI=null;
                    if (_settings.SaveScenario)
                    {
                        if (_simulation.WinFormElements != null)
                        {
                            WinFormElements wfe = _simulation.WinFormElements;
                            mainFormUI = wfe.MainFormUI;

                            if (wfe.ArgsToWorkerScenario.ID == 0)
                                mainFormUI.backgroundWorkersScenarios[0].ReportProgress(_time.Now);

                        }
                    }
                    else
                    {
                        mainFormUI = _simulation.WinFormElements.MainFormUI;
                        
                        if(_time.Now==_nextUIChartUpdateTime)
                        {
                            if (mainFormUI.NeedMicroData & _time.Now % 1 == 0)
                                collectMicroData();

                            _chartData.Wait = 0;
                            while (mainFormUI.Busy)
                            {
                                _chartData.Wait++;
                                Thread.Sleep(100);
                            }
                         
                            mainFormUI.backgroundWorker.ReportProgress(_time.Now, _chartData);
                            _nextUIChartUpdateTime += _settings.UIChartUpdateInterval;

                        }
                        else
                            mainFormUI.backgroundWorker.ReportProgress(_time.Now);
                    }

#endif
                    #endregion

                    #region More stuff
                    // Shock: Productivity shock
                    if (_time.Now == _settings.ShockPeriod)
                    {
                        if (_settings.Shock == EShock.Productivity)
                            _settings.MacroProductivity *= (1 + _settings.ShockSize);

                        if (_settings.Shock == EShock.ProductivityAR1)
                        {
                            _shockSizeAbs = _settings.ShockSize * _settings.MacroProductivity;
                            _macroProductivity0 = _settings.MacroProductivity;
                            _settings.MacroProductivity = _macroProductivity0 + _shockSizeAbs;
                        }

                        if (_settings.Shock == EShock.ProductivitySector0)
                            _sectorProductivity[0] = 1.1;

                    }
                    if (_time.Now > _settings.ShockPeriod)
                    {
                        if (_settings.Shock == EShock.ProductivityAR1)
                        {
                            _shockSizeAbs *= 0.98;
                            _settings.MacroProductivity = _macroProductivity0 + _shockSizeAbs;
                        }
                    }

                    int nFirmClosed = _nFirmCloseNatural + _nFirmCloseNegativeProfit + _nFirmCloseTooBig + _nFirmCloseZeroEmployment;
                    //_fileMacro.WriteLineTab(_scenario_id, Environment.MachineName, _runName, _time.Now, _expSharpeRatioTotal, _macroProductivity, _marketPriceTotal, 
                    _fileMacro.WriteLineTab(_settings.RandomSeed, Environment.MachineName, _runName, _time.Now, 
                                            _expSharpeRatioTotal, _settings.MacroProductivity, _marketPriceTotal,
                                            _marketWageTotal,n_firms, _totalEmployment, _totalSales, _laborSupplyProductivity, 
                                            _n_laborSupply, _n_unemployed,
                                            _totalProduction, _simulation.Households.Count, _nFirmNew, nFirmClosed, 
                                            _sigmaRiskTotal, _sharpeRatioTotal, 
                                            mean_age, tot_vacancies, _marketPrice[0], _marketWage[0], 
                                            _employment[0], _sales[0], 
                                            _simulation.Sector(0).Count, _expSharpeRatio[0], totalRevenues, 
                                            _totalPotensialSales, _expectedInterestRate, _wealth / _marketPriceTotal, _stock);

                    //_fileMacro.Flush();

                    for (int i = 0; i < _settings.NumberOfSectors; i++)
                    {
                        _fileSectors.WriteLineTab(_scenario_id, Environment.MachineName, _runName, _time.Now, i,
                        _marketPrice[i], _marketWage[i], _marketPriceTotal, _marketWageTotal, _employment[i], _production[i],
                        _sales[i], _expSharpeRatio[i], _simulation.Sector(i).Count);
                    }                    
                    //_fileSectors.Flush();

                    _nFirmCloseNatural = 0;
                    _nFirmCloseTooBig = 0;
                    _nFirmCloseNegativeProfit = 0;
                    _nFirmCloseZeroEmployment = 0;
                    _nFirmNew = 0;

                    if(_time.Now>12)
                        _vb = 0.9 * _vb + (1-0.9) * consValue / consBudget;

                    double g = Math.Pow(1 + _settings.FirmProductivityGrowth, 1.0 / _settings.PeriodsPerYear) - 1;
                    double real_w = (_marketWageTotal / _marketPriceTotal) * Math.Pow(1+g, -_time.Now);
                    double yr = 1.0 * _settings.StartYear + 1.0 * _time.Now / _settings.PeriodsPerYear;

                    double w_infl = 0;                    
                    if(_wage_lag>0)
                        w_infl = Math.Pow(_marketWageTotal / _wage_lag, 12) - 1;
                    _wage_lag = _marketWageTotal;

                    double p_infl = 0;
                    if (_price_lag > 0)
                        p_infl = Math.Pow(_marketPriceTotal / _price_lag, 12) - 1;
                    _price_lag = _marketPriceTotal;


                    //Console.WriteLine("{0:#.##}\t{1}\t{2}\t{3:#.###}\t{4:#.###}\t{5:#.####}\t{6:#.#}\t{7:#.#}\t{8:#.####}\t{9:#.####}\t{10:#.####}\t{11:#.####}", yr,
                    //    n_firms, _simulation.Households.Count, w_infl, p_infl, _avrProductivity, _totalSales/1000, totalConsumption/1000, 
                    //    _vb, real_w, _expSharpeRatio[0], 1.0*_n_unemployed/_n_laborSupply);
                    #endregion

                    break;

                case Event.System.Stop:
                    WriteAvrFile(12 * 50);

                    // Last picture
#if WIN_APP
                    if(_simulation.WinFormElements!=null)
                        _simulation.WinFormElements.DoWorkEventArgs.Result = _chartData;
#endif
                    CloseFiles();

                    #region R-stuff NOT USED
                    //if(!_settings.SaveScenario)
                    //{
                    //    Console.WriteLine("\nRunning R-scripts:");

                    //    Console.WriteLine("-- macro.R..");
                    //    RunRScript("..\\..\\..\\R\\macro.R");

                    //    Console.WriteLine("-- macro_q.R..");
                    //    RunRScript("..\\..\\..\\R\\macro_q.R");

                    //    Console.WriteLine("-- firm_reports.R..");
                    //    RunRScript("..\\..\\..\\R\\firm_reports.R");
                    //}
                    #endregion
                    break;

                default:
                    base.EventProc(idEvent);
                    break;
            }
        }
        
        public void Communicate(EStatistics comID, object o)
        {
            Firm f = null;
            Household h = null;
            switch (comID)
            {
                case EStatistics.FirmCloseNatural:
                    _nFirmCloseNatural++;
                    f = (Firm)o;
                    _firmInfo.Add(new FirmInfo(f));
                    return;

                case EStatistics.FirmCloseTooBig:
                    _nFirmCloseTooBig++;
                    f = (Firm)o;
                    _firmInfo.Add(new FirmInfo(f));
                    return;

                case EStatistics.FirmCloseNegativeProfit:
                    _nFirmCloseNegativeProfit++;
                    f = (Firm)o;
                    _firmInfo.Add(new FirmInfo(f));
                    return;

                case EStatistics.FirmCloseZeroEmployment:
                    _nFirmCloseZeroEmployment++;
                    f = (Firm)o;
                    _firmInfo.Add(new FirmInfo(f));
                    return;

                case EStatistics.Death:
                    //_totalProfitFromDefaults += (double)o;
                    return;

                case EStatistics.Profit:
                    f = (Firm)o;
                    _firmInfo.Add(new FirmInfo(f));
                    return;

                case EStatistics.FirmNew:
                    _nFirmNew += (double)o;
                    return;

                case EStatistics.CouldNotFindSupplier:
                    _n_couldNotFindSupplier ++;
                    return;

                case EStatistics.ChangeShopInSearchForShop:
                    _nChangeShopInSearchForShop++;
                    return;

                case EStatistics.ChangeShopInBuyFromShopNull:
                    _nChangeShopInBuyFromShopNull++;
                    return;
                    
                case EStatistics.ChangeShopInBuyFromShopLookingForGoods:
                    _nChangeShopInBuyFromShopLookingForGoods++;
                    return;

                case EStatistics.CouldNotFindFirmWithGoods:
                    _nCouldNotFindFirmWithGoods++;
                    return;

                case EStatistics.BuyFromShop:
                    _nBuyFromShop++;
                    return;

                case EStatistics.SuccesfullTrade:
                    _nSuccesfullTrade++;
                    return;

                case EStatistics.ZeroBudget:
                    _nZeroBudget++;
                    return;

                case EStatistics.SuccesfullTradeNonZero:
                    _nSuccesfullTradeNonZero++;
                    return;

                case EStatistics.JobFromUnemployment:    
                    _nJobFromUnemployment++;
                    return; 

                case EStatistics.JobFromJob:
                    _nFromJob++;
                    return;

                case EStatistics.JobFromUnemploymentAdvertise:
                    _nFromUnemploymentAdvertise++;
                    return;

                case EStatistics.JobFromJobAdvertise:   
                    _nFromJobAdvertise++;
                    return;

                case EStatistics.Inheritance:
                    double inh = (double)o;
                    _inheritence+=inh;
                    return;

                default:
                    return;
            }
        }

        #region Internal methods

#if WIN_APP
        void collectMicroData()
        { 

            List<double> productivity = new();
            List<double> profit = new();
            List<double> production = new();
            List<double> age = new();
            List<double> employment = new();
            List<double> price = new();
            List<double> wage = new();
            double corr = Math.Pow(1 + _growthPerPeriod, _time.Now);

            for (int i = 0; i < _settings.NumberOfSectors; i++)
                foreach (Firm f in _simulation.Sector(i))
                {
                    productivity.Add(f.Productivity / corr);
                    production.Add(f.Production / corr);
                    profit.Add(f.Profit / _marketPriceTotal / corr);
                    age.Add(1.0 * f.Age / 12);
                    employment.Add(f.Employment);
                    price.Add(f.Price / _marketPriceTotal);
                    wage.Add(f.Wage / _marketWageTotal);
                }

            _chartData.MicroData.Productivity = productivity.ToArray();
            _chartData.MicroData.Profit = profit.ToArray();
            _chartData.MicroData.Production = production.ToArray();
            _chartData.MicroData.Age = age.ToArray();
            _chartData.MicroData.Employment = employment.ToArray();
            _chartData.MicroData.Price = price.ToArray();
            _chartData.MicroData.Wage = wage.ToArray();

        }
#endif
        void Write()
        {
            _fileDBStatistics.WriteLineTab(_expSharpeRatioTotal, _settings.MacroProductivity, _marketPrice, _marketWage);

        }
        void RunRScript(string fileName)
        {

            Process r = new();

            r.StartInfo.FileName = _settings.RExe;
            r.StartInfo.Arguments = "CMD BATCH " + fileName;
            r.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            r.Start();
            r.WaitForExit();

            //            Thread.Sleep(100);

        }
        
        string Path(string fileName)
        {
#if !LINUX_APP            
            return _settings.ROutputDir + "\\" + fileName;
#else
            return _settings.ROutputDir + "/" + fileName;
#endif
        }
        void OpenFiles()
        {
            if (!_settings.SaveScenario)
            {

                string path = Path("data_year.txt");
                if (File.Exists(path)) File.Delete(path);
                using (StreamWriter sw = File.CreateText(path))
                    sw.WriteLine("Year\tPeriod\tn_Households\tavr_productivity\tnUnemployed\tnOptimalEmplotment\tP_star\tnEmployment\tnVacancies\tWage\tPrice\t" +
                        "Sales\tProfitPerHousehold\tnFirms\tProfitPerFirm\tMeanAge\tMeanValue\tnFirmCloseNatural\tnFirmCloseNegativeProfit\tnFirmCloseTooBig\t" +
                        "nFirmNew\tDiscountedProfits\tExpDiscountedProfits\tSharpeRatio\tExpSharpRatio\tLaborSupply\tYearConsumption\tYearEmployment\t" +
                        "PotensialSales\tProduction\tConsumption\tConsumptionValue\tConsumptionBudget\tnChangeShopInSearchForShop\t" +
                        "nChangeShopInBuyFromShopNull\tnChangeShopInBuyFromShopLookingForGoods\tnCouldNotFindFirmWithGoods\tnBuyFromShop\tnSuccesfullTrade\t" +
                        "nZeroBudget\tnSuccesfullTradeNonZero");

                path = Path("sector_year.txt");
                if (File.Exists(path)) File.Delete(path);
                using (StreamWriter sw = File.CreateText(path))
                    sw.WriteLine("Time\tSector\tPrice\tWage\tEmployment\tProduction\tSales\tnFirm\texpSharpeRatio\texpSharpeRatioTotal"); 

                path = Path("\\firm_reports.txt"); 
                if (File.Exists(path)) File.Delete(path);
                _fileFirmReport = File.CreateText(path);
                _fileFirmReport.WriteLine("Time\tID\tProductivity\tEmployment\tProduction\tSales\tVacancies\tExpectedPrice\tExpectedWage\tPrice\tWage\tApplications" +
                    "\tQuitters\tProfit\tValue\tPotensialSales\tOptimalEmployment\tOptimalProduction\tExpectedSales\texpApplications\texpQuitters\texpAvrProd\tMarketPrice" +
                    "\tMarketWage\tExpectedPotentialSales\tExpectedEmployment\tEmploymentMarkup\tRelativeTargetPrice\tRelativeTargetWage\tExpectedVacancies\tAge\tStock\tFirerings" +
                    "\tNewEmployment\tRelativeWage\tRelativePrice");

                path = Path("applications.txt");
                if (File.Exists(path)) File.Delete(path);
                _fileFirmApplications = File.CreateText(path);
                _fileFirmApplications.WriteLine("Time\tID\tProductivity");


                path = Path("household_reports.txt");
                if (File.Exists(path)) File.Delete(path);
                _fileHouseholdReport = File.CreateText(path);
                _fileHouseholdReport.WriteLine("Time\tID\tProductivity\tAge\tConsumption\tValConsumption\tIncome\tWealth\tWage" +
                    "\tP_macro\tConsumptionBudget\tPermanentIncome\tWealthTarget\tSearchJobOnJob\tSearchJobUnemployed\tUnemployed" +
                    "\tUnempDuration\tReservationWage");

                path = Path("output.txt");
                if (!File.Exists(path))
                    using (StreamWriter sw = File.CreateText(path))
                        sw.WriteLine("n_firms\tPrice\tWage\tDiscountedProfits");

            }

            string macroPath = Path("macro.txt");
            string sectorsPath = Path("sectors.txt");
            string settingsPath = Path("settings.json");

            #region Scenario-stuff
            if (_settings.SaveScenario)
            {
                if(_settings.NewScenarioDirs)
                {
#if !LINUX_APP
                    Directory.CreateDirectory(_settings.ROutputDir + "\\Scenarios");
                    Directory.CreateDirectory(_settings.ROutputDir + "\\Scenarios\\Macro");
                    Directory.CreateDirectory(_settings.ROutputDir + "\\Scenarios\\Sectors");
                    Directory.CreateDirectory(_settings.ROutputDir + "\\Scenarios\\Settings");
#else
                    Directory.CreateDirectory(_settings.ROutputDir + "/Scenarios");
                    Directory.CreateDirectory(_settings.ROutputDir + "/Scenarios/Macro");
                    Directory.CreateDirectory(_settings.ROutputDir + "/Scenarios/Sectors");
#endif
                }

                string scnPath = _settings.ROutputDir + "\\scenario_info.txt";
                if (_settings.Shock == EShock.Base) // Base run
                {

                    string seed = _settings.RandomSeed.ToString();

#if !LINUX_APP
                    macroPath = _settings.ROutputDir + "\\Scenarios\\Macro\\base_" + seed + "_" + Environment.MachineName + ".txt";
                    sectorsPath = _settings.ROutputDir + "\\Scenarios\\Sectors\\base_" + seed + "_" + Environment.MachineName + ".txt";
                    settingsPath = _settings.ROutputDir + "\\Scenarios\\Settings\\base_" + seed + "_" + Environment.MachineName + ".json";
#else
                    macroPath = _settings.ROutputDir + "/Scenarios/Macro/base_" + seed + "_" + Environment.MachineName + ".txt";
                    sectorsPath = _settings.ROutputDir + "/Scenarios/Sectors/base_" + seed + "_" + Environment.MachineName + ".txt";
                    settingsPath = _settings.ROutputDir + "/Scenarios/Settings/base_" + seed + "_" + Environment.MachineName + ".json";
#endif
                }
                else //Counterfactual
                {

                    _runName = _settings.Shock.ToString();
                    string seed = _settings.RandomSeed.ToString();

#if !LINUX_APP                    
                    macroPath = _settings.ROutputDir + "\\Scenarios\\Macro\\count_" + _runName + "_" + seed + "_" + Environment.MachineName + ".txt";
                    sectorsPath = _settings.ROutputDir + "\\Scenarios\\Sectors\\count_" + _runName + "_" + seed + "_" + Environment.MachineName + ".txt";
                    settingsPath = _settings.ROutputDir + "\\Scenarios\\Settings\\count_" + _runName + "_" + seed + "_" + Environment.MachineName + ".json";
#else
                    macroPath = _settings.ROutputDir + "/Scenarios/Macro/count_" + _runName + "_" + seed + "_" + Environment.MachineName + ".txt";
                    sectorsPath = _settings.ROutputDir + "/Scenarios/Sectors/count_" + _runName + "_" + seed + "_" + Environment.MachineName + ".txt";
                    settingsPath = _settings.ROutputDir + "/Scenarios/Settings/count_" + _runName + "_" + seed + "_" + Environment.MachineName + ".json";
#endif

                }
            }
#endregion

            if (File.Exists(macroPath)) File.Delete(macroPath);
            _fileMacro = File.CreateText(macroPath);
            _fileMacro.WriteLine("Scenario\tMachine\tRun\tTime\texpSharpeRatio\tmacroProductivity\tmarketPrice\t" +
                   "marketWage\tnFirms\tEmployment\tSales\tLaborSupply\tnLaborSupply\tnUnemployed\t" +
                   "Production\tnHouseholds\tnFirmNew\tnFirmClosed\tSigmaRisk\tSharpeRatio\tMeanAge\t" +
                   "Vacancies\tmarketPrice0\tmarketWage0\temployment0\tsales0\tnFirm0\texpShapeRatio0\ttotalRevenues\t" +
                   "PotensialSales\tInterestRate\tRealWealth\tStock");

            if (File.Exists(sectorsPath)) File.Delete(sectorsPath);
            _fileSectors = File.CreateText(sectorsPath);
            _fileSectors.WriteLine("Scenario\tMachine\tRun\tTime\tSector\tPrice\tWage\tPriceTotal\tWageTotal\tEmployment\tProduction\tSales\tExpShapeRatio\tnFirm");


            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //if (File.Exists(settingsPath)) File.Delete(settingsPath);
            //var options = new JsonSerializerOptions { WriteIndented = true };
            //string sJson = JsonSerializer.Serialize<Settings>(_settings, options);
            //File.WriteAllText(settingsPath, sJson);
            //File.WriteAllText(_settings.ROutputDir + "\\last_json.json", sJson);

        }
        void CloseFiles()
        {
            if (!_settings.SaveScenario)
            {
                _fileFirmReport.Close();
                _fileHouseholdReport.Close();
                _fileMacro.Close();
                _fileSectors.Close();
            }
        }
        void WriteAvrFile(int n)
        {
            //Write file with average over the last n opservations
#if !LINUX_APP            
            string path = _settings.ROutputDir + "\\Avr\\avr" + _settings.RandomSeed.ToString() + ".txt";
#else
            string path = _settings.ROutputDir + "/Avr/avr" + _settings.RandomSeed.ToString() + ".txt";
#endif
      
            if(File.Exists(path)) File.Delete(path); //Delete if same random seed

            using (StreamWriter sw = File.CreateText(path))
            {
                sw.WriteLine("nFirms\tnHouseholds\tSales\tRealWage\tPrice\tConsumption\tProduction\tSharpeRatio\tUnemploymentRate\tConsumptionLoss\tStock\tWealth\tEmployment\tProfitPerWealthUnit\tInterestRate\tInflation\tRealWageInflation\tExtra");
#if WIN_APP
                sw.WriteLineTab(_chartData.nFirms.Last(n).Average(),
                                _chartData.nHouseholds.Last(n).Average(),
                                _chartData.Sales.Last(n).Average(),
                                _chartData.RealWage.Last(n).Average(),
                                _chartData.Price.Last(n).Average(),
                                _chartData.Consumption.Last(n).Average(),
                                _chartData.Production.Last(n).Average(),
                                _chartData.SharpeRatio.Last(n).Average(),
                                _chartData.UnemploymentRate.Last(n).Average(),
                                _chartData.ConsumptionLoss.Last(n).Average(),
                                _chartData.Stock.Last(n).Average(),
                                _chartData.Wealth.Last(n).Average(),
                                _chartData.Employment.Last(n).Average(),
                                _chartData.InterestRate.Last(n).Average(),
                                _chartData.ExpectedInterestRate.Last(n).Average(),
                                _chartData.Inflation.Last(n).Average(),
                                _chartData.RealWageInflation.Last(n).Average(),
                                _chartData.Extra.Last(n).Average()
                               );
#endif
            }
        }
#endregion

        #region Public proporties
        public double[] MarketWage
        {
            get { return _marketWage; }
        }
        public double MarketWageTotal
        {
            //get { return _marketWageTotal; }
            get { return _wageMedian; }
        }
        public double MarketPriceTotal
        {
            get { return _marketPriceTotal; }
        }
        public double[] MarketPrice
        {
            //get { return _marketPrice; }
            get { return _priceMedian; }
        }
        public double GrowthPerPeriod
        {
            get { return _growthPerPeriod; }
        }
        public double[] SectorProductivity
        {
            get { return _sectorProductivity; }
        }
        public double ProfitPerHousehold
        {
            get { return _profitPerHousehold; }
        }
        public double MeanValue
        {
            get { return _meanValue; }
        }
        public double AverageProductivity
        {
            get { return _avrProductivity; }
        }
        public double ExpectedProfitPerFirm
        {
            get { return _expProfit; }
        }
        public double DiscountedProfits
        {
            get { return _discountedProfits; }
        }
        public double ExpectedDiscountedProfits
        {
            get { return _expDiscountedProfits; }
        }
        public double ExpectedSharpRatioTotal
        {
            get { return _expSharpeRatioTotal; }
        }
        public double[] ExpectedSharpRatio
        {
            get { return _expSharpeRatio; }
        }
        public double SharpRatioTotal
        {
            get { return _sharpeRatioTotal; }
        }
        /// <summary>
        /// Interest rate per period
        /// </summary>
        public double ExpectedInterestRate
        {
            get { return _expectedInterestRate; }
        }
        public double HouseholdWealth
        {
            get { return _wealth; }
        }
        /// <summary>
        /// Short run interest rate
        /// </summary>
        public double InterestRate
        {
            get { return _interestRate; }
        }
        public double Inflation
        {
            get { return _inflation; }
        }
        public double ExpectedInflation
        {
            get { return _expectedInflation; }
        }
        public double RealWageInflation
        {
            get { return _realwageInflation; }
        }
        public double ExpectedRealWageInflation
        {
            get { return _expectedRealwageInflation; }
        }
        public double RealInterestRate
        {
            get { return _realInterestRate; }
        }
        public double ExpectedRealInterestRate
        {
            get { return _expectedRealInterestRate; }
        }
        public StreamWriter StreamWriterFirmReport
        {
            get { return _fileFirmReport; }
        }
        public StreamWriter StreamWriterHouseholdReport
        {
            get { return _fileHouseholdReport; }
        }
        public StreamWriter StreamWriterDBHouseholds
        {
            get { return _fileDBHouseholds; }
        }
        public StreamWriter StreamWriterDBFirms
        {
            get { return _fileDBFirms; }
        }
        public StreamWriter StreamWriterFirmApplications
        {
            get { return _fileFirmApplications; }
        }
        public double TotalProfit { get { return _totalProfit; } }
        #endregion

    }
}



