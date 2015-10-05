using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOwi535
{
    public class JointException : Exception
    {
        public JointException(string message)
            : base(message)
        {
        }
    }
}
