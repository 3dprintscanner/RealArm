using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealArm
{
    class Arm : IArm
    {
        public IArmConfiguration ArmConfiguration
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public decimal[] Positions
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool CanMoveTo(decimal[] position)
        {
            throw new NotImplementedException();
        }

        public void Move(decimal[] movement)
        {
            throw new NotImplementedException();
        }

        public void Grab()
        {
            throw new NotImplementedException();
        }

        public void Release()
        {
            throw new NotImplementedException();
        }

        public event EventHandler MoveComplete;

        public event EventHandler StopReached;
    }
}
