

namespace FlamingoAirwaysAPI.Models
{
    public class UserRepository : IUserRepository

    {
        public Task AddUser(FlamingoAirwayModel.User user)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FlamingoAirwayModel.User>> GetAllUsers()
        {
            throw new NotImplementedException();
        }

        public Task<FlamingoAirwayModel.User> GetUserByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public Task<FlamingoAirwayModel.User> GetUserById(int id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUser(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateUser(FlamingoAirwayModel.User user)
        {
            throw new NotImplementedException();
        }
    }
}
