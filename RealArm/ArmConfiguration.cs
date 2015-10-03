using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealArm
{
    public class ArmConfiguration : IArmConfiguration
    {
        public decimal[] XYZThreshold { get; set; }
        private IList<Tuple<decimal,decimal>> minMax;

        public void SetMinMax(string axis, IList<Tuple<decimal, decimal>> minMax)
        {
            this.minMax = minMax;
        }

        public decimal QuantizeStep { get; set; }
    }
}
