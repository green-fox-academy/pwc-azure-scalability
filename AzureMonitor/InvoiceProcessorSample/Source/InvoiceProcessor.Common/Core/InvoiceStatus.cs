﻿namespace InvoiceProcessor.Common.Core
{
    public enum InvoiceStatus
    {
        Unknown = 0,
        Created,
        SentToExternalSystem,
        ProcessedByExternalSystem
    }
}
