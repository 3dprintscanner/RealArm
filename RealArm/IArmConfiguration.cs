using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealArm
{
    public interface IArmConfiguration
    {

        // This arm configuration stores the config data for the arm, the arm can be configured to:
        // set a threshold value for x, y and z movement.
        // set minimum and maximum values for displacement as time in seconds of movement
        // set a quantization rate for the movement commands

        decimal[] XYZThreshold { get; set; }
        void SetMinMax(Tuple[] minMax);
        decimal QuantizeStep{get; set;}
        
    }
}
