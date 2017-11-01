using System;
using System.Collections.Generic;
using System.Drawing;

namespace Checkers
{
    public class Board
    {
        private const int Size = 8;
        private Program.Square[,] _board = new Program.Square[8, 8];

        /// <summary>
        /// Make a deep copy of the board
        /// </summary>
        public Board DeepCopy()
        {
            Board other = (Board)this.MemberwiseClone();
            other._board = _board.Clone() as Program.Square[,];
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
        public Program.Square[,] GetState()
        {
            var newArr = new Program.Square[8, 8];

            Array.Copy(_board, newArr, _board.Length);

            /*                for (var row = 0; row < 8; row++)
                            for (var col = 0; col < 8; col++)
                                newArr[row, col] = _board[row, col];*/

            return newArr;
        }

        /// <summary>
        /// Set the state of the board using the state provided
        /// </summary>
        public void SetState(Program.Square[,] state)
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
            Program.Square pieceType = _board[fromRow, fromCol];

            // if move is more than 2 rows forwards/backwards, then it's illegal
            if (toRow - fromRow > 2 || toRow - fromRow < -2)
                Console.WriteLine("You can't move further than 2 rows");
            else
            {
                // if the piece has reached either first or last row, it becomes a king
                if (toRow == 0 && pieceType == Program.Square.Red || toRow == 7 && pieceType == Program.Square.White)
                {
                    SetKing(pieceType, toRow, toCol);
                    _board[fromRow, fromCol] = Program.Square.EmptyDark;
                }
                else
                {
                    // move piece and replace it with an empty dark square
                    _board[toRow, toCol] = _board[fromRow, fromCol];
                    _board[fromRow, fromCol] = Program.Square.EmptyDark;
                }
            }
        }

        /// <summary>
        /// Makes a regular red or white piece a king once it's reached the opposite end row of the board
        /// </summary>
        private void SetKing(Program.Square currentPlayer, int rowTo, int colTo)
        {
            if (currentPlayer == Program.Square.Red && rowTo == 0)
                _board[rowTo, colTo] = Program.Square.RedKing;

            if (currentPlayer == Program.Square.White && rowTo == 7)
                _board[rowTo, colTo] = Program.Square.WhiteKing;
        }

        /// <summary>
        /// Checks whether provided move is valid
        /// </summary>
        private bool IsMovePermitted(Program.Square currentPiece, Move move)
        {
            // can't move outside the board
            if (move.ToRow < 0 || move.ToRow > 7 || move.ToCol < 0 || move.ToCol > 7)
                return false;

            // can't move onto a light square
            if (_board[move.ToRow, move.ToCol] == Program.Square.EmptyLight)
                return false;

            if (currentPiece == Program.Square.Red)
            {
                // can't move backwards
                if (move.ToRow >= move.FromRow)
                    return false;
                // can only move to an empty dark square
                if (GetPieceAt(move.CoordinateTo) != Program.Square.EmptyDark)
                    return false;

                return true;
            }

            if (currentPiece == Program.Square.White)
            {
                // can't move backwards
                if (move.ToRow <= move.FromRow)
                    return false;
                // can only move to an empty dark square
                if (GetPieceAt(move.CoordinateTo) != Program.Square.EmptyDark)
                    return false;

                return true;
            }

            if (currentPiece == Program.Square.WhiteKing || currentPiece == Program.Square.RedKing)
            {
                // can only move to an empty dark square
                if (GetPieceAt(move.CoordinateTo) != Program.Square.EmptyDark)
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
                            _board[row, col] = Program.Square.EmptyLight;
                        else if (col % 2 == 1)
                            _board[row, col] = Program.Square.EmptyDark;
                    }
                    // if row is an odd number
                    else if (row % 2 == 1)
                    {
                        // and column is an even number
                        if (col % 2 == 0)
                            _board[row, col] = Program.Square.EmptyDark;
                        else if (col % 2 == 1)
                            _board[row, col] = Program.Square.EmptyLight;
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
                    Console.Write("   ");
                else
                    Console.Write(" {0} ", row);

                for (var col = 0; col < Size; col++)
                    if (row == -1)
                        Console.Write(" {0}  ", col);
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
                    if (row < 3 && _board[row, col] == Program.Square.EmptyDark)
                        _board[row, col] = Program.Square.White;

                    // if it's any of the last 3 rows and square is dark
                    if (row > 4 && _board[row, col] == Program.Square.EmptyDark)
                        _board[row, col] = Program.Square.Red;
                }
        }

        /// <summary>
        /// Checks whether provided jump is valid
        /// </summary>
        private bool CanJump(Piece piece, Move move)
        {
            Program.Square currentPlayer = piece.Type;
            Program.Square opponent;
            Program.Square opponentsKing;

            var toRow = move.ToRow;
            var toCol = move.ToCol;
            var colDirection = move.FromCol - move.ToCol;
            var rowDirection = move.FromRow - move.ToRow;

            // set opponent and opponentsKing variables to opposite colour to the currentPlayer's pieces
            if (currentPlayer == Program.Square.Red || currentPlayer == Program.Square.RedKing)
            {
                opponent = Program.Square.White;
                opponentsKing = Program.Square.WhiteKing;
            }
            else
            {
                opponent = Program.Square.Red;
                opponentsKing = Program.Square.RedKing;
            }

            if (currentPlayer == Program.Square.Red && !piece.HasJustJumped)
            {
                // checking jumps to the right
                if (colDirection == -2)
                {
                    // checking if a jump to the right can be made to the row below
                    if (GetPieceAt(new Point(toRow, toCol)) == Program.Square.EmptyDark &&
                        (GetPieceAt(new Point(toRow + 1, toCol - 1)) == opponent ||
                         GetPieceAt(new Point(toRow + 1, toCol - 1)) == opponentsKing))
                        return true;
                    return false;
                }
                // checking jumps to the left
                if (colDirection == 2)
                {
                    // checking if a jump to the left can be made to the row below
                    if (GetPieceAt(new Point(toRow, toCol)) == Program.Square.EmptyDark &&
                        (GetPieceAt(new Point(toRow + 1, toCol + 1)) == opponent ||
                         GetPieceAt(new Point(toRow + 1, toCol + 1)) == opponentsKing))
                        return true;
                    return false;
                }
            }

            if (currentPlayer == Program.Square.White && !piece.HasJustJumped)
            {
                // checking jumps to the right
                if (colDirection == -2)
                {
                    // checking if a jump to the right can be made to the row above
                    if (GetPieceAt(new Point(toRow, toCol)) == Program.Square.EmptyDark &&
                        (GetPieceAt(new Point(toRow - 1, toCol - 1)) == opponent ||
                         GetPieceAt(new Point(toRow - 1, toCol - 1)) == opponentsKing))
                        return true;
                    return false;
                }
                // checking jumps to the left
                if (colDirection == 2)
                {
                    // checking if a jump to the left can be made to the row above
                    if (GetPieceAt(new Point(toRow, toCol)) == Program.Square.EmptyDark &&
                        (GetPieceAt(new Point(toRow - 1, toCol + 1)) == opponent ||
                         GetPieceAt(new Point(toRow - 1, toCol + 1)) == opponentsKing))
                        return true;
                    return false;
                }
            }

            // king and regular piece that has just jumped can jump in all directions
            if (currentPlayer == Program.Square.RedKing || currentPlayer == Program.Square.WhiteKing || piece.HasJustJumped)
            {
                // checking jumps to the right
                if (colDirection == -2)
                {
                    // checking jumps to the row above
                    if (rowDirection == -2)
                    {
                        if (GetPieceAt(new Point(toRow, toCol)) == Program.Square.EmptyDark &&
                            (GetPieceAt(new Point(toRow - 1, toCol - 1)) == opponent ||
                             GetPieceAt(new Point(toRow - 1, toCol - 1)) == opponentsKing))
                            return true;
                    }

                    // checking jumps to the row below
                    if (rowDirection == 2)
                    {
                        if (GetPieceAt(new Point(toRow, toCol)) == Program.Square.EmptyDark &&
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
                        if (GetPieceAt(new Point(toRow, toCol)) == Program.Square.EmptyDark &&
                            (GetPieceAt(new Point(toRow - 1, toCol + 1)) == opponent ||
                             GetPieceAt(new Point(toRow - 1, toCol + 1)) == opponentsKing))
                        {
                            return true;
                        }
                    }

                    // checking jumps to the row below
                    if (rowDirection == 2)
                    {
                        if (GetPieceAt(new Point(toRow, toCol)) == Program.Square.EmptyDark &&
                            (GetPieceAt(new Point(toRow + 1, toCol + 1)) == opponent ||
                             GetPieceAt(new Point(toRow + 1, toCol + 1)) == opponentsKing))
                        {
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
        public void DoJump(Move move, Piece jumpingPiece)
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
                SetKing(_board[fromRow, fromCol], toRow, toCol);
                becameKing = true;
            }

            // perform a jump using red piece
            if (_board[fromRow, fromCol] == Program.Square.Red)
            {
                // jumping to the right
                if (directionCol == -2)
                {
                    if (becameKing)
                        _board[toRow, toCol] = Program.Square.RedKing;
                    else
                        _board[toRow, toCol] = Program.Square.Red;

                    _board[toRow + 1, toCol - 1] = Program.Square.EmptyDark;
                    _board[fromRow, fromCol] = Program.Square.EmptyDark;
                }
                // jumping to the left
                if (directionCol == 2)
                {
                    if (becameKing)
                        _board[toRow, toCol] = Program.Square.RedKing;
                    else
                        _board[toRow, toCol] = Program.Square.Red;

                    _board[toRow + 1, toCol + 1] = Program.Square.EmptyDark;
                    _board[fromRow, fromCol] = Program.Square.EmptyDark;
                }
            }

            // perform a jump using white piece
            if (_board[fromRow, fromCol] == Program.Square.White)
            {
                // jumping to the right
                if (directionCol == -2)
                {
                    if (becameKing)
                        _board[toRow, toCol] = Program.Square.WhiteKing;
                    else
                        _board[toRow, toCol] = Program.Square.White;

                    _board[toRow - 1, toCol - 1] = Program.Square.EmptyDark;
                    _board[fromRow, fromCol] = Program.Square.EmptyDark;
                }
                // jumping to the left
                if (directionCol == 2)
                {
                    if (becameKing)
                        _board[toRow, toCol] = Program.Square.WhiteKing;
                    else
                        _board[toRow, toCol] = Program.Square.White;

                    _board[toRow - 1, toCol + 1] = Program.Square.EmptyDark;
                    _board[fromRow, fromCol] = Program.Square.EmptyDark;
                }
            }

            // perform a jump using either RedKing, WhiteKing or a regular piece that is making another jump in a sequence
            if (_board[fromRow, fromCol] == Program.Square.WhiteKing || _board[fromRow, fromCol] == Program.Square.RedKing ||
                jumpingPiece.HasJustJumped)
            {
                // jumping to the row below
                if (directionRow == 2)
                {
                    // jumping to the right
                    if (directionCol == -2)
                    {
                        _board[toRow, toCol] = _board[fromRow, fromCol];
                        _board[toRow + 1, toCol - 1] = Program.Square.EmptyDark;
                        _board[fromRow, fromCol] = Program.Square.EmptyDark;
                    }
                    // jumping to the left
                    if (directionCol == 2)
                    {
                        _board[toRow, toCol] = _board[fromRow, fromCol];
                        _board[toRow + 1, toCol + 1] = Program.Square.EmptyDark;
                        _board[fromRow, fromCol] = Program.Square.EmptyDark;
                    }
                }

                // jumping to the row above
                if (directionRow == -2)
                {
                    // jumping to the right
                    if (directionCol == -2)
                    {
                        _board[toRow, toCol] = _board[fromRow, fromCol];
                        _board[toRow - 1, toCol - 1] = Program.Square.EmptyDark;
                        _board[fromRow, fromCol] = Program.Square.EmptyDark;
                    }
                    //jumping to the left
                    if (directionCol == 2)
                    {
                        _board[toRow, toCol] = _board[fromRow, fromCol];
                        _board[toRow - 1, toCol + 1] = Program.Square.EmptyDark;
                        _board[fromRow, fromCol] = Program.Square.EmptyDark;
                    }
                }
            }
        }

        public void DoJump(Move move)
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
                SetKing(_board[fromRow, fromCol], toRow, toCol);
                becameKing = true;
            }

            // perform a jump using red piece
            if (_board[fromRow, fromCol] == Program.Square.Red)
            {
                // jumping to the right
                if (directionCol == -2)
                {
                    if (becameKing)
                        _board[toRow, toCol] = Program.Square.RedKing;
                    else
                        _board[toRow, toCol] = Program.Square.Red;

                    _board[toRow + 1, toCol - 1] = Program.Square.EmptyDark;
                    _board[fromRow, fromCol] = Program.Square.EmptyDark;
                }
                // jumping to the left
                if (directionCol == 2)
                {
                    if (becameKing)
                        _board[toRow, toCol] = Program.Square.RedKing;
                    else
                        _board[toRow, toCol] = Program.Square.Red;

                    _board[toRow + 1, toCol + 1] = Program.Square.EmptyDark;
                    _board[fromRow, fromCol] = Program.Square.EmptyDark;
                }
            }

            // perform a jump using white piece
            if (_board[fromRow, fromCol] == Program.Square.White)
            {
                // jumping to the right
                if (directionCol == -2)
                {
                    if (becameKing)
                        _board[toRow, toCol] = Program.Square.WhiteKing;
                    else
                        _board[toRow, toCol] = Program.Square.White;

                    _board[toRow - 1, toCol - 1] = Program.Square.EmptyDark;
                    _board[fromRow, fromCol] = Program.Square.EmptyDark;
                }
                // jumping to the left
                if (directionCol == 2)
                {
                    if (becameKing)
                        _board[toRow, toCol] = Program.Square.WhiteKing;
                    else
                        _board[toRow, toCol] = Program.Square.White;

                    _board[toRow - 1, toCol + 1] = Program.Square.EmptyDark;
                    _board[fromRow, fromCol] = Program.Square.EmptyDark;
                }
            }

            // perform a jump using either RedKing, WhiteKing or a regular piece that is making another jump in a sequence
            if (_board[fromRow, fromCol] == Program.Square.WhiteKing || _board[fromRow, fromCol] == Program.Square.RedKing)
            {
                // jumping to the row below
                if (directionRow == 2)
                {
                    // jumping to the right
                    if (directionCol == -2)
                    {
                        _board[toRow, toCol] = _board[fromRow, fromCol];
                        _board[toRow + 1, toCol - 1] = Program.Square.EmptyDark;
                        _board[fromRow, fromCol] = Program.Square.EmptyDark;
                    }
                    // jumping to the left
                    if (directionCol == 2)
                    {
                        _board[toRow, toCol] = _board[fromRow, fromCol];
                        _board[toRow + 1, toCol + 1] = Program.Square.EmptyDark;
                        _board[fromRow, fromCol] = Program.Square.EmptyDark;
                    }
                }

                // jumping to the row above
                if (directionRow == -2)
                {
                    // jumping to the right
                    if (directionCol == -2)
                    {
                        _board[toRow, toCol] = _board[fromRow, fromCol];
                        _board[toRow - 1, toCol - 1] = Program.Square.EmptyDark;
                        _board[fromRow, fromCol] = Program.Square.EmptyDark;
                    }
                    //jumping to the left
                    if (directionCol == 2)
                    {
                        _board[toRow, toCol] = _board[fromRow, fromCol];
                        _board[toRow - 1, toCol + 1] = Program.Square.EmptyDark;
                        _board[fromRow, fromCol] = Program.Square.EmptyDark;
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of all pieces belonging to a current player that are still on the board
        /// </summary>
        public List<Piece> GetPlayersPieces(Program.Square currentPlayer)
        {
            var currentPlayersPieces = new List<Piece>();

            // set playersKing to either RedKing or WhiteKing
            var playersKing = currentPlayer == Program.Square.Red ? Program.Square.RedKing : Program.Square.WhiteKing;

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
        public void UpdatePosition(Piece piece, int rowFrom, int colFrom)
        {
            _board[piece.X, piece.Y] = piece.Type;
            _board[rowFrom, colFrom] = Program.Square.EmptyDark;

            if (piece.X == 0 || piece.X == 7)
                SetKing(piece.Type, piece.X, piece.Y);
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

                if (pieceType == Program.Square.Red)
                {
                    // check if jump to the right can be made and add it to the list of legal jumps if so
                    if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom - 2, colFrom + 2)))
                        legalJumps.Add(new Move(rowFrom, colFrom, rowFrom - 2, colFrom + 2, true));
                    // check if jump to the left can be made and add it to the list of legal jumps if so
                    if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom - 2, colFrom - 2)))
                        legalJumps.Add(new Move(rowFrom, colFrom, rowFrom - 2, colFrom - 2, true));
                }

                if (pieceType == Program.Square.White)
                {
                    // check if jump to the right can be made and add it to the list of legal jumps if so
                    if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom + 2, colFrom + 2)))
                        legalJumps.Add(new Move(rowFrom, colFrom, rowFrom + 2, colFrom + 2, true));
                    // check if jump to the left can be made and add it to the list of legal jumps if so
                    if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom + 2, colFrom - 2)))
                        legalJumps.Add(new Move(rowFrom, colFrom, rowFrom + 2, colFrom - 2, true));
                }

                // all 4 possible directions of a jump have to be checked for a king or a regular piece that has just jumped
                if (pieceType == Program.Square.RedKing || pieceType == Program.Square.WhiteKing || piece.HasJustJumped)
                {
                    // check if jump to the row below to the right can be made
                    if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom - 2, colFrom + 2)))
                        legalJumps.Add(new Move(rowFrom, colFrom, rowFrom - 2, colFrom + 2, true));
                    // check if jump to the row below to the left can be made
                    if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom - 2, colFrom - 2)))
                        legalJumps.Add(new Move(rowFrom, colFrom, rowFrom - 2, colFrom - 2, true));
                    // check if jump to the row above to the right can be made
                    if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom + 2, colFrom + 2)))
                        legalJumps.Add(new Move(rowFrom, colFrom, rowFrom + 2, colFrom + 2, true));
                    // check if jump to the row above to the left can be made
                    if (CanJump(piece, new Move(rowFrom, colFrom, rowFrom + 2, colFrom - 2)))
                        legalJumps.Add(new Move(rowFrom, colFrom, rowFrom + 2, colFrom - 2, true));
                }
            }

            /*                Console.WriteLine("Legal jumps: ");
                            foreach (var move in legalJumps)
                            {
                                Console.Write("From: {0}  To: {1}", move.CoordinateFrom, move.CoordinateTo);
                                Console.WriteLine();
                            }
                            Console.WriteLine();*/

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

            /*                Console.WriteLine("Legal moves: ");
                            foreach (var move in legalMoves)
                            {
                                Console.Write("From: {0},{1}  To: {2},{3}", move.FromRow, move.FromCol, move.ToRow, move.ToCol);
                                Console.WriteLine();
                            }
                            Console.WriteLine();*/

            return legalMoves;
        }

        /// <summary>
        /// Returns a type of piece that is currently placed under provided coordinates
        /// </summary>
        private Program.Square GetPieceAt(Point coordinates)
        {
            try
            {
                return _board[coordinates.X, coordinates.Y];
            }
            catch (IndexOutOfRangeException)
            {
                return Program.Square.EmptyLight;
            }
            catch (Exception)
            {
                return Program.Square.EmptyLight;
            }
        }
    }
}