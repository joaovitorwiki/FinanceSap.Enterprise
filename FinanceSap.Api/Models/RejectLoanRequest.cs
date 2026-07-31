using System.ComponentModel.DataAnnotations;

namespace FinanceSap.Api.Models
{
    public class RejectLoanRequest
    {
        [StringLength(500)]
        public string? Reason { get; set; }
    }
}