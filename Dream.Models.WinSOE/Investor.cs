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
        double _wealth;
        double _wealthTarget;
        double _takeOut;
        int _age = 0;
        double _kappa;
        #endregion

        #region Constructor
        public Investor(double wealth, double permanentIncome)
        {
            _simulation = Simulation.Instance;
            _settings = _simulation.Settings;
            _time = _simulation.Time;
            _statistics = _simulation.Statistics;

            _wealth = wealth;
            _permanentIncome = permanentIncome;
            //_kappa = _settings.InvestorWealthIncomeRatioTarget;


        }
        #endregion

        #region EventProc
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
        #endregion

        #region Communicate
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
        #endregion

        #region Iterate()
        public void Iterate()
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
                _wealthTarget = kappa * _permanentIncome;

                _takeOut = 0;
                if (_age > _settings.InvestorBuildUpPeriods)
                {

                    // Buffe-stock a la Carroll
                    double x_bar = _settings.InvestorShareOfPermanentIncome;
                    //double a = xi;
                    //double y_bar = _permanentIncome;

                    //double x = ((1 + r) * _wealth + _income) / y_bar;
                    //_takeOut = y_bar * (x_bar + a * (x - x_bar));
                                                            
                    _takeOut = r * _wealth + x_bar * _permanentIncome + xi * (_income - x_bar * _permanentIncome)
                                                             + eta * (_wealth - _wealthTarget);
                    if (_takeOut < 0) _takeOut = 0;

                    if ((1 + r) * _wealth + _income < 0) // Finansial Crises
                        _takeOut = (1 + r) * _wealth + _income; // Bail Out
                    else if (_takeOut > (1 + r) * _wealth + _income)
                        _takeOut = (1 + r) * _wealth + _income;

                    //_kappa += - 0.0001 * (_wealth - _wealthTarget);

                }

                _wealth = (1 + r) * _wealth + _income - _takeOut;

                _age++;

            }





        }
        #endregion

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

        /// <summary>
        /// Investors profit income
        /// </summary>
        public double PermanentIncome { get { return _permanentIncome; } }

        #endregion




    }
}
