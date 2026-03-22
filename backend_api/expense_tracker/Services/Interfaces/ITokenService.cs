using expense_tracker.Models;

namespace expense_tracker.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
