namespace KanbanApi.Models;

public class Card
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Order { get; set; }   // position within its column

    // Foreign key + navigation back to the parent Column.
    public int ColumnId { get; set; }
    public Column Column { get; set; } = null!;
}