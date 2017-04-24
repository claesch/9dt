namespace _9dt.Models
{
    public class MakeMoveResponse
    {
        public MakeMoveResponse(string gameId, int moveNumber)
        {
            Move = $"{gameId}/moves/{moveNumber}";
        }
        public string Move { get; }
    }
}