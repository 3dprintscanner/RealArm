using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealArm
{
    public class ArmConfiguration : IArmConfiguration
    {
        public decimal[] XyzThreshold { get; set; }
        private IList<Tuple<decimal,decimal>> _minMax;

        public void SetMinMax(string axis, IList<Tuple<decimal, decimal>> minMax)
        {
            this._minMax = minMax;
        }

        public decimal QuantizeStep { get; set; }

        public void SetMinMax(IList<Tuple<decimal,decimal>> minMax)
        {
            throw new NotImplementedException();
        }
    }
}
