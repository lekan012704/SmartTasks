using SmartTask.Application.Dto.Paystack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Interfaces
{
    public interface IPaystackService
    {
        Task<(bool Success, string AccountName, string Message)> ResolveAccountAsync(string accountNumber, string bankCode);
        Task<List<BankDto>> GetNigerianBanksAsync();
    }
}
