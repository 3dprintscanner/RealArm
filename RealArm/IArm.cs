using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealArm
{
    public interface IArm
    {
        IArmConfiguration ArmConfiguration { get; set; }
        decimal[] Positions { get; set; }
        bool canMoveTo(decimal[] position);
        void Move(decimal[] movement);
        void Grab();
        void Release();
        
        event EventHandler MoveComplete;
        event EventHandler StopReached;

        // sets the data for the arm, the arm has a default configuration too
        // an arm can receive movement commands
        // and arm can raise an event when a stop is reached
        // an arm can raise an event when a movement completes
        // an arm stores its current position
        // an arm stores its configuration
    }
}
