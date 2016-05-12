using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Go
{
    public class GoGame
    {
        public enum Players
        {
            Black,
            White,
            None,
        }
        public int Size { get; set; }
        public Field[,] Board { get; set; }
        LinkedList<Field> History;
        FieldGroup[] groups;
        int[] Breaths;
        public List<FieldGroup> Groups { get; set; }
        public int[] Points;
        public int[] Bonuses;
        public Players Turn { get; set; }
        public Field LastMove { get; private set; }
        public bool LastMoveDeadly { get; private set; }
        public Field MoveBeforeLast { get; private set; }

        public GoGame(int Size)
        {
            this.Size = Size;
            Board = new Field[Size, Size];
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    Board[i, j] = new Field(i, j);
            Breaths = new int[5];
            Points = new int[2] { 0, 0 };
            Bonuses = new int[2] { 0, 0 };
            groups = new FieldGroup[4];
            Groups = new List<FieldGroup>();
        }
        public GoGame(GoGame game)
        {
            Size = game.Size;
            Board = new Field[Size, Size];
            Turn = game.Turn;
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    Board[i, j] = new Field(game.Board[i, j]);
            Breaths = new int[5];
            Points = new int[2] { 0, 0 };
            Bonuses = new int[2] { 0, 0 };
            groups = new FieldGroup[4];
            Groups = new List<FieldGroup>();
            foreach (var group in game.Groups)
            {
                FieldGroup fg = new FieldGroup(group);
                foreach (var f in group.Fields)
                {
                    fg.Fields.Add(Board[f.X, f.Y]);
                }
                Groups.Add(fg);
            }
        }

        public void GoBack()
        {

        }

        public void SwapTurns()
        {
            if (Turn == Players.White)
                Turn = Players.Black;
            else
                Turn = Players.White;
        }

        internal bool MakeMove(int x, int y)
        {
            Field field = Board[x, y];

            if (field.Player != Players.None) return false;

            int numberOfGroups = 0;
            int ourBreaths = 0;
            int ourGroups = 0;
            bool atLeastOneGroupHasTwoBreaths = false;
            bool killingMove = false;

            if (field.X > 0 && Board[field.X - 1, field.Y].Group != -1)
                groups[numberOfGroups++] = Groups[Board[field.X - 1, field.Y].Group];
            if (field.Y > 0 && Board[field.X, field.Y - 1].Group != -1)
                groups[numberOfGroups++] = Groups[Board[field.X, field.Y - 1].Group];
            if (field.X < Size - 1 && Board[field.X + 1, field.Y].Group != -1)
                groups[numberOfGroups++] = Groups[Board[field.X + 1, field.Y].Group];
            if (field.Y < Size - 1 && Board[field.X, field.Y + 1].Group != -1)
                groups[numberOfGroups++] = Groups[Board[field.X, field.Y + 1].Group];

            ourBreaths = 4 - numberOfGroups;
            if ((field.X == 0 && field.Y == 0) || (field.X == 0 && field.Y == 9 - 1) || (field.X == 9 - 1 && field.Y == 0) || (field.X == 9 - 1 && field.Y == 9 - 1))
                ourBreaths -= 2;
            else if (field.X == 0 || field.Y == 0 || field.X == 9 - 1 || field.Y == 9 - 1)
                --ourBreaths;

            if (numberOfGroups <= 1) ;
            else if (numberOfGroups == 2)
            {
                if (groups[0].ID == groups[1].ID)
                    numberOfGroups--;
            }
            else if (numberOfGroups == 3)
            {
                if (groups[1].ID == groups[2].ID || groups[0].ID == groups[2].ID)
                    numberOfGroups--;
                if (groups[0].ID == groups[1].ID)
                    groups[1] = groups[--numberOfGroups];
            }
            else
            {
                if (groups[2].ID == groups[3].ID || groups[1].ID == groups[3].ID || groups[0].ID == groups[3].ID)
                    numberOfGroups--;
                if (groups[1].ID == groups[2].ID || groups[0].ID == groups[2].ID)
                    groups[2] = groups[--numberOfGroups];
                if (groups[0].ID == groups[1].ID)
                {
                    for (int i = 1; i < numberOfGroups; i++)
                        groups[i - 1] = groups[i];
                    numberOfGroups--;
                }
            }

            Players enemy = Turn == Players.Black ? Players.White : Players.Black;
            bool twoBreaths = false;
            for (int i = 0; i < numberOfGroups; i++)
            {
                twoBreaths = groups[i].CheckIfHasAtLeastTwoBreaths(Board, field);

                if (groups[i].Player() == Turn)
                {
                    ourGroups++;
                    atLeastOneGroupHasTwoBreaths = atLeastOneGroupHasTwoBreaths || twoBreaths;
                }
                else if (!killingMove && !twoBreaths)
                    killingMove = true;
            }

            if (!(ourBreaths > 0 || atLeastOneGroupHasTwoBreaths || killingMove) || LastMoveDeadly && LastMove != null && killingMove && (field.X - 1 == LastMove.X && field.Y == LastMove.Y || field.X + 1 == LastMove.X && field.Y == LastMove.Y || field.Y - 1 == LastMove.Y && field.X == LastMove.X || field.Y + 1 == LastMove.Y && field.X == LastMove.X))
            {
                return false;
            }

            if (killingMove) LastMoveDeadly = true;
            else LastMoveDeadly = false;
            MoveBeforeLast = LastMove;
            LastMove = field;

            FieldGroup fg;

            if (ourGroups == 0)
            {
                fg = FindNextFreeGroup();
                fg.Fields.Add(field);
                field.Group = fg.ID;
            }
            else
                for (int i = 0; i < numberOfGroups; i++)
                {
                    if (groups[i].Player() == Turn)
                    {
                        groups[i].Fields.Add(field);
                        field.Group = groups[i].ID;
                        break;
                    }
                }

            for (int i = 0; i < numberOfGroups; i++)
            {
                if (groups[i].Player() == Turn)
                    for (int j = i + 1; j < numberOfGroups; j++)
                        if (groups[j].Player() == Turn)
                        {
                            groups[i].MergeWith(groups[j]);
                            for (int k = j + 1; k < numberOfGroups; k++)
                                groups[k - 1] = groups[k];
                            numberOfGroups--;
                            j--;
                        }
            }

            field.Player = Turn;

            for (int i = 0; i < numberOfGroups; i++)
                if (groups[i].Player() == enemy)
                    groups[i].CheckIfHasAtLeastTwoBreaths(Board, field);

            SwapTurns();

            return true;
        }

        internal void ChangeInto(GoGame goGame)
        {
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Players player = Board[i, j].Player;
                    Players newPlayer = goGame.Board[i, j].Player;
                    int gr = Board[i, j].Group;
                    int newGr = goGame.Board[i, j].Group;

                    if (gr != newGr)
                    {
                        Board[i, j].Player = newPlayer;
                        Board[i, j].Group = newGr;

                        if (gr != -1)
                            Groups[gr].Fields.Clear();
                        while (Groups.Count <= newGr)
                            Groups.Add(new FieldGroup(Groups.Count));
                        if (newGr != -1)
                            Groups[newGr].Fields.Add(Board[i, j]);
                    }
                }
            }
            Turn = goGame.Turn;
        }

        public void CountPoints()
        {
            Points[0] = 0;
            Points[1] = 0;
            Bonuses[0] = 0;
            Bonuses[1] = 0;
            foreach (var f in Board)
            {
                int numberofgroups = 0;
                if (f.Player == Players.White)
                {
                    int nones = 0;
                    int blacks = 0;
                    if (f.X > 0)
                    {
                        if (Board[f.X - 1, f.Y].Player == Players.Black)
                        {
                            groups[numberofgroups++] = Groups[Board[f.X - 1, f.Y].Group];
                            blacks++;
                        }
                        if (Board[f.X - 1, f.Y].Player == Players.None)
                            nones++;
                    }
                    if (f.Y > 0)
                    {
                        if (Board[f.X, f.Y - 1].Player == Players.Black)
                        {
                            blacks++;
                            bool ok = true;
                            FieldGroup fg = Groups[Board[f.X, f.Y - 1].Group];
                            for (int i = 0; i < numberofgroups; i++)
                                if (fg.ID == groups[i].ID)
                                    ok = false;
                            if (ok)
                                groups[numberofgroups++] = fg;
                        }
                        if (Board[f.X, f.Y - 1].Player == Players.None)
                            nones++;
                    }
                    if (f.X < 9 - 1)
                    {
                        if (Board[f.X + 1, f.Y].Player == Players.Black)
                        {
                            blacks++;
                            bool ok = true;
                            FieldGroup fg = Groups[Board[f.X + 1, f.Y].Group];
                            for (int i = 0; i < numberofgroups; i++)
                                if (fg.ID == groups[i].ID)
                                    ok = false;
                            if (ok)
                                groups[numberofgroups++] = fg;
                        }
                        if (Board[f.X + 1, f.Y].Player == Players.None)
                            nones++;
                    }
                    if (f.Y < 9 - 1)
                    {
                        if (Board[f.X, f.Y + 1].Player == Players.Black)
                        {
                            blacks++;
                            bool ok = true;
                            FieldGroup fg = Groups[Board[f.X, f.Y + 1].Group];
                            for (int i = 0; i < numberofgroups; i++)
                                if (fg.ID == groups[i].ID)
                                    ok = false;
                            if (ok)
                                groups[numberofgroups++] = fg;
                        }
                        if (Board[f.X, f.Y + 1].Player == Players.None)
                            nones++;
                    }
                    int figs = Groups[f.Group].Fields.Count;
                    for (int i = 0; i < numberofgroups; i++)
                        Bonuses[0] += (groups[i].Fields.Count - figs / 2);
                    Bonuses[0] += 6 - Math.Abs(2 - (blacks + nones)) * 3;
                    Bonuses[0] += 3 - Math.Abs(1 - blacks);
                    //if (blacks + nones == 0)
                    //  Bonuses[0] -= 5;
                    Bonuses[0] += nones;
                    Bonuses[0] -= (Math.Abs(f.X - 4) + Math.Abs(f.Y - 4));
                    Points[0] += 150;
                }
                else if (f.Player == Players.Black)
                {
                    int nones = 0;
                    int blacks = 0;
                    if (f.X > 0)
                    {
                        if (Board[f.X - 1, f.Y].Player == Players.White)
                        {
                            blacks++;
                            groups[numberofgroups++] = Groups[Board[f.X - 1, f.Y].Group];
                        }
                        if (Board[f.X - 1, f.Y].Player == Players.None)
                            nones++;
                    }
                    if (f.Y > 0)
                    {
                        if (Board[f.X, f.Y - 1].Player == Players.White)
                        {
                            blacks++;
                            bool ok = true;
                            FieldGroup fg = Groups[Board[f.X, f.Y - 1].Group];
                            for (int i = 0; i < numberofgroups; i++)
                                if (fg.ID == groups[i].ID)
                                    ok = false;
                            if (ok)
                                groups[numberofgroups++] = fg;
                        }
                        if (Board[f.X, f.Y - 1].Player == Players.None)
                            nones++;
                    }
                    if (f.X < 9 - 1)
                    {
                        if (Board[f.X + 1, f.Y].Player == Players.White)
                        {
                            blacks++;
                            bool ok = true;
                            FieldGroup fg = Groups[Board[f.X + 1, f.Y].Group];
                            for (int i = 0; i < numberofgroups; i++)
                                if (fg.ID == groups[i].ID)
                                    ok = false;
                            if (ok)
                                groups[numberofgroups++] = fg;
                        }
                        if (Board[f.X + 1, f.Y].Player == Players.None)
                            nones++;
                    }
                    if (f.Y < 9 - 1)
                    {
                        if (Board[f.X, f.Y + 1].Player == Players.White)
                        {
                            blacks++;
                            bool ok = true;
                            FieldGroup fg = Groups[Board[f.X, f.Y + 1].Group];
                            for (int i = 0; i < numberofgroups; i++)
                                if (fg.ID == groups[i].ID)
                                    ok = false;
                            if (ok)
                                groups[numberofgroups++] = fg;
                        }
                        if (Board[f.X, f.Y + 1].Player == Players.None)
                            nones++;
                    }
                    int figs = Groups[f.Group].Fields.Count;
                    for (int i = 0; i < numberofgroups; i++)
                        Bonuses[1] += (groups[i].Fields.Count - figs / 2);
                    Bonuses[1] += 6 - Math.Abs(2 - (blacks + nones)) * 3;
                    Bonuses[1] += nones;
                    Bonuses[1] += 3 - Math.Abs(1 - blacks);
                    //if (blacks + nones == 0)
                    //  Bonuses[1] -= 5;
                    Bonuses[1] += (Math.Abs(f.X - 4) + Math.Abs(f.Y - 4));
                    Points[1] += 150;
                }
            }
        }

        FieldGroup FindNextFreeGroup()
        {
            foreach (var group in Groups)
            {
                if (group.Free())
                    return group;
            }
            Groups.Add(new FieldGroup(Groups.Count));
            return Groups.Last();
        }

        public bool IsPossibleMove(int x, int y, Players p)
        {
            Field field = Board[x, y];
            if (field.Player != Players.None) return false;

            int numberOfGroups = 0;
            int ourBreaths = 0;
            int ourGroups = 0;
            bool atLeastOneGroupHasTwoBreaths = false;
            bool killingMove = false;

            if (field.X > 0 && Board[field.X - 1, field.Y].Group != -1)
                groups[numberOfGroups++] = Groups[Board[field.X - 1, field.Y].Group];
            if (field.Y > 0 && Board[field.X, field.Y - 1].Group != -1)
                groups[numberOfGroups++] = Groups[Board[field.X, field.Y - 1].Group];
            if (field.X < Size - 1 && Board[field.X + 1, field.Y].Group != -1)
                groups[numberOfGroups++] = Groups[Board[field.X + 1, field.Y].Group];
            if (field.Y < Size - 1 && Board[field.X, field.Y + 1].Group != -1)
                groups[numberOfGroups++] = Groups[Board[field.X, field.Y + 1].Group];

            ourBreaths = 4 - numberOfGroups;
            if ((field.X == 0 && field.Y == 0) || (field.X == 0 && field.Y == 9 - 1) || (field.X == 9 - 1 && field.Y == 0) || (field.X == 9 - 1 && field.Y == 9 - 1))
                ourBreaths -= 2;
            else if (field.X == 0 || field.Y == 0 || field.X == 9 - 1 || field.Y == 9 - 1)
                --ourBreaths;

            if (ourBreaths > 0) return true;

            if (numberOfGroups <= 1) ;
            else if (numberOfGroups == 2)
            {
                if (groups[0].ID == groups[1].ID)
                    numberOfGroups--;
            }
            else if (numberOfGroups == 3)
            {
                if (groups[1].ID == groups[2].ID || groups[0].ID == groups[2].ID)
                    numberOfGroups--;
                if (groups[0].ID == groups[1].ID)
                    groups[1] = groups[--numberOfGroups];
            }
            else
            {
                if (groups[2].ID == groups[3].ID || groups[1].ID == groups[3].ID || groups[0].ID == groups[3].ID)
                    numberOfGroups--;
                if (groups[1].ID == groups[2].ID || groups[0].ID == groups[2].ID)
                    groups[2] = groups[--numberOfGroups];
                if (groups[0].ID == groups[1].ID)
                {
                    for (int i = 1; i < numberOfGroups; i++)
                        groups[i - 1] = groups[i];
                    numberOfGroups--;
                }
            }

            Players enemy = Turn == Players.Black ? Players.White : Players.Black;
            bool twoBreaths = false;
            for (int i = 0; i < numberOfGroups; i++)
            {
                twoBreaths = groups[i].CheckIfHasAtLeastTwoBreaths(Board, field);

                if (groups[i].Player() == Turn)
                {
                    ourGroups++;
                    atLeastOneGroupHasTwoBreaths = atLeastOneGroupHasTwoBreaths || twoBreaths;
                }
                else if (!killingMove && !twoBreaths)
                    killingMove = true;
            }

            if (!(atLeastOneGroupHasTwoBreaths || killingMove) || LastMoveDeadly && MoveBeforeLast != null && killingMove && LastMoveDeadly && LastMove != null && killingMove && (field.X - 1 == LastMove.X && field.Y == LastMove.Y || field.X + 1 == LastMove.X && field.Y == LastMove.Y || field.Y - 1 == LastMove.Y && field.X == LastMove.X || field.Y + 1 == LastMove.Y && field.X == LastMove.X))
            {
                return false;
            }
            return true;
        }
    }
}
