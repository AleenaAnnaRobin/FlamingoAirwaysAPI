using FlamingoAirwaysAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static FlamingoAirwaysAPI.Models.FlamingoAirwaysModel;
using static FlamingoAirwaysAPI.Models.FlamingoAirwaysModel;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FlamingoAirwaysAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlightController : ControllerBase
    {
        IFlightRepository _repo;
        public FlightController(IFlightRepository repo)
        {
            _repo = repo;
        }
       
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<List<FlamingoAirwaysModel>>> ShowAll()
        {
            var flights = await _repo.GetAllFlights();
            return Ok(flights);
        }

        // GET api/flight/5
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<FlamingoAirwaysModel>> FindFlight(int id)
        {
            var flight = await _repo.GetFlightById(id);
            if (flight == null)
            {
                return NotFound();
            }
            return Ok(flight);
        }

        // GET: api/Flight
        [HttpGet("origin")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Flight>>> GetFlights([FromQuery] string origin, [FromQuery] string destination, [FromQuery] DateTime departureDate)
        {
            var flights = await _repo.SearchFlightsAsync(origin, destination, departureDate);
            return Ok(flights);
        }

        // GET api/Flight/5


        // POST api/<FlamingoAirwaysController>
        [HttpPost]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] Flight value)
        {
            if (value == null)
            {
                return BadRequest();
            }

            await _repo.AddFlight(value);
            return Ok();
        }

        // PUT api/<FlamingoAirwaysController>/5
        
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutFlight(int id, [FromBody] Flight flight)
        {       
            // Check if the flight object is null
            if (flight == null)
            {
                return BadRequest("Flight data cannot be null.");
            }

            // Check if the ID in the URL matches the ID in the flight object
            if (id <= 0)
            {
                return BadRequest("Invalid flight ID.");
            }

            // Fetch the existing flight from the database
            var existingFlight = await _repo.GetByIdAsync(id);
            if (existingFlight == null)
            {
                return NotFound($"Flight with ID {id} not found.");
            }

            // Update the existing flight with new data
            existingFlight.Origin = flight.Origin;
            existingFlight.Destination = flight.Destination;
            existingFlight.DepartureDate = flight.DepartureDate;
            existingFlight.ArrivalDate = flight.ArrivalDate;
            existingFlight.Price = flight.Price;
            existingFlight.TotalNumberOfSeats = flight.TotalNumberOfSeats;
            existingFlight.AvailableSeats = flight.AvailableSeats;

            // Save changes to the database
            try
            {
                await _repo.UpdateAsync(existingFlight);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details and return a server error
                Console.WriteLine($"Error updating flight: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the flight.");
            }

            // Return a no-content response to indicate successful update
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Delete(int id)
        {
            var existingFlight = await _repo.GetFlightById(id);
            if (existingFlight == null)
            {
                return NotFound();
            }
            await _repo.RemoveFlight(id);
            return NoContent();
        }

    }
}
