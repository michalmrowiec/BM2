﻿namespace BM2.Domain.Entities;

public class RecordTagRelation : IEntity, IOwnedByUser
{
    public Guid Id { get; set; }
    public Guid RecordId { get; set; }
    public Guid TagId { get; set; }
    public Guid OwnedByUserId { get; set; }

    public Record Record { get; set; }
    public Tag Tag { get; set; }
    public User? OwnedByUser { get; set; }
}