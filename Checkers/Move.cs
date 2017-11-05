using System.Drawing;

namespace Checkers
{
    /// <summary>
    /// This class represents a move in a game of checkers. It can represent both regular move or a jump and 
    /// it doesn't validate the move as to whether it is legal or not.
    /// </summary>
    public class Move
    {
        private Point coordinatesFrom;
        private Point coordinatesTo;

        public bool IsJump { get; set; }

        public int FromRow { get; set; }

        public int FromCol { get; set; }

        public int ToRow { get; set; }

        public int ToCol { get; set; }

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
            FromRow = fromRow;
            FromCol = fromCol;
            ToRow = toRow;
            ToCol = toCol;
            coordinatesFrom = new Point(fromRow, fromCol);
            coordinatesTo = new Point(toRow, toCol);

            if (ToRow - fromRow == 2 || toRow - FromRow == -2)
                IsJump = true;
        }

        public Move(int fromRow, int fromCol, int toRow, int toCol, bool isJump)
        {
            FromRow = fromRow;
            FromCol = fromCol;
            ToRow = toRow;
            ToCol = toCol;
            IsJump = isJump;
            coordinatesFrom = new Point(fromRow, fromCol);
            coordinatesTo = new Point(toRow, toCol);

            if (ToRow - fromRow == 2 || toRow - FromRow == -2)
                IsJump = true;
        }

        protected bool Equals(Move other)
        {
            return FromRow == other.FromRow && FromCol == other.FromCol &&
                   ToRow == other.ToRow && ToCol == other.ToCol && coordinatesFrom.Equals(other.coordinatesFrom) &&
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
                var hashCode = FromRow;
                hashCode = (hashCode * 397) ^ FromCol;
                hashCode = (hashCode * 397) ^ ToRow;
                hashCode = (hashCode * 397) ^ ToCol;
                hashCode = (hashCode * 397) ^ coordinatesFrom.GetHashCode();
                hashCode = (hashCode * 397) ^ coordinatesTo.GetHashCode();
                return hashCode;
            }
        }
    }
}