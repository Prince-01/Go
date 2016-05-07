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

            Geometry.SetNearGroups(Board, field, Surroundings);
            Geometry.SetBreathsForNearGroups(Board, Surroundings, Breaths);

            if (!CheckIfLegal(field))
                return false;
            PushField(field);
            SwapTurns();
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
            
        }

        private bool CheckIfLegal(Field field)
        {
            if (field.Player != Players.None)
                return false;
            field.Player = Turn;
            if (CheckIfSuicidalMove(field))
            {
                field.Player = Players.None;
                return false;
            }
            return true;
        }

        private bool CheckIfSuicidalMove(Field field)
        {
            return !(Breaths[0] != 0 || Breaths[1] == 0 || Breaths[2] == 0 || Breaths[3] == 0 || Breaths[4] == 0);
        }
    }
}
