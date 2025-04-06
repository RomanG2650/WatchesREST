using WatchLibrary.Models;
using WatchLibrary.Repositories;
using System;
using System.Collections.Generic;

namespace WatchesREST.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;

        public UserService(UserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Henter alle brugere (f.eks. til debugging eller admin-brug)
        public IEnumerable<User> GetAllUsers()
        {
            return _userRepository.GetAll();
        }

        // Henter én bruger baseret på ID
        public User GetUserById(int id)
        {
            return _userRepository.GetById(id);
        }

        // Registrerer en ny bruger
        public User RegisterUser(User user)
        {
            // Tjek om e-mailen allerede findes
            if (_userRepository.EmailExists(user.Email))
            {
                throw new ArgumentException("En bruger med denne e-mail eksisterer allerede.");
            }

            Console.WriteLine($"Registrerer bruger: {user.Email}");

            // Valider brugerdata
            Console.WriteLine("Validerer brugerdata...");
            user.Validate();

            // Valider og hash password
            if (string.IsNullOrWhiteSpace(user.Password))
            {
                throw new ArgumentException("Adgangskode er påkrævet");
            }

            Console.WriteLine("Hasher password...");
            user.ValidateSetPassword(user.Password);

            // Sæt standardrolle hvis ikke angivet
            if (user.Role == User.UserRole.User)
            {
                user.Role = User.UserRole.User;
            }

            // Gem brugeren i databasen
            Console.WriteLine("Tilføjer bruger til database...");
            var createdUser = _userRepository.Add(user);

            Console.WriteLine($"Bruger oprettet med ID: {createdUser.Id}");
            return createdUser;
        }
    }
}

