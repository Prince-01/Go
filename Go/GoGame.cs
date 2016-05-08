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
            Turn = game.Turn == Players.Black ? Players.White : Players.Black;
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

        internal bool MakeMove(Point fieldSelected)
        {
            Field field = GetFromPoint(fieldSelected);

            if (field.Player != Players.None) return false;
            field.Player = Turn;

            if (field.X > 0 && Board[field.X - 1, field.Y].Group != -1)
            {
                groups[0] = Groups[Board[field.X - 1, field.Y].Group];
            }
            else
                groups[0] = null;
            if (field.Y > 0 && Board[field.X, field.Y - 1].Group != -1)
            {
                groups[1] = Groups[Board[field.X, field.Y - 1].Group];
            }
            else
                groups[1] = null;
            if (field.X < Size - 1 && Board[field.X + 1, field.Y].Group != -1)
            {
                groups[2] = Groups[Board[field.X + 1, field.Y].Group];
            }
            else
                groups[2] = null;
            if (field.Y < Size - 1 && Board[field.X, field.Y + 1].Group != -1)
            {
                groups[3] = Groups[Board[field.X, field.Y + 1].Group];
            }
            else
                groups[3] = null;

            if (groups[0] != null)
            {
                if (groups[1] != null && groups[0].ID == groups[1].ID) groups[1] = null;
                if (groups[2] != null && groups[0].ID == groups[2].ID) groups[2] = null;
                if (groups[3] != null && groups[0].ID == groups[3].ID) groups[3] = null;
            }
            if (groups[1] != null)
            {
                if (groups[2] != null && groups[1].ID == groups[2].ID) groups[2] = null;
                if (groups[3] != null && groups[1].ID == groups[3].ID) groups[3] = null;
            }
            if (groups[2] != null && groups[3] != null && groups[2].ID == groups[3].ID) groups[3] = null;

            int ourbreaths = 0;
            if (field.X > 0 && Board[field.X - 1, field.Y].Player == Players.None)
                ourbreaths++;
            if (field.Y > 0 && Board[field.X, field.Y - 1].Player == Players.None)
                ourbreaths++;
            if (field.X < 9 - 1 && Board[field.X + 1, field.Y].Player == Players.None)
                ourbreaths++;
            if (field.Y < 9 - 1 && Board[field.X, field.Y + 1].Player == Players.None)
                ourbreaths++;

            if (groups[0] != null && groups[0].Player() == Turn)
                ourbreaths += groups[0].Breaths - 1;
            if (groups[1] != null && groups[1].Player() == Turn)
                ourbreaths += groups[1].Breaths - 1;
            if (groups[2] != null && groups[2].Player() == Turn)
                ourbreaths += groups[2].Breaths - 1;
            if (groups[3] != null && groups[3].Player() == Turn)
                ourbreaths += groups[3].Breaths - 1;

            Players enemy = Turn == Players.Black ? Players.White : Players.Black;
            int enemyweakestbreath = 10;
            if (groups[0] != null && groups[0].Player() == enemy)
                enemyweakestbreath = Math.Min(enemyweakestbreath, groups[0].Breaths - 1);
            if (groups[1] != null && groups[1].Player() == enemy)
                enemyweakestbreath = Math.Min(enemyweakestbreath, groups[1].Breaths - 1);
            if (groups[2] != null && groups[2].Player() == enemy)
                enemyweakestbreath = Math.Min(enemyweakestbreath, groups[2].Breaths - 1);
            if (groups[3] != null && groups[3].Player() == enemy)
                enemyweakestbreath = Math.Min(enemyweakestbreath, groups[3].Breaths - 1);

            field.Player = Players.None;

            if (!(ourbreaths > 0 || enemyweakestbreath <= 0) || LastMoveDeadly && LastMove != null && enemyweakestbreath == 0 && Math.Abs(field.X - LastMove.X) <= 1 && Math.Abs(field.Y - LastMove.Y) <= 1)
            {
                return false;
            }

            if (enemyweakestbreath == 0) LastMoveDeadly = true;
            else LastMoveDeadly = false;
            LastMove = field;

            FieldGroup fg;
            if ((groups[0] == null || groups[0].Player() == enemy) && (groups[1] == null || groups[1].Player() == enemy) &&
                (groups[2] == null || groups[2].Player() == enemy) && (groups[3] == null || groups[3].Player() == enemy))
            {
                fg = FindNextFreeGroup();
                fg.Fields.Add(field);
                fg.Breaths = GiveBreathsSurrounding(field);
                field.Group = fg.ID;
            }


            if (groups[0] != null && groups[0].Player() == Turn)
            {
                groups[0].Fields.Add(field);
                field.Group = groups[0].ID;
                groups[0].Breaths += GiveBreathsSurrounding(field) - 1;
            }
            else if (groups[1] != null && groups[1].Player() == Turn)
            {
                groups[1].Fields.Add(field);
                field.Group = groups[1].ID;
                groups[1].Breaths += GiveBreathsSurrounding(field) - 1;
            }
            else if (groups[2] != null && groups[2].Player() == Turn)
            {
                groups[2].Fields.Add(field);
                field.Group = groups[2].ID;
                groups[2].Breaths += GiveBreathsSurrounding(field) - 1;
            }
            else if (groups[3] != null && groups[3].Player() == Turn)
            {
                groups[3].Fields.Add(field);
                field.Group = groups[3].ID;
                groups[3].Breaths += GiveBreathsSurrounding(field) - 1;
            }

            if (groups[0] != null && groups[0].Player() == Turn)
            {
                if (groups[1] != null && groups[1].Player() == Turn)
                {
                    if (groups[0].ID != groups[1].ID)
                        groups[0].MergeWith(groups[1]);

                    if (groups[2] != null && groups[2].Player() == Turn)
                    {
                        if (groups[0].ID != groups[2].ID)
                            groups[0].MergeWith(groups[2]);

                        if (groups[3] != null && groups[3].Player() == Turn)
                        {
                            if (groups[0].ID != groups[3].ID)
                                groups[0].MergeWith(groups[3]);
                        }
                    }
                }
                else
                {
                    if (groups[2] != null && groups[2].Player() == Turn)
                    {
                        if (groups[0].ID != groups[2].ID)
                            groups[0].MergeWith(groups[2]);

                        if (groups[3] != null && groups[3].Player() == Turn)
                        {
                            if (groups[0].ID != groups[3].ID)
                                groups[0].MergeWith(groups[3]);
                        }
                    }
                    else
                    {
                        if (groups[3] != null && groups[3].Player() == Turn)
                        {
                            if (groups[0].ID != groups[3].ID)
                                groups[0].MergeWith(groups[3]);
                        }
                        else
                        {

                        }
                    }
                }
            }
            else
            {
                if (groups[1] != null && groups[1].Player() == Turn)
                {
                    if (groups[2] != null && groups[2].Player() == Turn)
                    {
                        if (groups[1].ID != groups[2].ID)
                            groups[1].MergeWith(groups[2]);

                        if (groups[3] != null && groups[3].Player() == Turn)
                        {
                            if (groups[1].ID != groups[3].ID)
                                groups[1].MergeWith(groups[3]);
                        }
                    }
                }
                else
                {
                    if (groups[2] != null && groups[2].Player() == Turn)
                    {
                        if (groups[3] != null && groups[3].Player() == Turn)
                        {
                            if (groups[2].ID != groups[3].ID)
                                groups[2].MergeWith(groups[3]);
                        }
                    }
                    else
                    {
                        if (groups[3] != null)
                        {
                        }
                        else
                        {

                        }
                    }
                }
            }

            if (groups[0] != null && !groups[0].Free() && groups[0].Player() == enemy)
                groups[0].ForceUpdateBreaths(Board);
            if (groups[1] != null && !groups[1].Free() && groups[1].Player() == enemy)
                groups[1].ForceUpdateBreaths(Board);
            if (groups[2] != null && !groups[2].Free() && groups[2].Player() == enemy)
                groups[2].ForceUpdateBreaths(Board);
            if (groups[3] != null && !groups[3].Free() && groups[3].Player() == enemy)
                groups[3].ForceUpdateBreaths(Board);

            field.Player = Turn;

            if (groups[0] != null && !groups[0].Free() && groups[0].Player() != enemy)
                groups[0].ForceUpdateBreaths(Board);
            else if (groups[0] != null && !groups[0].Free() && groups[0].Player() == enemy)
                groups[0].DecrementBreath();
            if (groups[1] != null && !groups[1].Free() && groups[1].Player() != enemy)
                groups[1].ForceUpdateBreaths(Board);
            else if (groups[1] != null && !groups[1].Free() && groups[1].Player() == enemy)
                groups[1].DecrementBreath();
            if (groups[2] != null && !groups[2].Free() && groups[2].Player() != enemy)
                groups[2].ForceUpdateBreaths(Board);
            else if (groups[2] != null && !groups[2].Free() && groups[2].Player() == enemy)
                groups[2].DecrementBreath();
            if (groups[3] != null && !groups[3].Free() && groups[3].Player() != enemy)
                groups[3].ForceUpdateBreaths(Board);
            else if (groups[3] != null && !groups[3].Free() && groups[3].Player() == enemy)
                groups[3].DecrementBreath();



            SwapTurns();
            Points[0] = 0;
            Points[1] = 0;
            foreach (var f in Board)
            {
                if (f.Player == Players.White)
                    Points[0] += 1000;
                else if (f.Player == Players.Black)
                    Points[1] += 1000;
            }
            foreach (var g in Groups)
            {
                if (g.Player() == Players.White)
                    Points[0] += g.Breaths;
                else if (g.Player() == Players.Black)
                    Points[1] += g.Breaths;
            }
            return true;
        }

        int GiveBreathsSurrounding(Field f)
        {
            int breaths = 0;

            if (f.X > 0 && Board[f.X - 1, f.Y].Player == Players.None)
            {
                /*bool nok = false;
                if (f.X > 1 && Board[f.X - 2, f.Y].Group == f.Group)
                    nok = true;
                if (f.Y > 1 && Board[f.X, f.Y - 2].Group == f.Group)
                    nok = true;
                if (f.X < 9 - 2 && Board[f.X + 2, f.Y].Group == f.Group)
                    nok = true;
                if (f.Y < 9 - 2 && Board[f.X, f.Y + 2].Group == f.Group)
                    nok = true;
                if (!nok)*/
                breaths++;
            }
            if (f.Y > 0 && Board[f.X, f.Y - 1].Player == Players.None)
            {
                /*bool nok = false;
                if (f.X > 1 && Board[f.X - 2, f.Y].Group == f.Group)
                    nok = true;
                if (f.Y > 1 && Board[f.X, f.Y - 2].Group == f.Group)
                    nok = true;
                if (f.X < 9 - 2 && Board[f.X + 2, f.Y].Group == f.Group)
                    nok = true;
                if (f.Y < 9 - 2 && Board[f.X, f.Y + 2].Group == f.Group)
                    nok = true;
                if (!nok) */
                breaths++;
            }
            if (f.X < 9 - 1 && Board[f.X + 1, f.Y].Player == Players.None)
            {
                /*bool nok = false;
                if (f.X > 1 && Board[f.X - 2, f.Y].Group == f.Group)
                    nok = true;
                if (f.Y > 1 && Board[f.X, f.Y - 2].Group == f.Group)
                    nok = true;
                if (f.X < 9 - 2 && Board[f.X + 2, f.Y].Group == f.Group)
                    nok = true;
                if (f.Y < 9 - 2 && Board[f.X, f.Y + 2].Group == f.Group)
                    nok = true;
                if (!nok) */
                breaths++;
            }
            if (f.Y < 9 - 1 && Board[f.X, f.Y + 1].Player == Players.None)
            {
                /*bool nok = false;
                if (f.X > 1 && Board[f.X - 2, f.Y].Group == f.Group)
                    nok = true;
                if (f.Y > 1 && Board[f.X, f.Y - 2].Group == f.Group)
                    nok = true;
                if (f.X < 9 - 2 && Board[f.X + 2, f.Y].Group == f.Group)
                    nok = true;
                if (f.Y < 9 - 2 && Board[f.X, f.Y + 2].Group == f.Group)
                    nok = true;
                if (!nok) */
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

        public bool IsPossibleMove(int i, int j, Players p)
        {
            Field field = Board[i, j];
            if (field.Player != Players.None) return false;
            field.Player = Turn;

            if (field.X > 0 && Board[field.X - 1, field.Y].Group != -1)
            {
                groups[0] = Groups[Board[field.X - 1, field.Y].Group];
            }
            else
                groups[0] = null;
            if (field.Y > 0 && Board[field.X, field.Y - 1].Group != -1)
            {
                groups[1] = Groups[Board[field.X, field.Y - 1].Group];
            }
            else
                groups[1] = null;
            if (field.X < Size - 1 && Board[field.X + 1, field.Y].Group != -1)
            {
                groups[2] = Groups[Board[field.X + 1, field.Y].Group];
            }
            else
                groups[2] = null;
            if (field.Y < Size - 1 && Board[field.X, field.Y + 1].Group != -1)
            {
                groups[3] = Groups[Board[field.X, field.Y + 1].Group];
            }
            else
                groups[3] = null;

            int ourbreaths = 0;
            if (field.X > 0 && Board[field.X - 1, field.Y].Player == Players.None)
                ourbreaths++;
            if (field.Y > 0 && Board[field.X, field.Y - 1].Player == Players.None)
                ourbreaths++;
            if (field.X < 9 - 1 && Board[field.X + 1, field.Y].Player == Players.None)
                ourbreaths++;
            if (field.Y < 9 - 1 && Board[field.X, field.Y + 1].Player == Players.None)
                ourbreaths++;

            if (groups[0] != null && groups[0].Player() == Turn)
                ourbreaths += groups[0].Breaths - 1;
            if (groups[1] != null && groups[1].Player() == Turn)
                ourbreaths += groups[1].Breaths - 1;
            if (groups[2] != null && groups[2].Player() == Turn)
                ourbreaths += groups[2].Breaths - 1;
            if (groups[3] != null && groups[3].Player() == Turn)
                ourbreaths += groups[3].Breaths - 1;

            Players enemy = Turn == Players.Black ? Players.White : Players.Black;
            int enemyweakestbreath = 10;
            if (groups[0] != null && groups[0].Player() == enemy)
                enemyweakestbreath = Math.Min(enemyweakestbreath, groups[0].Breaths - 1);
            if (groups[1] != null && groups[1].Player() == enemy)
                enemyweakestbreath = Math.Min(enemyweakestbreath, groups[1].Breaths - 1);
            if (groups[2] != null && groups[2].Player() == enemy)
                enemyweakestbreath = Math.Min(enemyweakestbreath, groups[2].Breaths - 1);
            if (groups[3] != null && groups[3].Player() == enemy)
                enemyweakestbreath = Math.Min(enemyweakestbreath, groups[3].Breaths - 1);

            field.Player = Players.None;

            if (!(ourbreaths > 0 || enemyweakestbreath == 0) || LastMoveDeadly && LastMove != null && enemyweakestbreath == 0 && Math.Abs(field.X - LastMove.X) <= 1 && Math.Abs(field.Y - LastMove.Y) <= 1)
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
