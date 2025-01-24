namespace BM2.Domain.Entities;

public class AuditLogin : IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime DateTimeOfLogin { get; set; }
    
    public User User { get; set; }
}