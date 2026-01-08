using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaMoneyApp.DataAccess.Models;

[Table("table_salaries")]
public class Salary
{
    [Column("id")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }

    [Column("year")] [Required] public short Year { get; set; }

    [ForeignKey("position_id")] public Position? Position { get; set; }

    [Column("basic_wage")] [Required] public decimal BasicWage { get; set; }

    [Column("crew_overtime")] [Required] public decimal CrewOvertime { get; set; }

    [Column("fidelity_bonus")] [Required] public decimal FidelityBonus { get; set; }

    [Column("vacation")] [Required] public decimal Vacation { get; set; }

    [Column("company_bonus")] [Required] public decimal CompanyBonus { get; set; }

    [Column("performance_bonus")]
    [Required]
    public decimal PerformanceBonus { get; set; }

    [Column("is_deleted")] public bool IsDeleted { get; set; }

    public override string ToString()
    {
        return $"{Position} / Basic - {BasicWage}, CrewOvertime - {CrewOvertime}," +
               $" FidelityBonus - {FidelityBonus}, Vacation- {Vacation}," +
               $" CompanyBonus- {CompanyBonus}, PerformanceBonus- {PerformanceBonus}";
    }
}