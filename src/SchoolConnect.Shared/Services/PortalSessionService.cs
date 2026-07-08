using SchoolConnect.Shared.Configuration;

namespace SchoolConnect.Shared.Services;

public sealed class PortalSessionService
{
    public const string BrowserSessionRoleKey = "schoolconnect.activeRole";

    private readonly SchoolContentStore store;

    private string? currentRole;

    public event Action? Changed;

    public PortalSessionService(SchoolContentStore store)
    {
        this.store = store;
    }

    public bool IsStudentAuthenticated { get; private set; }

    public bool IsTeacherAuthenticated { get; private set; }

    public bool TrySignIn(string role, string? pin, string? password)
    {
        var options = store.Options;
        var student = options.PortalAuth.Student;
        if (role == PortalRoles.Student
            && string.Equals(pin?.Trim(), student.Pin, StringComparison.OrdinalIgnoreCase)
            && string.Equals(password, student.Password, StringComparison.Ordinal))
        {
            IsStudentAuthenticated = true;
            currentRole = PortalRoles.Student;
            NotifyChanged();
            return true;
        }

        var teacher = options.PortalAuth.Teacher;
        if (role == PortalRoles.Teacher
            && string.Equals(pin?.Trim(), teacher.Pin, StringComparison.OrdinalIgnoreCase)
            && string.Equals(password, teacher.Password, StringComparison.Ordinal))
        {
            IsTeacherAuthenticated = true;
            currentRole = PortalRoles.Teacher;
            NotifyChanged();
            return true;
        }

        return false;
    }

    public bool IsAuthenticated(string role) => role switch
    {
        PortalRoles.Student => IsStudentAuthenticated,
        PortalRoles.Teacher => IsTeacherAuthenticated,
        _ => false
    };

    public bool RestoreAuthenticatedRole(string? role)
    {
        if (role == PortalRoles.Student)
        {
            IsStudentAuthenticated = true;
            currentRole = PortalRoles.Student;
            NotifyChanged();
            return true;
        }

        if (role == PortalRoles.Teacher)
        {
            IsTeacherAuthenticated = true;
            currentRole = PortalRoles.Teacher;
            NotifyChanged();
            return true;
        }

        return false;
    }

    public PortalSessionInfo? GetActiveSession()
    {
        if (currentRole == PortalRoles.Student && IsStudentAuthenticated)
        {
            return CreateStudentSession();
        }

        if (currentRole == PortalRoles.Teacher && IsTeacherAuthenticated)
        {
            return CreateTeacherSession();
        }

        if (IsStudentAuthenticated)
        {
            return CreateStudentSession();
        }

        if (IsTeacherAuthenticated)
        {
            return CreateTeacherSession();
        }

        return null;
    }

    public void SignOut(string role)
    {
        if (role == PortalRoles.Student)
        {
            IsStudentAuthenticated = false;
        }
        else if (role == PortalRoles.Teacher)
        {
            IsTeacherAuthenticated = false;
        }

        if (currentRole == role)
        {
            currentRole = IsStudentAuthenticated
                ? PortalRoles.Student
                : IsTeacherAuthenticated
                    ? PortalRoles.Teacher
                    : null;
        }

        NotifyChanged();
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }

    private PortalSessionInfo CreateStudentSession()
    {
        var student = store.Options.PortalAuth.Student;
        return new PortalSessionInfo(
            PortalRoles.Student,
            "Student",
            student.DisplayName,
            student.ClassName,
            student.Gender,
            student.GuardianName,
            student.MobileNumber,
            student.ClassTeacher,
            student.SchoolJoinedYear,
            string.Empty,
            string.Empty,
            string.Empty);
    }

    private PortalSessionInfo CreateTeacherSession()
    {
        var teacher = store.Options.PortalAuth.Teacher;
        return new PortalSessionInfo(
            PortalRoles.Teacher,
            "Teacher",
            teacher.DisplayName,
            teacher.ClassName,
            teacher.Gender,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            teacher.ClassDealingWith,
            teacher.Subject,
            teacher.Qualification);
    }
}

public sealed record PortalSessionInfo(
    string Role,
    string RoleLabel,
    string DisplayName,
    string ClassName,
    string Gender,
    string GuardianName,
    string MobileNumber,
    string ClassTeacher,
    string SchoolJoinedYear,
    string ClassDealingWith,
    string Subject,
    string Qualification);

public static class PortalRoles
{
    public const string Student = "student";
    public const string Teacher = "teacher";
}
