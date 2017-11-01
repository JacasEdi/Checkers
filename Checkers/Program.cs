using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Checkers
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var board = new Board();

            Console.WriteLine("Choose the game mode");
            Console.WriteLine("1 - Human vs Human");
            Console.WriteLine("2 - Human vs AI");
            Console.WriteLine("3 - AI vs AI");

            var choice = Console.ReadLine();

            while (!int.TryParse(choice, out int val) || val > 3 || val < 1)
            {
                Console.WriteLine("Please provide a valid option");
                choice = Console.ReadLine();
            }

            switch (Int32.Parse(choice))
            {
                case 1:
                    HumanVsHuman(board);
                    break;
                case 2:
                    HumanVsAi(board);
                    break;
                case 3:
                    AiVsAi(board);
                    break;
            }
        }

        private static void AiVsAi(Board board)
        {
            Square currentPlayer = Square.Red;

            // list for storing all subsequent states of the game
            List<State> states = new List<State>();

            // add initial state of the game to the list
            State firstState = new State(board.GetState(), currentPlayer);
            states.Add(firstState);

            // initialize pointer variable and point it to the initial state of the game
            int pointer = 0;

            Ai bot = new Ai();

            // continue the game as long as current player has any pieces left
            while (board.GetPlayersPieces(currentPlayer).Any())
            {
                board.PrintBoard();

                // get the list of pieces belonging to current player
                var playersPieces = board.GetPlayersPieces(currentPlayer);
                // get the list of legal non-jump moves that player can make
                var legalMoves = board.GetLegalMoves(playersPieces);
                // get the list of legal jumps that player can (have to) make
                var legalJumps = board.GetLegalJumps(playersPieces);

                // if current player has no jumps and no regular moves to make, he's lost
                if (legalJumps.Count + legalMoves.Count == 0)
                    break;

                State state = new State(board.GetState(), currentPlayer);
                Move bestMove = bot.GetBestMove(board, state);

                Console.WriteLine("AI's move: from {0} to {1}", bestMove.CoordinateFrom, bestMove.CoordinateTo);

                if (bestMove.IsJump)
                {
                    while (legalJumps.Any())
                    {
                        Piece jumpingPiece =
                            playersPieces.Find(piece => piece.X == bestMove.FromRow && piece.Y == bestMove.FromCol);
                        bool kingBeforeJump = jumpingPiece.IsKing;

                        board.DoJump(bestMove, jumpingPiece);

                        // update variables of the piece that has just jumped
                        jumpingPiece.HasJustJumped = true;
                        jumpingPiece.X = bestMove.ToRow;
                        jumpingPiece.Y = bestMove.ToCol;

                        // all pieces other than the one that has just jumped are irrelevant when checking if more jumps can be made
                        playersPieces.RemoveAll(piece => piece.HasJustJumped == false);

                        legalJumps = board.GetLegalJumps(playersPieces);

                        // update position of the jumping piece on the board
                        board.UpdatePosition(jumpingPiece, bestMove.FromRow, bestMove.FromCol);

                        // no further jumps can be made if piece became a king after the first jump
                        if (!kingBeforeJump && bestMove.ToRow == 0 | bestMove.ToRow == 7)
                            break;

                        if (legalJumps.Any())
                            bestMove = legalJumps.First();
                    }
                }
                else
                {
                    board.MovePiece(bestMove);
                }

                currentPlayer = ChangeTurn(currentPlayer);

                // create new state of the game after move was made
                State newState = new State(board.GetState(), currentPlayer);

                // move was made after performing undo operation since pointer is not pointing to the last state
                if (pointer < states.Count - 1)
                    // newState now becomes the last state, remove all states that happened after it
                    states.RemoveAll(other => states.IndexOf(other) > pointer);

                states.Add(newState);

                // set pointer to the new state   
                pointer = states.IndexOf(newState);
            }

            // print the final state of the board once the game has ended
            board.PrintBoard();
            Console.WriteLine("{0} player won", ChangeTurn(currentPlayer));
        }

        private static void HumanVsAi(Board board)
        {
            Square currentPlayer = Square.Red;

            // list for storing all subsequent states of the game
            List<State> states = new List<State>();

            // add initial state of the game to the list
            State firstState = new State(board.GetState(), currentPlayer);
            states.Add(firstState);

            // initialize pointer variable and point it to the initial state of the game
            int pointer = 0;

            Ai bot = new Ai();

            // continue the game as long as current player has any pieces left
            while (board.GetPlayersPieces(currentPlayer).Any())
            {
                board.PrintBoard();

                // get the list of pieces belonging to current player
                var playersPieces = board.GetPlayersPieces(currentPlayer);
                // get the list of legal non-jump moves that player can make
                var legalMoves = board.GetLegalMoves(playersPieces);
                // get the list of legal jumps that player can (have to) make
                var legalJumps = board.GetLegalJumps(playersPieces);

                // if current player has no jumps and no regular moves to make, he's lost
                if (legalJumps.Count + legalMoves.Count == 0)
                    break;

                if (currentPlayer == Square.Red)
                {
                    Console.WriteLine("What would you like to do? Type 'Undo', 'Redo', or press enter to carry on");
                    var choice = Console.ReadLine();

                    if (choice.Equals("undo", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (pointer > 0)
                        {
                            pointer -= 2;
                            State previousState = states[pointer].DeepCopy();

                            Console.WriteLine("RESTORED STATE: " + pointer);

                            board.SetState(previousState.BoardState);
                            currentPlayer = previousState.CurrentPlayer;
                        }
                        else
                        {
                            Console.WriteLine("You can't go back any further");
                        }
                    }
                    else if (choice.Equals("redo", StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (pointer < states.Count - 1)
                        {
                            pointer += 2;
                            State redoState = states[pointer].DeepCopy();

                            Console.WriteLine("RESTORED STATE: " + pointer);

                            board.SetState(redoState.BoardState);
                            currentPlayer = redoState.CurrentPlayer;
                        }
                        else
                        {
                            Console.WriteLine("There are no moves to redo");
                        }
                    }
                    else
                    {
                        var move = RequestMove();

                        // no jumps can be made so a regular move has to be made
                        if (!legalJumps.Any())
                        {
                            while (!legalMoves.Contains(move))
                            {
                                Console.WriteLine("--------------------Please provide a legal move--------------------");
                                move = RequestMove();
                            }

                            // move can be made as it's in the list of legal moves, perform it
                            board.MovePiece(move);
                        }

                        // force the player to make a jump
                        while (legalJumps.Any())
                        {
                            // see if player's move matches any of the possible jumps
                            if (legalJumps.Contains(move))
                            {
                                // get reference to the piece that will be jumping
                                Piece jumpingPiece = playersPieces.Find(piece =>
                                    piece.X == move.FromRow && piece.Y == move.FromCol);
                                bool kingBeforeJump = jumpingPiece.IsKing;

                                board.DoJump(move, jumpingPiece);

                                // update variables of the piece that has just jumped
                                jumpingPiece.HasJustJumped = true;
                                jumpingPiece.X = move.ToRow;
                                jumpingPiece.Y = move.ToCol;

                                // all pieces other than the one that has just jumped are irrelevant when checking if more jumps can be made
                                playersPieces.RemoveAll(piece => piece.HasJustJumped == false);

                                legalJumps = board.GetLegalJumps(playersPieces);

                                // update position of the jumping piece on the board
                                board.UpdatePosition(jumpingPiece, move.FromRow, move.FromCol);

                                // show the state of the board after first jump if another one can be made
                                if (legalJumps.Any())
                                    board.PrintBoard();

                                // no further jumps can be made if piece became a king after the first jump
                                if (!kingBeforeJump && move.ToRow == 0 | move.ToRow == 7)
                                    break;
                            }
                            else
                            {
                                Console.WriteLine("--------------------You have to jump--------------------");
                                move = RequestMove();
                            }
                        }

                        // change turn
                        currentPlayer = ChangeTurn(currentPlayer);

                        // create new state of the game after move was made
                        State newState = new State(board.GetState(), currentPlayer);

                        // move was made after performing undo operation since pointer is not pointing to the last state
                        if (pointer < states.Count - 1)
                            // newState now becomes the last state, remove all states that happened after it
                            states.RemoveAll(other => states.IndexOf(other) > pointer);

                        states.Add(newState);

                        // set pointer to the new state   
                        pointer = states.IndexOf(newState);
                    }
                }
                else if (currentPlayer == Square.White)
                {
                    State state = new State(board.GetState(), currentPlayer);
                    Move bestMove = bot.GetBestMove(board, state);

                    Console.WriteLine("AI's move from {0} to {1}", bestMove.CoordinateFrom, bestMove.CoordinateTo);

                    if (bestMove.IsJump)
                    {
                        while (legalJumps.Any())
                        {
                            Piece jumpingPiece =
                                playersPieces.Find(piece => piece.X == bestMove.FromRow && piece.Y == bestMove.FromCol);
                            bool kingBeforeJump = jumpingPiece.IsKing;

                            board.DoJump(bestMove, jumpingPiece);

                            // update variables of the piece that has just jumped
                            jumpingPiece.HasJustJumped = true;
                            jumpingPiece.X = bestMove.ToRow;
                            jumpingPiece.Y = bestMove.ToCol;

                            // all pieces other than the one that has just jumped are irrelevant when checking if more jumps can be made
                            playersPieces.RemoveAll(piece => piece.HasJustJumped == false);

                            legalJumps = board.GetLegalJumps(playersPieces);

                            // update position of the jumping piece on the board
                            board.UpdatePosition(jumpingPiece, bestMove.FromRow, bestMove.FromCol);

                            // no further jumps can be made if piece became a king after the first jump
                            if (!kingBeforeJump && bestMove.ToRow == 0 | bestMove.ToRow == 7)
                                break;

                            if (legalJumps.Any())
                                bestMove = legalJumps.First();
                        }
                    }
                    else
                    {
                        board.MovePiece(bestMove);
                    }

                    currentPlayer = ChangeTurn(currentPlayer);

                    // create new state of the game after move was made
                    State newState = new State(board.GetState(), currentPlayer);

                    // move was made after performing undo operation since pointer is not pointing to the last state
                    if (pointer < states.Count - 1)
                        // newState now becomes the last state, remove all states that happened after it
                        states.RemoveAll(other => states.IndexOf(other) > pointer);

                    states.Add(newState);

                    // set pointer to the new state   
                    pointer = states.IndexOf(newState);
                }
            }

            // print the final state of the board once the game has ended
            board.PrintBoard();
            Console.WriteLine("{0} player won", ChangeTurn(currentPlayer));
        }

        private static void HumanVsHuman(Board board)
        {
            // Red player starts the game
            var currentPlayer = Square.Red;

            // list for storing all subsequent states of the game
            List<State> states = new List<State>();

            // add initial state of the game to the list
            State firstState = new State(board.GetState(), currentPlayer);
            states.Add(firstState);

            // initialize pointer variable and point it to the initial state of the game
            int pointer = 0;

            // continue the game as long as current player has any pieces left
            while (board.GetPlayersPieces(currentPlayer).Any())
            {
                Console.WriteLine("\nPlayer turn: {0}", currentPlayer);

                board.PrintBoard();

                // get the list of pieces belonging to current player
                var playersPieces = board.GetPlayersPieces(currentPlayer);
                // get the list of legal moves that player can make which are not jumps
                var legalMoves = board.GetLegalMoves(playersPieces);
                // get the list of legal jumps that player can (have to) make
                var legalJumps = board.GetLegalJumps(playersPieces);

                // if current player has no jumps and no regular moves to make, he's lost
                if (legalJumps.Count + legalMoves.Count == 0)
                    break;

                Console.WriteLine("What would you like to do? Type 'Undo', 'Redo', or press enter to carry on");
                var choice = Console.ReadLine();

                if (choice.Equals("undo", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (pointer > 0)
                    {
                        pointer--;
                        State previousState = states[pointer].DeepCopy();

                        Console.WriteLine("RESTORED STATE: " + pointer);

                        board.SetState(previousState.BoardState);
                        currentPlayer = previousState.CurrentPlayer;
                    }
                    else
                    {
                        Console.WriteLine("You can't go back any further");
                    }
                }
                else if (choice.Equals("redo", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (pointer < states.Count - 1)
                    {
                        pointer++;
                        State redoState = states[pointer].DeepCopy();

                        Console.WriteLine("RESTORED STATE: " + pointer);

                        board.SetState(redoState.BoardState);
                        currentPlayer = redoState.CurrentPlayer;
                    }
                    else
                    {
                        Console.WriteLine("There are no moves to redo");
                    }
                }
                else
                {
                    var move = RequestMove();

                    // no jumps can be made so a regular move has to be made
                    if (!legalJumps.Any())
                    {
                        while (!legalMoves.Contains(move))
                        {
                            Console.WriteLine("--------------------Please provide a legal move--------------------");
                            move = RequestMove();
                        }

                        // move can be made as it's in the list of legal moves, perform it
                        board.MovePiece(move);
                    }

                    // force the player to make a jump
                    while (legalJumps.Any())
                    {
                        // see if player's move matches any of the possible jumps
                        if (legalJumps.Contains(move))
                        {
                            // get reference to the piece that will be jumping
                            Piece jumpingPiece = playersPieces.Find(piece =>
                                piece.X == move.FromRow && piece.Y == move.FromCol);
                            bool kingBeforeJump = jumpingPiece.IsKing;

                            board.DoJump(move, jumpingPiece);

                            // update variables of the piece that has just jumped
                            jumpingPiece.HasJustJumped = true;
                            jumpingPiece.X = move.ToRow;
                            jumpingPiece.Y = move.ToCol;

                            // all pieces other than the one that has just jumped are irrelevant when checking if more jumps can be made
                            playersPieces.RemoveAll(piece => piece.HasJustJumped == false);

                            legalJumps = board.GetLegalJumps(playersPieces);

                            // update position of the jumping piece on the board
                            board.UpdatePosition(jumpingPiece, move.FromRow, move.FromCol);

                            // show the state of the board after first jump if another one can be made
                            if (legalJumps.Any())
                                board.PrintBoard();

                            // no further jumps can be made if piece became a king after the first jump
                            if (!kingBeforeJump && move.ToRow == 0 | move.ToRow == 7)
                                break;
                        }
                        else
                        {
                            Console.WriteLine("--------------------You have to jump--------------------");
                            move = RequestMove();
                        }
                    }

                    currentPlayer = ChangeTurn(currentPlayer);

                    // create new state of the game after move was made
                    State newState = new State(board.GetState(), currentPlayer);

                    // move was made after performing undo operation since pointer is not pointing to the last state
                    if (pointer < states.Count - 1)
                        // newState now becomes the last state, remove all states that happened after it
                        states.RemoveAll(other => states.IndexOf(other) > pointer);

                    states.Add(newState);

                    // set pointer to the new state   
                    pointer = states.IndexOf(newState);
                }
            }

            // print the final state of the board once the game has ended
            board.PrintBoard();
            Console.WriteLine("{0} player won", ChangeTurn(currentPlayer));
        }

        private static Move RequestMove()
        {
            int rowFrom;
            int colFrom;
            int rowTo;
            int colTo;

            Console.WriteLine("Enter row to move from");
            var inputRowFrom = Console.ReadLine();
            while (!int.TryParse(inputRowFrom, out rowFrom))
            {
                Console.WriteLine("Please provide valid (numeric) value");
                inputRowFrom = Console.ReadLine();
            }

            Console.WriteLine("Enter column to move from");
            var inputColFrom = Console.ReadLine();
            while (!int.TryParse(inputColFrom, out colFrom))
            {
                Console.WriteLine("Please provide valid (numeric) value");
                inputColFrom = Console.ReadLine();
            }

            Console.WriteLine("Enter row to move to");
            var inputRowTo = Console.ReadLine();
            while (!int.TryParse(inputRowTo, out rowTo))
            {
                Console.WriteLine("Please provide valid (numeric) value");
                inputRowTo = Console.ReadLine();
            }

            Console.WriteLine("Enter column to move to");
            var inputColTo = Console.ReadLine();
            while (!int.TryParse(inputColTo, out colTo))
            {
                Console.WriteLine("Please provide valid (numeric) value");
                inputColTo = Console.ReadLine();
            }

            return new Move(rowFrom, colFrom, rowTo, colTo);
        }

        public static Square ChangeTurn(Square player)
        {
            if (player == Square.Red)
                return Square.White;
            if (player == Square.White)
                return Square.Red;

            return Square.EmptyDark;
        }

        public enum Square
        {
            EmptyLight,
            EmptyDark,
            Red,
            White,
            RedKing,
            WhiteKing
        }
    }
}