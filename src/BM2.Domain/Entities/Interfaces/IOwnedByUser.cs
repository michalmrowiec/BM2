namespace BM2.Domain.Entities;

public interface IOwnedByUser
{
    public Guid UserId { get; set; }
}