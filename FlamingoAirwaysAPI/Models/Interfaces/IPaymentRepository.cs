using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlamingoAirwaysAPI.Models
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllAsync();
        Task<Payment> GetByIdAsync(int id);
        Task AddAsync(Payment payment);
        Task UpdateAsync(Payment payment);
        Task <Payment> GetByBookingIdAsync(int bookingId);
    }
}