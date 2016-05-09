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
        public static void Perform(GoGame go, bool d)
        {
            GoGame.Players player = go.Turn;

            if (!d)
                AB(go, int.MinValue, int.MaxValue, 0);
            else
                DoubleAB(go, double.MinValue, double.MaxValue, 0);
            //var allmoves = GiveAllPossibleMovesFor(go, go.Turn);
            //BestMove = allmoves[(new Random()).Next(allmoves.Count)];
            if (BestMove != null)
            {
                go.MakeMove(BestMove.X, BestMove.Y);
            }
        }

        public static int AB(GoGame game, int alpha, int beta, int d)
        {
            List<Field> children = GiveAllPossibleMovesFor(game, game.Turn);

            if (d == 4 || children.Count == 0)
            {
                if (game.Turn == GoGame.Players.White)
                    return game.Points[0];
                else
                    return game.Points[1];
            }

            if (game.Turn == GoGame.Players.White)
            {
                foreach (var child in children)
                {
                    GoGame newGame = new GoGame(game);
                    newGame.MakeMove(child.X, child.Y);
                    int score = AB(newGame, alpha, beta, d + 1);
                    if (score > alpha)
                    {
                        if (d == 0) BestMove = child; alpha = score;
                    }
                    if (alpha >= beta)
                    {
                        if (d == 0) BestMove = child; return alpha;
                    }
                }
                return alpha;

            }
            else
            {
                foreach (var child in children)
                {
                    GoGame newGame = new GoGame(game);
                    newGame.MakeMove(child.X, child.Y);
                    int score = AB(newGame, alpha, beta, d + 1);
                    if (score < beta)
                    {
                        if (d == 0) BestMove = child; beta = score;
                    }
                    if (alpha >= beta)
                    {
                        if (d == 0) BestMove = child; return beta;
                    }
                }
                return beta;
            }

        }

        public static int MM(GoGame game, int d)
        {
            List<Field> children = GiveAllPossibleMovesFor(game, game.Turn);

            if (d == 4 || children.Count == 0)
            {
                if (game.Turn == GoGame.Players.White)
                    return game.Points[0];
                else
                    return game.Points[1];
            }

            if (game.Turn == GoGame.Players.White)
            {
                int score = -1;
                foreach (var child in children)
                {
                    GoGame newGame = new GoGame(game);
                    newGame.MakeMove(child.X, child.Y);
                    int s = MM(newGame, d + 1);
                    if (score < s)
                    {
                        score = s;
                        if (d == 0)
                            BestMove = child;
                    }
                }
                return score;

            }
            else
            {
                int score = int.MaxValue;
                foreach (var child in children)
                {
                    GoGame newGame = new GoGame(game);
                    newGame.MakeMove(child.X, child.Y);
                    int s = MM(newGame, d + 1);
                    if (score > s)
                    {
                        score = s;
                        if (d == 0)
                            BestMove = child;
                    }
                }
                return score;
            }

        }

        public static double DoubleAB(GoGame game, double alpha, double beta, int d)
        {
            List<Field> children = GiveAllPossibleMovesFor(game, game.Turn);

            if (d == 4 || children.Count == 0)
            {
                if (game.Turn == GoGame.Players.White)
                    return game.Points[0] / (double)Math.Max(1, game.Points[1]);
                else
                    return game.Points[1] / (double)Math.Max(1, game.Points[0]);
            }

            if (game.Turn == GoGame.Players.Black)
            {
                foreach (var child in children)
                {
                    GoGame newGame = new GoGame(game);
                    newGame.MakeMove(child.X, child.Y);
                    double score = DoubleAB(newGame, alpha, beta, d + 1);
                    if (score > alpha)
                    {
                        if (d == 0) BestMove = child; alpha = score;
                    }
                    if (alpha >= beta)
                    {
                        if (d == 0) BestMove = child; return alpha;
                    }
                }
                return alpha;

            }
            else
            {
                foreach (var child in children)
                {
                    GoGame newGame = new GoGame(game);
                    newGame.MakeMove(child.X, child.Y);
                    double score = DoubleAB(newGame, alpha, beta, d + 1);
                    if (score < beta)
                    {
                        if (d == 0) BestMove = child; beta = score;
                    }
                    if (alpha >= beta)
                    {
                        if (d == 0) BestMove = child; return beta;
                    }
                }
                return beta;
            }

        }

        public static List<Field> GiveAllPossibleMovesFor(GoGame go, GoGame.Players turn)
        {
            List<Field> moves = new List<Field>(81);

            for (int i = 0; i < go.Size; i++)
                for (int j = 0; j < go.Size; j++)
                    if (go.IsPossibleMove(i, j, turn))
                        moves.Add(go.Board[i, j]);
            return moves;
        }
    }
}
