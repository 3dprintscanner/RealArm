using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealArm
{
    public class MoveEventArgs : EventArgs
    {
        // these event args capture the move and stop event data for the arm. This should contain the data for the axis which has reached a stop and the movement which has finished
        public int MoveFinishedArm{get; set;}
        public bool StopReached {get; set;}
        public decimal[] Position { get; set; } 

    }
}
