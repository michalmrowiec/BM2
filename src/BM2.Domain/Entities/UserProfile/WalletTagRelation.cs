﻿using BM2.Domain.Entities.Interfaces;

namespace BM2.Domain.Entities.UserProfile;

public class WalletTagRelation : IEntity, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public Guid TagId { get; set; }
    public bool IsActive { get; set; }
    public Guid OwnedByUserId { get; set; }

    public Wallet? Wallet { get; set; }
    public Tag? Tag { get; set; }
    public User? OwnedByUser { get; set; }

    private WalletTagRelation(Guid walletId, Guid tagId, Guid ownedByUserId, bool isActive = true)
    {
        Id = Guid.NewGuid();
        WalletId = walletId;
        TagId = tagId;
        IsActive = isActive;
        OwnedByUserId = ownedByUserId;
    }

    public static WalletTagRelation CreateInstance(Guid walletId, Guid tagId, Guid ownedByUserId, bool isActive = true)
    {
        return new WalletTagRelation(walletId, tagId, ownedByUserId);
    }
}