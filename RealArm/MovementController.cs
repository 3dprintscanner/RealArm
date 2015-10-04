using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealArm
{
    public class MovementController : MovementControllerBase
    {

        public IArm arm { get; set; }
        public ISensor sensor { get; set; }
        private bool moveInProgress = true;
          

        public IArmConfiguration ArmConfiguration { get; set; }

        public MovementController(IArm arm, ISensor sensor)
        {
            this.arm = arm;
            this.sensor = sensor;
            // register event handlers
            arm.MoveComplete += OnMoveComplete;
            arm.StopReached += OnStopReached;
        
        }

        protected void OnMoveComplete(object sender, EventArgs e)
        {
            // free the move in progress to allow another movement
            this.moveInProgress = false;
        }


        protected void OnStopReached(object sender, EventArgs e) 
        {
            // the stop has been reached, give an error??? do not allow the arm to move further in one direction
        }



        public override void Listen()
        {
            // attach the arm to the PXCM sensor
            throw new NotImplementedException();
        }

        public override void UnListen()
        {
            throw new NotImplementedException();
        }
    }
}
