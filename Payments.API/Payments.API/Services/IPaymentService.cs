using Payments.API.Dtos;
using Payments.API.Domain.Entities;

namespace Payments.API.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreateAsync(CreatePaymentRequest req, CancellationToken ct);
        Task<PaymentResponseDto?> GetAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<PaymentResponseDto>> ListAsync(int take = 50, int skip = 0, CancellationToken ct = default);
        Task<PaymentResponseDto> CaptureAsync(Guid id, CaptureRequest req, CancellationToken ct = default);
        Task<PaymentResponseDto> RefundAsync(Guid id, RefundRequest req, CancellationToken ct = default);
    }

}
