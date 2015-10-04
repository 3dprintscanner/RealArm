using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RealArm
{
    class MovementControllerException : Exception
    {
           public MovementControllerException(string message)
               :base(message)
           {}
    }
}
