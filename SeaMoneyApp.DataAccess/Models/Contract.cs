using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaMoneyApp.DataAccess.Models;

[Table("table_contracts")]
public class Contract
{
    [Column("id")]
    [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }
    
    [Column("begin_date")]
    public DateTime BeginDate { get; set; }
    
    [Column("end_date")]
    public DateTime EndDate { get; set; }
}