namespace BM2.Domain.Entities;

public interface IOwnedByUser
{
    public Guid OwnedByUserId { get; set; }
}