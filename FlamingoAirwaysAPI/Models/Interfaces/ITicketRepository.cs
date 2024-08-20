//using static FlamingoAirwaysAPI.Models.FlamingoAirwayModel;

namespace FlamingoAirwaysAPI.Models
{
    public interface ITicketRepository
    {
        
        Task<IEnumerable<Ticket>> GetByBookingIdAsync(int bookingId);
        Task DeleteAsync(int ticketId);
        Task AddAsync(Ticket ticket);
         Task UpdateAsync(Ticket ticket);
        Task<Ticket> GetByBookingIdAndTicketIdAsync(int bookingId, int ticketId);
    }
}
