using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Go
{
    class AlphaBeta
    {
        static Field BestMove;
        public static void Perform(GoGame go)
        {
            GoGame.Players player = go.Turn;

            AB(go, int.MinValue, int.MaxValue, 0);
            if (BestMove != null)
            {
                go.MakeMove(new Point(BestMove.X + 1, BestMove.Y + 1));
            }
        }

        public static int AB(GoGame game, int alpha, int beta, int d)
        {
            List<Field> children = GiveAllPossibleMovesFor(game, game.Turn);

            if (d == 2 || children.Count == 0)
            {
                int s = 0;
                foreach (var f in game.Board)
                {
                    if (f.Player == game.Turn)
                        s++;
                }
                return s;
            }

            if (game.Turn == GoGame.Players.White)
            {
                foreach (var child in children)
                {
                    GoGame newGame = new GoGame(game);
                    newGame.MakeMove(new Point(child.X + 1, child.Y + 1));
                    int score = AB(newGame, alpha, beta, d + 1);
                    if (score > alpha)
                    {
                        BestMove = child; alpha = score;
                    }
                    if (alpha >= beta)
                    {
                        BestMove = child; return alpha;
                    }
                }
                return alpha;

            }
            else
            {
                foreach (var child in children)
                {
                    GoGame newGame = new GoGame(game);
                    newGame.MakeMove(new Point(child.X + 1, child.Y + 1));
                    int score = AB(newGame, alpha, beta, d + 1);
                    if (score < beta)
                    {
                        BestMove = child; beta = score;
                    }
                    if (alpha >= beta)
                    {
                        BestMove = child; return beta;
                    }
                }
                return beta;
            }

        }

        public static List<Field> GiveAllPossibleMovesFor(GoGame go, GoGame.Players turn)
        {
            List<Field> moves = new List<Field>();
            for (int i = 0; i < go.Size; i++)
                for (int j = 0; j < go.Size; j++)
                    if (go.IsPossibleMove(i, j, turn))
                        moves.Add(go.Board[i, j]);
            return moves;
        }
    }
}
