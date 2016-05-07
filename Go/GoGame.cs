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
        int[] Breaths;
        public int[] Points;
        public Players Turn { get; set; }
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
        }
        public GoGame(GoGame game)
        {
            Size = game.Size;
            Board = new Field[Size, Size];
            Turn = game.Turn == Players.Black ? Players.White : Players.Black;
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    Board[i, j] = new Field(game.Board[i,j]);
            Surroundings = new HashSet<Field>[5];
            Breaths = new int[5];
            Points = new int[2] { 0, 0 };
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

            Geometry.SetNearGroups(Board, field, Surroundings);
            Geometry.SetBreathsForNearGroups(Board, Surroundings, Breaths);

            if (!CheckIfLegal())
            {
                field.Player = Players.None;
                return false;
            }
            PushField(field);
            SwapTurns();
            Points[0] = 0;
            Points[1] = 0;
            foreach (var f in Board)
            {
                if (f.Player == Players.White)
                    Points[0]++;
                else if (f.Player == Players.Black)
                    Points[1]++;
            }
            return true;
        }

        public bool IsPossibleMove(int i, int j, Players p)
        {
            Field field = Board[i, j];
            if (field.Player != Players.None) return false;
            field.Player = Turn;

            Geometry.SetNearGroups(Board, field, Surroundings);
            Geometry.SetBreathsForNearGroups(Board, Surroundings, Breaths);
            field.Player = Players.None;

            return CheckIfLegal();
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
