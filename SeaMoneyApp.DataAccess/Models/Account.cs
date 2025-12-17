using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaMoneyApp.DataAccess.Models;
[Serializable]
[Table("table_accounts")]
public class Account
{
    
    [Column("id")]
    [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }
    
    [Column("login")]
    [Required]
    public string Login { get; set; }
    
    [Column("password")]
    [Required]
    public string Password { get; set; }
    
    [ForeignKey("position_id")]
    public Position? Position { get; set; }
    
    [Column("tours_in_rank")]
    [Required]
    public short ToursInRank{ get; set; }
    
}