namespace Sky.Template.Backend.Core.Enums;

public enum StatusType
{
    Global,   // Common statuses: ACTIVE, INACTIVE, SUSPENDED, DELETED
    User,     // User-specific statuses
    Bus,      // Bus statuses
    Trip,     // Trip statuses
    Revenue   // Revenue statuses
}

public enum Status
{
    ACTIVE,
    INACTIVE,
    SUSPENDED,
    DELETED
}
