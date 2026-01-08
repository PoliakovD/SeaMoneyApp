using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaMoneyApp.DataAccess.Models;
[Table("table_personal_bonuses")]
public class PersonalBonus
{
    [Column("id")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }
    
    [Column("year")]
    [Required] public short year { get; set; }
    
    [ForeignKey("position_id")]
    public Position? Position { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted{ get; set; }
    
    [Column("personal_bonus")] [Required] public decimal PersonalBonusValue { get; set; }
    
    [Column("tours_in_rank")]
    [Required]
    public short ToursInRank{ get; set; }
}