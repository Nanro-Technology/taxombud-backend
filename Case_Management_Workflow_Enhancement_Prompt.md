# Professional Prompt for Case Management Workflow Enhancement

You are a Senior Backend Software Engineer and Solution Architect.

I have an existing backend application that contains a **Case Management** module. I want to redesign and enhance it into a configurable workflow-driven case management system that supports dynamic approval workflows, assignment rules, and role-based routing.

## Overall Objective

Transform the Case Management module into a configurable workflow engine where administrators can define how cases move through different approval and assignment stages without requiring code changes.

## 1. Workflow Definition

Create a configurable Workflow Management module.

The administrator should be able to:
- Create multiple workflows.
- Define workflow name and description.
- Enable/Disable workflow.
- Set workflow as default.
- Associate workflow with one or more case types/categories.
- Configure workflow versioning.

Each workflow consists of multiple sequential approval levels.

Example:

```text
Workflow: Loan Approval

Level 1 -> Loan Officer Role
Level 2 -> Branch Manager
Level 3 -> Regional Manager Role
Level 4 -> Compliance User
```

## 2. Approval Levels

Administrator should define:
- Number of approval levels
- Order of execution
- Level name
- Description
- SLA (optional)
- Escalation time (optional)
- Mandatory approval
- Require comments
- Require attachments

Actions:
- Approve
- Reject
- Return for correction
- Reassign
- Skip (optional)
- Escalate

## 3. Assignment Configuration

Each workflow level should support:
- Specific User
- Role
- User + Role
- Dynamic assignment (future extension)

## 4. Assignment Mode

Support:
- Manual Assignment
- Automatic Assignment

Automatic algorithms:
- Round Robin
- Least Workload
- Random
- First Available
- Lowest Active Cases
- Custom Strategy Interface

## 5. Case Routing

When submitted, the system should:
1. Determine workflow.
2. Determine current level.
3. Determine assignment rule.
4. Determine assignee(s).
5. Create workflow instance.
6. Create approval task.
7. Notify assignee.

## 6. Reassignment

Support reassignment with configurable permissions.

Audit:
- Previous assignee
- New assignee
- Date
- Reason
- Performed by

## 7. Approval Actions

Immutable history:
- User
- Role
- Action
- Date
- Comment
- Previous Status
- New Status
- Workflow Level

## 8. Workflow Status

- Draft
- Submitted
- Pending Approval
- In Progress
- Returned
- Rejected
- Approved
- Completed
- Cancelled
- Escalated
- Reassigned

## 9. Permissions

Examples:
- Workflow.Create
- Workflow.Update
- Workflow.Delete
- Workflow.Publish
- Workflow.Assign
- Workflow.Reassign
- Workflow.Override
- Workflow.Configure
- Case.Assign
- Case.Reassign
- Case.Approve
- Case.Reject
- Case.Return
- Case.Escalate

## 10. Notifications

Generate events for:
- Assignment
- Reassignment
- Approval Required
- Approved
- Rejected
- Returned
- Escalated
- Completed

## 11. Audit Trail

Track:
- Workflow changes
- Assignment/Reassignment
- Approvals
- Status changes
- Comments
- User actions
- Timestamp
- IP (if available)

## 12. Database Design

Suggested entities:
- Workflow
- WorkflowVersion
- WorkflowLevel
- WorkflowAssignmentRule
- WorkflowInstance
- WorkflowInstanceLevel
- Case
- CaseAssignment
- CaseApproval
- CaseHistory
- CaseComment
- CaseAttachment
- WorkflowTransition
- AssignmentAlgorithm
- NotificationEvent
- AuditLog

## 13. REST APIs

Implement:
- Workflow CRUD
- Workflow Level CRUD
- Assignment Rule CRUD
- Publish Workflow
- Submit Case
- Approve
- Reject
- Return
- Reassign
- Escalate
- Get Workflow
- Get Timeline
- Get Approval History
- Get Pending Tasks
- Get Assigned Cases
- Dashboard Statistics

## 14. Architecture

Follow:
- Clean Architecture
- SOLID
- Service Layer
- Repository Pattern (if existing)
- DTOs
- Validation
- Authorization
- Transactions
- Dependency Injection
- Strategy Pattern for assignment
- Event-driven notifications

## 15. Business Rules

- No duplicate workflow order.
- At least one approval level.
- Version completed workflows before modification.
- Only current assignee may approve unless override permission exists.
- Audit every reassignment.
- Transactional operations.
- Prevent circular routing.
- Prevent approval skipping unless configured.
- Design for future support of parallel approvals.

## 16. Deliverables

Analyze the existing codebase and:
1. Explain each change.
2. List modified/new files.
3. Update migrations.
4. Update models/entities.
5. Update services.
6. Update repositories.
7. Update controllers/endpoints.
8. Update authorization.
9. Add validation.
10. Add tests.
11. Preserve backward compatibility where possible.

## Expected Outcome

Deliver a production-ready, configurable enterprise workflow engine supporting configurable approval chains, user/role-based assignment, automatic and manual routing, reassignment, notifications, comprehensive audit logging, and future extensibility.
