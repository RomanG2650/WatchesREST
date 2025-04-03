using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchLibrary;
using WatchLibrary.Repositories;

[ApiController]
[Route("api/[controller]")]
public class WatchController : ControllerBase
{
    private readonly WatchRepository _watches;

    public WatchController(WatchRepository watchRepository)
    {
        _watches = watchRepository;
    }

    // GÆST, BRUGER OG ADMIN – alle må se ure
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]  // Ved succesfuld forespørgsel
    [ProducesResponseType(StatusCodes.Status204NoContent)]  // Hvis ingen ure findes
    public ActionResult<IEnumerable<WatchDTO>> Get()
    {
        var watches = _watches.GetAllAsDTO();
        if (!watches.Any()) return NoContent();
        return Ok(watches);
    }

    // GÆST, BRUGER OG ADMIN – alle må se enkelt ur
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]  // Ved succesfuld forespørgsel
    [ProducesResponseType(StatusCodes.Status404NotFound)]  // Hvis uret ikke findes
    public ActionResult<Watch> GetById(int id)
    {
        var watch = _watches.GetById(id);
        if (watch == null) return NotFound($"Watch with ID {id} was not found.");
        return Ok(watch);
    }

    // GÆST, BRUGER OG ADMIN – alle må søge
    [AllowAnonymous]
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]  // Ved succesfuld søgning
    [ProducesResponseType(StatusCodes.Status204NoContent)]  // Hvis ingen resultater
    public ActionResult<IEnumerable<WatchDTO>> Search(string query)
    {
        var results = _watches.Search(query);
        if (!results.Any()) return NoContent();
        return Ok(results);
    }

    // KUN ADMIN – må oprette ure
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]  // Ved succesfuld oprettelse
    [ProducesResponseType(StatusCodes.Status400BadRequest)]  // Hvis der er fejl i inputdata
    public ActionResult<Watch> Post([FromBody] Watch newWatch)
    {
        if (newWatch == null) return BadRequest("Watch data is required.");
        try
        {
            _watches.Add(newWatch);
            return Ok(newWatch);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // KUN ADMIN – må redigere ure
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]  // Ved succesfuld opdatering
    [ProducesResponseType(StatusCodes.Status400BadRequest)]  // Hvis der er fejl i inputdata
    [ProducesResponseType(StatusCodes.Status404NotFound)]  // Hvis uret ikke findes
    public ActionResult<Watch> Put(int id, [FromBody] Watch updatedWatch)
    {
        if (updatedWatch == null) return BadRequest("Watch data is required.");
        try
        {
            var updated = _watches.GetById(id);
            if (updated == null) return NotFound($"Watch with ID {id} not found.");
            _watches.Update(updatedWatch);
            return Ok(updatedWatch);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // KUN ADMIN – må slette ure
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]  // Ved succesfuld sletning
    [ProducesResponseType(StatusCodes.Status404NotFound)]  // Hvis uret ikke findes
    public ActionResult<Watch> Remove(int id)
    {
        var watch = _watches.GetById(id);
        if (watch == null) return NotFound($"Watch with ID {id} was not found.");
        _watches.Delete(id);
        return Ok(watch);
    }
}
