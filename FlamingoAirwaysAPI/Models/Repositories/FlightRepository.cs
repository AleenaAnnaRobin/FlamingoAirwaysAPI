using FlamingoAirwaysAPI.Models.Inteface;
using Microsoft.EntityFrameworkCore;
using static FlamingoAirwaysAPI.Models.FlamingoAirwayModel;
using static FlamingoAirwaysAPI.Models.FlamingoAirwaysModel;

namespace FlamingoAirwaysAPI.Models
{
    public class FlightRepository : IFlightRepository
    {
        FlamingoAirwaysDB _context;
        public FlightRepository(FlamingoAirwaysDB context)
        {
            _context = context;
        }
        public async Task AddFlight(FlamingoAirwayModel.Flight flight)
        {
            _context.Flights.Add(flight);
            await _context.SaveChangesAsync();
            //throw new NotImplementedException();
        }

        public async Task<IEnumerable<Flight>> GetAllFlights()
        {
            // Ensure to use ToListAsync() for asynchronous operation
            return await _context.Flights.ToListAsync();
        }

        public async Task<Flight> GetFlightById(int id)
        {
            // Ensure to use FindAsync() for asynchronous operation
            return await _context.Flights.FindAsync(id);
        }
        public async Task RemoveFlight(int id)
        {
            // Find the flight asynchronously
            var flight = await GetFlightById(id);

            _context.Flights.Remove(flight);

            // Save changes asynchronously
            await _context.SaveChangesAsync();
        }
        public async Task UpdateFlight(int id, Flight flight)
        {
            Flight f = _context.Flights.Find(id);
            f.Origin = flight.Origin;
            f.Destination = flight.Destination;
            f.ArrivalDate = flight.ArrivalDate;
            f.DepartureDate = flight.DepartureDate;
            f.Price = flight.Price;
            f.AvailableSeats = flight.AvailableSeats;
            f.TotalNumberOfSeats = flight.TotalNumberOfSeats;
            await _context.SaveChangesAsync();

            //throw new NotImplementedException();
        }

        public async Task<IEnumerable<Flight>> SearchFlightsAsync(string origin, string destination, DateTime departureDate)
        {
            return await _context.Flights
                .Where(f => f.Origin == origin && f.Destination == destination && f.DepartureDate.Date == departureDate.Date)
                .ToListAsync();
        }


    }
}