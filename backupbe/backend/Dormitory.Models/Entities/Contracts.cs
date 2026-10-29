using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dormitory.Models.Entities
{
    public class Contract : BaseEntity
    {
        public string ContractCode { get; set; } = string.Empty;
        public int StudentId { get; set; }
        //public int RoomId { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal MonthlyFee { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = "Active";
        public string PdfFileName { get; set; } = string.Empty;
        public string PdfContentType { get; set; } = string.Empty;
        public string PdfStoragePath { get; set; } = string.Empty;
        public string PdfPublicUrl { get; set; } = string.Empty;
        public DateTime? PdfUploadedAt { get; set; }
        public DateTime? ExpiryWarningSentAt { get; set; }

        public Students? Student { get; set; }
        public Rooms? Room { get; set; }
    }
}
