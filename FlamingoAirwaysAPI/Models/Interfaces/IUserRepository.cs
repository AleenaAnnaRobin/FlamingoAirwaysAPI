using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlamingoAirwaysAPI.Models
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(string id);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllAsync(); // Add this line
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);
        Task<bool> HasDuplicateEmailAsync(string email);
        Task<User> GetUserDetails(string userId);
    }
}