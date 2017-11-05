using System;
using System.Collections.Generic;

namespace Checkers
{
    /// <summary>
    /// This class handles all AI player's logic and calculations that lead to obtaining best moves for each turn.
    /// </summary>
    public class Ai
    {
        /// <summary>
        /// Retrieves the best move that AI player can perform that is associated with the highest score returned 
        /// by Negamax method
        /// </summary>
        /// <param name="board"></param>
        /// <param name="state"></param>
        /// <returns>Returns the best move out of all legal moves for AI player</returns>
        public Move GetBestMove(Board board, State state)
        {
            List<Piece> pieces = board.GetPlayersPieces(state.CurrentPlayer);
            HashSet<Move> legalJumps = board.GetLegalJumps(pieces);
            HashSet<Move> legalMoves = board.GetLegalMoves(pieces);
            legalMoves.UnionWith(legalJumps);

            var bestMoves = new List<Move>();

            // analyze all possible moves three turns ahead while searching for best move for this turn
            int depth = 3;
            double alpha = Double.MinValue;

            Program.Square currentPlayer = state.CurrentPlayer;
            Board boardCopy;
            State newState;
            var rand = new Random();

            foreach (var move in legalMoves)
            {
                // make a deep copy of the board to perform a move without changing the original board
                boardCopy = board.DeepCopy();

                if (move.IsJump)
                    boardCopy.DoJump(move);
                else
                    boardCopy.MovePiece(move);

                // create a new state of the game using copy of the original board after move has been performed
                newState = new State(boardCopy.GetState(), Program.ChangeTurn(currentPlayer));

                // get score for the move that has been applied to copy of the board
                double newScore = -Negamax(boardCopy, newState, depth - 1, Double.MinValue, -alpha);

                //Console.WriteLine("From {0} To {1} = {2}", move.CoordinateFrom, move.CoordinateTo, newScore);

                // if new score is better than alpha, it becomes a new alpha (highest score found so far)
                if (newScore > alpha)
                {
                    alpha = newScore;
                    bestMoves.Clear();
                    bestMoves.Add(move);
                }
                // if score for this move is equal to score of best move found so far, add it to the list of best moves
                else if (newScore == alpha)
                {
                    bestMoves.Add(move);
                }
            }

            // remove all regular moves from the list of best moves if any jumps were found
            if (bestMoves.Exists(other => other.IsJump))
                bestMoves.RemoveAll(notJump => !notJump.IsJump);

            return bestMoves[rand.Next(bestMoves.Count)];
        }

        /// <summary>
        /// Evaluates all possible moves that AI player can make during its current turn in order to
        /// find the highest score associated with one of these moves
        /// </summary>
        /// <param name="board"></param>
        /// <param name="state"></param>
        /// <param name="depth"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns>Returns the highest scored out of all possible moves</returns>
        private double Negamax(Board board, State state, int depth, double alpha, double beta)
        {
            List<Piece> pieces = board.GetPlayersPieces(state.CurrentPlayer);
            HashSet<Move> legalJumps = board.GetLegalJumps(pieces);
            HashSet<Move> legalMoves = board.GetLegalMoves(pieces);
            legalMoves.UnionWith(legalJumps);

            // evaluate the state of the board in order to obtain the score if depth has reached zero
            if (depth == 0)
                return Evaluate(state);

            Program.Square currentPlayer = state.CurrentPlayer;
            Board boardCopy;
            State newState;

            foreach (var move in legalMoves)
            {
                boardCopy = board.DeepCopy();

                if (move.IsJump)
                    boardCopy.DoJump(move);
                else
                    boardCopy.MovePiece(move);

                newState = new State(boardCopy.GetState(), Program.ChangeTurn(currentPlayer));
                double newScore = -Negamax(boardCopy, newState, depth - 1, -beta, -alpha);

                // alpha-beta cut-off
                if (newScore >= beta)
                    return newScore;
                if (newScore > alpha)
                    alpha = newScore;
            }

            return alpha;
        }

        /// <summary>
        /// Evaluates the state of the board provided by Negamax method by calculating the difference in number of 
        /// both players' pieces, either from Red or from White player's perspective
        /// </summary>
        /// <param name="state"></param>
        /// <returns>Returns the score for a move which is the difference between the number of players' pieces
        /// left on the board once the move has been applied</returns>
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

            // return the difference between the number of red and white pieces if calculating for red player
            if (currentPlayer == Program.Square.Red || currentPlayer == Program.Square.RedKing)
                return red - white;

            // return the difference between the number of white and red pieces if calculating for white player
            return white - red;
        }
    }
}