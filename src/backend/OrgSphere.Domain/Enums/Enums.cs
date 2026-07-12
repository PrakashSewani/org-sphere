namespace OrgSphere.Domain.Enums;

public enum TenantPlan
{
    Free,
    Starter,
    Professional,
    Enterprise
}

public enum UserRole
{
    Owner,
    Admin,
    HR,
    Manager,
    Employee,
    Contractor
}

public enum NodeType
{
    Company,
    Region,
    Office,
    Department,
    Team,
    Employee,
    Project
}

public enum EdgeType
{
    ReportsTo,
    MemberOf,
    LocatedIn,
    Owns,
    Manages
}

public enum LeaveStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled
}

public enum AttendanceStatus
{
    Present,
    Absent,
    Late,
    HalfDay,
    Holiday,
    Leave
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    Escalated,
    Delegated
}
