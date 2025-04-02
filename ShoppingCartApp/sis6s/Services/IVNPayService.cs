using Microsoft.AspNetCore.Http;
using sis6s.Models;

namespace sis6s.Services
{
    public interface IVNPayService
    {
        string CreatePaymentUrl(Order order, string returnUrl);
        VNPayResponse ProcessPaymentResponse(IQueryCollection query);
    }
} 