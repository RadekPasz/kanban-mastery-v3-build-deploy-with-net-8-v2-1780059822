namespace KanbanApi.Models;

//Join table for the many-to-many between users and boards.
public class BoardMember
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public int BoardId { get; set; }
    public Board Board { get; set; } = null!;

    public string Role { get; set; } = "Member";
}
