using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WatchLibrary.Models;
using WatchLibrary.Repositories;

namespace WatchesREST.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class OrderController : ControllerBase
	{
		private readonly OrderRepository _orderRepository;

		public OrderController(OrderRepository orderRepository)
		{
			_orderRepository = orderRepository;
		}

		//[Authorize] // Kun logged-in brugere kan købe
		[HttpPost("checkout")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult Checkout([FromBody] OrderDTO order)
		{
			if (order == null || order.Items == null || !order.Items.Any())
				return BadRequest("Order must contain at least one item.");

			try
			{
				_orderRepository.SaveOrder(order);
				return Ok("Order placed successfully!");
			}
			catch (Exception ex)
			{
				return BadRequest($"Order failed: {ex.Message}");
			}
		}
	}
}
