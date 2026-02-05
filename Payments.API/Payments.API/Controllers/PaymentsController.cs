using Microsoft.AspNetCore.Mvc;
using Payments.API.Dtos;
using Payments.API.Services;

namespace Payments.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _svc;

        public PaymentsController(IPaymentService svc) => _svc = svc;

        [HttpPost]
        public async Task<ActionResult<PaymentResponseDto>> Create([FromBody] CreatePaymentRequest req, CancellationToken ct)
        {
            var dto = await _svc.CreateAsync(req, ct);
            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PaymentResponseDto>> Get(Guid id, CancellationToken ct)
        {
            var dto = await _svc.GetAsync(id, ct);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentResponseDto>>> List([FromQuery] int take = 50, [FromQuery] int skip = 0, CancellationToken ct = default)
            => Ok(await _svc.ListAsync(take, skip, ct));

        [HttpPost("{id:guid}/capture")]
        public async Task<ActionResult<PaymentResponseDto>> Capture(Guid id, [FromBody] CaptureRequest req, CancellationToken ct)
            => Ok(await _svc.CaptureAsync(id, req, ct));

        [HttpPost("{id:guid}/refund")]
        public async Task<ActionResult<PaymentResponseDto>> Refund(Guid id, [FromBody] RefundRequest req, CancellationToken ct)
            => Ok(await _svc.RefundAsync(id, req, ct));
    }
}
