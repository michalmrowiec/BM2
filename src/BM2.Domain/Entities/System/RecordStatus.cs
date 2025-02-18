﻿using BM2.Domain.Entities.Interfaces;

namespace BM2.Domain.Entities.System;

public class RecordStatus : IEntity
{
    public Guid Id { get; set; }
    public Guid SystemCode { get; set; }
    public string RecordStatusName { get; set; } = null!;
    public bool ForRecords { get; set; }
    public bool ForPeriodicRecord  { get; set; }
}