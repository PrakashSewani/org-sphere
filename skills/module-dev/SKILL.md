# Module Development Skill

Load this skill when creating new modules or modifying existing modules in OrgSphere.

---

## When to Use

- Creating a new module
- Adding features to existing modules
- Refactoring module structure
- Integrating modules with the graph

---

## Module Structure

### Directory Layout

```
src/modules/
├── leave/
│   ├── index.ts              # Module entry point
│   ├── types.ts              # TypeScript types
│   ├── service.ts            # Business logic
│   ├── resolver.ts           # GraphQL resolvers
│   ├── controller.ts         # REST controllers
│   ├── events.ts             # Event handlers
│   ├── validation.ts         # Input validation
│   ├── permissions.ts        # Permission definitions
│   └── tests/
│       ├── service.test.ts
│       ├── resolver.test.ts
│       └── integration.test.ts
```

---

## Creating a New Module

### Step 1: Define Types

```typescript
// types.ts
export interface LeaveRequest {
  id: string;
  tenantId: string;
  employeeId: string;
  type: LeaveType;
  startDate: Date;
  endDate: Date;
  status: LeaveStatus;
  reason?: string;
  approvedBy?: string;
  createdAt: Date;
  updatedAt: Date;
}

export enum LeaveType {
  VACATION = 'vacation',
  SICK = 'sick',
  PERSONAL = 'personal',
  PARENTAL = 'parental'
}

export enum LeaveStatus {
  PENDING = 'pending',
  APPROVED = 'approved',
  REJECTED = 'rejected',
  CANCELLED = 'cancelled'
}
```

### Step 2: Create Service

```typescript
// service.ts
import { GraphService } from '../graph/service';
import { EventBus } from '../events/bus';

export class LeaveService {
  constructor(
    private graph: GraphService,
    private events: EventBus
  ) {}

  async createRequest(
    input: CreateLeaveRequestInput,
    tenantId: string
  ): Promise<LeaveRequest> {
    // 1. Validate tenant context
    validateTenantContext(tenantId);

    // 2. Validate employee exists and belongs to tenant
    const employee = await this.graph.getNode(
      input.employeeId,
      tenantId
    );
    if (!employee) {
      throw new Error('Employee not found');
    }

    // 3. Validate leave policy
    await this.validatePolicy(input, tenantId);

    // 4. Create request
    const request = await this.repository.create({
      ...input,
      tenantId,
      status: LeaveStatus.PENDING,
      createdAt: new Date(),
      updatedAt: new Date()
    });

    // 5. Publish event
    await this.events.publish({
      type: 'leave.requested',
      tenantId,
      data: { requestId: request.id, employeeId: input.employeeId }
    });

    // 6. Route to approver
    await this.routeToApprover(request, tenantId);

    return request;
  }

  async approveRequest(
    requestId: string,
    approvedBy: string,
    tenantId: string
  ): Promise<LeaveRequest> {
    // 1. Validate request exists
    const request = await this.repository.findById(requestId, tenantId);
    if (!request) {
      throw new Error('Leave request not found');
    }

    // 2. Validate approver has permission
    await this.validateApproverPermission(
      approvedBy,
      request.employeeId,
      tenantId
    );

    // 3. Update request
    const updated = await this.repository.update(requestId, {
      status: LeaveStatus.APPROVED,
      approvedBy,
      updatedAt: new Date()
    }, tenantId);

    // 4. Publish event
    await this.events.publish({
      type: 'leave.approved',
      tenantId,
      data: { requestId, employeeId: request.employeeId }
    });

    // 5. Update leave balance
    await this.updateBalance(request, tenantId);

    return updated;
  }

  private async validatePolicy(
    input: CreateLeaveRequestInput,
    tenantId: string
  ): Promise<void> {
    const policy = await this.getPolicy(input.type, tenantId);
    const balance = await this.getBalance(
      input.employeeId,
      input.type,
      tenantId
    );

    const daysRequested = this.calculateDays(
      input.startDate,
      input.endDate
    );

    if (daysRequested > balance.available) {
      throw new Error('Insufficient leave balance');
    }
  }

  private async routeToApprover(
    request: LeaveRequest,
    tenantId: string
  ): Promise<void> {
    // Find direct manager via graph
    const manager = await this.graph.traverse({
      start: request.employeeId,
      relationship: 'REPORTS_TO',
      direction: 'OUTGOING',
      depth: 1,
      tenantId
    });

    if (manager.length === 0) {
      throw new Error('No approver found');
    }

    // Create approval record
    await this.approvalService.create({
      type: 'leave',
      requestId: request.id,
      approverId: manager[0].id,
      tenantId
    });
  }
}
```

### Step 3: Create Resolvers

```typescript
// resolver.ts
export const leaveResolvers = {
  Query: {
    leaveRequests: async (_, args, context) => {
      return leaveService.listRequests(
        context.tenantId,
        args.filters
      );
    },

    leaveRequest: async (_, { id }, context) => {
      return leaveService.getRequest(id, context.tenantId);
    },

    leaveBalance: async (_, { employeeId, type }, context) => {
      return leaveService.getBalance(
        employeeId,
        type,
        context.tenantId
      );
    }
  },

  Mutation: {
    createLeaveRequest: async (_, { input }, context) => {
      return leaveService.createRequest(input, context.tenantId);
    },

    approveLeaveRequest: async (_, { id }, context) => {
      return leaveService.approveRequest(
        id,
        context.userId,
        context.tenantId
      );
    },

    rejectLeaveRequest: async (_, { id, reason }, context) => {
      return leaveService.rejectRequest(
        id,
        context.userId,
        reason,
        context.tenantId
      );
    }
  },

  Employee: {
    leaveRequests: async (employee, _, context) => {
      return leaveService.getEmployeeRequests(
        employee.id,
        context.tenantId
      );
    },

    leaveBalance: async (employee, _, context) => {
      return leaveService.getEmployeeBalances(
        employee.id,
        context.tenantId
      );
    }
  }
};
```

### Step 4: Define Permissions

```typescript
// permissions.ts
export const leavePermissions = {
  'leave:request:create': {
    description: 'Create leave request',
    scope: 'own'
  },

  'leave:request:read': {
    description: 'Read leave requests',
    scope: 'team'  // Managers can see team requests
  },

  'leave:request:approve': {
    description: 'Approve leave requests',
    scope: 'team',
    requiresGraphRelationship: 'REPORTS_TO'
  },

  'leave:policy:manage': {
    description: 'Manage leave policies',
    scope: 'company',
    requiresRole: 'hr_admin'
  }
};
```

### Step 5: Create Events

```typescript
// events.ts
export const leaveEvents = {
  'leave.requested': {
    handler: async (event) => {
      // Notify manager
      await notificationService.notify({
        type: 'approval_required',
        userId: event.data.managerId,
        message: `New leave request from ${event.data.employeeName}`,
        tenantId: event.tenantId
      });
    }
  },

  'leave.approved': {
    handler: async (event) => {
      // Notify employee
      await notificationService.notify({
        type: 'request_approved',
        userId: event.data.employeeId,
        message: 'Your leave request has been approved',
        tenantId: event.tenantId
      });

      // Update team availability
      await analyticsService.trackEvent(
        'leave_approved',
        event.data,
        event.tenantId
      );
    }
  },

  'leave.rejected': {
    handler: async (event) => {
      // Notify employee
      await notificationService.notify({
        type: 'request_rejected',
        userId: event.data.employeeId,
        message: `Your leave request was rejected: ${event.data.reason}`,
        tenantId: event.tenantId
      });
    }
  }
};
```

---

## Integration Checklist

When creating or modifying a module, ensure:

- [ ] Types are properly defined
- [ ] Service handles all business logic
- [ ] Resolvers follow GraphQL conventions
- [ ] Permissions are defined for all operations
- [ ] Events are published for state changes
- [ ] Validation is comprehensive
- [ ] Multi-tenant isolation is enforced
- [ ] Tests are written
- [ ] Documentation is updated

---

## Common Patterns

### Repository Pattern

```typescript
export class LeaveRepository {
  async create(data, tenantId) { /* ... */ }
  async findById(id, tenantId) { /* ... */ }
  async update(id, data, tenantId) { /* ... */ }
  async delete(id, tenantId) { /* ... */ }
  async list(filters, tenantId) { /* ... */ }
}
```

### Service Pattern

```typescript
export class LeaveService {
  constructor(
    private repository: LeaveRepository,
    private graph: GraphService,
    private events: EventBus,
    private notifications: NotificationService
  ) {}
}
```

### Event Pattern

```typescript
// Always publish events after successful operations
await this.events.publish({
  type: 'entity.action',
  tenantId,
  data: { /* relevant data */ }
});
```
