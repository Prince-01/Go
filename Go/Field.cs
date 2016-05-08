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
            Group = -1;
        }
        public Field(Field f)
        {
            X = f.X;
            Y = f.Y;
            Player = f.Player;
            Group = f.Group;
        }

        public GoGame.Players Player { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Group { get; set; }

        public override bool Equals(object obj)
        {
            Field f = obj as Field;
            return (1000 * X + Y) == (1000 * f.X + f.Y);
        }

        public override int GetHashCode()
        {
            return 1000 * X + Y;
        }
    }
}
