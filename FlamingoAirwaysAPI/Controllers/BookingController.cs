using Microsoft.AspNetCore.Mvc;
using FlamingoAirwaysAPI.Models;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

namespace FlamingoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
   // [Authorize(Roles = "User")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IFlightRepository _flightRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly ITicketRepository _ticketRepo;
        private readonly IUserRepository _userRepo;

        public BookingController(IBookingRepository bookingRepo, IFlightRepository flightRepo, IPaymentRepository paymentRepo, ITicketRepository ticketRepo,IUserRepository userRepo)
        {
            _bookingRepo = bookingRepo;
            _flightRepo = flightRepo;
            _paymentRepo = paymentRepo;
            _ticketRepo = ticketRepo;
            _userRepo = userRepo;
        }
        // POST api/Booking
        [HttpPost]
        [Authorize(Roles = "User", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Booking>> PostBooking([FromBody] BookingRequest request) 
        {
            if (request == null ||
                request.Seats <= 0 ||
                request.Payment == null ||
                string.IsNullOrEmpty(request.Payment.CardNumber) ||
                string.IsNullOrEmpty(request.Payment.CardHolderName) ||
                string.IsNullOrEmpty(request.Payment.BankName) ||
                request.Payment.CVV <= 0) // Assuming CVV should be a positive integer
            {
                return BadRequest("Invalid request data.");
            }
            if (!Regex.IsMatch(request.Payment.CVV.ToString(), @"^\d{3}$"))
            {
                return BadRequest("Invalid CVV. It must be 3 digits.");
            }
            if (!Regex.IsMatch(request.Payment.CardNumber, @"^\d{13,16}$"))
            {
                return BadRequest("Invalid card number. It must be 13 to 16 digits.");
            }

            var validBank = new List<string> { "Kotak", "SBI", "Axis", "HDFC" };
            if (!validBank.Contains(request.Payment.BankName))
            {
                return BadRequest("Invalid bank name. Please select a valid bank.");
            }
            var validCard = new List<string> { "credit", "debit" };
            if(!validCard.Contains(request.Payment.PaymentType))
            {
                return BadRequest("Invalid Card type please enter credit or debit");
            }

            var flight = await _flightRepo.GetFlightById(request.FlightId);
            if (flight == null)
            {
                return NotFound("Flight not found.");
            }

            if (request.Seats > flight.AvailableSeats)
            {
                return BadRequest("Not enough seats available.");
            }

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserID");

            //if (!int.TryParse(userIdClaim.Value, out var UserID))
            //{
            //    return Unauthorized("Invalid User ID format in token.");
            //}
            //
            // Create booking
            var booking = new Booking
            {
                FlightIdFK = request.FlightId,
                UserId_FK = HttpContext.User.FindFirst("UserID")?.Value, // Assuming user ID is provided in the request
                BookingDate = DateTime.UtcNow,
                PNR = GeneratePnr(), // Implement GeneratePnr() to create unique PNR
                IsCancelled = false
            };

            await _bookingRepo.AddAsync(booking);

            // Process payment
            var payment = new Payment
            {
                BookingIdFK = booking.BookingId,
                PaymentType = request.Payment.PaymentType,
                CardNumber = request.Payment.CardNumber,
                CardHolderName = request.Payment.CardHolderName,
                PaymentDate = DateTime.UtcNow,
                Amount = flight.Price * request.Seats
            };

            await _paymentRepo.AddAsync(payment);

            for (int i = 0; i < request.Seats; i++)
            {
                var ticket = new Ticket
                {
                    BookingIdF = booking.BookingId,
                    SeatNumber = $"Seat-{flight.AvailableSeats - i}", // Generate seat number as needed
                    PassengerName = request.PassengerNames[i],
                    Price = flight.Price
                };
                await _ticketRepo.AddAsync(ticket);

            }
            //decrease seat from flight 
            flight.AvailableSeats = flight.AvailableSeats - request.Seats;
            await _flightRepo.UpdateAsync(flight);
            return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, booking);
        }

        // GET api/Booking/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            return Ok(booking);
        }

        [HttpGet]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAllBookings()
        {
            var bookings = await _bookingRepo.GetAllBookingsAsync();
            return Ok(bookings);
        }

        [HttpGet("download-ticket/{bookingId}")]
        public async Task<IActionResult> DownloadTicket(int bookingId)
        {
            var booking = await _bookingRepo.GetByIdAsync(bookingId);
            if (booking == null)
            {
                return NotFound("Booking not found.");
            }

            var currUser = await _userRepo.GetByIdAsync(booking.UserId_FK);
            var tickets = await _ticketRepo.GetByBookingIdAsync(bookingId);
            if (!tickets.Any())
            {
                return NotFound("No tickets found for this booking.");
            }

            var flight = await _flightRepo.GetByIdAsync(booking.FlightIdFK);
            if (flight == null)
            {
                return NotFound("Flight not found.");
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream))
                {
                    writer.WriteLine("********************************************************");
                    writer.WriteLine("                     FLAMINGO AIRWAYS                   ");
                    writer.WriteLine("--------------------------------------------------------");
                    writer.WriteLine("                     FLIGHT TICKET                     ");
                    writer.WriteLine("********************************************************");
                    writer.WriteLine();
                    writer.WriteLine($"Passenger: {currUser.FirstName} {currUser.LastName}");
                    writer.WriteLine($"PNR: {booking.PNR}");
                    writer.WriteLine();
                    writer.WriteLine("Flight Information:");
                    writer.WriteLine($"  Origin: {flight.Origin}");
                    writer.WriteLine($"  Destination: {flight.Destination}");
                    writer.WriteLine($"  Departure: {flight.DepartureDate:dddd, MMMM dd, yyyy HH:mm}");
                    writer.WriteLine($"  Arrival: {flight.ArrivalDate:dddd, MMMM dd, yyyy HH:mm}");
                    writer.WriteLine();
                    writer.WriteLine("Ticket Details:");
                    writer.WriteLine("--------------------------------------------------------");

                    foreach (var ticket in tickets)
                    {
                        writer.WriteLine($"  Seat: {ticket.SeatNumber}");
                        writer.WriteLine($"  Passenger: {ticket.PassengerName}");
                        writer.WriteLine($"  Price: ₹{ticket.Price:F2}");
                        writer.WriteLine("--------------------------------------------------------");
                    }

                    writer.WriteLine();
                    writer.WriteLine("Thank you for flying with us!");
                    writer.WriteLine("********************************************************");

                    writer.Flush();
                    memoryStream.Position = 0;

                    var fileName = $"Ticket_{booking.PNR}.txt";
                    return File(memoryStream.ToArray(), "text/plain", fileName);
                }
            }
        }

        [HttpGet("allmybookings")]
        [Authorize(Roles = "User", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<Booking>>> GetAllMy()
        {
            var userIdClaim = HttpContext.User.FindFirst("UserId")?.Value;
            var allMy = await _bookingRepo.GetByUserIdAsync(userIdClaim);
            return Ok(allMy);

            //if (int.TryParse(userIdClaim, out int userId))
            //{
            //    var allMy = await _bookingRepo.GetByUserIdAsync(userId);
            //    return Ok(allMy);
            //}
            //else
            //{
            //    return BadRequest("Invalid UserId");
            //}
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "User", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            var userBooking = await _bookingRepo.GetByIdAsync(id);
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
            if (userBooking.UserId_FK.ToString() != currentUserId)
            {
                return Forbid("You are not authorized to delete this ticket.");
            }

            var flight = await _flightRepo.GetByBookingIdAsync(id); //GetByBookingIdAsync(id);
            // Delete tickets
            var tickets = await _ticketRepo.GetByBookingIdAsync(id);
            if (tickets != null)
            {
                foreach (var ticket in tickets)
                {

                    await _ticketRepo.DeleteAsync(ticket.TicketId);
                    flight.AvailableSeats += 1;
                    await _flightRepo.UpdateAsync(flight);

                }
            }

            // Delete booking (set cancelled to 1)
            await _bookingRepo.CancelAsync(id);

            //update payments
            var payment = await _paymentRepo.GetByBookingIdAsync(id);
            if (payment != null)
            {
                payment.Retainer += payment.Amount * 0.5m;
                payment.Refund += payment.Amount * 0.5m;
                payment.Amount = 0;

                await _paymentRepo.UpdateAsync(payment); // Assuming there's an UpdateAsync method to save changes
            }

            return NoContent();
        }

        // DELETE api/Booking/5
        [HttpDelete("{bookingId}/ticket/{ticketId}")]
        [Authorize(Roles = "User", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> DeleteTicket(int bookingId, int ticketId)
        {
            // Fetch the ticket by its ID and Booking ID
            var ticket = await _ticketRepo.GetByBookingIdAndTicketIdAsync(bookingId, ticketId);
            if (ticket == null)
            {
                return NotFound();
            }
            var userBooking = await _bookingRepo.GetByIdAsync(bookingId);
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
            if (userBooking.UserId_FK.ToString() != currentUserId)
            {
                return Forbid("You are not authorized to delete this ticket.");
            }

            // Calculate 50% refund
            var payment = await _paymentRepo.GetByBookingIdAsync(bookingId);
            payment.Amount -= ticket.Price;
            payment.Retainer += ticket.Price * 0.5m;
            payment.Refund += ticket.Price * 0.5m;


            // Delete the specific ticket
            var flight = await _flightRepo.GetByBookingIdAsync(bookingId); //GetByBookingIdAsync(id);
            await _ticketRepo.DeleteAsync(ticket.TicketId);
            flight.AvailableSeats += 1;
            await _flightRepo.UpdateAsync(flight);

            // Check if there are any remaining tickets for this booking
            var remainingTickets = await _ticketRepo.GetByBookingIdAsync(bookingId);

            if (remainingTickets == null || !remainingTickets.Any())
            {
                // If no tickets remain, cancel the booking
                await _bookingRepo.CancelAsync(bookingId);
            }

            return NoContent();
        }

        // Helper method to generate a unique PNR (for illustration purposes)
        private string GeneratePnr()
        {
            return Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        }

       
    }

    // DTO for booking request
    public class BookingRequest
    {
        public int FlightId { get; set; }
        public int Seats { get; set; }
        public List<string> PassengerNames { get; set; }
        public PaymentRequest Payment { get; set; }
        
       
       
    }

    // DTO for payment details
    public class PaymentRequest
    {
        public string BankName { get; set; }
        public string PaymentType { get; set; } // CreditCard or DebitCard
        public string CardNumber { get; set; }
        public int CVV { get; set; }
        public string CardHolderName { get; set; }
    }
}