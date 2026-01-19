using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapManagement.Shared.Models
{
    public class VatReportItem
    {
        public Guid CarId { get; set; }
        public string CarPlate { get; set; }
        public int Year { get; set; }
        public int Quarter { get; set; }
        public decimal TotalVat { get; set; } // Sum of VatAmount from all expenses
    }
}
