using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRSystem.Common.Enums;

namespace HRSystem.Data.Models;

public class PayrollItem
{
    public int Id { get; set; }

    public int PayrollId { get; set; }

    public ItemType ItemType { get; set; }

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
}
