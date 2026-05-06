namespace Core_API.Web.Controllers
{
    //[ApiController]
    //[Route("api/[controller]")]
    //[Authorize]
    //public class PaymentController(
    //    IPaymentService paymentService,
    //    ILogger<PaymentController> logger) : BaseApiController
    //{
    //    private readonly IPaymentService _paymentService = paymentService;
    //    private readonly ILogger<PaymentController> _logger = logger;

    //    /// <summary>
    //    /// Get payment by ID
    //    /// </summary>
    //    [HttpGet("{id}")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //    public async Task<IActionResult> GetPaymentById(int id)
    //    {
    //        var result = await _paymentService.GetPaymentByIdAsync(id);
    //        if (!result.IsSuccess)
    //        {
    //            return NotFound(new ProblemDetails
    //            {
    //                Title = "Payment Not Found",
    //                Detail = result.ErrorMessage,
    //                Status = StatusCodes.Status404NotFound
    //            });
    //        }

    //        return Ok(result.Data);
    //    }

    //    /// <summary>
    //    /// Get paged list of payments
    //    /// </summary>
    //    [HttpGet]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //    public async Task<IActionResult> GetPagedPayments([FromQuery] PaymentFilterDto filter)
    //    {
    //        if (!filter.IsValid())
    //        {
    //            return BadRequest(new ProblemDetails
    //            {
    //                Title = "Invalid Filter Parameters",
    //                Detail = "Page number and page size must be greater than 0, and page size cannot exceed 100.",
    //                Status = StatusCodes.Status400BadRequest
    //            });
    //        }

    //        var result = await _paymentService.GetPagedPaymentsAsync(filter, CurrentContext);
    //        if (!result.IsSuccess)
    //        {
    //            return BadRequest(new ProblemDetails
    //            {
    //                Title = "Payment Retrieval Failed",
    //                Detail = result.ErrorMessage,
    //                Status = StatusCodes.Status400BadRequest
    //            });
    //        }

    //        return Ok(result.Data);
    //    }

    //    /// <summary>
    //    /// Create a new payment
    //    /// </summary>
    //    [HttpPost]
    //    [Authorize(Roles = "Admin,User")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        var result = await _paymentService.CreatePaymentAsync(dto, CurrentContext);
    //        if (!result.IsSuccess)
    //        {
    //            return BadRequest(new ProblemDetails
    //            {
    //                Title = "Payment Creation Failed",
    //                Detail = result.ErrorMessage,
    //                Status = StatusCodes.Status400BadRequest
    //            });
    //        }

    //        return Ok(result.Data);
    //    }

    //    /// <summary>
    //    /// Process a payment (with gateway integration)
    //    /// </summary>
    //    [HttpPost("process")]
    //    [Authorize(Roles = "Admin,User,Customer")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        var result = await _paymentService.ProcessPaymentAsync(dto, CurrentContext);
    //        if (!result.IsSuccess)
    //        {
    //            return BadRequest(new ProblemDetails
    //            {
    //                Title = "Payment Processing Failed",
    //                Detail = result.ErrorMessage,
    //                Status = StatusCodes.Status400BadRequest
    //            });
    //        }

    //        return Ok(result.Data);
    //    }

    //    /// <summary>
    //    /// Update a payment
    //    /// </summary>
    //    [HttpPut]
    //    [Authorize(Roles = "Admin")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //    public async Task<IActionResult> UpdatePayment([FromBody] UpdatePaymentDto dto)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        var result = await _paymentService.UpdatePaymentAsync(dto, CurrentContext);
    //        if (!result.IsSuccess)
    //        {
    //            return BadRequest(new ProblemDetails
    //            {
    //                Title = "Payment Update Failed",
    //                Detail = result.ErrorMessage,
    //                Status = StatusCodes.Status400BadRequest
    //            });
    //        }

    //        return Ok(result.Data);
    //    }

    //    /// <summary>
    //    /// Delete a payment
    //    /// </summary>
    //    [HttpDelete("{id}")]
    //    [Authorize(Roles = "Admin")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status404NotFound)]
    //    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //    public async Task<IActionResult> DeletePayment(int id)
    //    {
    //        var result = await _paymentService.DeletePaymentAsync(id, CurrentContext);
    //        if (!result.IsSuccess)
    //        {
    //            return NotFound(new ProblemDetails
    //            {
    //                Title = "Payment Not Found",
    //                Detail = result.ErrorMessage,
    //                Status = StatusCodes.Status404NotFound
    //            });
    //        }

    //        return Ok();
    //    }

    //    /// <summary>
    //    /// Refund a payment
    //    /// </summary>
    //    [HttpPost("refund")]
    //    [Authorize(Roles = "Admin")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    //    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //    public async Task<IActionResult> RefundPayment([FromBody] RefundPaymentDto dto)
    //    {
    //        if (!ModelState.IsValid)
    //        {
    //            return BadRequest(ModelState);
    //        }

    //        var result = await _paymentService.RefundPaymentAsync(dto, CurrentContext);
    //        if (!result.IsSuccess)
    //        {
    //            return BadRequest(new ProblemDetails
    //            {
    //                Title = "Refund Failed",
    //                Detail = result.ErrorMessage,
    //                Status = StatusCodes.Status400BadRequest
    //            });
    //        }

    //        return Ok(result.Data);
    //    }

    //    /// <summary>
    //    /// Get payment statistics
    //    /// </summary>
    //    [HttpGet("stats")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //    public async Task<IActionResult> GetPaymentStats()
    //    {
    //        var result = await _paymentService.GetPaymentStatsAsync(CurrentContext);
    //        if (!result.IsSuccess)
    //        {
    //            return BadRequest(new ProblemDetails
    //            {
    //                Title = "Stats Retrieval Failed",
    //                Detail = result.ErrorMessage,
    //                Status = StatusCodes.Status400BadRequest
    //            });
    //        }

    //        return Ok(result.Data);
    //    }

    //    /// <summary>
    //    /// Get customer outstanding balance
    //    /// </summary>
    //    [HttpGet("customer/{customerId}/outstanding")]
    //    [ProducesResponseType(StatusCodes.Status200OK)]
    //    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //    public async Task<IActionResult> GetCustomerOutstandingBalance(int customerId)
    //    {
    //        var result = await _paymentService.GetCustomerOutstandingBalanceAsync(customerId, CurrentContext);
    //        if (!result.IsSuccess)
    //        {
    //            return BadRequest(new ProblemDetails
    //            {
    //                Title = "Balance Retrieval Failed",
    //                Detail = result.ErrorMessage,
    //                Status = StatusCodes.Status400BadRequest
    //            });
    //        }

    //        return Ok(new { OutstandingBalance = result.Data });
    //    }
    //}
}