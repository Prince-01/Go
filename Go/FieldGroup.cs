using System;
using System.Collections.Generic;

namespace Go
{
    public class FieldGroup
    {
        public int ID { get; set; }
        public List<Field> Fields { get; set; }
        public int Breaths { get; set; }
        public bool AtLeastTwoBreaths { get; set; }

        public FieldGroup(int ID)
        {
            this.ID = ID;
            Breaths = 0;
            Fields = new List<Field>();
        }
        public FieldGroup(FieldGroup fg)
        {
            Breaths = fg.Breaths;
            ID = fg.ID;
            Fields = new List<Field>();
            AtLeastTwoBreaths = fg.AtLeastTwoBreaths;
        }
        public bool Free()
        {
            return Fields.Count == 0;
        }
        public GoGame.Players Player()
        {
            return !Free() ? Fields[0].Player : GoGame.Players.None;
        }
        public bool CheckIfHasAtLeastTwoBreaths(Field[,] board, Field f)
        {
            int numberOfBreaths = f.Player == GoGame.Players.None ? 1 : 0;

            foreach (var field in Fields)
            {
                if (!(field.X - 1 == f.X && field.Y == f.Y) && field.X > 0 && board[field.X - 1, field.Y].Player == GoGame.Players.None)
                    return (AtLeastTwoBreaths = numberOfBreaths == 1 ? true : false);
                if (!(field.X == f.X && field.Y - 1 == f.Y) && field.Y > 0 && board[field.X, field.Y - 1].Player == GoGame.Players.None)
                    return (AtLeastTwoBreaths = numberOfBreaths == 1 ? true : false);
                if (!(field.X + 1 == f.X && field.Y == f.Y) && field.X < 9 - 1 && board[field.X + 1, field.Y].Player == GoGame.Players.None)
                    return (AtLeastTwoBreaths = numberOfBreaths == 1 ? true : false);
                if (!(field.X == f.X && field.Y + 1 == f.Y) && field.Y < 9 - 1 && board[field.X, field.Y + 1].Player == GoGame.Players.None)
                    return (AtLeastTwoBreaths = numberOfBreaths == 1 ? true : false);
            }

            if (numberOfBreaths == 0) {
                foreach (var field in Fields)
                {
                    field.Group = -1;
                    field.Player = GoGame.Players.None;
                }
                Fields.Clear();
            }
            return AtLeastTwoBreaths = false;
        }
        public void MergeWith(FieldGroup group)
        {
            foreach (var field in group.Fields)
            {
                field.Group = ID;
            }
            Fields.AddRange(group.Fields);
            group.Fields.Clear();
        }

        internal void DecrementBreath()
        {
            Breaths--;
            if (Breaths == 0)
            {
                foreach (var field in Fields)
                {
                    field.Group = -1;
                    field.Player = GoGame.Players.None;
                }
                Fields.Clear();
            }
        }
    }
}