using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkers
{
    public class Ai
    {
        public Move GetBestMove(Board board, State state)
        {
            List<Piece> pieces = board.GetPlayersPieces(state.CurrentPlayer);
            HashSet<Move> legalJumps = board.GetLegalJumps(pieces);
            HashSet<Move> legalMoves = board.GetLegalMoves(pieces);

            foreach (var move in legalJumps)
            {
                move.IsJump = true;
            }

            legalMoves.UnionWith(legalJumps);

            var bestMoves = new List<Move>();

            int depth = 3;
            double alpha = Double.MinValue;

            Program.Square currentPlayer = state.CurrentPlayer;
            Board boardCopy;
            State newState;
            var rand = new Random();

            foreach (var move in legalMoves)
            {
                boardCopy = board.DeepCopy();

                if (move.IsJump)
                {
                    boardCopy.DoJump(move);
                }
                else
                {
                    boardCopy.MovePiece(move);
                }

                newState = new State(boardCopy.GetState(), Program.ChangeTurn(currentPlayer));
                double newScore = -Negamax(boardCopy, newState, depth - 1, Double.MinValue, -alpha);

                //Console.WriteLine("From {0} To {1} = {2}", move.CoordinateFrom, move.CoordinateTo, newScore);

                int index = bestMoves.FindIndex(other => other.IsJump);

                if (newScore > alpha)
                {
                    alpha = newScore;

                    // no jumps were found so far, this move becomes best move
                    if (index == -1)
                    {
                        bestMoves.Clear();
                        bestMoves.Add(move);
                    }
                    // jumps were already in the list of bestMoves
                    else
                    {
                        // if this move is also a jump, then it becomes new best move
                        if (move.IsJump)
                        {
                            bestMoves.Clear();
                            bestMoves.Add(move);
                        }
                    }
                }
                // this move is equally as good as the best moves found so far
                else if (newScore == alpha)
                {
                    // none of the best moves found so far was a jump, so this move can be added to a list
                    if (index == -1)
                        bestMoves.Add(move);
                    // jumps were already in the list of bestMoves
                    else
                    {
                        // if this move is also a jump, then it can be added to a list
                        if (move.IsJump)
                        {
                            bestMoves.Add(move);
                        }
                    }
                }
            }

            return bestMoves[rand.Next(bestMoves.Count)];
        }

        private double Negamax(Board board, State state, int depth, double alpha, double beta)
        {
            List<Piece> pieces = board.GetPlayersPieces(state.CurrentPlayer);
            HashSet<Move> legalJumps = board.GetLegalJumps(pieces);
            HashSet<Move> legalMoves = board.GetLegalMoves(pieces);

            foreach (var move in legalJumps)
            {
                move.IsJump = true;
            }

            legalMoves.UnionWith(legalJumps);

            if (depth == 0)
                return Evaluate(state);

            Program.Square currentPlayer = state.CurrentPlayer;
            Board boardCopy;
            State newState;

            foreach (var move in legalMoves)
            {
                boardCopy = board.DeepCopy();

                if (move.IsJump)
                {
                    boardCopy.DoJump(move);
                }
                else
                {
                    boardCopy.MovePiece(move);
                }

                newState = new State(boardCopy.GetState(), Program.ChangeTurn(currentPlayer));
                double newScore = -Negamax(boardCopy, newState, depth - 1, -beta, -alpha);

                if (newScore >= beta)
                    return newScore;
                if (newScore > alpha)
                    alpha = newScore;
            }

            return alpha;
        }

        private static int Evaluate(State state)
        {
            int red = 0;
            int white = 0;
            Program.Square currentPlayer = state.CurrentPlayer;
            Program.Square[,] board = state.BoardState;

            for (var row = 0; row < 8; row++)
            {
                for (var col = 0; col < 8; col++)
                {
                    if (board[row, col] == Program.Square.Red)
                        red++;
                    if (board[row, col] == Program.Square.White)
                        white++;
                    if (board[row, col] == Program.Square.RedKing)
                        red += 2;
                    if (board[row, col] == Program.Square.WhiteKing)
                        white += 2;
                }
            }

            if (currentPlayer == Program.Square.Red || currentPlayer == Program.Square.RedKing)
                return red - white;

            return white - red;
        }
    }
}