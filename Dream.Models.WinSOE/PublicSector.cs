using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dream.AgentClass;

namespace Dream.Models.WinSOE
{
    public class PublicSector : Agent
    {

        #region Private fields
        Simulation _simulation;
        Settings _settings;
        Time _time;
        Statistics _statistics;

        double _corporateTaxRevenue=0;
        double _lumpsumTax=0;
        #endregion

        #region Constructor
        public PublicSector()
        {
            _simulation = Simulation.Instance;
            _settings = _simulation.Settings;
            _time = _simulation.Time;
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
                    _corporateTaxRevenue = 0;
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

                case ECommunicate.PayCorporateTax:
                    _corporateTaxRevenue += (double)o;

                    return ECommunicate.Ok;
                default:
                    return ECommunicate.Ok;
            }
        }
        #endregion

        public double CorporateTaxRevenue
        {
            get { return _corporateTaxRevenue; }
        }

        /// <summary>
        /// Lumpsum tax per household (Payed by household)
        /// </summary>
        public double LumpsumTax
        {
            get { return _lumpsumTax; }
        }


    }
}
