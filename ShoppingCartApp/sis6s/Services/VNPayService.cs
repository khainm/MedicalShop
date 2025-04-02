using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using sis6s.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http;

namespace sis6s.Services
{
    public class VNPayService : IVNPayService
    {
        private readonly VNPayConfig _config;

        public VNPayService(IOptions<VNPayConfig> config)
        {
            _config = config.Value;
        }

        public string CreatePaymentUrl(Order order, string returnUrl)
        {
            var vnp_Url = _config.BaseUrl;
            var vnp_HashSecret = _config.HashSecret;
            var vnp_TmnCode = _config.TmnCode;

            var vnp_TxnRef = DateTime.Now.Ticks.ToString();
            var vnp_OrderInfo = "Thanh toan don hang";
            var vnp_OrderType = "billpayment";
            var vnp_Amount = ((long)(order.TotalAmount * 100)).ToString();
            var vnp_Locale = _config.Locale;
            var vnp_IpAddr = "127.0.0.1";
            var vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");

            var vnp_Version = "2.1.0";
            var vnp_Command = _config.Command;
            var vnp_CurrCode = _config.CurrCode;

            var vnp_Title = "Thanh toan don hang";
            var vnp_OrderId = order.Id.ToString();

            var vnp_Params = new Dictionary<string, string>
            {
                { "vnp_Version", vnp_Version },
                { "vnp_Command", vnp_Command },
                { "vnp_TmnCode", vnp_TmnCode },
                { "vnp_Locale", vnp_Locale },
                { "vnp_CurrCode", vnp_CurrCode },
                { "vnp_TxnRef", vnp_TxnRef },
                { "vnp_OrderInfo", vnp_OrderInfo },
                { "vnp_OrderType", vnp_OrderType },
                { "vnp_Amount", vnp_Amount },
                { "vnp_ReturnUrl", returnUrl },
                { "vnp_IpAddr", vnp_IpAddr },
                { "vnp_CreateDate", vnp_CreateDate },
                { "vnp_Title", vnp_Title },
                { "vnp_OrderId", vnp_OrderId }
            };

            var sortedParams = vnp_Params.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            var queryString = new StringBuilder();
            foreach (var param in sortedParams)
            {
                queryString.Append($"{param.Key}={param.Value}&");
            }

            var hashData = queryString.ToString();
            var signature = CreateHash(hashData);
            queryString.Append($"vnp_SecureHash={signature}");

            return $"{vnp_Url}?{queryString}";
        }

        public VNPayResponse ProcessPaymentResponse(IQueryCollection query)
        {
            var vnp_OrderId = query["vnp_OrderId"].ToString() ?? "";
            var vnp_TransactionNo = query["vnp_TransactionNo"].ToString() ?? "";
            var vnp_ResponseCode = query["vnp_ResponseCode"].ToString() ?? "";
            var vnp_SecureHash = query["vnp_SecureHash"].ToString() ?? "";

            // Tạo response object
            var response = new VNPayResponse
            {
                OrderId = vnp_OrderId,
                TransactionId = vnp_TransactionNo,
                ResponseCode = vnp_ResponseCode
            };

            return response;
        }

        private string CreateHash(string data)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(_config.HashSecret);
            var dataBytes = Encoding.UTF8.GetBytes(data);

            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(dataBytes);
                foreach (var b in hashBytes)
                {
                    hash.Append(b.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }

    public class VNPayResponse
    {
        public string OrderId { get; set; }
        public string TransactionId { get; set; }
        public string ResponseCode { get; set; }
        public string Message { get; set; }
    }

    public class VNPayConfig
    {
        public string BaseUrl { get; set; }
        public string TmnCode { get; set; }
        public string HashSecret { get; set; }
        public string Command { get; set; }
        public string CurrCode { get; set; }
        public string Locale { get; set; }
    }
} 