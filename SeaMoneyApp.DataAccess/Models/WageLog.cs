using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaMoneyApp.DataAccess.Models;

[Table("table_wage_log")]
public class WageLog
{
    [Column("id")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    [Column("date")] [Required]
    public DateTime Date { get; set; }
    
    [Column("amount_in_rub")] [Required]
    public decimal AmountInRub { get; set; }
    
    [ForeignKey("change_rub_to_dollar_id")] public ChangeRubToDollar? ChangeRubToDollar { get; set; }
    
    
}