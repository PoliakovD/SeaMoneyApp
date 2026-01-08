using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaMoneyApp.DataAccess.Models;

[Serializable]
[Table("table_positions")]
public class Position
{
    [Column("id")]
    [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }
    
    [Column("name")]
    [Required]
    public string Name { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted{ get; set; }
    public override string ToString()
    {
        return Name;
    }
}