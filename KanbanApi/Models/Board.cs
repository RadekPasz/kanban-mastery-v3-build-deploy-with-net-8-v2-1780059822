namespace KanbanApi.Models;

public class Board
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // A Board has many Columns (one-to-many).
    public ICollection<Column> Columns { get; set; } = new List<Column>();

    // A Board has many Members through the BoardMember join table.
    public ICollection<BoardMember> Members { get; set; } = new List<BoardMember>();
}