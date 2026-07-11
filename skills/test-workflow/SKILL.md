# Test Workflow Skill

Load this skill when writing or running tests for OrgSphere.

---

## When to Use

- Writing unit tests for services
- Writing integration tests for APIs
- Writing end-to-end tests for workflows
- Running test suites
- Debugging test failures

---

## Testing Strategy

### Test Pyramid

```
        ┌─────────┐
        │   E2E   │  Few, slow, high confidence
        ├─────────┤
        │Integration│  Moderate, test boundaries
        ├─────────┤
        │  Unit   │  Many, fast, focused
        └─────────┘
```

### Coverage Targets

- Unit tests: 80%+
- Integration tests: Critical paths
- E2E tests: User journeys

---

## Unit Tests

### Service Tests

```typescript
// leave.service.test.ts
import { LeaveService } from './service';
import { mockGraph, mockEvents } from '../test-utils';

describe('LeaveService', () => {
  let service: LeaveService;

  beforeEach(() => {
    service = new LeaveService(
      mockGraph,
      mockEvents,
      mockRepository
    );
  });

  describe('createRequest', () => {
    it('should create leave request', async () => {
      // Arrange
      const input = {
        employeeId: 'emp-001',
        type: LeaveType.VACATION,
        startDate: new Date('2025-02-01'),
        endDate: new Date('2025-02-05')
      };
      const tenantId = 'tenant-123';

      mockGraph.getNode.mockResolvedValue({
        id: 'emp-001',
        tenantId
      });

      // Act
      const result = await service.createRequest(input, tenantId);

      // Assert
      expect(result).toBeDefined();
      expect(result.status).toBe(LeaveStatus.PENDING);
      expect(mockEvents.publish).toHaveBeenCalledWith(
        expect.objectContaining({
          type: 'leave.requested',
          tenantId
        })
      );
    });

    it('should throw if employee not found', async () => {
      // Arrange
      mockGraph.getNode.mockResolvedValue(null);

      // Act & Assert
      await expect(
        service.createRequest(input, tenantId)
      ).rejects.toThrow('Employee not found');
    });

    it('should throw if insufficient balance', async () => {
      // Arrange
      mockRepository.getBalance.mockResolvedValue({ available: 2 });

      // Act & Assert
      await expect(
        service.createRequest({
          ...input,
          startDate: new Date('2025-02-01'),
          endDate: new Date('2025-02-10')  // 10 days
        }, tenantId)
      ).rejects.toThrow('Insufficient leave balance');
    });
  });

  describe('approveRequest', () => {
    it('should approve leave request', async () => {
      // Arrange
      const requestId = 'req-001';
      const approvedBy = 'mgr-001';

      mockRepository.findById.mockResolvedValue({
        id: requestId,
        employeeId: 'emp-001',
        status: LeaveStatus.PENDING
      });

      // Act
      const result = await service.approveRequest(
        requestId,
        approvedBy,
        tenantId
      );

      // Assert
      expect(result.status).toBe(LeaveStatus.APPROVED);
      expect(result.approvedBy).toBe(approvedBy);
    });
  });
});
```

### Graph Service Tests

```typescript
// graph.service.test.ts
describe('GraphService', () => {
  describe('createNode', () => {
    it('should create node with tenant context', async () => {
      const node = await graphService.createNode({
        type: 'Employee',
        tenantId: 'tenant-123',
        properties: { name: 'John Doe' }
      });

      expect(node.tenantId).toBe('tenant-123');
      expect(node.type).toBe('Employee');
    });

    it('should reject node without tenant', async () => {
      await expect(
        graphService.createNode({
          type: 'Employee',
          properties: { name: 'John Doe' }
        })
      ).rejects.toThrow('tenantId is required');
    });
  });

  describe('traverse', () => {
    it('should only return nodes from same tenant', async () => {
      const results = await graphService.traverse({
        start: 'emp-001',
        relationship: 'REPORTS_TO',
        direction: 'INCOMING',
        tenantId: 'tenant-123'
      });

      results.forEach(node => {
        expect(node.tenantId).toBe('tenant-123');
      });
    });
  });
});
```

---

## Integration Tests

### API Tests

```typescript
// leave.api.test.ts
import request from 'supertest';
import { app } from '../app';

describe('Leave API', () => {
  let authToken: string;

  beforeEach(async () => {
    // Setup test user and get auth token
    authToken = await getAuthToken({
      email: 'test@company.com',
      role: 'manager'
    });
  });

  describe('POST /api/leave/requests', () => {
    it('should create leave request', async () => {
      const response = await request(app)
        .post('/api/leave/requests')
        .set('Authorization', `Bearer ${authToken}`)
        .send({
          employeeId: 'emp-001',
          type: 'vacation',
          startDate: '2025-02-01',
          endDate: '2025-02-05'
        });

      expect(response.status).toBe(201);
      expect(response.body.status).toBe('pending');
    });

    it('should return 401 without auth', async () => {
      const response = await request(app)
        .post('/api/leave/requests')
        .send({ /* ... */ });

      expect(response.status).toBe(401);
    });

    it('should return 403 for unauthorized employee', async () => {
      const response = await request(app)
        .post('/api/leave/requests')
        .set('Authorization', `Bearer ${authToken}`)
        .send({
          employeeId: 'emp-other-tenant',  // Different tenant
          type: 'vacation',
          startDate: '2025-02-01',
          endDate: '2025-02-05'
        });

      expect(response.status).toBe(403);
    });
  });
});
```

### Database Tests

```typescript
// leave.repository.test.ts
describe('LeaveRepository', () => {
  let db: Database;

  beforeEach(async () => {
    db = await createTestDatabase();
    await db.migrate();
  });

  afterEach(async () => {
    await db.cleanup();
  });

  describe('create', () => {
    it('should create leave request in database', async () => {
      const request = await repository.create({
        employeeId: 'emp-001',
        type: 'vacation',
        startDate: new Date('2025-02-01'),
        endDate: new Date('2025-02-05'),
        tenantId: 'tenant-123'
      });

      const found = await repository.findById(
        request.id,
        'tenant-123'
      );

      expect(found).toBeDefined();
      expect(found.employeeId).toBe('emp-001');
    });

    it('should isolate data by tenant', async () => {
      await repository.create({
        employeeId: 'emp-001',
        tenantId: 'tenant-123'
      });

      const found = await repository.findById(
        request.id,
        'tenant-other'  // Different tenant
      );

      expect(found).toBeNull();
    });
  });
});
```

---

## E2E Tests

### User Journey Tests

```typescript
// leave-journey.e2e.test.ts
describe('Leave Request Journey', () => {
  it('employee requests leave, manager approves', async () => {
    // 1. Employee logs in
    const employee = await login('employee@company.com');

    // 2. Employee creates leave request
    const request = await employee.api.leave.create({
      type: 'vacation',
      startDate: '2025-02-01',
      endDate: '2025-02-05'
    });

    expect(request.status).toBe('pending');

    // 3. Manager logs in
    const manager = await login('manager@company.com');

    // 4. Manager sees pending request
    const pendingRequests = await manager.api.leave.listPending();
    expect(pendingRequests).toContainEqual(
      expect.objectContaining({ id: request.id })
    );

    // 5. Manager approves
    const approved = await manager.api.leave.approve(request.id);
    expect(approved.status).toBe('approved');

    // 6. Employee sees approval
    const updated = await employee.api.leave.get(request.id);
    expect(updated.status).toBe('approved');
  });
});
```

---

## Test Utilities

### Mock Factory

```typescript
// test-utils.ts
export const mockGraph = {
  getNode: jest.fn(),
  createNode: jest.fn(),
  traverse: jest.fn(),
  query: jest.fn()
};

export const mockEvents = {
  publish: jest.fn(),
  subscribe: jest.fn()
};

export const mockRepository = {
  create: jest.fn(),
  findById: jest.fn(),
  update: jest.fn(),
  delete: jest.fn(),
  list: jest.fn()
};
```

### Test Database

```typescript
// test-db.ts
export async function createTestDatabase(): Promise<Database> {
  const db = await Database.connect({
    host: 'localhost',
    database: 'orgsphere_test',
    // Use test schema
    schema: 'test'
  });

  return db;
}
```

### Auth Helper

```typescript
// auth-helper.ts
export async function getAuthToken(
  user: TestUser
): Promise<string> {
  const response = await request(app)
    .post('/api/auth/login')
    .send({
      email: user.email,
      password: user.password
    });

  return response.body.token;
}
```

---

## Running Tests

### Commands

```bash
# Run all tests
npm test

# Run unit tests only
npm run test:unit

# Run integration tests only
npm run test:integration

# Run E2E tests only
npm run test:e2e

# Run tests with coverage
npm run test:coverage

# Run specific test file
npm test -- leave.service.test.ts

# Run tests in watch mode
npm run test:watch
```

### CI/CD Integration

```yaml
# .github/workflows/test.yml
name: Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-node@v3
        with:
          node-version: '20'
      - run: npm ci
      - run: npm run test:coverage
      - run: npm run lint
      - run: npm run typecheck
```

---

## Test Checklist

Before merging:

- [ ] All tests pass
- [ ] Coverage meets target
- [ ] No test isolation issues
- [ ] Mocks are cleaned up
- [ ] Test data is properly managed
- [ ] Tests are readable and maintainable
