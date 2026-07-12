using SchoolConnect.Shared.Services;

namespace SchoolConnect.Tests;

[TestFixture]
public sealed class PortalSessionServiceTests
{
    [Test]
    public void TrySignIn_WithValidStudentCredentials_AuthenticatesStudentAndPublishesProfile()
    {
        var session = TestData.CreatePortalSession();
        var changeCount = 0;
        session.Changed += () => changeCount++;

        var signedIn = session.TrySignIn(PortalRoles.Student, " stu1001 ", "student@123");
        var activeSession = session.GetActiveSession();

        Assert.That(signedIn, Is.True);
        Assert.That(changeCount, Is.EqualTo(1));
        Assert.That(session.IsAuthenticated(PortalRoles.Student), Is.True);
        Assert.That(session.IsAuthenticated(PortalRoles.Teacher), Is.False);
        Assert.That(activeSession, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activeSession!.Role, Is.EqualTo(PortalRoles.Student));
            Assert.That(activeSession.DisplayName, Is.EqualTo("Student User"));
            Assert.That(activeSession.ClassName, Is.EqualTo("Class 5"));
            Assert.That(activeSession.GuardianName, Is.EqualTo("Parent User"));
            Assert.That(activeSession.ClassTeacher, Is.EqualTo("K. Ravi"));
        }
    }

    [Test]
    public void TrySignIn_WithValidTeacherCredentials_AuthenticatesTeacherAndPublishesProfile()
    {
        var session = TestData.CreatePortalSession();

        var signedIn = session.TrySignIn(PortalRoles.Teacher, "TCH1001", "teacher@123");
        var activeSession = session.GetActiveSession();

        Assert.That(signedIn, Is.True);
        Assert.That(session.IsAuthenticated(PortalRoles.Teacher), Is.True);
        Assert.That(session.IsAuthenticated(PortalRoles.Student), Is.False);
        Assert.That(activeSession, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activeSession!.Role, Is.EqualTo(PortalRoles.Teacher));
            Assert.That(activeSession.DisplayName, Is.EqualTo("Teacher User"));
            Assert.That(activeSession.ClassDealingWith, Is.EqualTo("Class 5"));
            Assert.That(activeSession.Subject, Is.EqualTo("Science"));
            Assert.That(activeSession.Qualification, Is.EqualTo("B.Ed"));
        }
    }

    [Test]
    public void TrySignIn_WithInvalidCredentials_DoesNotAuthenticateOrNotify()
    {
        var session = TestData.CreatePortalSession();
        var changeCount = 0;
        session.Changed += () => changeCount++;

        var signedIn = session.TrySignIn(PortalRoles.Student, "STU1001", "wrong-password");

        Assert.That(signedIn, Is.False);
        Assert.That(changeCount, Is.Zero);
        Assert.That(session.GetActiveSession(), Is.Null);
        Assert.That(session.IsAuthenticated(PortalRoles.Student), Is.False);
    }

    [Test]
    public void RestoreAuthenticatedRole_RehydratesStudentSessionFromBrowserRole()
    {
        var session = TestData.CreatePortalSession();
        session.TrySignIn(PortalRoles.Student, "STU1001", "student@123");
        var token = session.CreateSessionToken();
        session.SignOut(PortalRoles.Student);

        var restored = session.RestoreAuthenticatedRole(token);
        var activeSession = session.GetActiveSession();

        Assert.That(restored, Is.True);
        Assert.That(activeSession, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(activeSession!.Role, Is.EqualTo(PortalRoles.Student));
            Assert.That(activeSession.DisplayName, Is.EqualTo("Student User"));
        }
    }

    [Test]
    public void RestoreAuthenticatedRole_WithTamperedToken_IsRejected()
    {
        var session = TestData.CreatePortalSession();

        var restored = session.RestoreAuthenticatedRole("admin|9999999999");

        Assert.That(restored, Is.False);
        Assert.That(session.GetActiveSession(), Is.Null);
    }

    [Test]
    public void TrySignIn_WithValidAdminCredentials_AuthenticatesAdministrator()
    {
        var session = TestData.CreatePortalSession();

        var signedIn = session.TrySignIn(PortalRoles.Admin, "ADMIN", "admin-test-password");

        Assert.That(signedIn, Is.True);
        Assert.That(session.IsAuthenticated(PortalRoles.Admin), Is.True);
        Assert.That(session.GetActiveSession()?.Role, Is.EqualTo(PortalRoles.Admin));
    }

    [Test]
    public void SignOut_WhenCurrentRoleIsSignedOut_ClearsActiveSession()
    {
        var session = TestData.CreatePortalSession();
        var changeCount = 0;
        session.Changed += () => changeCount++;

        session.TrySignIn(PortalRoles.Student, "STU1001", "student@123");
        session.SignOut(PortalRoles.Student);

        Assert.That(changeCount, Is.EqualTo(2));
        Assert.That(session.IsAuthenticated(PortalRoles.Student), Is.False);
        Assert.That(session.GetActiveSession(), Is.Null);
    }

    [Test]
    public void SignOut_WhenMultipleRolesAreAuthenticated_FallsBackToRemainingRole()
    {
        var session = TestData.CreatePortalSession();
        session.TrySignIn(PortalRoles.Student, "STU1001", "student@123");
        session.TrySignIn(PortalRoles.Teacher, "TCH1001", "teacher@123");

        session.SignOut(PortalRoles.Teacher);

        var activeSession = session.GetActiveSession();
        Assert.That(activeSession, Is.Not.Null);
        Assert.That(activeSession!.Role, Is.EqualTo(PortalRoles.Student));
    }

    [Test]
    public void TrySignIn_AfterRepeatedFailures_TemporarilyLocksRoleEvenWithCorrectPassword()
    {
        var session = TestData.CreatePortalSession();

        for (var attempt = 0; attempt < LoginAttemptGuard.MaximumFailures; attempt++)
        {
            Assert.That(session.TrySignIn(PortalRoles.Student, "STU1001", "incorrect"), Is.False);
        }

        Assert.That(session.TrySignIn(PortalRoles.Student, "STU1001", "student@123"), Is.False);
        Assert.That(session.IsAuthenticated(PortalRoles.Student), Is.False);
    }

    [Test]
    public void TrySignIn_FailuresForOneRole_DoNotLockAnotherRole()
    {
        var session = TestData.CreatePortalSession();
        for (var attempt = 0; attempt < LoginAttemptGuard.MaximumFailures; attempt++)
        {
            session.TrySignIn(PortalRoles.Student, "STU1001", "incorrect");
        }

        Assert.That(session.TrySignIn(PortalRoles.Teacher, "TCH1001", "teacher@123"), Is.True);
    }
}
