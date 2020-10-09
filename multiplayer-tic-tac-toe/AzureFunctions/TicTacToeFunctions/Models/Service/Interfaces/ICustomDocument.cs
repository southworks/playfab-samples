namespace TicTacToeFunctions.Models.Service.Interfaces
{
    public interface ICustomDocument
    {
        string id { get; }

        string _etag { get; set; }
    }
}