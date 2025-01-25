﻿namespace BM2.Domain.Entities;

public class Category : IEntity, IEntityAudit, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string CategoryName { get; set; } = null!;
    public Guid OwnedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedBy { get; set; }
    
    public Wallet? Wallet { get; set; }
    public User? OwnedByUser { get; set; }
    public ICollection<Record> Records { get; set; } = [];
}