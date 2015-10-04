using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOwi535;

namespace RealArm
{
    public class MovementController : MovementControllerBase
    {

        public IArm arm { get; set; }
        public ISensor sensor { get; set; }
        private bool moveInProgress = false;
        private RobotArm robotArm;
        private int frameCounter = 25;
        private int currentFrame = 0;
        private Coord3D referencePosition;
        private Coord3D newPosition;
          

        public IArmConfiguration ArmConfiguration { get; set; }

        public MovementController(IArm arm, ISensor sensor)
        {
            this.arm = arm;
            this.sensor = sensor;
            // register event handlers
            arm.MoveComplete += OnMoveComplete;
            arm.StopReached += OnStopReached;
            BlinkLight();
        
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


        public void OnMovementRecieved(object sender, EventArgs e)
        { 
            // use this method to decide what to do with the movement?
            // get the movement and compare it in absolute space to the last movement, store a list of the last 10 movements and average the distance moved between them?? 
            currentFrame++;
            if (currentFrame % frameCounter == 0)
            {
                // record the movement of the list of positions and move the arm to the new position
                // check whether the arm can move different axis simultaneously
                // raise an event when the pinch gesture is made..
                newPosition = Get3DCoordsFromSensor(e);
                if (moveInProgress == false)
                {
                    robotArm.moveTo(newPosition);
                    moveInProgress = true;
                }
            }

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

        public override void ActivateActuator()
        {
            this.robotArm = new RobotArm();
            robotArm.moveToZero();
            referencePosition = new Coord3D(0, 0, 0);
        }

        public override void DeactivateActuator()
        {
            robotArm.close();
            this.robotArm = null;
        }

        private void BlinkLight()
        {
            robotArm.setLight(true);
            Task.Delay(1000);
            robotArm.setLight(false);
        }

        private Coord3D Get3DCoordsFromSensor(EventArgs e)
        {
            return new Coord3D(1, 1, 1);
        }

    }
}
