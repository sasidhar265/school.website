using SchoolConnect.Shared.Models;

namespace SchoolConnect.Shared.Configuration;

public sealed class SchoolConnectOptions
{
    public SchoolIdentityOptions School { get; set; } = new();

    public PortalAuthOptions PortalAuth { get; set; } = new();

    public List<Notice> Notices { get; set; } = [];

    public List<AcademicEvent> AcademicEvents { get; set; } = [];

    public List<QuickAction> QuickActions { get; set; } = [];

    public List<RoleModule> StudentModules { get; set; } = [];

    public List<RoleModule> TeacherModules { get; set; } = [];

    public List<BusRoute> BusRoutes { get; set; } = [];

    public List<FacultyContact> FacultyContacts { get; set; } = [];

    public AboutUsOptions AboutUs { get; set; } = new();

    public List<StudentTimetableOptions> StudentTimetables { get; set; } = [];

    public List<StudentCurriculumOptions> StudentCurricula { get; set; } = [];

    public List<StudentContentOptions> StudentContents { get; set; } = [];

    public List<StudentProgressOptions> StudentProgresses { get; set; } = [];

    public List<GalleryYearGroupOptions> GalleryYearGroups { get; set; } = [];
}

public sealed class SchoolIdentityOptions
{
    public string Name { get; set; } = string.Empty;

    public string ShortName { get; set; } = string.Empty;

    public string Tagline { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string AdmissionsBanner { get; set; } = string.Empty;

    public string AdmissionsHeadline { get; set; } = string.Empty;

    public string CampusLabel { get; set; } = string.Empty;

    public string CampusMapTitle { get; set; } = string.Empty;

    public string BoardLabel { get; set; } = string.Empty;

    public string StateLabel { get; set; } = string.Empty;
}

public sealed class PortalAuthOptions
{
    public PortalAccountOptions Student { get; set; } = new();

    public PortalAccountOptions Teacher { get; set; } = new();
}

public sealed class PortalAccountOptions
{
    public string Pin { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string ClassName { get; set; } = string.Empty;

    public string Gender { get; set; } = string.Empty;

    public string GuardianName { get; set; } = string.Empty;

    public string MobileNumber { get; set; } = string.Empty;

    public string ClassTeacher { get; set; } = string.Empty;

    public string SchoolJoinedYear { get; set; } = string.Empty;

    public string ClassDealingWith { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Qualification { get; set; } = string.Empty;
}

public sealed class AboutUsOptions
{
    public string Title { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public List<string> Highlights { get; set; } = [];

    public List<string> Principles { get; set; } = [];
}

public sealed class StudentTimetableOptions
{
    public string ClassName { get; set; } = string.Empty;

    public List<string> PeriodLabels { get; set; } = [];

    public List<StudentTimetableDayOptions> Days { get; set; } = [];
}

public sealed class StudentTimetableDayOptions
{
    public string Day { get; set; } = string.Empty;

    public List<string> Lessons { get; set; } = [];
}

public sealed class StudentCurriculumOptions
{
    public string ClassName { get; set; } = string.Empty;

    public List<string> Subjects { get; set; } = [];

    public List<StudentCurriculumUnitOptions> Units { get; set; } = [];
}

public sealed class StudentCurriculumUnitOptions
{
    public string Subject { get; set; } = string.Empty;

    public List<string> Topics { get; set; } = [];

    public string Outcome { get; set; } = string.Empty;
}

public sealed class GalleryYearGroupOptions
{
    public string PassedOutYear { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<GalleryPhotoOptions> Photos { get; set; } = [];
}

public sealed class GalleryPhotoOptions
{
    public string Title { get; set; } = string.Empty;

    public string Caption { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;
}

public sealed class StudentContentOptions
{
    public string ClassName { get; set; } = string.Empty;

    public List<string> Categories { get; set; } = [];

    public List<StudentContentItemOptions> Items { get; set; } = [];
}

public sealed class StudentContentItemOptions
{
    public string Category { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Detail { get; set; } = string.Empty;

    public string ActionLabel { get; set; } = string.Empty;
}

public sealed class StudentProgressOptions
{
    public string ClassName { get; set; } = string.Empty;

    public string Attendance { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;

    public List<StudentProgressSubjectOptions> Subjects { get; set; } = [];
}

public sealed class StudentProgressSubjectOptions
{
    public string Subject { get; set; } = string.Empty;

    public string Score { get; set; } = string.Empty;

    public string Remark { get; set; } = string.Empty;
}
