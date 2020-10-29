namespace TicTacToeFunctions.Models
{
    public interface ICustomDocument
    {
        string Id { get; }

        string ETag { get; set; }
    }
}
