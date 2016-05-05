using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Go
{
    public class Field
    {
        public Field(int i, int j)
        {
            X = i;
            Y = j;
            Player = GoGame.Players.None;
            Opened = true;
        }

        public GoGame.Players Player { get; set; }
        public bool Opened { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }
}
