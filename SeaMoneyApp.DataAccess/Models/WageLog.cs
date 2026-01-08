using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaMoneyApp.DataAccess.Models;

[Table("table_wage_log")]
public class WageLog
{
    [Column("id")]
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }

    [Column("date")] [Required] public DateTime? Date { get; set; }

    [Column("amount_in_rub")] [Required] public decimal? AmountInRub { get; set; }
    
    [Column("tours_in_rank")]
    [Required]
    public short ToursInRank{ get; set; }
    
    [ForeignKey("contract_id")] public Contract? Contract { get; set; }

    [ForeignKey("position_id")] public Position? Position { get; set; }

    [ForeignKey("account_id")] public Account? Account { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted{ get; set; }
    
    [ForeignKey("change_rub_to_dollar_id")]
    public ChangeRubToDollar? ChangeRubToDollar { get; set; }

    public WageLog()
    {
        
    }

    public WageLog(WageLog wageLog)
    {
        Id = wageLog.Id;
        Date = wageLog.Date;
        AmountInRub = wageLog.AmountInRub;
        ToursInRank = wageLog.ToursInRank;
        Account = wageLog.Account;
        ChangeRubToDollar = wageLog.ChangeRubToDollar;
        Contract = wageLog.Contract;
        Position = wageLog.Position;
        IsDeleted = wageLog.IsDeleted;
    }
    public override string ToString()
    {
        return $"{Date:d}, {AmountInRub},  Account - {Account}, {Contract},{Position}";
    }

}