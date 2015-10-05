using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOwi535
{
    public class Coord3D
    {
        public Coord3D()
        { X = 0; Y = 0; Z = 0; }

        public Coord3D(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public override String ToString()
        { 
            return string.Format("({0}, {1}, {2}", this.X, this.Y, this.Z); 
        }

    }  // end of Coord3D class
}
