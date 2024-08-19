using static FlamingoAirwaysAPI.Models.FlamingoAirwayModel;

namespace FlamingoAirwaysAPI.Models
{
    public interface IFlightRepository
    {
        Task<Flight> GetFlightById(int id);
        Task<IEnumerable<Flight>> GetAllFlights();
        //Task<Flight> GetFlightForUser(string origin);
        Task AddFlight(Flight flight);
        Task UpdateFlight(int id,Flight flight);
        Task RemoveFlight(int id);
        Task<IEnumerable<Flight>> SearchFlightsAsync(string origin, string destination, DateTime departureDate);
       // Task GetByIdAsync(int id);
    }
}
