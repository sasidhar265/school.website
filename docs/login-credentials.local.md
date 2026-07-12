# Local Login Configuration

The repository does not contain working portal passwords. Set development-only secrets in your shell before starting the application:

```bash
export SchoolConnect__PortalAuth__Student__Password='<unique-student-password>'
export SchoolConnect__PortalAuth__Teacher__Password='<unique-teacher-password>'
export SchoolConnect__PortalAuth__Admin__Password='<unique-admin-password>'
```

The configured local identifiers are `STU1001`, `TCH1001`, and `ADMIN`. Never reuse production passwords locally or commit secret values to this file.
