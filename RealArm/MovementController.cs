using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealArm
{
    public class MovementController : IMovementController
    {

        public IArm arm { get; set; }
        public ISensor sensor { get; set; }
          

        public IArmConfiguration ArmConfiguration { get; set; }

        public MovementController(IArm arm, ISensor sensor)
        {
            this.arm = arm;
            this.sensor = sensor;
            // register event handlers
            arm.MoveComplete += OnMoveComplete;
            arm.StopReached += OnStopReached;
        
        }

        protected void OnMoveComplete(object sender, MoveEventArgs e) { }

        public void Listen()
        {
            
        }

        public void UnListen()
        {
            throw new NotImplementedException();
        }
    }
}
