using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Dto.Paystack
{
    // File: Application/DTOs/AccountVerificationResponseDto.cs

    public class AccountVerificationResponseDto
    {
        public bool Success { get; set; }
        public string AccountName { get; set; }
        public string? Message { get; set; }
    }
}
