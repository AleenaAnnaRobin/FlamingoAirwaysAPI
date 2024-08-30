using static FlamingoAirwaysAPI.Models.FlamingoAirwaysModel;

namespace FlamingoAirwaysAPI.Models.Repositories
{

    public interface ILogin
    {
         User ValidateUser(string uname, string pwd);
        //bool ValidateUser(string email, string password);
        string GetUserId(string email);
    }
    public class LoginRepo : ILogin
    {

        FlamingoAirwaysDB _context;
        public LoginRepo(FlamingoAirwaysDB context)
        {
            _context = context;
        }

        public string GetUserId(string email)
        {
            var user=_context.Users.FirstOrDefault(u => u.Email == email);
            return user.UserId;
        }


        public User? ValidateUser(string email, string password)
        {
            User user = _context.Users.SingleOrDefault(u => u.Email == email );

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return user;
            }

            return null;  // Return null if user is not found or password doesn't match
        }

        
    }

}