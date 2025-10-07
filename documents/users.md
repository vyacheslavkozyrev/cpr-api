# Run API Scripts - User Reference

This document describes the users configured in the run-api scripts.

## Available Scripts

### 1. run-api-as-employee.cmd
**User:** Eve Adams  
**Position:** Senior Frontend Engineer  
**User ID:** `c7746e91-a5e8-4f8b-9f22-f48374ffa2a4`  
**Employee ID:** `32323232-3232-4323-2323-323232323232`  
**Reports To:** Diana Prince (Staff Frontend Engineer)  
**Department:** Engineering  

**Use Case:** Testing API as a mid-level individual contributor (IC) employee with standard permissions.

### 2. run-api-as-manager.cmd
**User:** Henry Wilson  
**Position:** Engineering Manager  
**User ID:** `977f4f1f-b3ce-4244-98fc-2c0d0248de88`  
**Employee ID:** `00000000-0000-0000-0000-00000000000c`  
**Reports To:** Ryan King (Director of Frontend Engineering)  
**Department:** Engineering  
**Direct Reports:**
- Diana Prince (Staff Frontend Engineer)
- Yale Bennett (Frontend Engineer)

**Use Case:** Testing API as a people manager with direct reports and elevated permissions.

## Organizational Context

```
Ryan King (Director of Frontend Engineering)
└── Henry Wilson (Engineering Manager) ← run-api-as-manager.cmd
    ├── Diana Prince (Staff Frontend Engineer)
    │   └── Eve Adams (Senior Frontend Engineer) ← run-api-as-employee.cmd
    │       └── Paul Hayes (Frontend Engineer)
    └── Yale Bennett (Frontend Engineer)
```

## Other Notable Users in Database

All 55 users from the seeded data are available for testing. Here are some other interesting test users:

- **Iris Davis** - VP of Engineering (top of org chart)
- **Sara Wright** - Senior Engineering Manager
- **Uma Patel** - Principal Frontend Engineer (senior IC)
- **Bob Johnson** - Senior Director of DevOps Engineering
- **Grace Lee** - Director of Backend Engineering
- **Frank Miller** - Director of Security Engineering

## How to Add More Scripts

To create additional run-api scripts for different user personas:

1. Query the database to get user details:
   ```sql
   SELECT u.id as user_id, u.display_name, e.id as employee_id, p.title 
   FROM users u 
   JOIN employees e ON u.id = e.user_id 
   JOIN positions p ON e.position_id = p.id 
   WHERE u.display_name = 'Your User Name';
   ```

2. Copy an existing script (e.g., `run-api-as-employee.cmd`)

3. Update the user ID, employee ID, name, and position in the script

4. Update the documentation in this file
