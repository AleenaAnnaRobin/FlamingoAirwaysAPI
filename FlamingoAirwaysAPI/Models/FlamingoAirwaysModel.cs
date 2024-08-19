using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FlamingoAirwaysAPI.Models
{
    public class FlamingoAirwayModel
    {
        public class User
        {
            [Key]
            [Required]
            public int UserId { get; set; }

            [Required]
            [StringLength(50)]
            [MinLength(2)]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50)]
            [MinLength(2)]
            public string LastName { get; set; }

            [Required]
            [DataType(DataType.EmailAddress)]
            [StringLength(100)]
            public string Email { get; set; }

            [Required]
            [StringLength(64)] // Assuming a fixed length for the hash
            public string Password { get; set; }

            public string PhoneNo { get; set; }

            [Required]
            [StringLength(20)]
            public string Role { get; set; } // Admin or Customer

            //public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        }

        public class Flight
        {
            [Key]
            [Required]
            public int FlightId { get; set; }

            [Required]
            [StringLength(100)]
            public string Origin { get; set; }

            [Required]
            [StringLength(100)]
            public string Destination { get; set; }

            [Required]
            [DataType(DataType.DateTime)]
            public DateTime DepartureDate { get; set; }

            [Required]
            [DataType(DataType.DateTime)]
            public DateTime ArrivalDate { get; set; }

            [Required]
            [DataType(DataType.Currency)]
            [Column(TypeName = "decimal(18, 2)")] // Adjust based on your needs
            public decimal Price { get; set; }

            [Required]
            [Range(0, int.MaxValue)]
            public int TotalNumberOfSeats { get; set; }

            [Required]
            [Range(0, int.MaxValue)]
            public int AvailableSeats { get; set; }

            //public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        }

        public class Booking
        {
            [Key]
            [Required]
            public int BookingId { get; set; }

            [Required]
            public int FlightId { get; set; }

            [Required]
            public int UserId { get; set; }

            [Required]
            [DataType(DataType.DateTime)]
            public DateTime BookingDate { get; set; }

            [Required]
            [StringLength(20)]
            public string PNR { get; set; }

            [Required]
            public bool IsCancelled { get; set; }

            //[ForeignKey("FlightId")]
            //public Flight Flight { get; set; }

            //[ForeignKey("UserId")]
            //public User User { get; set; }

            //public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        }

        public class Ticket
        {
            [Key]
            [Required]
            public int TicketId { get; set; }

            [Required]
            public int BookingId { get; set; }

            [Required]
            [StringLength(5)]
            public string SeatNumber { get; set; }

            [Required]
            [Column(TypeName = "decimal(18,2)")]
            public decimal Price { get; set; }

            //[ForeignKey("BookingId")]
            //public Booking Booking { get; set; }
        }

        public class Payment
        {
            [Key]
            [Required]
            public int PaymentId { get; set; }

            [Required]
            public int BookingId { get; set; }

            [Required]
            [StringLength(20)]
            public string PaymentType { get; set; } // CreditCard or DebitCard

            [Required]
            [StringLength(16, MinimumLength = 13)]
            [DataType(DataType.CreditCard)]
            public string CardNumber { get; set; }
            [Required]
            [StringLength(16, MinimumLength = 5)]
            public string CardHolderName { get; set; }

            [Required]
            [DataType(DataType.DateTime)]
            public DateTime PaymentDate { get; set; }

            [Required]
            [DataType(DataType.Currency)]
            [Column(TypeName = "decimal(18, 2)")] // Adjust based on your needs
            public decimal Amount { get; set; }

            //[ForeignKey("BookingId")]
            //public Booking Booking { get; set; }
        }
    }
}