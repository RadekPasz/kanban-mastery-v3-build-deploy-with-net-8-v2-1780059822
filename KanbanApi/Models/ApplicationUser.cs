using Microsoft.AspNetCore.Identity;

namespace KanbanApi.Models;

// Inherits all the built-in identity fields (Id, UserName, Email,
// PasswordHash, etc.) from IdentityUser.
public class ApplicationUser : IdentityUser
{
    //The boards this user belongs to, via the BoardMember join table.
    public ICollection<BoardMember> BoardMemberships { get; set; } = new List<BoardMember>();
}
