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
        HashSet<Field>[] Surroundings;
        FieldGroup[] groups;
        int[] Breaths;
        public List<FieldGroup> Groups { get; set; }
        public int[] Points;
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
            Surroundings = new HashSet<Field>[5];
            Breaths = new int[5];
            Points = new int[2] { 0, 0 };
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
            Surroundings = new HashSet<Field>[5];
            Breaths = new int[5];
            Points = new int[2] { 0, 0 };
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

            if (!(ourBreaths > 0 || atLeastOneGroupHasTwoBreaths || killingMove) || LastMoveDeadly && MoveBeforeLast != null && killingMove && field.Equals(MoveBeforeLast))
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
                fg.AtLeastTwoBreaths = GiveBreathsSurrounding(field) >= 2;
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
            Points[0] = 0;
            Points[1] = 0;
            foreach (var f in Board)
            {
                if (f.Player == Players.White)
                    Points[0] += 10;
                else if (f.Player == Players.Black)
                    Points[1] += 10;
            }
            return true;
        }

        int GiveBreathsSurrounding(Field f)
        {
            int breaths = 0;

            if (f.X > 0 && Board[f.X - 1, f.Y].Player == Players.None)
            {
                breaths++;
            }
            if (f.Y > 0 && Board[f.X, f.Y - 1].Player == Players.None)
            {
                breaths++;
            }
            if (f.X < 9 - 1 && Board[f.X + 1, f.Y].Player == Players.None)
            { 
                breaths++;
            }
            if (f.Y < 9 - 1 && Board[f.X, f.Y + 1].Player == Players.None)
            {
                breaths++;
            }
            return breaths;
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

            if (!(atLeastOneGroupHasTwoBreaths || killingMove) || LastMoveDeadly && MoveBeforeLast != null && killingMove && field.Equals(MoveBeforeLast))
            {
                return false;
            }
            return true;
        }

        private Field GetFromPoint(Point fieldSelected)
        {
            return Board[fieldSelected.X - 1, fieldSelected.Y - 1];
        }

        private void PushField(Field field)
        {
            AffectBoard(field);
        }

        private void AffectBoard(Field field)
        {
            if (Breaths[1] == 0)
                SetFieldsToNone(Surroundings[1]);
            if (Breaths[2] == 0)
                SetFieldsToNone(Surroundings[2]);
            if (Breaths[3] == 0)
                SetFieldsToNone(Surroundings[3]);
            if (Breaths[4] == 0)
                SetFieldsToNone(Surroundings[4]);
        }

        private void SetFieldsToNone(HashSet<Field> toRemove)
        {
            foreach (var field in toRemove)
            {
                field.Player = Players.None;
            }
        }

        private bool CheckIfLegal()
        {
            if (CheckIfSuicidalMove())
            {
                return false;
            }
            return true;
        }

        private bool CheckIfSuicidalMove()
        {
            return !(Breaths[0] != 0 || Breaths[1] == 0 || Breaths[2] == 0 || Breaths[3] == 0 || Breaths[4] == 0);
        }
    }
}
