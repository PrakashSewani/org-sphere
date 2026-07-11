# Recruitment

## Applicant Tracking System (ATS)

Integrated recruitment module that connects hiring to the Organization Graph.

---

## Job Management

### Job Creation

- Job title
- Department
- Team
- Location
- Employment type
- Salary range
- Description
- Requirements

### Job Posting

- Internal posting
- External job boards
- Company career page
- Social media

### Job Pipeline

```
Draft → Open → Interviewing → Offer → Closed
```

---

## Candidate Management

### Application Tracking

- Resume parsing
- Application form
- Cover letter
- Portfolio links

### Candidate Profile

- Contact information
- Resume
- Skills
- Experience
- Education
- References

### Candidate Stages

```
Applied → Screening → Interview → Offer → Hired
```

---

## Interview Process

### Scheduling

- Interview scheduling
- Calendar integration
- Timezone handling
- Buffer time

### Interview Types

- Phone screen
- Technical interview
- Cultural fit
- Final interview

### Feedback

- Interview scorecard
- Structured feedback
- Comparison view
- Decision matrix

---

## Offer Management

### Offer Creation

- Salary calculation
- Benefits package
- Start date
- Terms and conditions

### Offer Approval

- Manager approval
- HR approval
- Finance approval

### Offer Delivery

- Digital offer letter
- E-signature
- Acceptance tracking

---

## Graph Integration

### Relationships

```
Job
  ├── BELONGS_TO → Department
  ├── PART_OF → Team
  ├── LOCATED_AT → Office
  ├── MANAGED_BY → Recruiter
  └── HIRES_FOR → Hiring Manager

Candidate
  ├── APPLIES_TO → Job
  ├── INTERVIEWED_BY → Interviewer
  └── HIRED_AS → Employee
```

### Graph Queries

- Open positions by department
- Candidates in pipeline
- Interview load per interviewer
- Hiring velocity

---

## Analytics

### Metrics

- Time to hire
- Cost per hire
- Offer acceptance rate
- Source effectiveness

### Insights

- Pipeline bottlenecks
- Interviewer performance
- Source quality
- Diversity metrics
