using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlamingoAirwaysAPI.Models
{
    public class TicketRepository : ITicketRepository
    {
        private readonly FlamingoAirwaysDB _context;

        public TicketRepository(FlamingoAirwaysDB context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Ticket>> GetByBookingIdAsync(int bookingId)
        {
            return await _context.Tickets
                .Where(t => t.BookingIdF == bookingId)
                .ToListAsync();
        }

        public async Task AddAsync(Ticket ticket)
        {
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateAsync(Ticket ticket)
        {
            _context.Tickets.Update(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task<Ticket?> GetByBookingIdAndTicketIdAsync(int bookingId, int ticketId)
        {
            return await _context.Tickets
                .FirstOrDefaultAsync(t => t.BookingIdF == bookingId && t.TicketId == ticketId);
        }
    }
}