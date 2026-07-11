# Terminology

## Core Concepts

### Organization Graph

The central data structure representing an entire organization as a network of interconnected entities (nodes) and relationships (edges).

### Node

Any entity in the Organization Graph: employee, department, team, office, project, workflow, approval chain, etc.

### Edge

A relationship between two nodes: reports-to, member-of, manages, approves, collaborates-with, etc.

### Tenant

An isolated customer environment within the multi-tenant SaaS platform. Each tenant has its own graph, users, and configuration.

### OrgOS

Organization Operating Platform. The category OrgSphere defines. Beyond HRMS—operating the entire organization.

---

## Entities

### Employee

A person working for the organization. Has a profile, skills, certifications, employment history, and relationships.

### Department

A major organizational unit. Contains teams and employees. Has a head and reporting structure.

### Team

A group of employees working together. Belongs to a department. Has a lead and members.

### Office

A physical location. Employees and departments can be associated with offices.

### Project

A temporary initiative with goals, members, and timelines.

### Committee

A group formed for specific purposes (hiring, review, etc.).

---

## Relationships

### Reports-To

The reporting hierarchy. An employee reports to a manager.

### Member-Of

An employee belongs to a team or department.

### Manages

A manager leads a department, team, or project.

### Approves

An approver authorizes requests in a workflow.

### Collaborates-With

Two entities work together on initiatives.

### Supervises

A temporary or permanent supervisory relationship.

---

## Workflows

### Approval Chain

A sequence of approvers for a request type (leave, expense, etc.).

### Delegation

Temporarily assigning approval authority to another person.

### Escalation

Automatic routing when an approver is unavailable or a deadline is missed.

---

## AI Concepts

### Organizational Reasoning

AI that understands organizational structure, relationships, and context to answer questions and suggest improvements.

### Natural Language Query

Asking questions in plain language (e.g., "Who reports to Alice?").

### Pattern Recognition

AI identifying organizational patterns like overloaded managers or skill gaps.

---

## Platform Concepts

### Self-Service

Employees managing their own information and requests without HR intervention.

### Graph Analytics

Metrics and insights derived from the Organization Graph structure.

### Digital Twin

A living digital representation of the physical organization that evolves in real-time.
