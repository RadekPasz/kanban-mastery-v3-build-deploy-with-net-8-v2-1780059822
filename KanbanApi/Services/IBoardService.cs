using KanbanApi.Models;

namespace KanbanApi.Services;

// Manages boards scoped to the calling user's membership.
// Every method takes the userId so the service can enforce that the
// user is a member of the board before reading or modifying it.
public interface IBoardService
{
    // All boards the user is a member of.
    Task<IReadOnlyList<Board>> GetBoardsForUserAsync(string userId);

    // A single board (including its columns and cards), or null if it
    // doesn't exist or the user isn't a member.
    Task<Board?> GetBoardAsync(int boardId, string userId);

    // Creates a board and adds the creator as its first member with the
    // "Owner" role.
    Task<Board> CreateBoardAsync(string name, string userId);

    // Renames a board. Returns false if the board doesn't exist or the
    // user isn't a member.
    Task<bool> RenameBoardAsync(int boardId, string name, string userId);

    // Deletes a board. Returns false unless the user is an "Owner".
    Task<bool> DeleteBoardAsync(int boardId, string userId);
}
