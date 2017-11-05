using System;

namespace Checkers
{
    /// <summary>
    /// This class represents a state of the game, ie. how pieces are positioned on the board at this particular 
    /// stage of the game and whether it's Red or White player's turn
    /// </summary>
    public class State
    {
        public State(Program.Square[,] state, Program.Square currentPlayer)
        {
            BoardState = state;
            CurrentPlayer = currentPlayer;
        }

        /// <summary>
        /// Makes a deep copy of the state to use it during undo/redo operations
        /// </summary>
        public State DeepCopy()
        {
            State other = (State)this.MemberwiseClone();
            other.BoardState = BoardState.Clone() as Program.Square[,];
            other.CurrentPlayer = (Program.Square)((int)this.CurrentPlayer);

            return other;
        }

        public Program.Square[,] BoardState { get; set; }

        public Program.Square CurrentPlayer { get; set; }

        /// <summary>
        /// Prints the state of the board, ie. how pieces are positioned at this stage of the game
        /// </summary>
        public void PrintState()
        {
            Console.WriteLine();
            for (var row = -1; row < 8; row++)
            {
                if (row == -1)
                    Console.Write("   ");
                else
                    Console.Write(" {0} ", row);

                for (var col = 0; col < 8; col++)
                    if (row == -1)
                        Console.Write(" {0}  ", col);
                    else
                        switch ((int)BoardState[row, col])
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
    }
}