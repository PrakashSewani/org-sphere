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

public enum EmploymentType
{
    FullTime,
    PartTime,
    Contractor
}

public enum EmployeeStatus
{
    Active,
    Inactive,
    OnLeave
}

public enum LeaveType
{
    Annual,
    Sick,
    Personal,
    Maternity,
    Paternity,
    Unpaid,
    Other
}

public enum AttendanceCheckType
{
    CheckIn,
    CheckOut
}

public enum ApprovalType
{
    LeaveRequest,
    ExpenseReport,
    EquipmentRequest,
    AccessRequest,
    PromotionRequest,
    Custom
}

public enum ApprovalStepStatus
{
    Pending,
    Approved,
    Rejected,
    Escalated,
    Delegated,
    Skipped
}
