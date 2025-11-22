namespace Core_API.Web.Controllers;

//[Route("api/[controller]")]
//[ApiController]
//[Authorize]
//public class CartController(ICartService cartService, ILogger<CartController> logger) : ControllerBase
//{
//    private readonly ICartService _cartService = cartService;
//    private readonly ILogger<CartController> _logger = logger;

//    /// <summary>
//    /// Retrieves the shopping cart for the authenticated user.
//    /// </summary>
//    /// <returns>A list of cart items if successful, or an error response.</returns>
//    /// <response code="200">Returns the shopping cart successfully.</response>
//    /// <response code="401">If the user is not authenticated.</response>
//    /// <response code="404">If the cart is not found for the user (optional, depending on service implementation).</response>
//    /// <response code="500">If an unhandled error occurs on the server.</response>
//    [HttpGet]
//    [ProducesResponseType(typeof(CartViewModel), StatusCodes.Status200OK)]
//    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
//    [ProducesResponseType(StatusCodes.Status404NotFound)]
//    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
//    public async Task<IActionResult> GetCart()
//    {
//        string userId = null;
//        try
//        {
//            userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

//            if (string.IsNullOrEmpty(userId))
//            {
//                _logger.LogWarning("GetCart: User ID not found in claims. Request is unauthorized.");
//                return Unauthorized("User not authenticated.");
//            }

//            _logger.LogInformation("GetCart: Attempting to retrieve cart for user ID: {UserId}", userId);
//            var cart = await _cartService.GetCartAsync(userId);

//            if (cart == null)
//            {
//                _logger.LogInformation("GetCart: Cart not found for user ID: {UserId}", userId);
//                return NotFound($"Cart not found for user.");
//            }

//            _logger.LogInformation("GetCart: Successfully retrieved cart for user ID: {UserId}", userId);
//            return Ok(cart);
//        }
//        catch (UnauthorizedAccessException ex)
//        {
//            _logger.LogError(ex, "GetCart: Unauthorized access attempt for user ID: {UserId}", userId);
//            return Unauthorized("You are not authorized to perform this action.");
//        }
//        catch (InvalidOperationException ex)
//        {
//            // Specific exception for business logic errors, e.g., if a cart operation is invalid
//            _logger.LogWarning(ex, "GetCart: Invalid operation for user ID: {UserId}. Message: {Message}", userId, ex.Message);
//            return BadRequest($"Invalid operation: {ex.Message}");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "GetCart: An unhandled error occurred while retrieving cart for user ID: {UserId}", userId);
//            return StatusCode(StatusCodes.Status500InternalServerError, "An internal server error occurred.");
//        }
//    }

//    [HttpPost]
//    public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
//    {
//        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        if (string.IsNullOrEmpty(userId))
//        {
//            return Unauthorized("User not authenticated.");
//        }

//        try
//        {
//            var cart = await _cartService.AddToCartAsync(userId, request);
//            return Ok(cart);
//        }
//        catch (System.Exception ex)
//        {
//            return BadRequest(ex.Message);
//        }
//    }

//    [HttpPut("{cartItemId}")]
//    public async Task<IActionResult> UpdateCartItemCount(int cartItemId, [FromBody] int count)
//    {
//        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        if (string.IsNullOrEmpty(userId))
//        {
//            return Unauthorized("User not authenticated.");
//        }

//        try
//        {
//            var cart = await _cartService.UpdateCartItemCountAsync(userId, cartItemId, count);
//            return Ok(cart);
//        }
//        catch (System.Exception ex)
//        {
//            return BadRequest(ex.Message);
//        }
//    }

//    [HttpDelete("{cartItemId}")]
//    public async Task<IActionResult> RemoveFromCart(int cartItemId)
//    {
//        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//        if (string.IsNullOrEmpty(userId))
//        {
//            return Unauthorized("User not authenticated.");
//        }

//        try
//        {
//            var cart = await _cartService.RemoveFromCartAsync(userId, cartItemId);
//            return Ok(cart);
//        }
//        catch (System.Exception ex)
//        {
//            return BadRequest(ex.Message);
//        }
//    }
//}
