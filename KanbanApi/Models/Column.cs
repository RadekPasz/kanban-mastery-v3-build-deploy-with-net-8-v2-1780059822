namespace KanbanApi.Models;

public class Column
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;   // e.g. "To Do", "In Progress"
    public int Order { get; set; }                       // position of the column on the board

    // Foreign key + navigation back to the parent Board.
    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    // A Column has many Cards (one-to-many).
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}