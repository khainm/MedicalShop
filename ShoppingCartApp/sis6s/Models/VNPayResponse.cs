// using System.Text;

// namespace sis6s.Models
// {
//     public class VNPayResponse
//     {
//         private readonly SortedList<string, string> _responseData = new SortedList<string, string>();

//         public bool IsValidSignature { get; set; }
//         public int OrderId { get; set; }
//         public string TransactionId { get; set; }
//         public string ResponseCode { get; set; }
//         public string ResponseMessage { get; set; }
//         public DateTime PaymentDate { get; set; }

//         public void AddResponseData(string key, string value)
//         {
//             if (!string.IsNullOrEmpty(value))
//             {
//                 _responseData.Add(key, value);
//             }
//         }

//         public string GetResponseData(string key = null)
//         {
//             if (string.IsNullOrEmpty(key))
//             {
//                 var queryString = new StringBuilder();
//                 foreach (var (k, v) in _responseData)
//                 {
//                     if (!string.IsNullOrEmpty(v) && k != "vnp_SecureHash")
//                     {
//                         queryString.Append($"{k}={v}&");
//                     }
//                 }
//                 return queryString.ToString().TrimEnd('&');
//             }

//             return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
//         }
//     }
// } 