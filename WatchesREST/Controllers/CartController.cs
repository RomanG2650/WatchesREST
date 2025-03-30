using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using WatchLibrary.Models;

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
        if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
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
        if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Administratorer kan ikke tilføje varer til kurven.");
        }

        try
        {
            _cartService.AddToCart(item);
            return StatusCode(StatusCodes.Status201Created, _cartService.GetCart());
        }
        catch (System.Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Fjerner en vare fra kurven for gæster og brugere. Administratorer kan ikke fjerne varer.
    [HttpDelete("remove/{watchId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RemoveFromCart(int watchId)
    {
        if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Administratorer kan ikke fjerne varer fra kurven.");
        }

        var success = _cartService.RemoveFromCart(watchId);
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
        if (User.Identity.IsAuthenticated && User.IsInRole("Admin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Administratorer kan ikke rydde kurven.");
        }

        _cartService.ClearCart();
        return Ok("Kurven er nu tom.");
    }
}
