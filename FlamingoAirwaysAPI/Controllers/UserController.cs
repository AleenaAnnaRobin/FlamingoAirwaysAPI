using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Add this line
using FlamingoAirwaysAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace FlamingoAirwaysAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationCotroller : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IBookingRepository _bookingrepo;
       // private readonly IUserRepository _userrepo;

        public RegistrationCotroller(IUserRepository repo)
        {
            _repo = repo;
        }

        // GET: api/User
        [HttpGet]
        [Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _repo.GetAllAsync();
            return Ok(users);
        }

        // GET: api/User/5
        //[HttpGet("{id}")]
        //[Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public async Task<ActionResult<User>> GetUser(int id)
        //{
        //    var user = await _repo.GetByIdAsync(id);

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(user);
        //}
        // GET: api/User/email/example@example.com

        [HttpGet("GetMyDetails")]
        public async Task<ActionResult<User>> GetUser()
        {
            var UserIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
            var MyDetails = await _repo.GetUserDetails(UserIdClaim);
            return Ok(MyDetails);

            
        }

        [HttpGet("email/{email}")]
        [Authorize(Roles = "User", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<User>> GetUserByEmail(string email)
        {
            var user = await _repo.GetByEmailAsync(email);

            if (user == null)
            {
                return NotFound();
            }
            var userBooking = await _repo.GetByEmailAsync(email);
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;
            if (userBooking.UserId.ToString() != currentUserId)
            {
                return Forbid("You are not authorized to get this ticket.");
            }
            return Ok(user);
        }

        // POST: api/User
        // POST: api/User
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<User>> PostUser([FromBody] UserReg reg)
        {
            var user =new  User();

            if (reg == null)
            {
                return BadRequest();
            }
            bool hasDuplicates = await _repo.HasDuplicateEmailAsync(reg.Email);
            if (hasDuplicates)
            {
                return NotFound($"Repetition not permitted");
            }
            user.UserId=GenerateUserid();
            user.FirstName = reg.FirstName;
            user.LastName = reg.LastName;
            user.Email = reg.Email;
            user.Password = reg.Password;
            user.PhoneNo=reg.PhoneNo;
            user.Role = reg.Role;
            // Hash the password before storing
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

            await _repo.AddAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, user);
        }

        // PUT: api/User/5

        [HttpPut("update")]
        public async Task<IActionResult> PutUser([FromBody] UserUpdate user)
        {


            var UserIdClaims = HttpContext.User.FindFirst("UserId")?.Value;
            bool x = int.TryParse(UserIdClaims, out int userIdClaim);

            var UserRoleClaim = HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;


            var existingUser = await _repo.GetByIdAsync(UserIdClaims);
            if (existingUser == null)
            {
                return NotFound($"User not found.");
            }

            existingUser.UserId = UserIdClaims;
            existingUser.Role = UserRoleClaim;

            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Email = user.Email;
            existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            existingUser.PhoneNo = user.PhoneNo;



            try
            {
                await _repo.UpdateAsync(existingUser);
            }
            catch (DbUpdateException ex)
            {
                // Log the exception details and return a server error
                Console.WriteLine($"Error updating user: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the user.");
            }

            return NoContent();
        }




        private string GenerateUserid()
        {
            return Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
        }


        // DELETE: api/User/5
        //[HttpDelete("{id}")]
        //[Authorize(Roles = "Admin", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //public async Task<IActionResult> DeleteUser(string id)
        //{
        //    var user = await _repo.GetByIdAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    await _repo.DeleteAsync(id);
        //    return NoContent();
        //}

        public class UserReg
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string PhoneNo { get; set; }
            public string Role { get; set; }

        }
        public class UserEdit
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string PhoneNo { get; set; }
            

        }
        public class UserUpdate
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string PhoneNo { get; set; }
        }

    }
}