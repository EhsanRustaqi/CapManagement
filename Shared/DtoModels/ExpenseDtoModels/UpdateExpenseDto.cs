using CapManagement.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapManagement.Shared.DtoModels.ExpenseDtoModels
{
    public class UpdateExpenseDto
    {
        public Guid ExpenseId { get; set; }

        public Guid CarId { get; set; }
        public Guid CompanyId { get; set; }

        public ExpenseType Type { get; set; }

        public decimal Amount { get; set; }

        public decimal VatPercent { get; set; }

        public DateTime ExpenseDate { get; set; }

        public int Quarter { get; set; }
    }

}
