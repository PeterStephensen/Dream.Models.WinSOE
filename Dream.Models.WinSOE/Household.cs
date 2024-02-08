
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dream.AgentClass;
using Dream.IO;  

namespace Dream.Models.WinSOE
{
    public class Household : Agent
    {

        #region Private fields
        Simulation _simulation;
        Settings _settings;
        Time _time;
        Random _random;
        Statistics _statistics;
        int _age;
        Firm _firmEmployment=null, _firmShop=null;
        //bool _unemp = false; // Primo: unemployed? 
        double _w = 0; //Wage
        int _unempDuration = 0;
        double _productivity = 0;
        bool _initialHousehold = false;
        double _yr_consumption = 0;
        int _yr_employment = 0;
        bool _startFromDatabase = false;
        bool _report = false;
        double _consumption = 0;
        double _income = 0;
        double _consumption_budget = 0;
        double _consumption_value = 0;
        double _wealth = 0;
        double _wealth_P = 0;   // Pension
        double _wealth_U = 0;   // Unemployment
        double _wealth_UI = 0;  // UnIntensionally
        double[] _c = null;  // Consumption
        double[] _vc = null; // Value of consumption
        double[] _budget = null;
        Firm[] _firmShopArray = null;
        double[] _s_CES = null;
        double _P_CES = 0;
        bool _fired = false;
        int _no, _ok;
        int _nShoppings = 0;
        //bool _dead = false;
        double _wealth_target = 0;
        double _permanent_income = 0;
        double _w_exp; // Expected wage
        bool _bbudget = false;
        #endregion

        #region Constructors
        #region Household()
        public Household()
        {

            _simulation = Simulation.Instance;
            _settings = _simulation.Settings;
            _time = _simulation.Time;
            _random = _simulation.Random;
            _statistics = _simulation.Statistics;
            
            _productivity = 1.0;
            _age = _settings.HouseholdStartAge;
            _c = new double[_settings.NumberOfSectors];
            _vc = new double[_settings.NumberOfSectors];
            _budget = new double[_settings.NumberOfSectors];
            _firmShopArray = new Firm[_settings.NumberOfSectors];
            _wealth = 0;
            _wealth_P = 0;
            _wealth_U = 0;
            _wealth_UI = 0;

            _s_CES = new double[_settings.NumberOfSectors];
            for (int i = 0; i < _settings.NumberOfSectors; i++)   // Random share parameters in the CES-function
                _s_CES[i] = 1.0;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //_s_CES[i] = _random.NextDouble();

            if (_random.NextEvent(_settings.StatisticsHouseholdReportSampleSize))
                _report = true;


        }
        #endregion

        #region Household(TabFileReader file) : this()
        public Household(TabFileReader file) : this()
        {

            // WIP
            throw new NotImplementedException();
            
            _age = file.GetInt32("Age");
            _productivity = file.GetDouble("Productivity");

            int firmEmploymentID = file.GetInt32("FirmEmploymentID");
            int firmShopID = file.GetInt32("FirmShopID");
            
            if(firmEmploymentID != -1)
                _firmEmployment = _simulation.GetFirmFromID(firmEmploymentID);                        

            if(firmShopID != -1)            
                _firmShop = _simulation.GetFirmFromID(firmShopID);

            if(_firmEmployment != null)
                _firmEmployment.Communicate(ECommunicate.Initialize, this);

            _startFromDatabase = true;
            

        }
        #endregion
        #endregion

        #region EventProc

        public override void EventProc(int idEvent)
        {
            switch (idEvent)
            {

                case Event.System.Start:  // Initial households
                    #region Event.System.Start
                    if (_startFromDatabase)
                    {
                        _initialHousehold = false;
                    }
                    else
                    {
                        _age = _random.Next(_settings.HouseholdStartAge, _settings.HouseholdPensionAge);   
                        _productivity = Math.Exp(_random.NextGaussian(_settings.HouseholdProductivityLogMeanInitial, _settings.HouseholdProductivityLogSigmaInitial));
                        _initialHousehold = true;
                    }
                    break;
                    #endregion

                case Event.System.PeriodStart:
                    #region Event.System.PeriodStart

                    _bbudget = false;
                    bool unemployed = _firmEmployment == null;
                    //bool unemployed = _fired | _w == 0;  // Unemployed if just fired or if wage is zero
                    //if (_fired) _fired = false; // Fired only 1 period

                    ReportToStatistics();

                    _unempDuration = !unemployed ? 0 : _unempDuration+1;
                    if (_time.Now == 0) _w = _simulation.Statistics.PublicMarketWageTotal;

                    if(_time.Now % _settings.PeriodsPerYear==0)  // Skal fjernes!!!!!!!!!!!!!!!
                    {
                        _yr_consumption = 0;
                        _yr_employment = 0;
                    }

                    if (!unemployed) _yr_employment++;                  
                    
                    if (unemployed) _w = 0;
                    _no = 0;
                    _ok = 0;
                    _nShoppings = 0; // Initialize
                    for (int s = 0; s < _settings.NumberOfSectors; s++)
                    {
                        _c[s] = 0; // Initialization
                        _vc[s] = 0; // Initialization
                    }
                    break;
                    #endregion

                case Event.Economics.Update:
                    #region Event.Economics.Update
                    _bbudget = true;


                    _w = _firmEmployment == null ? 0.0 : _firmEmployment.FullWage;

                    //double r = _statistics.PublicInterestRate;
                    double r = 0;  // Interest income comes from profit income
                    double smooth = _settings.HouseholdIncomeSmooth;

                    // Income calculadet here because PublicProfitPerHousehold is calculated during PeriodStart-event by the Statistics object
                    //_income = _w * _productivity + _settings.HouseholdProfitShare * _simulation.Statistics.PublicProfitPerHousehold;
                    
                    //_wealth = _wealth_P + _wealth_U + _wealth_UI;            <===============================
                    
                    _income = _w * _productivity + _settings.HouseholdProfitShare * _simulation.Statistics.PublicProfitPerWealthUnit * _wealth;
                    _permanent_income = smooth * _permanent_income + (1 - smooth) * _income;
                    if(_w>0)
                        _w_exp = smooth * _w_exp + (1 - smooth) * _w * _productivity;
                    _wealth_target = _settings.HouseholdTargetWealthIncomeRatio * _permanent_income; 

                    #region Not used (Saving)
                    //if (_age >= _settings.HouseholdPensionAge) // If pensioner
                    //{
                    //    _consumption_budget = _settings.HouseholdDisSaveRatePensioner * _wealth;
                    //    _wealth = _wealth - _consumption_budget;
                    //}
                    //else if (_w > 0)                            // If employed
                    //{
                    //    _consumption_budget = (1 - _settings.HouseholdSaveRate) * _income;
                    //    _wealth = _wealth + _settings.HouseholdSaveRate * _income;
                    //}
                    //else                                     // If unemployed 
                    //{
                    //    _consumption_budget = _settings.HouseholdDisSaveRateUnemployed * _wealth;
                    //    _wealth = _wealth - _consumption_budget;
                    //}
                    #endregion

                    if (_time.Now <= _settings.BurnInPeriod2)
                    {
                        _wealth = 0;
                        _wealth_P = 0;
                        _wealth_U = 0;
                        _wealth_UI = 0;
                        _consumption_budget = _income;

                    }
                    else
                    {
                        bool bufferStock = true; // OLD specification
                        
                        if(bufferStock)
                        {
                            //_consumption_budget = _income + _settings.HouseholdMPCWealth * _wealth;
                            double mpc_w = _settings.HouseholdMPCWealth;
                            double mpc_i = _settings.HouseholdMPCIncome;
                            double r_extra = 0;

                            if (_age > _settings.HouseholdPensionAge)
                            {
                                r_extra = 0.02;
                                _consumption_budget = (r + r_extra) * _wealth + _income;
                            }
                            else
                            {
                                _consumption_budget = r * _wealth + _permanent_income + mpc_i * (_income - _permanent_income)
                                                     + mpc_w * (_wealth - _wealth_target);

                                if (_consumption_budget < 0) _consumption_budget = 0;


                            }

                            if ((1 + r) * _wealth + _income < 0)
                                _consumption_budget = 0; // Default!!!!!!!!!!!!!!!!  CODE THIS !!!!!!
                            else if (_consumption_budget > (1 + r) * _wealth + _income)
                                _consumption_budget = (1 + r) * _wealth + _income;

                            if (_consumption_budget < 0)
                                throw new Exception("_consumption_budget < 0");



                        }

                        if (!bufferStock)  // NEW specification
                        {
                            if (_age >= _settings.HouseholdPensionAge)  // Retirement
                            {
                                if (_firmEmployment != null)           // If employed
                                {
                                    _firmEmployment.Communicate(ECommunicate.IQuit, this);
                                    _firmEmployment = null;
                                }
                            }

                            double save_P = 0;
                            double save_U = 0;
                            r = _statistics.PublicExpectedInterestRate;   // Expected future rate of interest
                            int n_U = _settings.HouseholdUnemployedTimeHorizon;

                            if (_firmEmployment == null & _age < _settings.HouseholdPensionAge)  // If unemployed and in labor force
                            {
                                // No savings
                                _consumption_budget = _wealth_U / n_U;
                                _wealth_U = (1 + _statistics.PublicProfitPerWealthUnit) * _wealth_U - _consumption_budget;   // Actual interest rate
                                _wealth_P = (1 + _statistics.PublicProfitPerWealthUnit) * _wealth_P;

                            }
                            else 
                            {
                                
                                if(_age < _settings.HouseholdPensionAge) // If employed and in labor force
                                {
                                    int aP = _settings.HouseholdPensionAge;
                                    double alphaP = r / (1 - Math.Pow(1.0 / (1 + r), _settings.HouseholdPensionTimeHorizon));
                                    double alpha = r / (1 - Math.Pow(1.0 / (1 + r), 1 + aP - _age));
                                    double xi_P = _settings.HouseholdPensionIncomeRate;
                                    double u_bar = _settings.HouseholdExpectedUnemploymentRate;
                                    
                                    save_P = (xi_P * _w_exp - alphaP * Math.Pow(1 + r, 1 + aP - _age) * _wealth_P) 
                                           / (xi_P + (1 - u_bar) * Math.Pow(1 + r, 1 + aP - _age) * alphaP / alpha);                                   
                                    
                                    _wealth_P = (1 + _statistics.PublicProfitPerWealthUnit) * _wealth_P + save_P;
                                    //if (_wealth_P < 0)
                                    //{
                                    //    save_P = -(1 + _statistics.PublicProfitPerWealthUnit) * _wealth_P;
                                    //    _wealth_P = 0;
                                    //}

                                    double y_bar = _w_exp - save_P;
                                    double eta_U = _settings.HouseholdUnemploymentAdjustmentSpeed;
                                    save_U = eta_U * (n_U * y_bar - _wealth_U) - _statistics.PublicProfitPerWealthUnit * _wealth_U;
                                    _wealth_U = (1 + _statistics.PublicProfitPerWealthUnit) * _wealth_U + save_U;
                                    if (_wealth_U < 0)
                                    {
                                        save_U = -(1 + _statistics.PublicProfitPerWealthUnit) * _wealth_U;
                                        _wealth_U = 0;
                                    }

                                    _consumption_budget = _w * _productivity - save_U - save_P;
                                    if (_consumption_budget < 0)
                                        throw new Exception("_consumption_budget < 0");

                                }
                                else  // If pension
                                {
                                    int aP = _settings.HouseholdPensionAge;
                                    double alphaP = r / (1 - Math.Pow(1.0 / (1 + r), _settings.HouseholdPensionTimeHorizon));

                                    _consumption_budget = alphaP * _wealth_P;
                                    _wealth_P = (1 + _statistics.PublicProfitPerWealthUnit) * _wealth_P - _consumption_budget;


                                }


                            }

                            if(_wealth_UI>0)
                            {
                                double c_UI = 0.2 * _wealth_UI;
                                _consumption_budget += c_UI;
                                _wealth_UI = (1 + _statistics.PublicProfitPerWealthUnit) * _wealth_UI - c_UI;

                            }

                        }
                    }


                    if (_age >= _settings.HouseholdPensionAge)  // Retirement
                    {
                        if (_firmEmployment != null)           // If employed
                        {
                            _firmEmployment.Communicate(ECommunicate.IQuit, this);
                            _firmEmployment = null;
                        }
                    }
                    else if (_age < _settings.HouseholdPensionAge) // If in labor force
                    {
                        if (_firmEmployment == null | _fired)  // If unemployed
                            SearchForJob();
                        else  // If employed
                        {
                            // If job is changed, it is from next period. 
                            if (_random.NextEvent(_settings.HouseholdProbabilityOnTheJobSearch))
                                SearchForJob();

                            if (_random.NextEvent(_settings.HouseholdProbabilityQuitJob))
                            {
                                _firmEmployment.Communicate(ECommunicate.IQuit, this);
                                _firmEmployment = null;
                            }
                        }
                    }

                    if (_age > _settings.HouseholdPensionAge & _firmEmployment != null)
                        throw new Exception("Pensionist is working!");


                    for (int s = 0; s < _settings.NumberOfSectors; s++)
                        if (_random.NextEvent(_settings.HouseholdProbabilitySearchForShop))
                            SearchForShop(s);

                    MakeBudget();
                    _bbudget = true;
                    break;
                    #endregion

                case Event.Economics.Shopping:
                    #region Event.Economics.Shopping
                    int tt = _time.Now;
                    bool b = _bbudget;

                    if (_nShoppings == 0)
                        if (_consumption_budget > 0 & _budget[0] <= 0 & _time.Now>12*1) //???????????????????????????????!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            throw new Exception("_consumption_budget > 0 & _budget[0] <= 0");
                            //_budget[0] = _consumption_budget;

                    BuyFromShops();
                    _yr_consumption += _consumption;  // Kill this

                    // Consume before die: Last shopping in period                    
                    if (_nShoppings + 1 == _settings.HouseholdNumberShoppingsPerPeriod)
                    {
                        
                        _consumption_value = 0;
                        for (int s = 0; s < _settings.NumberOfSectors; s++)
                            _consumption_value += _vc[s];

                        _wealth += _income - _consumption_value;  // No interest!! Interest in _income
                        
                        _wealth_UI += _consumption_budget - _consumption_value; //PSP  //Unintentional wealth

                        //if (_time.Now > 12*40 & _wealth < 0)
                        //    MessageBox.Show("");

                        if (_random.NextEvent(ProbabilityDeath())) // If dead
                        {
                            if (_firmEmployment != null)
                            {
                                _firmEmployment.Communicate(ECommunicate.Death, this);
                                _statistics.Communicate(EStatistics.Death, _w * _productivity); // Wage earned this period
                            }

                            //Inheritance();
                            RemoveThisAgent();
                            return;
                        }
                    }

                    _nShoppings++;
                    break;
                    #endregion

                case Event.System.PeriodEnd:
                    #region Event.System.PeriodEnd
                    if (_time.Now == _settings.StatisticsWritePeriode)
                        Write();

                    if (!_initialHousehold)
                        if (_age < _settings.HouseholdPensionAge)
                            _productivity *= Math.Exp(_random.NextGaussian(0, _settings.HouseholdProductivityErrorSigma));
                        else
                            _productivity = 0;

                    if(_fired) _fired = false;  

                    _age++;
                    break;
                    #endregion

                case Event.System.Stop:
                    break;

                default:
                    base.EventProc(idEvent);
                    break;
            }
        }
        #endregion

        #region Internal methods
        #region BuyFromShops()
        void BuyFromShops()
        {

            // Buy goods
            //_consumption_value = 0;
            for (int s = 0; s < _settings.NumberOfSectors; s++)
            {
                BuyFromShop(s);
                //_consumption_value = _vc[s];
                
            }

        }

        #endregion

        #region BuyFromShop()
        void BuyFromShop(int sector)
        {

            _statistics.Communicate(EStatistics.BuyFromShop, this);
            if (_budget[sector] < 0)
                throw new Exception("Budget is negative");
          
            int nRemaining = _settings.HouseholdNumberShoppingsPerPeriod - _nShoppings;  // Remaining shoppings this period
            if (nRemaining == 0)
                throw new Exception("nRemaining is 0");

            if (_firmShopArray[sector] == null)
            {
                _firmShopArray[sector] = _simulation.GetRandomOpenFirm(sector, 1000); //SearchForShop ???????????????????????
                _statistics.Communicate(EStatistics.ChangeShopInBuyFromShopNull, this);

                if (_firmShopArray[sector] == null)
                {
                    _statistics.Communicate(EStatistics.CouldNotFindOpenFirm, this);
                    return;
                }
            }

            if(_budget[sector] == 0)
            {
                _statistics.Communicate(EStatistics.ZeroBudget, this);
            }

            _ok++;
            double buy = _budget[sector] / nRemaining;            
            if (buy < 0)
                throw new Exception("Can only buy positive number.");

            if (_firmShopArray[sector].Communicate(ECommunicate.CanIBuy, buy / _firmShopArray[sector].Price) == ECommunicate.Yes)
            {
                
                _c[sector] += buy / _firmShopArray[sector].Price;
                _vc[sector] += buy;
                _budget[sector] -= buy;

                if (_budget[sector] < 0)
                    throw new Exception("Negative budget");
                

                _statistics.Communicate(EStatistics.SuccesfullTrade, this);
                if (_budget[sector]>0) 
                    _statistics.Communicate(EStatistics.SuccesfullTradeNonZero, this);
                return;
            }
            else
            {
                
                double c = (double)_firmShopArray[sector].ReturnObject;
                if(c>0) 
                {
                    double vc = (double)_firmShopArray[sector].Price * c;

                    _c[sector] += c;
                    _vc[sector] += vc;
                    _budget[sector] -= vc;
                    buy -= vc;

                    if (_budget[sector] < 0)
                        throw new Exception("Negative budget");

                }

                Firm f = _simulation.GetNextFirmWithGoods(buy, sector, _settings.HouseholdNumberFirmsLookingForGoods);
                
                if (f != null)
                {
                    if (f.Communicate(ECommunicate.CanIBuy, buy / f.Price) == ECommunicate.Yes)
                    {
                        _firmShopArray[sector] = f;
                        _c[sector] += buy / _firmShopArray[sector].Price;
                        _vc[sector] += buy;
                        _budget[sector] -= buy;

                        if (_budget[sector] < 0)
                            throw new Exception("Negative budget");


                    }
                    _statistics.Communicate(EStatistics.ChangeShopInBuyFromShopLookingForGoods, this);
                }
                else
                {
                    _statistics.Communicate(EStatistics.CouldNotFindFirmWithGoods, this);
                }

                _no++;
                _ok--;
                _statistics.Communicate(EStatistics.CouldNotFindSupplier, this);
                return;

            }
        }

        #endregion

        #region MakeBudget()
        void MakeBudget()
        {
            // Calculate CES-priceindex
            _P_CES = 0;
            for (int s = 0; s < _settings.NumberOfSectors; s++)
            {
                if (_firmShopArray[s] == null)
                {
                    if(_time.Now<2*12)                        
                        _firmShopArray[s] = _simulation.GetRandomFirm(s); // Just to get up and running
                    else
                        _firmShopArray[s] = _simulation.GetRandomFirmsFromHouseholdsGood(1, s)[0];

                }

                _P_CES += _s_CES[s] * Math.Pow(_firmShopArray[s].Price, 1 - _settings.HouseholdCES_Elasticity);
            }
            _P_CES = Math.Pow(_P_CES, 1 / (1 - _settings.HouseholdCES_Elasticity));

            // Calculate budget 
            for (int s = 0; s < _settings.NumberOfSectors; s++)
            {
                _budget[s] = _s_CES[s] * Math.Pow(_firmShopArray[s].Price / _P_CES, 1 - _settings.HouseholdCES_Elasticity) * _consumption_budget;
                _c[s] = 0; // Initialization
                _vc[s] = 0; // Initialization

                if (_budget[s] < 0)
                    throw new Exception("Negative budget!!");

                if (_budget[s] == 0 & _consumption_budget>0)
                    throw new Exception("_budget[s] == 0 & _consumption_budget>0");


            }


        }

        #endregion

        #region SearchForJob()
        void SearchForJob()
        {

            
            double wageNow = _firmEmployment != null ? _firmEmployment.Wage : 0.0;
            if(_fired) wageNow = 0.0;

            // Intensive search first year
            int n_search = _settings.HouseholdNumberFirmsSearchJob;
            if(_age<_settings.HouseholdStartAge + _settings.PeriodsPerYear)
                n_search = _settings.HouseholdNumberFirmsSearchJobNew;

            var firms = _simulation.GetRandomFirmsFromHouseholdsEmployment(n_search);
            //firms = firms.OrderByDescending(x => x.Wage).ToArray<Firm>(); // Order by wage. Highest wage first
            var sfirms = firms.OrderByDescending(x => x.Wage); // Order by wage. Highest wage first

            foreach (Firm f in sfirms)
            {
                if(f.Wage > wageNow)
                    if (f.Communicate(ECommunicate.JobApplication, this) == ECommunicate.Yes)
                    {
                        if (_firmEmployment != null & !_fired)
                            _firmEmployment.Communicate(ECommunicate.IQuit, this);

                        if(_fired) _fired = false;

                        _firmEmployment = f;
                        break;
                    }
            }
        }
        #endregion

        #region SearchForShop()
        void SearchForShop(int sector)
        {

            //Firm[] firms = _simulation.GetRandomOpenFirms(_settings.HouseholdNumberFirmsSearchShop, sector);

            Firm[] firms = null;
            if (_time.Now<12*5) // Solving up-start problem  
                firms = _simulation.GetRandomOpenFirms(_settings.HouseholdNumberFirmsSearchShop, sector, 1000);
            else
                firms = _simulation.GetRandomFirmsFromHouseholdsGood(_settings.HouseholdNumberFirmsSearchShop, sector);

            double min_price = Double.MaxValue;
            Firm best_firm=null;

            foreach (Firm f in firms) 
            { 
                if(f.Price<min_price)
                {
                    min_price = f.Price;
                    best_firm = f;
                }
            }

            if (_firmShopArray[sector] == null || min_price < _firmShopArray[sector].Price)
                _firmShopArray[sector] = best_firm;


            //var sfirms = firms.OrderBy(x => x.Price); // Order by price. Lowest price first

                //if (_firmShopArray[sector] == null || sfirms.First().Price < _firmShopArray[sector].Price)
                //{
                //    _firmShopArray[sector] = sfirms.First();
                //    _statistics.Communicate(EStatistics.ChangeShopInSearchForShop, this);
                //}
        }
        #endregion

        #region Inheritance
        void Inheritance()
        {
            if (_wealth == 0)
                return;
            
            double inheritance = _wealth / _settings.NumberOfInheritors;
            int inh = 0;
            while(inh<_settings.NumberOfInheritors)
            {
                Household h = _simulation.GetRandomHousehold();
                if(h.Age<_settings.HouseholdPensionAge)
                {
                    h.Communicate(ECommunicate.Inheritance, inheritance);
                    inh++;
                }
            }
            _wealth = 0;

        }
        #endregion

        #region ProbabilityDeath()
        double ProbabilityDeath()
        {
            return Math.Pow(1 + Math.Exp(0.1 * _age / _settings.PeriodsPerYear - 10.0), 1.0/_settings.PeriodsPerYear) - 1;

        }
        #endregion

        #region ReportToStatistics()
        void ReportToStatistics()
        {
            if (_report & !_settings.SaveScenario)
            {
                _statistics.StreamWriterHouseholdReport.WriteLineTab(_time.Now, this.ID, _productivity, _age, 
                    _consumption, _consumption_value, _income, _wealth, _w, _statistics.PublicMarketPriceTotal, _consumption_budget, _permanent_income, _wealth_target);
                
                //_statistics.StreamWriterHouseholdReport.Flush();
            }
        }
        #endregion
        #endregion

        #region Communicate
        public ECommunicate Communicate(ECommunicate comID, object o)
        {
            Firm f=null;
            switch (comID)
            {
                case ECommunicate.YouAreFired:
                    _fired = true;
                    return ECommunicate.Ok;

                case ECommunicate.AvertiseJob:
                    if(_random.NextEvent(_settings.HouseholdProbabilityReactOnAdvertisingJob))
                    {
                        f = (Firm)o;
                        if (f.Wage > _w)
                            if (f.Communicate(ECommunicate.JobApplication, this) == ECommunicate.Yes)
                            {
                                if (_firmEmployment != null)
                                    _firmEmployment.Communicate(ECommunicate.IQuit, this);

                                _firmEmployment = f;
                            }

                        return ECommunicate.Ok;
                    }
                    return ECommunicate.No;

                case ECommunicate.AvertiseGood:
                    if (_random.NextEvent(_settings.HouseholdProbabilityReactOnAdvertisingGood))
                    {
                        f = (Firm)o;
                        if (_firmShopArray[f.Sector] != null)
                            if (f.Price < _firmShopArray[f.Sector].Price)
                                _firmShopArray[f.Sector] = f;
                        return ECommunicate.Ok;

                    }
                    return ECommunicate.No;

                case ECommunicate.Initialize:
                    _firmEmployment = (Firm)o;
                    return ECommunicate.Ok;
                
                case ECommunicate.Inheritance:
                    _wealth += (double)o;
                    return ECommunicate.Ok;

                default:
                    return ECommunicate.Ok;
            }
        }
        #endregion

        #region Write()
        void Write()
        {

            int firmEmploymentID = _firmEmployment != null ? _firmEmployment.ID : -1;
            int firmShopID = _firmShop != null ? _firmShop.ID : -1;

            if (!_settings.SaveScenario)
                _statistics.StreamWriterDBHouseholds.WriteLineTab(ID, _age/ _settings.PeriodsPerYear, firmEmploymentID, firmShopID, _productivity);

        }
        #endregion

        #region Public proporties
        public int Age
        {
            get { return _age; }
        }
        /// <summary>
        /// True if unemployed primo
        /// </summary>
        public bool Unemployed
        {
            get { return _w == 0.0; }
        }

        /// <summary>
        /// Duration of unemployment spell
        /// </summary>
        public int UnemploymentDuration
        {
            get { return _unempDuration; }
        }
        public double Wealth
        {
            get { return _wealth; }
        }
        public double Income
        {
            get { return _income; }
        }

        public int No
        {
            get { return _no; }
        }

        public int Ok
        {
            get { return _ok; }
        }

        public double Productivity
        {
            get { return _productivity; }
        }
        public double YearConsumption
        {
            get { return _yr_consumption; }
        }
        public int YearEmployment
        {
            get { return _yr_employment; }
        }

        public double CES_Price
        {
            get { return _P_CES; }
        }

        public Firm FirmEmployment
        {
            get { return _firmEmployment; }
        }
        public Firm FirmShopArray(int sector)
        {
            return _firmShopArray[sector]; 
        }

        public double ConsumptionBudget
        {
            get { return _consumption_budget; }
        }

        public double ConsumptionValue
        {
            get { return _consumption_value; }
        }
        public double Consumption
        {
            get { return _c[0]; } //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        #endregion

    }
}
