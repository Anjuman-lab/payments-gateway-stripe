using Microsoft.EntityFrameworkCore;
using Payments.API.Data;
using Payments.API.Domain.Entities;
using Payments.API.Domain.Enums;
using Payments.API.Dtos;
using Stripe;
using System.Globalization;

namespace Payments.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public PaymentService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
            StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
        }

        public async Task<PaymentResponseDto> CreateAsync(CreatePaymentRequest req, CancellationToken ct)
        {
            // 1️⃣ Create Stripe PaymentIntent
            var options = new PaymentIntentCreateOptions
            {
                Amount = req.Amount,
                Currency = req.Currency,
                Description = req.Description,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                },
                Metadata = new Dictionary<string, string>()
            };

            if (!string.IsNullOrWhiteSpace(req.OrderId))
                options.Metadata["order_id"] = req.OrderId;

            if (!req.Capture)
                options.CaptureMethod = "manual";  // Manual auth-only

            var stripeSvc = new PaymentIntentService();
            var intent = await stripeSvc.CreateAsync(options, cancellationToken: ct);

            // 2️⃣ Save to DB
            var entity = new Payment
            {
                OrderId = req.OrderId ?? intent.Id,
                Amount = req.Amount,
                Currency = req.Currency,
                Status = PaymentStatus.Created,
                Provider = "stripe",
                ProviderPaymentId = intent.Id,
                CreatedUtc = DateTime.UtcNow
            };

            _db.Payments.Add(entity);
            await _db.SaveChangesAsync(ct);

            // 3️⃣ Return DTO (ClientSecret -> used by React)
            return new PaymentResponseDto
            {
                Id = entity.Id,
                OrderId = entity.OrderId,
                Amount = entity.Amount,
                Currency = entity.Currency,
                Status = entity.Status,
                Provider = entity.Provider,
                ProviderPaymentId = entity.ProviderPaymentId,
                ClientSecret = intent.ClientSecret,
                CreatedUtc = entity.CreatedUtc
            };
        }

        public async Task<PaymentResponseDto> CaptureAsync(Guid id, CaptureRequest req, CancellationToken ct = default)
        {
            var payment = await _db.Payments.FirstAsync(p => p.Id == id, ct);

            var stripeSvc = new PaymentIntentService();
            var intent = await stripeSvc.CaptureAsync(payment.ProviderPaymentId);

            payment.Status = PaymentStatus.Captured;
            payment.UpdatedUtc = DateTime.UtcNow;
            _db.Payments.Update(payment);

            var ev = new TransactionEvent
            {
                PaymentId = payment.Id,
                Type = "capture",
                Status = "approved",
                RequestJson = $"{{\"amount\":{payment.Amount}}}",
                ResponseJson = $"{{\"stripe_id\":\"{intent.Id}\",\"status\":\"{intent.Status}\"}}"
            };

            _db.TransactionEvents.Add(ev);
            await _db.SaveChangesAsync(ct);

            return ToDto(payment);
        }

        public async Task<PaymentResponseDto> RefundAsync(Guid id, RefundRequest req, CancellationToken ct = default)
        {
            var payment = await _db.Payments.FirstAsync(p => p.Id == id, ct);

            var refundSvc = new RefundService();
            var refund = await refundSvc.CreateAsync(new RefundCreateOptions
            {
                PaymentIntent = payment.ProviderPaymentId,
                Amount = req.Amount
            });

            payment.Status = PaymentStatus.Refunded;
            payment.UpdatedUtc = DateTime.UtcNow;
            _db.Payments.Update(payment);

            var ev = new TransactionEvent
            {
                PaymentId = payment.Id,
                Type = "refund",
                Status = refund.Status ?? "unknown",
                RequestJson = $"{{\"amount\":{req.Amount}}}",
                ResponseJson = $"{{\"refund_id\":\"{refund.Id}\",\"status\":\"{refund.Status}\"}}"
            };

            _db.TransactionEvents.Add(ev);
            await _db.SaveChangesAsync(ct);

            return ToDto(payment);
        }

        public async Task<PaymentResponseDto?> GetAsync(Guid id, CancellationToken ct = default)
        {
            var payment = await _db.Payments.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return payment is null ? null : ToDto(payment);
        }

        public async Task<IReadOnlyList<PaymentResponseDto>> ListAsync(int take = 50, int skip = 0, CancellationToken ct = default)
        {
            return await _db.Payments.AsNoTracking()
                .OrderByDescending(p => p.CreatedUtc)
                .Skip(skip)
                .Take(take)
                .Select(p => ToDto(p))
                .ToListAsync(ct);
        }

        private static PaymentResponseDto ToDto(Payment p) => new()
        {
            Id = p.Id,
            OrderId = p.OrderId,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status,
            Provider = p.Provider,
            ProviderPaymentId = p.ProviderPaymentId,
            CreatedUtc = p.CreatedUtc,
            UpdatedUtc = p.UpdatedUtc
        };
    }
}
