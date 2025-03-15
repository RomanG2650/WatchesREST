using Microsoft.AspNetCore.Mvc;
using WatchLibrary.Models;
using WatchesREST.Services;

namespace WatchesREST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public ActionResult<IEnumerable<User>> Get() //Debugging
        {
            var users = _userService.GetAllUsers();
            if (!users.Any()) return NoContent();
            return Ok(users);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<User> Register([FromBody] User user)
        {
            try
            {
                var createdUser = _userService.RegisterUser(user);
                return CreatedAtAction(nameof(GetById), new { id = createdUser.Id }, createdUser);
            }
            catch (Exception ex)
            {
                return BadRequest("Fejl ved tilføjelse af bruger: " + ex.Message);
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<User> GetById(int id)
        {
            var user = _userService.GetUserById(id);
            if (user == null) return NotFound($"User with ID {id} was not found.");
            return Ok(user);
        }
    }
}

