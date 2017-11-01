namespace Checkers
{
    /// <summary>
    /// Class to represent a piece in the game.
    /// </summary>
    public class Piece
    {
        public Program.Square Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public bool IsKing { get; set; }
        public bool HasJustJumped { get; set; }

        public Piece(Program.Square type, bool isKing, int x, int y)
        {
            Type = type;
            IsKing = isKing;
            X = x;
            Y = y;
        }
    }
}