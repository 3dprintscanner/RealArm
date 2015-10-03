using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealArm
{
    public interface IMovementController
    {
        // The IMovementController takes a set of movements from the camera displacement and dispatches the movement to the USBArm Interface Class

        // register the arm with then arm controller

        IArm Arm { get; set; }

        // register the sensor interface to send the movement commands

        ISensor Sensor { get; set; }

        // set the arm configuration for a specified arm, this allows resetting of arm configuration during the running of the program 
        IArmConfiguration ArmConfiguration {get; set;}

        // activate the movement controller to recieve movements from the arm 
        void Listen();

        // deactivate the movement controller to not receive movements from the arm 
        void UnListen();

        delegate void MoveEventHandler(object sender, MoveEventArgs e);

        MoveEventHandler OnMoveComplete;
        MoveEventHandler OnStopReached;


    }
}
