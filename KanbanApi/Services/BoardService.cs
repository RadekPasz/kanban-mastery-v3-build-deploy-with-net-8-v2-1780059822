using KanbanApi.Data;
using KanbanApi.Models;
using Microsoft.EntityFrameworkCore;

namespace KanbanApi.Services;

public class BoardService : IBoardService
{
    private readonly ApplicationDbContext _db;

    public BoardService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Board>> GetBoardsForUserAsync(string userId)
    {
        return await _db.Boards
            .Where(b => b.Members.Any(m => m.UserId == userId))
            .OrderBy(b => b.Name)
            .ToListAsync();
    }

    public async Task<Board?> GetBoardAsync(int boardId, string userId)
    {
        return await _db.Boards
            .Where(b => b.Id == boardId && b.Members.Any(m => m.UserId == userId))
            .Include(b => b.Columns.OrderBy(c => c.Order))
                .ThenInclude(c => c.Cards.OrderBy(card => card.Order))
            .FirstOrDefaultAsync();
    }

    public async Task<Board> CreateBoardAsync(string name, string userId)
    {
        var board = new Board
        {
            Name = name,
            Members =
            {
                new BoardMember { UserId = userId, Role = "Owner" }
            }
        };

        _db.Boards.Add(board);
        await _db.SaveChangesAsync();
        return board;
    }

    public async Task<bool> RenameBoardAsync(int boardId, string name, string userId)
    {
        var board = await _db.Boards
            .Where(b => b.Id == boardId && b.Members.Any(m => m.UserId == userId))
            .FirstOrDefaultAsync();

        if (board is null)
            return false;

        board.Name = name;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBoardAsync(int boardId, string userId)
    {
        var board = await _db.Boards
            .Where(b => b.Id == boardId
                && b.Members.Any(m => m.UserId == userId && m.Role == "Owner"))
            .FirstOrDefaultAsync();

        if (board is null)
            return false;

        _db.Boards.Remove(board);
        await _db.SaveChangesAsync();
        return true;
    }
}
