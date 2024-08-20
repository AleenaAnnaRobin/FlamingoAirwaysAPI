using FlamingoAirwaysAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlamingoAirwaysAPI.Models
{
    public class BookingRepository : IBookingRepository
    {
        private readonly FlamingoAirwaysDB _context;

        public BookingRepository(FlamingoAirwaysDB context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Booking>> GetByUserIdAsync(int userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId_FK == userId) // Use UserIdFK instead of UserId
                .ToListAsync();
        }

        public async Task<Booking> GetByIdAsync(int id)
        {
            return await _context.Bookings
                .FindAsync(id);
        }

        public async Task<Booking> GetByPnrAsync(string pnr)
        {
            return await _context.Bookings
                .FirstOrDefaultAsync(b => b.PNR == pnr);
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task CancelAsync(int id)
        {
            var booking = await _context.Bookings
                .FindAsync(id);
            if (booking != null)
            {
                booking.IsCancelled = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteTicketsByBookingIdAsync(int bookingId)
        {
            var tickets = await _context.Tickets
                .Where(t => t.BookingIdF == bookingId)
                .ToListAsync();

            if (tickets.Any())
            {
                _context.Tickets.RemoveRange(tickets);
                await _context.SaveChangesAsync();
            }
        }
    }
}