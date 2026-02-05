using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payments.API.Data;
using Payments.API.Domain.Entities;
using Payments.API.Domain.Enums;
using Stripe;

namespace Payments.API.Controllers
{
    [ApiController]
    [Route("webhooks/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;

        public StripeWebhookController(IConfiguration config, AppDbContext db)
        {
            _config = config;
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();
            var signatureHeader = Request.Headers["Stripe-Signature"];
            var secret = _config["Stripe:WebhookSecret"];

            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, secret);
            }
            catch (StripeException e)
            {
                Console.WriteLine($"⚠️ Webhook signature verification failed: {e.Message}");
                return BadRequest();
            }

            Console.WriteLine($"🔔 Received event: {stripeEvent.Type}");

            switch (stripeEvent.Type)
            {
                case "payment_intent.succeeded":
                    {
                        var intent = stripeEvent.Data.Object as PaymentIntent;
                        if (intent != null)
                        {
                            var payment = await _db.Payments
                                .FirstOrDefaultAsync(p => p.ProviderPaymentId == intent.Id);

                            if (payment != null)
                            {
                                payment.Status = PaymentStatus.Captured;
                                payment.UpdatedUtc = DateTime.UtcNow;
                                await _db.SaveChangesAsync();
                                Console.WriteLine($"✅ Payment succeeded: {intent.Id}");
                            }
                        }
                        break;
                    }

                case "payment_intent.payment_failed":
                    {
                        var intent = stripeEvent.Data.Object as PaymentIntent;
                        if (intent != null)
                        {
                            var payment = await _db.Payments
                                .FirstOrDefaultAsync(p => p.ProviderPaymentId == intent.Id);

                            if (payment != null)
                            {
                                payment.Status = PaymentStatus.Failed;
                                payment.UpdatedUtc = DateTime.UtcNow;
                                await _db.SaveChangesAsync();
                                Console.WriteLine($"❌ Payment failed: {intent.Id}");
                            }
                        }
                        break;
                    }

                default:
                    Console.WriteLine($"Unhandled event type: {stripeEvent.Type}");
                    break;
            }


            return Ok();
        }
    }
}
