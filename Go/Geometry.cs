using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Go.GoGame;

namespace Go
{
    public static class Geometry
    {
        public static int GetNumberOfBreaths(Field[,] plane, HashSet<Field> group)
        {
            int breaths = 0;
            foreach (var field in group)
            {
                breaths += GiveSurroundings(plane, field, GoGame.Players.None).Count;
            }
            return breaths;
        }
        public static void SetBreathsForNearGroups(Field[,] plane, HashSet<Field>[] surroundings, int[] breaths)
        {
            for(int i = 0; i < 5; i++)
            {
                breaths[i] = GetNumberOfBreaths(plane, surroundings[i]);
                if (i != 0)
                    breaths[i]--;
            }
        }
        public static void SetNearGroups(Field[,] plane, Field f, HashSet<Field>[] surroundings)
        {
            Players enemy = f.Player == Players.Black ? Players.White : Players.Black;
            surroundings[0] = GetAllCanGetTo(plane, f, f.Player);
            if (f.X > 0)
                surroundings[1] = GetAllCanGetTo(plane, plane[f.X - 1, f.Y], enemy);
            else
                surroundings[1] = new HashSet<Field>();
            if (f.X < 9 - 1)
                surroundings[2] = GetAllCanGetTo(plane, plane[f.X + 1, f.Y], enemy);
            else
                surroundings[2] = new HashSet<Field>();
            if (f.Y > 0)
                surroundings[3] = GetAllCanGetTo(plane, plane[f.X, f.Y - 1], enemy);
            else
                surroundings[3] = new HashSet<Field>();
            if (f.Y < 9 - 1)
                surroundings[4] = GetAllCanGetTo(plane, plane[f.X, f.Y + 1], enemy);
            else
                surroundings[4] = new HashSet<Field>();
        }
        public static HashSet<Field> GetAllCanGetTo(Field[,] plane, Field f, GoGame.Players turn)
        {
            HashSet<Field> beenTo = new HashSet<Field>() { f };
            var surroundings = GiveSurroundings(plane, f, turn);
            List<Field> toGo = new List<Field>();
            toGo.AddRange(surroundings);

            while(toGo.Count > 0)
            {
                surroundings = GiveSurroundings(plane, toGo[0], turn);
                toGo.AddRange(surroundings.FindAll(fld => !beenTo.Contains(fld)));
                beenTo.Add(toGo[0]);
                toGo.RemoveAt(0);
            }

            return beenTo;
        }

        private static List<Field> GiveSurroundings(Field[,] plane, Field f, GoGame.Players turn)
        {
            List<Field> result;
            int x = f.X; int y = f.Y;

            if (x == 9 - 1)
                if (y == 9 - 1)
                    result = new List<Field> { plane[x - 1, y], plane[x, y - 1]};
                    else if(y == 0)
                    result = new List<Field> { plane[x - 1, y], plane[x, y + 1] };
                else
                    result = new List<Field> { plane[x - 1, y], plane[x, y - 1], plane[x, y + 1] };
            else if(x == 0)
                if (y == 9 - 1)
                    result = new List<Field> { plane[x + 1, y], plane[x, y - 1] };
                else if (y == 0)
                    result = new List<Field> { plane[x + 1, y], plane[x, y + 1] };
                else
                    result = new List<Field> { plane[x + 1, y], plane[x, y - 1], plane[x, y + 1] };
            else
                if (y == 9 - 1)
                result = new List<Field> { plane[x - 1, y], plane[x + 1, y], plane[x, y - 1] };
            else if (y == 0)
                result = new List<Field> { plane[x - 1, y], plane[x + 1, y], plane[x, y + 1] };
            else
                result = new List<Field> { plane[x - 1, y], plane[x + 1, y], plane[x, y - 1], plane[x, y + 1] };

            result.RemoveAll(fld => fld.Player != turn);

            return result;
        }
    }
}
