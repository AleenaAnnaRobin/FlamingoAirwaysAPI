using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlamingoAirwaysAPI.Models
{
    public interface IBookingRepository
    {
        Task<IEnumerable<Booking>> GetByUserIdAsync(string userId);
        Task<Booking> GetByIdAsync(int id);
        Task<Booking> GetByPnrAsync(string pnr);
        Task AddAsync(Booking booking);
        Task CancelAsync(int id); // Cancel entire booking
        Task DeleteTicketsByBookingIdAsync(int bookingId); // New method for deleting tickets
        Task<IEnumerable<Booking>> GetAllBookingsAsync();
        //Task<IEnumerable<Booking>> GetBookingsByUserId(int userId);
        
    }
}