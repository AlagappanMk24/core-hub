using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core_API.Domain.Entities
{
    public class InvoiceSequence
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int CompanyId { get; set; }
        public int CurrentNumber { get; set; }
        public int CurrentYear { get; set; }
        public DateTime UpdatedDate { get; set; }

        [ConcurrencyCheck]
        public int Version { get; set; }
    }
}