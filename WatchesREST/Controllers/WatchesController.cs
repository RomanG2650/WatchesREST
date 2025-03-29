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



	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public ActionResult<IEnumerable<WatchDTO>> Get()
	{
		var watches = _watches.GetAllAsDTO(); // Husk at du skal lave denne metode i repo
		if (!watches.Any()) return NoContent();
		return Ok(watches);
	}

	[HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Watch> GetById(int id)
    {
        var watch = _watches.GetById(id);
        if (watch == null) return NotFound($"Watch with ID {id} was not found.");
        return Ok(watch);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<Watch> Remove(int id)
    {
        var watch = _watches.GetById(id);
        if (watch == null) return NotFound($"Watch with ID {id} was not found.");
        _watches.Delete(id);
        return Ok(watch);
    }

	[HttpGet("search")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status204NoContent)]
	public ActionResult<IEnumerable<WatchDTO>> Search(string query)
	{
		var results = _watches.Search(query);
		if (!results.Any()) return NoContent();
		return Ok(results);
	}

}