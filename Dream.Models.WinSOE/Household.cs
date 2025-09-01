
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dream.AgentClass;
using Dream.IO;  

namespace Dream.Models.WinSOE
{
    public enum EHouseholdTheory
    {
        BufferStock,
        BehavioralSavings,
        FixedSavingsRate
    }

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
        //double _w = 0; //Wage
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
        double _expected_consumption_value = 0;
        double _wealth = 0;
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
        double _expected_income = 0;
        //double _expected_r = 0;
        double _w_exp; // Expected wage
        double _w_reservation=0;
        double _pension_fixed_cost = -1;
        bool _searchJobUnemployed = false;
        bool _searchJobOnJob = false;
        double _education;
        #endregion

        #region Constructors
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
            _education = _random.NextDouble();

            _s_CES = new double[_settings.NumberOfSectors];
            for (int i = 0; i < _settings.NumberOfSectors; i++)   // Random share parameters in the CES-function
                _s_CES[i] = 1.0;  // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //_s_CES[i] = _random.NextDouble();

            if (_random.NextEvent(_settings.StatisticsHouseholdReportSampleSize))
                _report = true;


        }
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

        public override void EventProc(int idEvent)
        {
            switch (idEvent)
            {
                case Event.System.Start:  // Initial households
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

                case Event.System.PeriodStart:
                    bool unemployed = _firmEmployment == null;
                    //bool unemployed = _fired | _w == 0;  // Unemployed if just fired or if wage is zero
                    //if (_fired) _fired = false; // Fired only 1 period

                    ReportToStatistics();

                    _unempDuration = !unemployed ? 0 : _unempDuration+1;
                    //if (_time.Now == 0) _w = _simulation.Statistics.MarketWageTotal;

                    if(_time.Now % _settings.PeriodsPerYear==0)  // Skal fjernes!!!!!!!!!!!!!!!
                    {
                        _yr_consumption = 0;
                        _yr_employment = 0;
                    }

                    if (!unemployed) _yr_employment++;                  
                    
                    //if (unemployed) _w = 0;
                    _no = 0;
                    _ok = 0;
                    _nShoppings = 0; // Initialize
                    for (int s = 0; s < _settings.NumberOfSectors; s++)
                    {
                        _c[s] = 0; // Initialization
                        _vc[s] = 0; // Initialization
                    }
                    break;

                case Event.Economics.Update:
                    //_w = _firmEmployment == null ? 0.0 : _firmEmployment.FullWage;

                    _income = 0;
                    if(_firmEmployment!=null)
                        _income = _firmEmployment.FullWage * _productivity;

                    if (_age < _settings.HouseholdStartAge+6) // Start up
                    {
                        _expected_income = _income;
                        _expected_consumption_value = _income;
                        //_expected_r = _simulation.Statistics.PublicProfitPerWealthUnit;
                    }
                    else if(_firmEmployment!=null)  // If employed
                    {
                        double smooth = _settings.HouseholdIncomeSmooth;
                        _expected_income = smooth * _expected_income + (1 - smooth) * _income;
                        _expected_consumption_value = smooth * _expected_consumption_value + (1 - smooth) * _consumption_value;
                        //_expected_r = smooth * _expected_r + (1 - smooth) * _simulation.Statistics.PublicProfitPerWealthUnit;
                        //_expected_r = _statistics.PublicExpectedInterestRate;

                    }

                    // Not used
                    //if (_w>0)
                    //    _w_exp = smooth * _w_exp + (1 - smooth) * _w * _productivity;
                    
                    if (_time.Now <= _settings.BurnInPeriod2)
                    {
                        _wealth = 0;
                        _consumption_budget = _income;
                    }
                    else
                    {
                        
                        switch (_settings.HouseholdTheory)
                        {
                            case EHouseholdTheory.BufferStock:
                                BufferStock();
                                break;

                            case EHouseholdTheory.BehavioralSavings:
                                BehavioralSavings();
                                break;

                            case EHouseholdTheory.FixedSavingsRate:
                                FixedSavingsRate();
                                break;

                            default:
                                throw new Exception("Unknown household theory");
                        }

                        // No credit market
                        double r = _simulation.Statistics.InterestRate;
                        double wealth = (1 + r) * _wealth + _income - _consumption_budget;
                        if (wealth < 0)
                            _consumption_budget = (1 + r) * _wealth + _income;

                        // Remove rounding errors
                        if (_consumption_budget < 0 & _consumption_budget > -1.0e-5) _consumption_budget = 0;

                        if (_consumption_budget < 0)
                            throw new Exception("_consumption_budget < 0");

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
                        {
                            //if(!_fired)
                            SearchForJob();
                        }    
                        else  // If employed
                        {
                            // If job is changed, it is from next period. 
                            if (_random.NextEvent(_settings.HouseholdProbabilityOnTheJobSearch))
                                SearchForJob(onTheJobSearch:true);

                            if (_random.NextEvent(_settings.HouseholdProbabilityQuitJob))
                            {
                                _firmEmployment.Communicate(ECommunicate.IQuit, this);
                                _w_reservation = _firmEmployment.FullWage;
                                _firmEmployment = null;
                            }
                        }
                    }

                    if (_age > _settings.HouseholdPensionAge & _firmEmployment != null)
                        throw new Exception("Pensionist is working!");

                    for (int s = 0; s < _settings.NumberOfSectors; s++)
                        if (_random.NextEvent(_settings.HouseholdProbabilitySearchForShop))
                            SearchForShop(s);

                    // Simplification: Hand-to-mouth
                    if (_settings.SimplificationConsumption)
                    {
                        //if (_time.Now > _settings.BurnInPeriod3)
                        //    _income += _statistics.ProfitPerHousehold;
                        _consumption_budget = Math.Max(_income + 0.1 * _wealth, 0.0);
                    }


                    MakeBudget();
                    break;

                case Event.Economics.Shopping:
                    if (_nShoppings == 0)
                        if (_consumption_budget > 0 & _budget[0] <= 0 & _time.Now>12*1) //???????????????????????????????!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            throw new Exception("_consumption_budget > 0 & _budget[0] <= 0");

                    BuyFromShops();
                    _yr_consumption += _consumption;  // Kill this

                    // Consume before die: Last shopping in period                    
                    if (_nShoppings + 1 == _settings.HouseholdNumberShoppingsPerPeriod)
                    {
                        
                        _consumption_value = 0;
                        for (int s = 0; s < _settings.NumberOfSectors; s++)
                            _consumption_value += _vc[s];

                        if(_settings.SimplificationConsumption)
                            _wealth += _income - _consumption_value;  // No interest!! Interest in _income
                        else
                            _wealth += _statistics.InterestRate * _wealth + _income - _consumption_value;

                        if (_wealth < 0)   // Round off errors !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                            _wealth = 0;                                            
                        
//                        if (_wealth < -1e-3)
//                        {
//#if WIN_APP                            
//                            //MessageBox.Show("Wealth < -1e-7: " + _wealth.ToString());
//#endif
//                            //throw new Exception(string.Format("_wealth < 0: _wealth={0}", _wealth));
//                            _wealth = 0;
//                        }



                        if (_random.NextEvent(ProbabilityDeath())) // If dead
                        {
                            if (_firmEmployment != null)
                            {
                                _firmEmployment.Communicate(ECommunicate.Death, this);
                                _statistics.Communicate(EStatistics.Death, _firmEmployment.FullWage * _productivity); // Wage earned this period
                            }

                            Inheritance();
                            RemoveThisAgent();
                            return;
                        }
                    }

                    _nShoppings++;
                    break;

                case Event.System.PeriodEnd:
                    if (_time.Now == _settings.StatisticsWritePeriode)
                        Write();

                    if (!_initialHousehold)
                        if (_age < _settings.HouseholdPensionAge)
                            _productivity *= Math.Exp(_random.NextGaussian(0, _settings.HouseholdProductivityErrorSigma));
                        else
                            _productivity = 0;

                    if (_fired) _fired = false;
                    
                    // If unemployed reduce reservation wage
                    if(_firmEmployment==null)
                        _w_reservation *= (1 + _statistics.ExpectedInflation) * _settings.HouseholdReservationWageReduction;
                        //_w_reservation *= _settings.HouseholdReservationWageReduction;

                    _age++;
                    break;

                case Event.System.Stop:
                    break;

                default:
                    base.EventProc(idEvent);
                    break;
            }
        }

        #region Internal methods
        void BufferStock()
        {

            double mpc_w = _settings.HouseholdMPCWealth;
            double mpc_i = _settings.HouseholdMPCIncome;
            double mpc_c = _settings.HouseholdMPCCapitalIncome;
            //int aP = _settings.HouseholdPensionAge;

            //double r_exp = _simulation.Statistics.ExpectedInterestRate;
            double r_exp = _simulation.Statistics.ExpectedRealInterestRate;
            double r = _simulation.Statistics.InterestRate;

            if (_age >= _settings.HouseholdPensionAge) // If pensionist
            {
                _consumption_budget = Rho(r_exp, _settings.HouseholdPensionTimeHorizon) * _wealth;
                _income = 0;
            }
            else if (IsUnemployed())  
            {
                _consumption_budget = _settings.HouseholdUnemployedConsumptionRate * _expected_income;
            }
            else                               // If imployed
            {               
                _wealth_target = _settings.HouseholdTargetWealthIncomeRatio * _expected_income;
                //_wealth_target = _expected_income / Rho(r_exp, aP - _age);

                _consumption_budget = r_exp * _wealth + _expected_income
                    + mpc_i * (_income - _expected_income)
                    + mpc_c * (r - r_exp) * _wealth
                    + mpc_w * (_wealth - _wealth_target);

                
                double min_budget = 0.5 * _expected_income; //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                if(_consumption_budget < min_budget)
                    _consumption_budget = min_budget;


                if (_consumption_budget < 0)
                    throw new Exception("_consumption_budget < 0");


            }
        }
        void BehavioralSavings()
        {
            
            //double r_exp = (1+_statistics.ExpectedRealInterestRate) / (1 + _statistics.GrowthPerPeriod) - 1.0;  //!!!!!!!!!!!!!!!!!!! Interest rate går mod 0 !
            double r_exp = _statistics.ExpectedRealInterestRate;
            int aP = _settings.HouseholdPensionAge;
            double rho_pens;

            if (_age >= aP)           // If pensionier 
            {
                //int horiz = Math.Min(_settings.HouseholdPensionTimeHorizon, 102*12 - _age);
                int horiz = _settings.HouseholdPensionTimeHorizon;
                rho_pens = Rho(r_exp, horiz);

                _consumption_budget = rho_pens * _wealth;
            }
            else if (IsUnemployed())  
            {
                _consumption_budget = _settings.HouseholdUnemployedConsumptionRate * _expected_income;
            }
            else                              // If imployed
            {
                if (_simulation.Random.NextEvent(_settings.HouseholdProbabilityRecalculateBudget) || _pension_fixed_cost == -1)
                {
                    double rho = Rho(r_exp, aP - _age);
                    double gammaP = _settings.HouseholdPensionerConsumptionRate;
                    rho_pens = Rho(r_exp, _settings.HouseholdPensionTimeHorizon);

                    _pension_fixed_cost = _expected_consumption_value * (r_exp / (1 + r_exp)) * gammaP
                    //_pension_fixed_cost = gammaP * _expected_income * (r_exp / (1 + r_exp))  
                                          / ((Math.Pow(1 + r_exp, aP - _age) - 1) * rho_pens)
                                          - _wealth * rho;

                    if (double.IsNaN(_pension_fixed_cost))
                        throw new Exception("_pension_fixed_cost is NaN");

                }
                else
                {
                    //_pension_fixed_cost *= (1 + _statistics.ExpectedInflation)* (1 + _statistics.GrowthPerPeriod);
                    _pension_fixed_cost *= (1 + _statistics.ExpectedInflation);
                }

                _consumption_budget = _income - _pension_fixed_cost;

                double cons_min = _settings.HouseholdMinimumConsumptionShare * _expected_income; 
                if (_consumption_budget<cons_min)
                    _consumption_budget = cons_min;

            }

            if (_consumption_budget < 0)
                throw new Exception("_consumption_budget < 0");

            if (double.IsNaN(_consumption_budget))
                throw new Exception("_consumption_budget is NaN");

            if (double.IsNegativeInfinity(_consumption_budget))
                throw new Exception("_consumption_budget is -Infinity");
        }
        void FixedSavingsRate() 
        {
            double r_exp = _simulation.Statistics.ExpectedInterestRate;
            int aP = _settings.HouseholdPensionAge;

            if (_age >= aP)
            {
                _consumption_budget = Rho(r_exp, _settings.HouseholdPensionTimeHorizon) * _wealth;
                _income = 0;
            }
            else if (_firmEmployment == null)  // If unemployed
            {
                _consumption_budget = _settings.HouseholdUnemployedConsumptionRate * _expected_income;
            }
            else
            {
                _consumption_budget = _income - 0.03 * _expected_income;

                double cons_min = 0.5 * _expected_income;
                if (_consumption_budget < cons_min)
                    _consumption_budget = cons_min;

            }
        }
        void BuyFromShops()
        {

            for (int s = 0; s < _settings.NumberOfSectors; s++)
            {
                BuyFromShop(s);               
            }
        }
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
                throw new Exception("buy < 0");

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
                
                double c = (double)_firmShopArray[sector].ReturnObject; // How much the firm can sell
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
                if(_settings.NumberOfSectors>1)
                    _budget[s] = _s_CES[s] * Math.Pow(_firmShopArray[s].Price / _P_CES, 1 - _settings.HouseholdCES_Elasticity) * _consumption_budget;
                else
                    _budget[s] = _consumption_budget;

                _c[s] = 0; // Initialization
                _vc[s] = 0; // Initialization

                if (_budget[s] < 0)
                    throw new Exception(string.Format("Negative budget!! _budget[s] = {0}", _budget[s]));

                if (_budget[s] == 0 & _consumption_budget>0)
                    throw new Exception("_budget[s] == 0 & _consumption_budget>0");


            }


        }
        void SearchForJob(bool onTheJobSearch=false)
        {

            if(onTheJobSearch)
                _searchJobOnJob = true;
            else
                _searchJobUnemployed = true;
                       
            double w_res = IsEmployed() ? _firmEmployment.Wage : _w_reservation;
            if(_fired) w_res = _w_reservation;
            if(onTheJobSearch) w_res *= (1.0 + _settings.HouseholdWageMarkupOnTheJobSearch); //!!!!!!!!!!!!!!!!!!!

            // Intensive search first year
            int n_search = _settings.HouseholdNumberFirmsSearchJob;
            if(_age<_settings.HouseholdStartAge + _settings.PeriodsPerYear)
                n_search = _settings.HouseholdNumberFirmsSearchJobNew;

            var firms = _simulation.GetRandomFirmsFromEmployedHouseholds(n_search);
            var sfirms = firms.OrderByDescending(x => x.Wage); // Order by wage. Highest wage first
            
            foreach (Firm f in sfirms)
            {
                if(f.Wage > w_res)
                    if (f.Communicate(ECommunicate.JobApplication, this) == ECommunicate.Yes)
                    {
                        if (IsEmployed() & !_fired)
                        {
                            _firmEmployment.Communicate(ECommunicate.IQuit, this);
                            _statistics.Communicate(EStatistics.JobFromJob, this);

                        }
                        else
                            _statistics.Communicate(EStatistics.JobFromUnemployment, this);

                        if (_fired) _fired = false;

                        _firmEmployment = f;
                        break;
                    }
            }
        }
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

        }
        void Inheritance()
        {
            if (_wealth == 0)
                return;
            
            _statistics.Communicate(EStatistics.Inheritance, _wealth);
            double inheritance = (1 + _statistics.InterestRate) * _wealth / _settings.NumberOfInheritors;
            int i = 0;
            while(i<_settings.NumberOfInheritors)
            {
                Household h = _simulation.GetRandomHousehold();
                if(h.Age<_settings.HouseholdPensionAge-12*1 & h.Age>_settings.HouseholdStartAge+12*15)
                {
                    h.Communicate(ECommunicate.Inheritance, inheritance);
                    i++;
                }
            }
            _wealth = 0;

        }
        double Rho(double r, int n)
        {
            //if (r < 0)
            //    throw new Exception("r < 0 in method Rho.");

            double rho;
            if (r > 0)
                rho = (r / (1 + r)) / (1 - Math.Pow(1 / (1 + r), n));
            else
                rho = 1 / n;

            return rho;
        }
        double ProbabilityDeath()
        {
            return Math.Pow(1 + Math.Exp(0.1 * _age / _settings.PeriodsPerYear - 10.0), 1.0/_settings.PeriodsPerYear) - 1;

        }
        int OneIfTrue(bool b)
        {
            int r = 0;
            if (b) r = 1;
            return r;
        }
        bool IsEmployed()
        {
            return _firmEmployment != null;
        }
        bool IsUnemployed()
        {
            return _firmEmployment == null;
        }
        void ReportToStatistics()
        {
            if (_report & !_settings.SaveScenario)
            {
                double wage = IsEmployed() ? _firmEmployment.FullWage : 0;

                _statistics.StreamWriterHouseholdReport.WriteLineTab(_time.Now, this.ID, _productivity, _age, 
                    _consumption, _consumption_value, _income, _wealth, wage, _statistics.MarketPriceTotal, _consumption_budget, _expected_income, _wealth_target,
                    OneIfTrue(_searchJobOnJob), OneIfTrue(_searchJobUnemployed), OneIfTrue(!IsEmployed()), _unempDuration, 
                    _w_reservation);

                _searchJobOnJob = false;            
                _searchJobUnemployed = false;       
            }
        }
        #endregion

        public ECommunicate Communicate(ECommunicate comID, object o)
        {
            Firm f=null;
            switch (comID)
            {
                case ECommunicate.YouAreFired:
                    _fired = true;
                    f = (Firm)o;
                    _w_reservation = f.FullWage;
                    return ECommunicate.Ok;

                case ECommunicate.AvertiseJob:
                    if(_random.NextEvent(_settings.HouseholdProbabilityReactOnAdvertisingJob))
                    {
                        f = (Firm)o;
                        
                        double w_res;
                        if (!IsEmployed())
                            w_res = _w_reservation;                                                           
                        else
                            w_res = _firmEmployment.FullWage * (1.0 + _settings.HouseholdWageMarkupOnTheJobSearch);   // 0 * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                          //w_res = f.FullWage * (1.0 + _settings.HouseholdWageMarkupOnTheJobSearch);   // 0 * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                        if (f.FullWage > w_res)
                            if (f.Communicate(ECommunicate.JobApplication, this) == ECommunicate.Yes)
                            {
                                if (_firmEmployment != null)
                                {
                                    _firmEmployment.Communicate(ECommunicate.IQuit, this);
                                    _statistics.Communicate(EStatistics.JobFromJobAdvertise, this); 

                                }
                                else
                                    _statistics.Communicate(EStatistics.JobFromUnemploymentAdvertise, this);

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

        void Write()
        {

            int firmEmploymentID = _firmEmployment != null ? _firmEmployment.ID : -1;
            int firmShopID = _firmShop != null ? _firmShop.ID : -1;

            if (!_settings.SaveScenario)
                _statistics.StreamWriterDBHouseholds.WriteLineTab(ID, _age/ _settings.PeriodsPerYear, firmEmploymentID, firmShopID, _productivity);

        }
        
        #region Public proporties
        public int Age
        {
            get { return _age; }
        }
        public bool Unemployed
        {
            get { return !IsEmployed(); }
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
        public double ConsumptionSector0
        {
            get { return _c[0]; } //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }

        public double Education
        {
            get { return _education; }
        }

        #endregion

    }
}
