namespace _9dt.Models
{
    public enum MoveType
    {
        QUIT,
        MOVE
    }

    public abstract class BaseMove
    {
        public BaseMove(MoveType type, string player)
        {
            Type = type;
            Player = player;
        }
        public MoveType Type { get; }
        public string Player { get; }
    }

    public class QuitMove : BaseMove
    {
        public QuitMove(string player) : base(MoveType.QUIT, player) { }
    }

    public class BoardMove : BaseMove
    {
        public BoardMove(string player, int column, int row) : base(MoveType.MOVE, player)
        {
            Column = column;
            Row = row;
        }
        internal int Column { get; }
        internal int Row { get; }
    }
}