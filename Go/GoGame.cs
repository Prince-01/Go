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
        public Players Turn { get; set; }
        public int OpenedLeft { get; set; }
        public GoGame(int Size)
        {
            this.Size = Size;
            Board = new Field[Size, Size];
            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    Board[i, j] = new Field(i, j);
            OpenedLeft = Size * Size;
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
            if (!CheckIfLegal(field))
                return false;
            PushField(field);
            SwapTurns();
            OpenedLeft--;
            return true;
        }

        private Field GetFromPoint(Point fieldSelected)
        {
            return Board[fieldSelected.X - 1, fieldSelected.Y - 1];
        }

        private void PushField(Field field)
        {
            AffectBoard(field);
            field.Player = Turn;
        }
        
        private void AffectBoard(Field field)
        {
            
        }

        private bool CheckIfLegal(Field field)
        {
            if (field.Player != Players.None)
                return false;
            if (CheckIfSuicidalMove(field))
                return false;
            return true;
        }

        private bool CheckIfSuicidalMove(Field field)
        {
            if (field.Opened)
                return false;
            for (int X = field.X + 1; X < Size; X++)
            {
                if (Board[X, field.Y].Player == Turn)
                    return false;
            }
            for (int X = Size - 1; X >= 0; X--)
            {
                if (Board[X, field.Y].Player == Turn)
                    return false;
            }
            return true;
        }
    }
}
