using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CapManagement.Shared.Models.Car_CompanyReportModels
{
    public class ExpenseReportItemDto
    {
        public ExpenseType Type { get; set; }
        public decimal TotalNetAmount { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal TotalGrossAmount { get; set; }
    }
    public class ExpenseReportSummaryDto
    {
        public Guid? CarId { get; set; }
        public string? CarName { get; set; }
        public Guid CompanyId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public decimal TotalNetAmount { get; set; }
        public decimal TotalVatAmount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public List<ExpenseReportItemDto> ByType { get; set; } = new();
    }
}