using WatchLibrary.Models;
using WatchLibrary.Repositories;

namespace WatchesREST.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _userRepository.GetAll();
        }

        public User GetUserById(int id)
        {
            return _userRepository.GetById(id);
        }

        public User RegisterUser(User user)
        {
            if (string.IsNullOrWhiteSpace(user.Password))
                throw new ArgumentException("Password is required");

            user.ValidateSetPassword(user.Password); // Hasher password
            var createdUser = _userRepository.Add(user);

            user.Password = null; // Rens password
            return createdUser;
        }
    }
}
