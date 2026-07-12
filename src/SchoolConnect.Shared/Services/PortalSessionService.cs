using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SchoolConnect.Shared.Configuration;

namespace SchoolConnect.Shared.Services;

public sealed class PortalSessionService
{
    public const string BrowserSessionRoleKey = "schoolconnect.sessionToken";
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(8);

    private readonly SchoolContentStore store;
    private readonly IPasswordHasher<PortalAccountOptions> passwordHasher;
    private readonly IDataProtector sessionProtector;
    private readonly LoginAttemptGuard loginAttemptGuard;
    private readonly ILogger<PortalSessionService> logger;
    private string? currentRole;

    public PortalSessionService(
        SchoolContentStore store,
        IPasswordHasher<PortalAccountOptions> passwordHasher,
        IDataProtectionProvider dataProtectionProvider,
        LoginAttemptGuard loginAttemptGuard,
        ILogger<PortalSessionService> logger)
    {
        this.store = store;
        this.passwordHasher = passwordHasher;
        sessionProtector = dataProtectionProvider.CreateProtector("SchoolConnect.PortalSession.v1");
        this.loginAttemptGuard = loginAttemptGuard;
        this.logger = logger;
    }

    public event Action? Changed;
    public bool IsStudentAuthenticated { get; private set; }
    public bool IsTeacherAuthenticated { get; private set; }
    public bool IsAdminAuthenticated { get; private set; }

    public bool TrySignIn(string role, string? pin, string? password)
    {
        if (!loginAttemptGuard.IsAllowed(role))
        {
            logger.LogWarning("Blocked a portal sign-in attempt because the {Role} role is temporarily locked.", role);
            return false;
        }

        var account = GetAccount(role);
        if (account is null || string.IsNullOrWhiteSpace(password)
            || !string.Equals(pin?.Trim(), account.Pin, StringComparison.OrdinalIgnoreCase)
            || !VerifyPassword(account, password))
        {
            loginAttemptGuard.RecordFailure(role);
            logger.LogWarning("Portal sign-in failed for the {Role} role.", role);
            return false;
        }

        loginAttemptGuard.RecordSuccess(role);
        SetAuthenticated(role, true);
        currentRole = role;
        NotifyChanged();
        return true;
    }

    public bool IsAuthenticated(string role) => role switch
    {
        PortalRoles.Student => IsStudentAuthenticated,
        PortalRoles.Teacher => IsTeacherAuthenticated,
        PortalRoles.Admin => IsAdminAuthenticated,
        _ => false
    };

    public string? CreateSessionToken()
    {
        if (currentRole is null || !IsAuthenticated(currentRole))
        {
            return null;
        }

        var issuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return sessionProtector.Protect($"{currentRole}|{issuedAt}");
    }

    public bool RestoreAuthenticatedRole(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        try
        {
            var payload = sessionProtector.Unprotect(token);
            var parts = payload.Split('|', 2);
            if (parts.Length != 2 || !long.TryParse(parts[1], out var issuedAtSeconds))
            {
                return false;
            }

            var role = parts[0];
            var issuedAt = DateTimeOffset.FromUnixTimeSeconds(issuedAtSeconds);
            if (DateTimeOffset.UtcNow - issuedAt > SessionLifetime || GetAccount(role) is null)
            {
                return false;
            }

            SetAuthenticated(role, true);
            currentRole = role;
            NotifyChanged();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public PortalSessionInfo? GetActiveSession()
    {
        if (currentRole is not null && IsAuthenticated(currentRole))
        {
            return CreateSession(currentRole);
        }

        if (IsStudentAuthenticated) return CreateSession(PortalRoles.Student);
        if (IsTeacherAuthenticated) return CreateSession(PortalRoles.Teacher);
        if (IsAdminAuthenticated) return CreateSession(PortalRoles.Admin);
        return null;
    }

    public void SignOut(string role)
    {
        SetAuthenticated(role, false);
        if (currentRole == role)
        {
            currentRole = IsStudentAuthenticated ? PortalRoles.Student
                : IsTeacherAuthenticated ? PortalRoles.Teacher
                : IsAdminAuthenticated ? PortalRoles.Admin
                : null;
        }

        NotifyChanged();
    }

    private bool VerifyPassword(PortalAccountOptions account, string password)
    {
        var hash = account.PasswordHash;
        if (string.IsNullOrWhiteSpace(hash))
        {
            if (string.IsNullOrWhiteSpace(account.Password)) return false;
            hash = passwordHasher.HashPassword(account, account.Password);
        }

        return passwordHasher.VerifyHashedPassword(account, hash, password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    private PortalAccountOptions? GetAccount(string role) => role switch
    {
        PortalRoles.Student => store.Options.PortalAuth.Student,
        PortalRoles.Teacher => store.Options.PortalAuth.Teacher,
        PortalRoles.Admin => store.Options.PortalAuth.Admin,
        _ => null
    };

    private void SetAuthenticated(string role, bool value)
    {
        if (role == PortalRoles.Student) IsStudentAuthenticated = value;
        else if (role == PortalRoles.Teacher) IsTeacherAuthenticated = value;
        else if (role == PortalRoles.Admin) IsAdminAuthenticated = value;
    }

    private void NotifyChanged() => Changed?.Invoke();

    private PortalSessionInfo CreateSession(string role)
    {
        var account = GetAccount(role)!;
        var label = role switch
        {
            PortalRoles.Student => "Student",
            PortalRoles.Teacher => "Teacher",
            PortalRoles.Admin => "Administrator",
            _ => "User"
        };

        return new PortalSessionInfo(
            role, label, account.DisplayName, account.ClassName, account.Gender,
            account.GuardianName, account.MobileNumber, account.ClassTeacher,
            account.SchoolJoinedYear, account.ClassDealingWith, account.Subject,
            account.Qualification);
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
    public const string Admin = "admin";
}
