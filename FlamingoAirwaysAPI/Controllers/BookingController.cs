﻿using Microsoft.AspNetCore.Mvc;
using FlamingoAirwaysAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace FlamingoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IFlightRepository _flightRepo;
        private readonly IPaymentRepository _paymentRepo;
        private readonly ITicketRepository _ticketRepo;

        public BookingController(IBookingRepository bookingRepo, IFlightRepository flightRepo, IPaymentRepository paymentRepo, ITicketRepository ticketRepo)
        {
            _bookingRepo = bookingRepo;
            _flightRepo = flightRepo;
            _paymentRepo = paymentRepo;
            _ticketRepo = ticketRepo;
        }

        // POST api/Booking
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking([FromBody] BookingRequest request)
        {

            if (!Regex.IsMatch(request.CVV.ToString(), @"^\d{3}$"))
            {
                return BadRequest("Invalid CVV. It must be 3 digits.");
            }
            if (!Regex.IsMatch(request.Payment.CardNumber, @"^\d{12}$"))
            {
                return BadRequest("Invalid card number. It must be 12 digits.");
            }

            var validBank = new List<string> { "Kotak", "SBI", "Axis", "HDFC" };
            if (!validBank.Contains(request.BankName))
            {
                return BadRequest("Invalid bank name. Please select a valid bank.");
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

            // Create booking
            var booking = new Booking
            {
                FlightIdFK = request.FlightId,
                UserId_FK = request.UserId, // Assuming user ID is provided in the request
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

            // Create tickets
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
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            return Ok(booking);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var booking = await _bookingRepo.GetByIdAsync(id);
            if (booking == null)
            {
                return NotFound();
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
                payment.Amount = 0;

                await _paymentRepo.UpdateAsync(payment); // Assuming there's an UpdateAsync method to save changes
            }

            return NoContent();
        }



        // DELETE api/Booking/5
        [HttpDelete("{bookingId}/ticket/{ticketId}")]
        public async Task<IActionResult> DeleteTicket(int bookingId, int ticketId)
        {
            // Fetch the ticket by its ID and Booking ID
            var ticket = await _ticketRepo.GetByBookingIdAndTicketIdAsync(bookingId, ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            // Calculate 50% refund
            var payment = await _paymentRepo.GetByBookingIdAsync(bookingId);
            payment.Amount -= ticket.Price;
            payment.Retainer += ticket.Price * 0.5m;



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
        public int UserId { get; set; }
        public int Seats { get; set; }
       
        public List<string> PassengerNames { get; set; }
        public PaymentRequest Payment { get; set; }
        public string BankName { get;  set; }
        public int CVV { get; set; }
       
    }

    // DTO for payment details
    public class PaymentRequest
    {
        public string PaymentType { get; set; } // CreditCard or DebitCard
        public string CardNumber { get; set; }
        public string CardHolderName { get; set; }
    }
}