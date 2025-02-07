namespace BM2.Domain.Entities.Interfaces;

public interface IOwnedByUser
{
    public Guid OwnedByUserId { get; set; }
}