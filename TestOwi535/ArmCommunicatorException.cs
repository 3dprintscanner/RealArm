using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOwi535
{
    public class ArmCommunicatorException : Exception
    {
        public ArmCommunicatorException(string message)
            : base(message)
        { }
    }
}
