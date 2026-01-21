// File: Infrastructure/Services/PaystackService.cs (C#)

using Microsoft.Extensions.Options;
using SmartTask.Application.Dto.Paystack;
using SmartTask.Application.Interfaces;
using SmartTask.Domain.Entities;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

public class PaystackService : IPaystackService
{
    private readonly HttpClient _client;
    private readonly PaystackSettings _settings;
    public PaystackService(HttpClient client, IOptions<PaystackSettings> settings)
    {
        _client = client;
        _settings = settings.Value;
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.SecretKey}");
        _client.BaseAddress = new Uri(_settings.BaseUrl);
    }

    public async Task<(bool Success, string AccountName, string Message)> ResolveAccountAsync(string accountNumber, string bankCode)
    {
            var requestUrl = $"bank/resolve?account_number={accountNumber}&bank_code={bankCode}";

        var response = await _client.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadFromJsonAsync<PaystackResolveResponse>();
            return (false, "Verification Failed", errorContent?.message ?? "External API Error");
        }

        var content = await response.Content.ReadFromJsonAsync<PaystackResolveResponse>();

        if (content?.status == true && content.data != null)
        {
            return (true, content.data.account_name, "Account successfully verified.");
        }

        return (false, "Verification Failed", content?.message ?? "Account not found or invalid.");
    }
    public async Task<List<BankDto>> GetNigerianBanksAsync()
    {

        var response = await _client.GetAsync("bank?country=nigeria&use_cursor=false");

        if (!response.IsSuccessStatusCode)
        {

            throw new Exception($"Failed to retrieve bank list. Status: {response.StatusCode}");
        }

        var wrapper = await response.Content.ReadFromJsonAsync<PaystackBankListWrapper>();

        if (wrapper == null || !wrapper.Status)
        {

            throw new Exception(wrapper?.Message ?? "Invalid API response structure during bank fetch.");
        }


        var banks = wrapper.Data.Select(b => new BankDto
        {
            Name = b.Name,
            Code = b.Code
        }).ToList();

        return banks;
    }
}