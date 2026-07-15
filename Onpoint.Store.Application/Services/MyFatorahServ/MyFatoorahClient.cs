using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Onpoint.Store.Application.Common;
using Onpoint.Store.Application.DTOs.Payment;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Onpoint.Store.Application.Services.PaymentServ
{
    public class MyFatoorahClient : IMyFatoorahClient
    {
        private readonly HttpClient _httpClient;
        private readonly MyFatoorahOptions _options;

        public MyFatoorahClient(HttpClient httpClient, IOptions<MyFatoorahOptions> options)
        {
            _options = options.Value;

            // 🔍 أسطر تشخيصية: احذفها بعد حل المشكلة
            System.Console.WriteLine($"[DEBUG] BaseUrl: {_options.BaseUrl}");
            System.Console.WriteLine($"[DEBUG] ApiKey is Null or Empty: {string.IsNullOrWhiteSpace(_options.ApiKey)}");
            if (!string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                System.Console.WriteLine($"[DEBUG] ApiKey Length: {_options.ApiKey.Length}");
                System.Console.WriteLine($"[DEBUG] ApiKey First 5 Chars: {_options.ApiKey.Substring(0, Math.Min(5, _options.ApiKey.Length))}");
            }

            // حماية: ارمِ خطأ واضح فوراً إذا كان المفتاح مفقوداً
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("MyFatoorah API Key is missing or empty in appsettings.json!");
            }

            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);

            // استخدام .Trim() لإزالة أي مسافات زائدة تم نسخها بالخطأ
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.ApiKey.Trim());
        }

        public async Task<List<PaymentMethodDto>> InitiatePaymentAsync(decimal amount, string currencyIso, CancellationToken ct = default)
        {
            var payload = new { InvoiceAmount = amount, CurrencyIso = currencyIso };
            var response = await PostAsync("/v2/InitiatePayment", payload, ct);

            var methods = response["Data"]?["PaymentMethods"];
            if (methods == null) return new List<PaymentMethodDto>();

            return methods.Select(m => new PaymentMethodDto
            {
                PaymentMethodId = (int)m["PaymentMethodId"]!,
                PaymentMethodEn = (string)m["PaymentMethodEn"]! ?? string.Empty,
                PaymentMethodAr = (string)m["PaymentMethodAr"]! ?? string.Empty,
                ImageUrl = (string)m["ImageUrl"]! ?? string.Empty,
                IsDirectPayment = (bool)(m["IsDirectPayment"] ?? false),
                ServiceCharge = (double)(m["ServiceCharge"] ?? 0),
                TotalAmount = (double)(m["TotalAmount"] ?? 0),
                PaymentCurrencyIso = (string)m["PaymentCurrencyIso"]! ?? string.Empty
            }).ToList();
        }

        public async Task<ExecutePaymentResultDto> ExecutePaymentAsync(ExecutePaymentRequestModel request, CancellationToken ct = default)
        {
            var response = await PostAsync("/v2/ExecutePayment", request, ct);

            var data = response["Data"];
            return new ExecutePaymentResultDto
            {
                InvoiceId = data?["InvoiceId"]?.ToString() ?? string.Empty,
                PaymentUrl = data?["PaymentURL"]?.ToString() ?? string.Empty
            };
        }

        public async Task<MyFatoorahPaymentStatusResult> GetPaymentStatusAsync(string key, string keyType, CancellationToken ct = default)
        {
            var payload = new { Key = key, KeyType = keyType };
            var response = await PostAsync("/v2/GetPaymentStatus", payload, ct);

            var data = response["Data"];
            return new MyFatoorahPaymentStatusResult
            {
                InvoiceId = data?["InvoiceId"]?.ToString() ?? string.Empty,
                InvoiceStatus = data?["InvoiceStatus"]?.ToString() ?? string.Empty, // "Paid", "Pending", "Failed", "Expired"
                InvoiceValue = data?["InvoiceValue"]?.Value<decimal>() ?? 0,
                CustomerReference = data?["CustomerReference"]?.ToString(), // هنستخدمها لتخزين OrderId
                InvoiceTransactions = data?["InvoiceTransactions"]?.FirstOrDefault() is JToken tx
                    ? new MyFatoorahTransactionDetail
                    {
                        TransactionId = tx["TransactionId"]?.ToString() ?? string.Empty,
                        PaymentId = tx["PaymentId"]?.ToString() ?? string.Empty,
                        TransactionStatus = tx["TransactionStatus"]?.ToString() ?? string.Empty,
                        PaidCurrencyValue = tx["PaidCurrencyValue"]?.Value<decimal>() ?? 0,
                        Error = tx["Error"]?.ToString()
                    }
                    : null
            };
        }

        private async Task<JObject> PostAsync(string endpoint, object payload, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content, ct);
            var responseBody = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
                throw new InvalidOperationException($"MyFatoorah API error ({response.StatusCode}): {responseBody}");

            var parsed = JObject.Parse(responseBody);

            if (parsed["IsSuccess"]?.Value<bool>() != true)
            {
                var errorMsg = parsed["Message"]?.ToString() ?? "Unknown MyFatoorah error";
                throw new InvalidOperationException($"MyFatoorah request failed: {errorMsg}");
            }

            return parsed;
        }
    }





}