using Microsoft.AspNetCore.Mvc;
using WatchLibrary.Models;
using WatchLibrary.Repositories;

namespace WatchesREST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository _users;

        public UsersController(UserRepository userRepository)
        {
            _users = userRepository;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<User> Register([FromBody] User user)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(user.Password))
                    return BadRequest("Password is required");

                user.SetPassword(user.Password); // Hasher password
                var createdUser = _users.Add(user); // Tilføjer bruger via repository

                user.Password = null; // Rens password efter brug
                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<User> GetById(int id)
        {
            var user = _users.GetById(id);
            if (user == null) return NotFound($"User with ID {id} was not found.");
            return Ok(user);
        }
    }
}
