using Microsoft.AspNetCore.Mvc;
using WatchLibrary.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;


[Route("api/cart")]
[ApiController]
public class CartController : ControllerBase
{
    private readonly CartService _cartService;

    public CartController(CartService cartService)
    {
        _cartService = cartService;
    }

    // Henter kurven for den aktuelle bruger. Administratorer har ikke adgang til kurven.
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetCart()
    {
        if (User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Administratorer har ikke adgang til kurven.");
        }

        var cart = _cartService.GetCart();
        return Ok(cart);
    }

    // Tilføjer en vare til kurven for gæster og brugere. Administratorer kan ikke tilføje varer.
    [HttpPost("add")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult AddToCart([FromBody] CartItem item)
    {
        if (User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Administratorer kan ikke tilføje varer til kurven.");
        }

        // Hvis UserId ikke er angivet eller er 0, og brugeren er logget ind, kan vi sætte det manuelt
        if (item.UserId <= 0 && User?.Identity?.IsAuthenticated == true)
        {
            // Hvis brugeren er logget ind, kan vi sætte en standardværdi for UserId
            item.UserId = 1; // Eksempel: Brug en fast værdi eller hent fra en anden kilde
        }

        try
        {
            _cartService.AddToCart(item);  // Tilføjer produktet til kurven
            return StatusCode(StatusCodes.Status201Created, _cartService.GetCart());  // Returnerer den opdaterede kurv
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);  // Returnerer fejlmeddelelse, hvis noget går galt
        }
    }

    // Opdaterer mængden af en vare i kurven for gæster og brugere. Administratorer kan ikke opdatere mængden.
    [HttpPut("update/{watchId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult UpdateQuantity(int watchId, [FromQuery] int newQuantity, [FromQuery] decimal price)
    {
        if (User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Administratorer kan ikke opdatere mængder i kurven.");
        }

        try
        {
            _cartService.UpdateQuantity(watchId, newQuantity, price);  // Opdaterer mængde og pris på produktet
            return Ok(_cartService.GetCart());  // Returnerer den opdaterede kurv
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);  // Returnerer fejlmeddelelse, hvis noget går galt
        }
    }

    // Fjerner en vare fra kurven for gæster og brugere. Administratorer kan ikke fjerne varer.
    [HttpDelete("remove/{watchId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RemoveFromCart(int watchId)
    {
        if (User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Administratorer kan ikke fjerne varer fra kurven.");
        }

        // Hent UserId fra den aktuelle bruger (dette er et eksempel, justér efter din løsning)
        int userId = 1; // Erstat med den faktiske UserId. Dette skal hentes fra den autentificerede bruger.

        var success = _cartService.RemoveFromCart(watchId, userId);  // ⚠️ Brug userId her!
        if (success)
        {
            return Ok(_cartService.GetCart());
        }

        return NotFound("Vare ikke fundet i kurven.");
    }

    // Rydder kurven for gæster og brugere. Administratorer kan ikke rydde kurven.
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult ClearCart()
    {
        if (User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Administratorer kan ikke rydde kurven.");
        }

        _cartService.ClearCart();  // Tømmer kurven
        return Ok("Kurven er nu tom.");  // Returnerer besked om, at kurven er blevet ryddet
    }
}
