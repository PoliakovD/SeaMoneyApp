using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeaMoneyApp.DataAccess.Models;

[Table("table_contracts")]
public class Contract
{
    [Column("id")]
    [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int? Id { get; set; }
    
    [Column("vessel_name")]
    public string? VesselName { get; set; }
    
    [Column("contract_description")]
    public string? ContractDescription { get; set; }
    
    [Column("begin_date")]
    public DateTime BeginDate { get; set; }
    
    [Column("end_date")]
    public DateTime? EndDate { get; set; }
    [ForeignKey("position_id")] public Position? Position { get; set; }
    
    [ForeignKey("account_id")]
    public Account? Account { get; set; }
    [Column("is_deleted")]
    public bool IsDeleted{ get; set; }
    public Contract()
    {
        
    }

    public Contract(Contract contract)
    {
        Id = contract.Id;
        VesselName = contract.VesselName;
        ContractDescription = contract.ContractDescription;
        BeginDate = contract.BeginDate.Date;
        EndDate = contract.EndDate.Value.Date;
        Account = contract.Account;
        Position = contract.Position;
        IsDeleted = contract.IsDeleted;
    }
    public override string ToString()
    {
        return $"{VesselName}, {BeginDate:d} -  {EndDate:d},  Account - {Account}";
    }
}