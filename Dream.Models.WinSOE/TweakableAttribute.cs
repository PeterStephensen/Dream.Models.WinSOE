using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dream.Models.WinSOE
{
    /// <summary>
    /// Class to to mark a Settings-property as tweakable between a min and max value
    /// </summary>
    public class TweakableAttribute : Attribute
    {

        public object Maximum { get; set; }
        public object Minimum { get; set; }
        public TweakableAttribute(object minimum, object maximum)
        {
            Maximum = maximum;
            Minimum = minimum;
        }
    }
}
