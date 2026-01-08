using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaMoneyApp.DataAccess.Models;


[Table("table_rub_to_dollar")]
public class ChangeRubToDollar
{
    [Column("id")]
    [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }
    
    [Column("value")]
    public decimal Value { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("is_deleted")]
    public bool IsDeleted{ get; set; }
    public override string ToString()
    {
        return $"{Date:d} - {Value}";
    }
}