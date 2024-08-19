using static FlamingoAirwaysAPI.Models.FlamingoAirwayModel;

namespace FlamingoAirwaysAPI.Models
{
    public interface ITicketRepository
    {
        Task<Ticket> GetTicketById(int id);
        Task<IEnumerable<Ticket>> GetAllTicket();
        Task AddTicket(Ticket ticket);
        Task UpdateTicket(Ticket ticket);
        Task RemoveTicket(Ticket ticket);
    }
}
