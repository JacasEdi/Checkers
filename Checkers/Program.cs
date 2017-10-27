﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Checkers
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var board = new Board();
            //PlayerVsPlayer(board);
            PlayerVsAI(board);
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

        public enum GameType
        {
            PvP,
            PvC,
            CvC
        }

        private static void PlayerVsAI(Board board)
        {
            Square currentPlayer = Square.Red;
            State state = new State(board.GetState(), ChangeTurn(currentPlayer));

            double val = AlphaBeta(board, state, 3, Double.NegativeInfinity, Double.PositiveInfinity);
            Console.WriteLine("Val: " + val);
        }

           
        private static double AlphaBeta(Board board, State state, int depth, double alpha, double beta)
        {
            if (depth == 0)
                return Evaluate(state);

            var playersPieces = board.GetPlayersPieces(state.CurrentPlayer);
            var legalMoves = board.GetLegalMoves(playersPieces);
            var legalJumps = board.GetLegalJumps(playersPieces);

            //this line could be rubbish
            legalMoves.UnionWith(legalJumps);

            foreach (var move in legalMoves)
            {
                var boardCopy = board.DeepCopy();
                boardCopy.MovePiece(move);

                State newState = new State(boardCopy.GetState(), ChangeTurn(state.CurrentPlayer));
                // Unmake move here if I decide to not use a shallow copy but original board

                double val = -AlphaBeta(boardCopy, newState, depth - 1, -beta, -alpha);

                if (val >= beta)
                    return beta;
                if (val > alpha)
                    alpha = val;
            }

            return alpha;
        }

        /*        
       private float NegaMax(Board board, State state, int depth)
       {
           var playersPieces = board.GetPlayersPieces(state.CurrentPlayer);
           var legalMoves = board.GetLegalMoves(playersPieces);
           var legalJumps = board.GetLegalJumps(playersPieces);

           foreach (var move in legalMoves)
           {
               var boardCopy = board.ShallowCopy();
               boardCopy.MovePiece(move);

               board.MovePiece(move);
               State newState = new State(board.GetState(), ChangeTurn(state.CurrentPlayer));
               float newValue = NegaMax(board, newState, depth + 1);
           }
       }*/

        private static double Evaluate(State state)
        {
            double red = 0;
            double white = 0;
            Square currentPlayer = state.CurrentPlayer;
            Square[,] board = state.BoardState;

            for (var row = 0; row < 8; row++)
            {
                for (var col = 0; col < 8; col++)
                {
                    if (board[row, col] == Square.Red)
                        red++;
                    if (board[row, col] == Square.White)
                        white++;
                    if (board[row, col] == Square.RedKing)
                        red += 1.5;
                    if (board[row, col] == Square.WhiteKing)
                        white += 1.5;
                }
            }

            if (currentPlayer == Square.Red || currentPlayer == Square.RedKing)
                return red - white;
            else
            {
                return white - red;
            }
        }

        private static void PlayerVsPlayer(Board board)
        {
            // Red player starts the game
            var currentPlayer = Square.Red;

            // array lists for storing all subsequent states of the game and pointers to them
            var states = new ArrayList();
            var pointers = new ArrayList();

            // add initial state of the game to the list
            states.Add(new State(board.GetState(), currentPlayer));

            // initialize pointer variable and add pointer to the initial state of the game to the list
            var pointer = 0;
            pointers.Add(pointer);

            // continue the game as long as current player has any pieces left
            while (board.GetPlayersPieces(currentPlayer).Count > 0)
            {
                Console.WriteLine("\nPlayer turn: {0}", currentPlayer);

/*
                Console.Write("Pointers list: ");
                foreach (var point in pointers)
                    Console.Write(" {0}, ", point);
                Console.WriteLine();
                Console.Write("States list: ");
                foreach (var state in states)
                    Console.Write("state" + states.IndexOf(state) + ", ");

                Console.WriteLine();
                Console.WriteLine("Pointer: " + pointer);

                Console.WriteLine("Current state: " + states.IndexOf(states[pointer]));
*/
                board.PrintBoard();

                // get the list of pieces belonging to current player
                var playersPieces = board.GetPlayersPieces(currentPlayer);

                // get the list of legal non-jump moves that player can make
                var legalMoves = board.GetLegalMoves(playersPieces);

                // get the list of legal jumps that player can (have to) make
                var legalJumps = board.GetLegalJumps(playersPieces);

                Console.WriteLine("What would you like to do? Type 'Undo', 'Redo', or press enter to carry on");
                var choice = Console.ReadLine();

                if (choice.Equals("undo", StringComparison.CurrentCultureIgnoreCase))
                {
                    //Console.WriteLine("Pointer: " + (pointer) + ", pointers[pointers.IndexOf(pointer)-1]: " + pointers[pointers.IndexOf(pointer)-1]);                    
                    if (pointer > 0 && pointer - (int)pointers[pointers.IndexOf(pointer) - 1] == 1)
                    {
                        var previousState = (State)states[pointer - 1];
                        Console.WriteLine("Restoring state #" + states.IndexOf(previousState));
                        board.SetState(previousState.BoardState);

                        currentPlayer = previousState.CurrentPlayer;

                        pointer = states.IndexOf(previousState);
                        pointers.Add(pointer);
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
                        var redoState = (State)states[pointer + 1];
                        board.SetState(redoState.BoardState);

                        currentPlayer = redoState.CurrentPlayer;

                        pointer = states.IndexOf(redoState);
                        pointers.Add(pointer);
                    }
                    else
                    {
                        Console.WriteLine("There are no moves to redo");
                    }
                }
                else
                {
                    var move = RequestMove();

                    // see if any jumps can be (have to be) made
                    if (legalJumps.Count > 0)
                    {
                        // see if user's move matches any of the possible jumps
                        if (legalJumps.Contains(move))
                        {
                            // jump is legal, perform it
                            board.DoJump(move, false);

                            // add new state to the list after jump has been made                                             
                            var newState = new State(board.GetState(), ChangeTurn(currentPlayer));
                            states.Add(newState);

                            // add pointer to the newly created state to the list of pointers
                            pointer = states.IndexOf(newState);
                            pointers.Add(pointer);

                            // get reference to the piece that has just jumped                      
                            Piece jumpingPiece = playersPieces.Find(piece => piece.X == move.FromRow && piece.Y == move.FromCol);

                            // update state of the board before checking if another jump can be made by the same piece
                            board.UpdateBoard(jumpingPiece, move.FromRow, move.FromCol);

                            // update the x and y corrdinates of jumping piece and indicate that it's just jumped
                            jumpingPiece.X = move.ToRow;
                            jumpingPiece.Y = move.ToCol;
                            jumpingPiece.HasJustJumped = true;

                            // refresh the list of legal jumps to see if another jump can be made by the same piece                  
                            legalJumps = board.GetLegalJumps(playersPieces);
                            legalJumps.RemoveWhere(anotherMove => !anotherMove.CoordinateFrom.Equals(move.CoordinateTo));

                            // if piece has not landed at the either end of the board, allow it to make multiple jump
                            if (move.ToRow != 0 && move.ToRow != 7)
                            {
                                while (legalJumps.Count > 0)
                                {
                                    Console.WriteLine("Another jump can be made");

                                    board.PrintBoard();

                                    // force the user to enter a move that matches coordinates of the multiple jump
                                    do
                                    {
                                        move = RequestMove();

                                        if (!legalJumps.Contains(move))
                                            Console.WriteLine("--------------------Please provide a legal jump--------------------");
                                    } while (!legalJumps.Contains(move));

                                    // make a jump and indicate that it's another jump in a sequence of multiple jumps
                                    board.DoJump(move, true);

                                    // add new state to the list after another jump has been performed                                              
                                    var postJumpState = new State(board.GetState(), ChangeTurn(currentPlayer));
                                    states.Add(postJumpState);
                                    pointer = states.IndexOf(postJumpState);
                                    pointers.Add(pointer);
                    
                                    jumpingPiece = playersPieces.Find(piece => piece.X == move.FromRow && piece.Y == move.FromCol);

                                    // update the x and y corrdinates of jumping piece and indicate that it's just jumped
                                    jumpingPiece.X = move.ToRow;
                                    jumpingPiece.Y = move.ToCol;
                                    jumpingPiece.HasJustJumped = true;

                                    board.UpdateBoard(jumpingPiece, move.FromRow, move.FromCol);

                                    // refresh the list of legal jumps to see if another jump can be made by the same piece                    
                                    legalJumps = board.GetLegalJumps(playersPieces);
                                    legalJumps.RemoveWhere(anotherMove => !anotherMove.CoordinateFrom.Equals(move.CoordinateTo));
                                }
                            }                           
                            currentPlayer = ChangeTurn(currentPlayer);
                        }
                        else
                        {
                            Console.WriteLine("--------------------You have to jump--------------------");
                        }
                    }
                    // otherwise force the user to perform any legal move and change turn
                    else if (legalMoves.Contains(move))
                    {
                        // move can be made as it's in the list of legal moves, perform it
                        board.MovePiece(move);

                        // add new state to the list after move has been performed    
                        var newState = new State(board.GetState(), ChangeTurn(currentPlayer));
                        states.Add(newState);

                        pointer = states.IndexOf(newState);
                        pointers.Add(pointer);

                        // change turn
                        currentPlayer = ChangeTurn(currentPlayer);
                    }
                    else
                    {
                        Console.WriteLine("--------------------Please provide a legal move--------------------");
                    }
                }
            }

            board.PrintBoard();

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

        private static Square ChangeTurn(Square player)
        {
            if (player == Square.Red)
                return Square.White;
            if (player == Square.White)
                return Square.Red;

            return Square.EmptyDark;
        }

        private class Board
        {
            private const int Size = 8;
            private Square[,] _board = new Square[8, 8];

            /// <summary>
            /// Make a deep copy of the board
            /// </summary>
            public Board DeepCopy()
            {
                Board other = (Board)this.MemberwiseClone();
                other._board = _board.Clone() as Square[,];
                return other;
            }

            /// <summary>
            /// Initialize an empty board and place red and white pieces in relevant squares
            /// </summary>
            public Board()
            {
                InitializeBoard();
                SetupPieces();
            }

            /// <summary>
            /// Get current state of a board, ie. how squares are laid out
            /// </summary>
            public Square[,] GetState()
            {
                var newArr = new Square[8, 8];

                for (var row = 0; row < 8; row++)
                for (var col = 0; col < 8; col++)
                    newArr[row, col] = _board[row, col];

                return newArr;
            }

            /// <summary>
            /// Set the state of the board using the state provided
            /// </summary>
            public void SetState(Square[,] state)
            {
                _board = state;
            }

            /// <summary>
            /// Changes the location of a piece using the provided move
            /// </summary>
            public void MovePiece(Move move)
            {
                var fromRow = move.FromRow;
                var fromCol = move.FromCol;
                var toRow = move.ToRow;
                var toCol = move.ToCol;

                // if move is more than 2 rows forwards/backwards, then it's illegal
                if (toRow - fromRow > 2 || toRow - fromRow < -2)
                {
                    Console.WriteLine("You can't move further than 2 rows");
                }
                // TODO this is probably allowing RedKing to make funny jumps
                // if move requested is 2 rows away
                /*                else if (toRow - fromRow == 2 || toRow - fromRow == -2)
                                {
                                    // player is attempting to jump, check if it's legal
                                    CanJump(move);
                                }*/
                else //if (IsMovePermitted(move))
                {
                    // if the piece has reached either first or last row, it becomes a king
                    if (toRow == 0 || toRow == 7)
                    {
                        MakeKing(_board[fromRow, fromCol], toRow, toCol);
                        _board[fromRow, fromCol] = Square.EmptyDark;
                    }
                    else
                    {
                        // move piece and replace it with an empty dark square
                        _board[toRow, toCol] = _board[fromRow, fromCol];
                        _board[fromRow, fromCol] = Square.EmptyDark;
                    }

                    Console.WriteLine("Move made");
                }
            }

            /// <summary>
            /// Makes a regular red or white piece a king once it's reached the opposite end row of the board
            /// </summary>
            private void MakeKing(Square currentPlayer, int rowTo, int colTo)
            {
                if (currentPlayer == Square.Red && rowTo == 0)
                    _board[rowTo, colTo] = Square.RedKing;

                if (currentPlayer == Square.White && rowTo == 7)
                    _board[rowTo, colTo] = Square.WhiteKing;
            }

            /// <summary>
            /// Checks whether provided move is valid
            /// </summary>
            private bool IsMovePermitted(Square currentPiece, Move move)
            {
                // can't move outside the board
                if (move.ToRow < 0 || move.ToRow > 7 || move.ToCol < 0 || move.ToCol > 7)
                    return false;

                // can't move onto a light square
                if (_board[move.ToRow, move.ToCol] == Square.EmptyLight)
                    return false;

                if (currentPiece == Square.Red)
                {
                    // can't move backwards
                    if (move.ToRow >= move.FromRow)
                        return false;
                    // can only move to an empty dark square
                    if (GetPieceAt(move.CoordinateTo) != Square.EmptyDark)
                        return false;

                    return true;
                }

                if (currentPiece == Square.White)
                {
                    // can't move backwards
                    if (move.ToRow <= move.FromRow)
                        return false;
                    // can only move to an empty dark square
                    if (GetPieceAt(move.CoordinateTo) != Square.EmptyDark)
                        return false;

                    return true;
                }

                if (currentPiece == Square.WhiteKing || currentPiece == Square.RedKing)
                {
                    // can only move to an empty dark square
                    if (GetPieceAt(move.CoordinateTo) != Square.EmptyDark)
                        return false;

                    return true;
                }

                return false;
            }

            /// <summary>
            /// Initializes the 8x8 board by placing Light and Dark squares in relevant coordinates
            /// </summary>
            private void InitializeBoard()
            {
                // looping through each row
                for (var row = 0; row < Size; row++)
                // looping through each column within the row
                for (var col = 0; col < Size; col++)
                    // if row is an even number
                    if (row % 2 == 0)
                    {
                        // and if column is also an even number
                        if (col % 2 == 0)
                            _board[row, col] = Square.EmptyLight;
                        else if (col % 2 == 1)
                            _board[row, col] = Square.EmptyDark;
                    }
                    // if row is an odd number
                    else if (row % 2 == 1)
                    {
                        // and column is an even number
                        if (col % 2 == 0)
                            _board[row, col] = Square.EmptyDark;
                        else if (col % 2 == 1)
                            _board[row, col] = Square.EmptyLight;
                    }
            }

            /// <summary>
            /// Print out the current state of the board to the console
            /// </summary>
            public void PrintBoard()
            {
                Console.WriteLine();
                for (var row = -1; row < Size; row++)
                {
                    if (row == -1)
                        Console.Write("     ");
                    else
                        Console.Write("X={0}  ", row);

                    for (var col = 0; col < Size; col++)
                        if (row == -1)
                            Console.Write("Y={0} ", col);
                        else
                            switch ((int)_board[row, col])
                            {
                                case 0:
                                    Console.Write(" {0}  ", "-");
                                    break;
                                case 1:
                                    Console.Write(" {0}  ", "-");
                                    break;
                                case 2:
                                    Console.Write(" {0}  ", "R");
                                    break;
                                case 3:
                                    Console.Write(" {0}  ", "W");
                                    break;
                                case 4:
                                    Console.Write(" {0} ", "RK");
                                    break;
                                case 5:
                                    Console.Write(" {0} ", "WK");
                                    break;
                            }

                    Console.WriteLine();
                }

                Console.WriteLine();
            }

            /// <summary>
            /// Populates the board with white and red pieces in relevant squares
            /// </summary>
            private void SetupPieces()
            {
                for (var row = 0; row < Size; row++)
                for (var col = 0; col < Size; col++)
                {
                    // if it's any of the first 3 rows and square is dark
                    if (row < 3 && _board[row, col] == Square.EmptyDark)
                        _board[row, col] = Square.White;
                        
                    // if it's any of the last 3 rows and square is dark
                    if (row > 4 && _board[row, col] == Square.EmptyDark)
                        _board[row, col] = Square.Red;
                }
            }

            /// <summary>
            /// Checks whether provided jump is valid
            /// </summary>
            private bool CanJump(Piece piece, Move move)
            {
                Square currentPlayer = piece.Type;
                Square opponent;
                Square opponentsKing;

                var toRow = move.ToRow;
                var toCol = move.ToCol;
                var colDirection = move.FromCol - move.ToCol;
                var rowDirection = move.FromRow - move.ToRow;

                // set opponent and opponentsKing variables to opposite colour to the currentPlayer's pieces
                if (currentPlayer == Square.Red || currentPlayer == Square.RedKing)
                {
                    opponent = Square.White;
                    opponentsKing = Square.WhiteKing;
                }
                else
                {
                    opponent = Square.Red;
                    opponentsKing = Square.RedKing;
                }

                if (currentPlayer == Square.Red && !piece.HasJustJumped)
                {
                    // checking jumps to the right
                    if (colDirection == -2)
                    {
                        // checking if a jump to the right can be made to the row below
                        if (GetPieceAt(new Point(toRow, toCol)) == Square.EmptyDark &&
                            (GetPieceAt(new Point(toRow + 1, toCol - 1)) == opponent ||
                             GetPieceAt(new Point(toRow + 1, toCol - 1)) == opponentsKing))
                            return true;
                        return false;
                    }
                    // checking jumps to the left
                    if (colDirection == 2)
                    {
                        // checking if a jump to the left can be made to the row below
                        if (GetPieceAt(new Point(toRow, toCol)) == Square.EmptyDark &&
                            (GetPieceAt(new Point(toRow + 1, toCol + 1)) == opponent ||
                            GetPieceAt(new Point(toRow + 1, toCol + 1)) == opponentsKing))
                            return true;
                        return false;
                    }
                }

                if (currentPlayer == Square.White && !piece.HasJustJumped)
                {
                    // checking jumps to the right
                    if (colDirection == -2)
                    {
                        // checking if a jump to the right can be made to the row above
                        if (GetPieceAt(new Point(toRow, toCol)) == Square.EmptyDark &&
                            (GetPieceAt(new Point(toRow - 1, toCol - 1)) == opponent ||
                             GetPieceAt(new Point(toRow - 1, toCol - 1)) == opponentsKing))
                            return true;
                        return false;
                    }
                    // checking jumps to the left
                    if (colDirection == 2)
                    {
                        // checking if a jump to the left can be made to the row above
                        if (GetPieceAt(new Point(toRow, toCol)) == Square.EmptyDark &&
                            (GetPieceAt(new Point(toRow - 1, toCol + 1)) == opponent ||
                             GetPieceAt(new Point(toRow - 1, toCol + 1)) == opponentsKing))
                            return true;
                        return false;
                    }
                }

                // king and regular piece that has just jumped can jump in all directions
                if (currentPlayer == Square.RedKing || currentPlayer == Square.WhiteKing || piece.HasJustJumped)
                {                   
                    // checking jumps to the right
                    if (colDirection == -2)
                    {
                        // checking jumps to the row above
                        if (rowDirection == -2)
                        {
                            if (GetPieceAt(new Point(toRow, toCol)) == Square.EmptyDark &&
                                (GetPieceAt(new Point(toRow - 1, toCol - 1)) == opponent ||
                                 GetPieceAt(new Point(toRow - 1, toCol - 1)) == opponentsKing))
                                return true;
                        }

                        // checking jumps to the row below
                        if (rowDirection == 2)
                        {
                            if (GetPieceAt(new Point(toRow, toCol)) == Square.EmptyDark &&
                                (GetPieceAt(new Point(toRow + 1, toCol - 1)) == opponent ||
                                 GetPieceAt(new Point(toRow + 1, toCol - 1)) == opponentsKing))
                                return true;
                        }

                        return false;
                    }

                    // checking jumps to the left
                    if (colDirection == 2)
                    {
                        // checking jumps to the row above
                        if (rowDirection == -2)
                        {
                            if (GetPieceAt(new Point(toRow, toCol)) == Square.EmptyDark &&
                                (GetPieceAt(new Point(toRow - 1, toCol + 1)) == opponent ||
                                 GetPieceAt(new Point(toRow - 1, toCol + 1)) == opponentsKing))
                            {
                                Console.WriteLine("Kingus you can jump");
                                return true;
                            }
                        }

                        // checking jumps to the row below
                        if (rowDirection == 2)
                        {
                            if (GetPieceAt(new Point(toRow, toCol)) == Square.EmptyDark &&
                                (GetPieceAt(new Point(toRow + 1, toCol + 1)) == opponent ||
                                 GetPieceAt(new Point(toRow + 1, toCol + 1)) == opponentsKing))
                            {
                                Console.WriteLine("Kingus you can jump LOL");
                                return true;
                            }
                        }

                        return false;
                    }
                }

                return false;
            }

            /// <summary>
            /// Performs a jump. It is assumed that this jump is valid.
            /// </summary>
            public void DoJump(Move move, bool isMultiple)
            {
                var toRow = move.ToRow;
                var toCol = move.ToCol;
                var fromRow = move.FromRow;
                var fromCol = move.FromCol;
                var directionCol = move.FromCol - move.ToCol;
                var directionRow = move.FromRow - move.ToRow;
                var becameKing = false;

                // if row to jump to is at either end of the board, a regular piece becomes a king
                if (toRow == 0 || toRow == 7)
                {
                    MakeKing(_board[fromRow, fromCol], toRow, toCol);
                    becameKing = true;
                    Console.WriteLine("KING SET AFTER JUMP");
                }

                // perform a jump using red piece
                if (_board[fromRow, fromCol] == Square.Red)
                {
                    // jumping to the right
                    if (directionCol == -2)
                    {
                        if(becameKing)
                            _board[toRow, toCol] = Square.RedKing;
                        else
                            _board[toRow, toCol] = Square.Red;

                        _board[toRow + 1, toCol - 1] = Square.EmptyDark;
                        _board[fromRow, fromCol] = Square.EmptyDark;
                    }
                    // jumping to the left
                    if (directionCol == 2)
                    {
                        if (becameKing)
                            _board[toRow, toCol] = Square.RedKing;
                        else
                            _board[toRow, toCol] = Square.Red;

                        _board[toRow + 1, toCol + 1] = Square.EmptyDark;
                        _board[fromRow, fromCol] = Square.EmptyDark;
                    }
                }

                // perform a jump using white piece
                if (_board[fromRow, fromCol] == Square.White)
                {
                    // jumping to the right
                    if (directionCol == -2)
                    {
                        if (becameKing)
                            _board[toRow, toCol] = Square.WhiteKing;
                        else
                            _board[toRow, toCol] = Square.White;

                        _board[toRow - 1, toCol - 1] = Square.EmptyDark;
                        _board[fromRow, fromCol] = Square.EmptyDark;
                    }
                    // jumping to the left
                    if (directionCol == 2)
                    {
                        if (becameKing)
                            _board[toRow, toCol] = Square.WhiteKing;
                        else
                            _board[toRow, toCol] = Square.White;

                        _board[toRow - 1, toCol + 1] = Square.EmptyDark;
                        _board[fromRow, fromCol] = Square.EmptyDark;
                    }
                }

                // perform a jump using either RedKing, WhiteKing or a regular piece that is making another jump in a sequence
                if (_board[fromRow, fromCol] == Square.WhiteKing || _board[fromRow, fromCol] == Square.RedKing || isMultiple)
                {
                    // jumping to the row below
                    if (directionRow == 2)
                    {
                        // jumping to the right
                        if (directionCol == -2)
                        {
                            _board[toRow, toCol] = _board[fromRow, fromCol];
                            _board[toRow + 1, toCol - 1] = Square.EmptyDark;
                            _board[fromRow, fromCol] = Square.EmptyDark;
                        }
                        // jumping to the left
                        if (directionCol == 2)
                        {
                            _board[toRow, toCol] = _board[fromRow, fromCol];
                            _board[toRow + 1, toCol + 1] = Square.EmptyDark;
                            _board[fromRow, fromCol] = Square.EmptyDark;
                        }
                    }

                    // jumping to the row above
                    if (directionRow == -2)
                    {
                        // jumping to the right
                        if (directionCol == -2)
                        {
                            _board[toRow, toCol] = _board[fromRow, fromCol];
                            _board[toRow - 1, toCol - 1] = Square.EmptyDark;
                            _board[fromRow, fromCol] = Square.EmptyDark;
                        }
                        //jumping to the left
                        if (directionCol == 2)
                        {
                            _board[toRow, toCol] = _board[fromRow, fromCol];
                            _board[toRow - 1, toCol + 1] = Square.EmptyDark;
                            _board[fromRow, fromCol] = Square.EmptyDark;
                        }
                    }
                }
            }

            /// <summary>
            /// Returns a list of all pieces belonging to a current player that are still on the board
            /// </summary>
            public List<Piece> GetPlayersPieces(Square currentPlayer)
            {
                var currentPlayersPieces = new List<Piece>();

                // set playersKing to either RedKing or WhiteKing
                var playersKing = currentPlayer == Square.Red ? Square.RedKing : Square.WhiteKing;

                // scan the board to identify squares where current player's pieces are
                for (var row = 0; row < Size; row++)
                for (var col = 0; col < Size; col++)
                    // if current row,column coordinate within the board holds current player's piece
                    if (_board[row, col] == currentPlayer)
                        currentPlayersPieces.Add(new Piece(currentPlayer, false, row, col));
                    else if (_board[row, col] == playersKing)
                        currentPlayersPieces.Add(new Piece(playersKing, true, row, col));

                return currentPlayersPieces;
            }

            /// <summary>
            /// Updates the position on the board of the provided piece
            /// </summary>
            public void UpdateBoard(Piece piece, int rowFrom, int colFrom)
            {
                _board[piece.X, piece.Y] = piece.Type;
                _board[rowFrom, colFrom] = Square.EmptyDark;

                if(piece.X == 0 || piece.X == 7)
                    MakeKing(piece.Type, piece.X, piece.Y);
            }

            /// <summary>
            /// Returns a list of jumps that can be performed by pieces belonging to a player that are still on the board
            /// </summary>
            public HashSet<Move> GetLegalJumps(List<Piece> playersPieces)
            {
                var legalJumps = new HashSet<Move>();

                // loop through each piece belonging to current player to check if it can jump from it's current location
                foreach (var piece in playersPieces)
                {
                    var rowFrom = piece.X;
                    var colFrom = piece.Y;

                    // determine whether whether it's Red/White/RedKing/WhiteKing
                    var pieceType = piece.Type;

                    if (pieceType == Square.Red)
                    {
                        // check if jump to the right can be made and add it to the list of legal jumps if so
                        if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom - 2, colFrom + 2)))
                            legalJumps.Add(new Move(rowFrom, colFrom, rowFrom - 2, colFrom + 2));
                        // check if jump to the left can be made and add it to the list of legal jumps if so
                        if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom - 2, colFrom - 2)))
                            legalJumps.Add(new Move(rowFrom, colFrom, rowFrom - 2, colFrom - 2));
                    }

                    if (pieceType == Square.White)
                    {
                        // check if jump to the right can be made and add it to the list of legal jumps if so
                        if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom + 2, colFrom + 2)))
                            legalJumps.Add(new Move(rowFrom, colFrom, rowFrom + 2, colFrom + 2));
                        // check if jump to the left can be made and add it to the list of legal jumps if so
                        if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom + 2, colFrom - 2)))
                            legalJumps.Add(new Move(rowFrom, colFrom, rowFrom + 2, colFrom - 2));
                    }

                    // all 4 possible directions of a jump have to be checked for a king or a regular piece that hust jumped
                    if (pieceType == Square.RedKing || pieceType == Square.WhiteKing || piece.HasJustJumped)
                    {
                        // check if jump to the row below to the right can be made
                        if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom - 2, colFrom + 2)))
                            legalJumps.Add(new Move(rowFrom, colFrom, rowFrom - 2, colFrom + 2));
                        // check if jump to the row below to the left can be made
                        if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom - 2, colFrom - 2)))
                            legalJumps.Add(new Move(rowFrom, colFrom, rowFrom - 2, colFrom - 2));
                        // check if jump to the row above to the right can be made
                        if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom + 2, colFrom + 2)))
                            legalJumps.Add(new Move(rowFrom, colFrom, rowFrom + 2, colFrom + 2));
                        // check if jump to the row above to the left can be made
                        if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom + 2, colFrom - 2)))                       
                            legalJumps.Add(new Move(rowFrom, colFrom, rowFrom + 2, colFrom - 2));                        
                    }
                }

                Console.WriteLine("Legal jumps: ");
                foreach (var move in legalJumps)
                {
                    Console.Write("From: {0}  To: {1}", move.CoordinateFrom, move.CoordinateTo);
                    Console.WriteLine();
                }
                Console.WriteLine();

                return legalJumps;
            }

            /// <summary>
            /// Returns a list of moves that can be performed by pieces belonging to a player that are still on the board
            /// </summary>
            public HashSet<Move> GetLegalMoves(List<Piece> playersPieces)
            {
                var legalMoves = new HashSet<Move>();

                // go through each piece that belongs to current player to check if it can move from it's current location
                foreach (var piece in playersPieces)
                {
                    var rowFrom = piece.X;
                    var colFrom = piece.Y;

                    // determine whether whether it's Red/White/RedKing/WhiteKing
                    var pieceType = piece.Type;

                    // each piece can go in up to 7 directions from where it stands
                    for (var i = -1; i < 7; i++)
                    // checking if it can move to any of the 3 squares directly below itself
                        if (i < 2)
                        {
                            if (IsMovePermitted(pieceType, new Move(rowFrom, colFrom, rowFrom - 1, colFrom + i)))
                                legalMoves.Add(new Move(rowFrom, colFrom, rowFrom - 1, colFrom + i));
                        }
                        // checking if it can move to square directly to the left of itself (this should always fail)
                        else if (i == 2)
                        {
                            if (IsMovePermitted(pieceType, new Move(rowFrom, colFrom, rowFrom, colFrom - 1)))
                                legalMoves.Add(new Move(rowFrom, colFrom, rowFrom, colFrom - 1));
                        }
                        // checking if it can move to square directly to the right of itself (this should always fail)
                        else if (i == 3)
                        {
                            if (IsMovePermitted(pieceType, new Move(rowFrom, colFrom, rowFrom, colFrom + 1)))
                                legalMoves.Add(new Move(rowFrom, colFrom, rowFrom, colFrom + 1));
                        }
                        // checking if it can move to any of the 3 squares directly above itself
                        else if (i > 3)
                        {
                            if (IsMovePermitted(pieceType, new Move(rowFrom, colFrom, rowFrom + 1, colFrom + (i - 5))))
                                legalMoves.Add(new Move(rowFrom, colFrom, rowFrom + 1, colFrom + (i - 5)));
                        }
                }

                Console.WriteLine("Legal moves: ");
                foreach (var move in legalMoves)
                {
                    Console.Write("From: {0},{1}  To: {2},{3}", move.FromRow, move.FromCol, move.ToRow, move.ToCol);
                    Console.WriteLine();
                }
                Console.WriteLine();

                return legalMoves;
            }

            /// <summary>
            /// Returns a type of piece that is currently placed under provided coordinates
            /// </summary>
            private Square GetPieceAt(Point coordinates)
            {
                try
                {
                    return _board[coordinates.X, coordinates.Y];
                }
                catch (IndexOutOfRangeException)
                {
                    return Square.EmptyLight;
                }
                catch (Exception)
                {
                    return Square.EmptyLight;
                }
            }

        }

        /// <summary>
        /// Class to represent a move in a game
        /// </summary>
        private class Move
        {
            private int fromRow;
            private int fromCol;
            private int toRow;
            private int toCol;
            private Point coordinatesFrom;
            private Point coordinatesTo;

            public int FromRow
            {
                get => fromRow;
                set => fromRow = value;
            }

            public int FromCol
            {
                get => fromCol;
                set => fromCol = value;
            }

            public int ToRow
            {
                get => toRow;
                set => toRow = value;
            }

            public int ToCol
            {
                get => toCol;
                set => toCol = value;
            }

            public Point CoordinateFrom
            {
                get => coordinatesFrom;
                set => coordinatesFrom = value;
            }

            public Point CoordinateTo
            {
                get => coordinatesTo;
                set => coordinatesTo = value;
            }

            public Move(int fromRow, int fromCol, int toRow, int toCol)
            {
                this.fromRow = fromRow;
                this.fromCol = fromCol;
                this.toRow = toRow;
                this.toCol = toCol;
                coordinatesFrom = new Point(fromRow, fromCol);
                coordinatesTo = new Point(toRow, toCol);
            }

            protected bool Equals(Move other)
            {
                return fromRow == other.fromRow && fromCol == other.fromCol &&
                       toRow == other.toRow && toCol == other.toCol && coordinatesFrom.Equals(other.coordinatesFrom) &&
                       coordinatesTo.Equals(other.coordinatesTo);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((Move)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = fromRow;
                    hashCode = (hashCode * 397) ^ fromCol;
                    hashCode = (hashCode * 397) ^ toRow;
                    hashCode = (hashCode * 397) ^ toCol;
                    hashCode = (hashCode * 397) ^ coordinatesFrom.GetHashCode();
                    hashCode = (hashCode * 397) ^ coordinatesTo.GetHashCode();
                    return hashCode;
                }
            }
        }

        /// <summary>
        /// Class to represent a state of the game, ie. how pieces are positioned on the board and which player's turn is it
        /// </summary>
        private class State
        {
            public State(Square[,] state, Square currentPlayer)
            {
                BoardState = state;
                CurrentPlayer = currentPlayer;
            }

            public Square[,] BoardState { get; set; }

            public Square CurrentPlayer { get; set; }
        }

        /// <summary>
        /// Class to represent a piece in the game.
        /// </summary>
        private class Piece
        {
/*            private Square type;
            private bool isKing;
            private bool hasJustJumped;

            private int x;
            private int y;*/

            public Piece(Square type, bool isKing, int x, int y)
            {
                Type = type;
                IsKing = isKing;
                X = x;
                Y = y;
            }

            public Square Type { get; set; }

            public int X { get; set; }
            public int Y { get; set; }
            public bool IsKing { get; set; }
            public bool HasJustJumped { get; set; }
        }
    }
}