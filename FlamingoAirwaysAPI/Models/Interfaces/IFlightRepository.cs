using static FlamingoAirwaysAPI.Models.FlamingoAirwaysModel;

namespace FlamingoAirwaysAPI.Models
{
    public interface IFlightRepository
    {
        Task<Flight> GetFlightById(int id);
        Task<IEnumerable<Flight>> GetAllFlights();
        //Task<Flight> GetFlightForUser(string origin);
        Task AddFlight(Flight flight);
        Task UpdateFlight(int id, Flight flight);
        Task UpdateFlightnew(Flight flight);
        Task RemoveFlight(int id);
        Task<IEnumerable<Flight>> SearchFlightsAsync(string origin, string destination, DateTime departureDate);
        Task <Flight>GetByBookingIdAsync(int bookingId);
        Task UpdateAsync(Flight flight);
        Task DeleteAsync(int id);
        Task<Flight> GetByIdAsync(int id);
        // Task GetByIdAsync(int id);
    }
}