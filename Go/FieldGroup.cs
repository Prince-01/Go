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
        public bool NeedsRefresh { get; set; }

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
            NeedsRefresh = fg.NeedsRefresh;
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
        public void ForceUpdateBreaths(Field[,] board)
        {
            uniqueFields.Clear();
            foreach (var field in Fields)
            {
                if (field.X > 0 && board[field.X - 1, field.Y].Player == GoGame.Players.None)
                    uniqueFields.Add(board[field.X - 1, field.Y]);
                if (field.Y > 0 && board[field.X, field.Y - 1].Player == GoGame.Players.None)
                    uniqueFields.Add(board[field.X, field.Y - 1]);
                if (field.X < 9 - 1 && board[field.X + 1, field.Y].Player == GoGame.Players.None)
                    uniqueFields.Add(board[field.X + 1, field.Y]);
                if (field.Y < 9 - 1 && board[field.X, field.Y + 1].Player == GoGame.Players.None)
                    uniqueFields.Add(board[field.X, field.Y + 1]);
            }
            Breaths = uniqueFields.Count;
        }
        Field[] fields = new Field[4];
        HashSet<Field> uniqueFields = new HashSet<Field>();
        public bool CheckIfHasAtLeastTwoBreaths(Field[,] board)
        {
            //if (!NeedsRefresh) return AtLeastTwoBreaths;
            //else NeedsRefresh = false;

            uniqueFields.Clear();

            foreach (var field in Fields)
            {
                int n = 0;
                if (field.X > 0 && board[field.X - 1, field.Y].Player == GoGame.Players.None)
                    fields[n++] = board[field.X - 1, field.Y];
                if (field.Y > 0 && board[field.X, field.Y - 1].Player == GoGame.Players.None)
                    fields[n++] = board[field.X, field.Y - 1];
                if (field.X < 9 - 1 && board[field.X + 1, field.Y].Player == GoGame.Players.None)
                    fields[n++] = board[field.X + 1, field.Y];
                if (field.Y < 9 - 1 && board[field.X, field.Y + 1].Player == GoGame.Players.None)
                    fields[n++] = board[field.X, field.Y + 1];
                if (n >= 2 || uniqueFields.Count >= 2)
                    return AtLeastTwoBreaths = true;
                else
                    for (int i = 0; i < n; i++)
                        uniqueFields.Add(fields[i]);
            }
            if (uniqueFields.Count == 0)
            {
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