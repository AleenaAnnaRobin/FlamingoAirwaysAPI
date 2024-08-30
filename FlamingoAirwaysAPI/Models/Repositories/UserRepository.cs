using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
//using FlamingoAirwaysAPI.Models.Interfaces;
//using FlamingoAirwaysAPI.Models.FlamingoAirwaysDbContext;
using FlamingoAirwaysAPI.Models;

namespace FlamingoAirwaysAPI.Models
{
    public class UserRepository : IUserRepository
    {
        private readonly FlamingoAirwaysDB _context;

        public UserRepository(FlamingoAirwaysDB context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<IEnumerable<User>> GetAllAsync() // Implement this method
        {
            return await _context.Users.ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> HasDuplicateEmailAsync(string email)
        {
            // Count the number of users with the given email
            var count = await _context.Users
                .Where(u => u.Email == email)
                .CountAsync();

            // If count is greater than 1, it means there are duplicates
            return count > 1;
        }

        public Task<User> GetByIdAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<User> GetUserDetails(string userId)
        {
            //throw new NotImplementedException();
            return await _context.Users.Where(b => b.UserId == userId).FirstOrDefaultAsync();
        }
    }
}