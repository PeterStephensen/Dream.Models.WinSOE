using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dream.AgentClass;

namespace Dream.Models.WinSOE
{
    public class Investor : Agent
    {

        #region Public get-set'ers
        public bool Active { get; set; } = false;
        #endregion

        #region Private fields
        Simulation _simulation;
        Settings _settings;
        Time _time;
        Statistics _statistics;

        double _income;
        double _permanentIncome;
        double _wealth=0;
        double _wealthTarget;
        double _takeOut;
        int _age = 0;
        double _kappa;
        bool _initialized = false;

        double[] _sharpeRatio;
        double[] _expectedSharpeRatio;
        double[] _discExpProfits;   // Discounted expected profits. Used to calculate Sharp ratio
        double[] _sigmaRisk;              // Risk (standard deviation) in sector
        double[] _nNewFirms;
        double[][] _nFirmNewHistory;
        double _interestRate;
        #endregion

        public Investor(double wealth, double permanentIncome)
        {
            _simulation = Simulation.Instance;
            _settings = _simulation.Settings;
            _time = _simulation.Time;
            _statistics = _simulation.Statistics;

            _wealth = wealth;
            _permanentIncome = permanentIncome;

            _sharpeRatio = new double[_settings.NumberOfSectors];
            _expectedSharpeRatio = new double[_settings.NumberOfSectors];
            _discExpProfits = new double[_settings.NumberOfSectors];   // Discounted expected profits. Used to calculate Sharp ratio
            _sigmaRisk = new double[_settings.NumberOfSectors];              // Risk (standard deviation) in sector
            _nNewFirms = new double[_settings.NumberOfSectors];

            _nFirmNewHistory = new double[(1 + _settings.EndYear - _settings.StartYear) * _settings.PeriodsPerYear][];
            for (int i = 0; i < _nFirmNewHistory.Length; i++)
                _nFirmNewHistory[i] = new double[_settings.NumberOfSectors];

        }

        public override void EventProc(int idEvent)
        {
            switch (idEvent)
            {

                case Event.System.Start:
                    break;

                case Event.System.PeriodStart:
                    break;

                case Event.System.PeriodEnd:
                    break;

                case Event.System.Stop:
                    break;

                default:
                    base.EventProc(idEvent);
                    break;
            }
        }

        public ECommunicate Communicate(ECommunicate comID, object o)
        {
            switch (comID)
            {
                case ECommunicate.Yes:
                    return ECommunicate.Ok;
                default:
                    return ECommunicate.Ok;
            }
        }

        public void CalculateSharpeRatiosAndInterestRate()
        {
            // This method should be called in Statistics under Event.System.PeriodStart
            //--------------------------------------------------------------------------------

            // Interest rate - Inflation and growth corrected
            double r = (1 + _statistics.ExpectedRealInterestRate) / (1 + _statistics.GrowthPerPeriod) - 1.0;

            // The list _statistics.FirmInfo contains info on firms that existed primo last period (now alive or defaulted)

            int[] nFirms = new int[_settings.NumberOfSectors];                 // Number of firms in sector
            foreach (var fi in _statistics.FirmInfos)
            {
                double n = _nFirmNewHistory[_time.Now - fi.Age][fi.Sector];  // Number of firms started fi.Age periods ago
                double disc = 1 / Math.Pow(1 + r, fi.Age);                           // Discount factor

                _discExpProfits[fi.Sector] += n > 0 ? fi.Profit * disc / n : 0;       // Discounted expected profit per new born firm
                nFirms[fi.Sector]++;
            }

            for(int i=0; i<_settings.NumberOfSectors; i++)
                _discExpProfits[i] = _discExpProfits[i] / nFirms[i];

            foreach(var fi in _statistics.FirmInfos)
            {
                double n = _nFirmNewHistory[_time.Now - fi.Age][fi.Sector];  // Number of firms started fi.Age periods ago
                double disc = 1 / Math.Pow(1 + r, fi.Age);                           // Discount factor

                _sigmaRisk[fi.Sector] += n > 0 ? Math.Pow((fi.Profit * disc / n) - _discExpProfits[fi.Sector], 2) : 0;       // Discounted expected profit per new born firm
                //sigmaRisk[fi.Sector] = 1;
            }
            for (int i = 0; i < _settings.NumberOfSectors; i++) _sigmaRisk[i] = Math.Sqrt(_sigmaRisk[i] / nFirms[i]);

            for (int i = 0; i < _settings.NumberOfSectors; i++)
            {
                _sharpeRatio[i] = _sigmaRisk[i] > 0 ? _discExpProfits[i] / _sigmaRisk[i] : 0; 
                _expectedSharpeRatio[i] = _settings.StatisticsExpectedSharpeRatioSmooth * _expectedSharpeRatio[i] + (1 - _settings.StatisticsExpectedSharpeRatioSmooth) * _sharpeRatio[i];
            }

            Iterate();

            if (_time.Now < _settings.BurnInPeriod3 | _settings.SimplificationInterestRate)
            {
                _interestRate = _settings.StatisticsInitialInterestRate;
            }
            else
            {
                double totWealth = 0;
                foreach (Household h in _simulation.Households)
                    totWealth += h.Wealth;

                if (totWealth > 0)
                    _interestRate = _statistics.TotalProfit / totWealth;
                    //_interestRate = _simulation.Investor.TakeOut / totWealth;
            }
        }

        public void Invest()
        {
            if (_time.Now < _settings.BurnInPeriod2)
            {
                for(int i=0; i<_settings.NumberOfSectors; i++)
                {
                    _nNewFirms[i] = _settings.InvestorInitialInflow;
                }
            }
            else
            {
                for (int i = 0; i < _settings.NumberOfSectors; i++)
                {
                    double investorProfitSensitivity = _settings.InvestorProfitSensitivity;
                    if (_time.Now < _settings.BurnInPeriod3)
                        investorProfitSensitivity = _settings.InvestorProfitSensitivityBurnIn;

                    _nNewFirms[i] += investorProfitSensitivity * _expectedSharpeRatio[i] * _nNewFirms[i];
                }
            }

            for (int i = 0; i < _settings.NumberOfSectors; i++)
            {
                if(_nNewFirms[i] > 0)
                {
                    _nFirmNewHistory[_time.Now][i] = _nNewFirms[i];

                    int n = _simulation.Random.NextInteger(_nNewFirms[i]);
                    for (int j = 0; j < n; j++)
                        _simulation.SectorList[i] += new Firm(i);
                }
            }
        }

        public void Iterate()
        {
            if (Active)
            {
                // See "Behavoristisk Buffer-Stock-husholdning (v0.4)"
                double gamma = _settings.InvestorSmoothIncome;
                double kappa = _settings.InvestorWealthIncomeRatioTarget;
                double xi = _settings.InvestorMPCIncome;
                double eta = _settings.InvestorMPCWealth;

                if(!_initialized)
                {
                    _wealth = kappa * _statistics.HouseholdWealth;
                    _permanentIncome = _statistics.TotalProfit;
                    _initialized = true;
                }
                
                _income = _statistics.TotalProfit;
                _permanentIncome = gamma * _permanentIncome + (1 - gamma) * _income;

                _wealthTarget = kappa * _statistics.HouseholdWealth;

                _takeOut = 0;
                if (_time.Now >= _settings.InvestorBuildUpPeriods)
                {                   
                    
                    // Buffe-stock                                                            
                    _takeOut = _permanentIncome + xi * (_income - _permanentIncome)
                                                             + eta * (_wealth - _wealthTarget);
                }

                _wealth = _wealth + _income - _takeOut;

                _age++;

            }


        }


        #region Public Properties
        /// <summary>
        /// Take out for consumption (can be negative = loan from households)
        /// </summary>
        public double TakeOut { get{ return _takeOut; }}
        
        /// <summary>
        /// Investors buffer-stock wealth (can not be negative)
        /// </summary>
        public double Wealth { get { return _wealth; } }

        /// <summary>
        /// Investors buffer-stock wealth target
        /// </summary>
        public double WealthTarget { get { return _wealthTarget; } }

        /// <summary>
        /// Investors profit income
        /// </summary>
        public double Income { get { return _income; } }

        public double InterestRate { get { return _interestRate; } }

        /// <summary>
        /// Investors profit income
        /// </summary>
        public double PermanentIncome { get { return _permanentIncome; } }

        public double[] SharpeRatio { get { return _sharpeRatio; } }
        public double[] ExpectedSharpeRatio { get { return _expectedSharpeRatio; } }

        public double[] SigmaRisk { get { return _sigmaRisk; } }
        public double[] DiscountedExpectedProfits { get { return _discExpProfits; } }


        #endregion






        #region OldStuff
        public void Iterate_OLD()
        {
            if (Active)
            {
                // See "Behavoristisk Buffer-Stock-husholdning (v0.4)"
                double gamma = _settings.InvestorSmoothIncome;
                double kappa = _settings.InvestorWealthIncomeRatioTarget;
                double xi = _settings.InvestorMPCIncome;
                double eta = _settings.InvestorMPCWealth;
                //double r = _statistics.PublicInterestRate;
                double r = 0;

                _income = _statistics.TotalProfit;
                _permanentIncome = gamma * _permanentIncome + (1 - gamma) * _income;

                //_wealthTarget = kappa * _permanentIncome;
                _wealthTarget = kappa * _statistics.HouseholdWealth;
                //_wealthTarget = 0.1 * _statistics.PublicHouseholdWealth;

                _takeOut = 0;
                if (_age > _settings.InvestorBuildUpPeriods)
                {

                    // Buffe-stock 
                    //double x_bar = _settings.InvestorShareOfPermanentIncome;

                    //_takeOut = r * _wealth + x_bar * _permanentIncome + xi * (_income - x_bar * _permanentIncome)
                    //                                         + eta * (_wealth - _wealthTarget);

                    //if (_takeOut < 0) _takeOut = 0;

                    //if ((1 + r) * _wealth + _income < 0) // Finansial Crises
                    //    _takeOut = (1 + r) * _wealth + _income; // Bail Out
                    //else if (_takeOut > (1 + r) * _wealth + _income)
                    //    _takeOut = (1 + r) * _wealth + _income;

                    _takeOut = _income;  //!!!!!!!!!!!!!!!!!!!!!
                }

                //_wealth = (1 + r) * _wealth + _income - _takeOut;



                _wealth = 0; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                _wealthTarget = 0;
                _permanentIncome = 0;

                _age++;

            }

        }

        #endregion


    }
}
