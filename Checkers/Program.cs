using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkers
{
    public class Program
    {
        private static List<List<State>> previousGames = new List<List<State>>();

        private static void Main(string[] args)
        {
            int choice;

            do
            {
                var board = new Board();

                Console.WriteLine("Choose the option");
                Console.WriteLine("1 - Human vs Human");
                Console.WriteLine("2 - Human vs AI");
                Console.WriteLine("3 - AI vs AI");
                Console.WriteLine("4 - Replay previous game");
                Console.WriteLine("5 - Quit");

                var gameMode = Console.ReadLine();

                while (!int.TryParse(gameMode, out int val) || val > 5 || val < 1)
                {
                    Console.WriteLine("Please provide a valid option");
                    gameMode = Console.ReadLine();
                }

                switch (Int32.Parse(gameMode))
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
                    case 4:
                        if (previousGames.Any())
                        {
                            Console.WriteLine("Select a game to replay");
                            foreach (var game in previousGames)
                            {
                                Console.WriteLine("Game #{0}", previousGames.IndexOf(game) + 1);
                            }

                            String gameId = Console.ReadLine();

                            while (!int.TryParse(gameId, out int val) || val > previousGames.Count || val < 1)
                            {
                                Console.WriteLine("Please provide a valid option");
                                gameId = Console.ReadLine();
                            }

                            ReplayGame(previousGames[int.Parse(gameId) - 1]);
                        }
                        else
                        {
                            Console.WriteLine("No games to replay");
                        }
                        break;
                    case 5:
                        Environment.Exit(0);
                        break;
                }

                Console.WriteLine("\nWhat next?");
                Console.WriteLine("1 - Play another game or replay previous one");
                Console.WriteLine("2 - Quit");

                String answer = Console.ReadLine();

                while (!int.TryParse(answer, out int val) || val > 2 || val < 1)
                {
                    Console.WriteLine("Please provide a valid option");
                    answer = Console.ReadLine();
                }

                choice = int.Parse(answer);
            } while (choice != 2);
        }

        /// <summary>
        /// Allows two AI players to play against each other. Records the state of the game after each turn to
        /// allow the game to be replayed
        /// </summary>
        /// <param name="board"></param>
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
                Console.WriteLine("\n\nPlayer turn: {0}", currentPlayer);
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

                // get the move from the AI player based on the current state of the board
                Move bestMove = bot.GetBestMove(board, state);

                // AI is jumping
                if (bestMove.IsJump)
                {
                    while (legalJumps.Any())
                    {
                        Piece jumpingPiece =
                            playersPieces.Find(piece => piece.X == bestMove.FromRow && piece.Y == bestMove.FromCol);
                        bool kingBeforeJump = jumpingPiece.IsKing;

                        board.DoJump(bestMove, jumpingPiece);
                        Console.WriteLine("AI's move: from {0} to {1}", bestMove.CoordinateFrom, bestMove.CoordinateTo);

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

                        // make a subsequent jump if there is one
                        if (legalJumps.Any())
                            bestMove = legalJumps.First();
                    }
                }
                else
                {
                    board.MovePiece(bestMove);
                    Console.WriteLine("AI's move: from {0} to {1}", bestMove.CoordinateFrom, bestMove.CoordinateTo);
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

            // add states all subsequent states of the game to the list of previous games
            previousGames.Add(states);

            Console.WriteLine("Replay the game? Plase answer Y/N");
            string answer = Console.ReadLine();

            while (!answer.Equals("y") && !answer.Equals("n"))
            {
                Console.WriteLine("Please type 'Y' to replay the game or 'N' to skip");
                answer = Console.ReadLine();
            }

            if (answer.Equals("y"))
                ReplayGame(states);
        }

        /// <summary>
        /// Allows human player to play against AI player. Records the state of the game after each turn to provide
        /// undo/redo/replay functionalities
        /// </summary>
        /// <param name="board"></param>
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
                Console.WriteLine("\n\nPlayer turn: {0}", currentPlayer);
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

                // human player's turn
                if (currentPlayer == Square.Red)
                {
                    Console.WriteLine("What would you like to do? Type 'Undo', 'Redo', or press enter to carry on");
                    var choice = Console.ReadLine();

                    if (choice.Equals("undo", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // check whether there were any states before current state
                        if (pointer > 0)
                        {
                            pointer -= 2;

                            // get a deep copy of the previous state of the game
                            State previousState = states[pointer].DeepCopy();

                            //Console.WriteLine("RESTORED STATE: " + pointer);

                            // set previous state of the game to be the current state and reverse player's turn
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
                        // check whether there were any states after current state
                        if (pointer < states.Count - 1)
                        {
                            pointer += 2;

                            // get a deep copy of the next state of the game
                            State redoState = states[pointer].DeepCopy();

                            //Console.WriteLine("RESTORED STATE: " + pointer);

                            // set next state of the game to be the current state and reverse player's turn
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
                // AI player's turn
                else if (currentPlayer == Square.White)
                {
                    State state = new State(board.GetState(), currentPlayer);

                    // get the move from the AI player based on the current state of the board
                    Move bestMove = bot.GetBestMove(board, state);

                    // AI is jumping
                    if (bestMove.IsJump)
                    {
                        while (legalJumps.Any())
                        {
                            Piece jumpingPiece =
                                playersPieces.Find(piece => piece.X == bestMove.FromRow && piece.Y == bestMove.FromCol);
                            bool kingBeforeJump = jumpingPiece.IsKing;

                            board.DoJump(bestMove, jumpingPiece);
                            Console.WriteLine("AI's move from {0} to {1}", bestMove.CoordinateFrom, bestMove.CoordinateTo);

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

                            // make a subsequent jump if there is one
                            if (legalJumps.Any())
                                bestMove = legalJumps.First();
                        }
                    }
                    else
                    {
                        board.MovePiece(bestMove);
                        Console.WriteLine("AI's move from {0} to {1}", bestMove.CoordinateFrom, bestMove.CoordinateTo);
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

            // add states all subsequent states of the game to the list of previous games
            previousGames.Add(states);

            Console.WriteLine("Replay the game? Plase answer Y/N");
            string answer = Console.ReadLine();

            while (!answer.Equals("y") && !answer.Equals("n"))
            {
                Console.WriteLine("Please type 'Y' to replay the game or 'N' to skip");
                answer = Console.ReadLine();
            }

            if (answer.Equals("y"))
                ReplayGame(states);
        }

        /// <summary>
        /// Allows two human players to play against each other. Records the state of the game after each turn to 
        /// provide undo/redo/replay functionalities
        /// </summary>
        /// <param name="board"></param>
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
                Console.WriteLine("\n\nPlayer turn: {0}", currentPlayer);
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
                    // check whether there were any states before current state
                    if (pointer > 0)
                    {
                        pointer--;

                        // get a deep copy of the previous state of the game
                        State previousState = states[pointer].DeepCopy();

                        //Console.WriteLine("RESTORED STATE: " + pointer);

                        // set previous state of the game to be the current state and reverse player's turn
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
                    // check whether there were any states after current state
                    if (pointer < states.Count - 1)
                    {
                        pointer++;

                        // get a deep copy of the next state of the game
                        State redoState = states[pointer].DeepCopy();

                        //Console.WriteLine("RESTORED STATE: " + pointer);

                        // set next state of the game to be the current state and reverse player's turn
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

            // add states all subsequent states of the game to the list of previous games
            previousGames.Add(states);

            Console.WriteLine("Replay the game? Plase answer Y/N");
            string answer = Console.ReadLine();

            while (!answer.Equals("y") && !answer.Equals("n"))
            {
                Console.WriteLine("Please type 'Y' to replay the game or 'N' to skip");
                answer = Console.ReadLine();
            }

            if (answer.Equals("y"))
                ReplayGame(states);
        }

        /// <summary>
        /// Allows the user to replay the game once it's finished, ie. see how the state of the game was changing
        /// between the turns requested by the user.
        /// </summary>
        /// <param name="states"></param>
        private static void ReplayGame(List<State> states)
        {
            Console.WriteLine("There were {0} turns in the game", states.Count);
            Console.WriteLine("Turn to replay from? Type 'first' to replay from first turn or provide a number");
            var replayFrom = Console.ReadLine();

            int indexFrom;
            if (replayFrom.Equals("first"))
            {
                indexFrom = states.IndexOf(states.First());
            }
            else
            {
                while (!int.TryParse(replayFrom, out int val) || val > states.Count - 1 || val < 1)
                {
                    Console.WriteLine("Please provide a valid turn number");
                    replayFrom = Console.ReadLine();
                }

                indexFrom = int.Parse(replayFrom) - 1;
            }

            Console.WriteLine("Turn to replay to? Type 'last' to replay to the last turn or provide a number");
            var replayTo = Console.ReadLine();

            int indexTo;
            if (replayTo.Equals("last"))
            {
                indexTo = states.IndexOf(states.Last());
            }
            else
            {
                while (!int.TryParse(replayTo, out int val) || val > states.Count || val < 2 || val <= indexFrom)
                {
                    Console.WriteLine("Please provide a valid turn number");
                    replayTo = Console.ReadLine();
                }

                indexTo = int.Parse(replayTo) - 1;
            }

            // print each following state of the board between the requested turns
            for (int i = indexFrom; i <= indexTo; i++)
            {
                if (i == states.Count - 1)
                    Console.WriteLine("Turn #{0}, {1} player won", i + 1, ChangeTurn(states[i].CurrentPlayer));
                else
                    Console.WriteLine("Turn #{0}, {1} player's turn", i + 1, states[i].CurrentPlayer);

                states[i].PrintState();
            }
        }

        /// <summary>
        /// Obtains the new move from the user. It doesn't determine whether the move provided is valid or not.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Changes colour of the player provided in the parameter to opposite one.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static Square ChangeTurn(Square player)
        {
            if (player == Square.Red)
                return Square.White;
            if (player == Square.White)
                return Square.Red;

            return Square.EmptyDark;
        }

        /// <summary>
        /// Represents 6 possible types of squares/pieces that can be present on the board in the game of checkers
        /// </summary>
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