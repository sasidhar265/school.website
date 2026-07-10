namespace SchoolConnect.Shared.Models;

public sealed record SchoolProfile(
    string Name,
    string ShortName,
    string Tagline,
    string Location,
    string Phone,
    string Email,
    string AdmissionsBanner,
    string AdmissionsHeadline,
    string CampusLabel,
    string CampusMapTitle,
    string BoardLabel,
    string StateLabel,
    string[] Languages,
    IReadOnlyList<Notice> Notices,
    IReadOnlyList<AcademicEvent> AcademicEvents,
    IReadOnlyList<QuickAction> QuickActions,
    IReadOnlyList<RoleModule> StudentModules,
    IReadOnlyList<RoleModule> TeacherModules,
    IReadOnlyList<BusRoute> BusRoutes,
    IReadOnlyList<FacultyContact> FacultyContacts,
    AboutUsSection AboutUs,
    IReadOnlyList<StudentTimetable> StudentTimetables,
    IReadOnlyList<StudentCurriculum> StudentCurricula,
    IReadOnlyList<StudentStudyContent> StudentContents,
    IReadOnlyList<StudentProgressSummary> StudentProgresses,
    IReadOnlyList<GalleryYearGroup> GalleryYearGroups);

public sealed record Notice(string Title, string Audience, string Date, string Summary, string Priority);

public sealed record AcademicEvent(string Date, string Title, string Description);

public sealed record QuickAction(string Title, string Description, string Route, string Metric);

public sealed record RoleModule(string Title, string Description, string[] Tasks);

public sealed record BusRoute(string Name, string StartsAt, string Areas, string Driver);

public sealed record FacultyContact(string Name, string Role, string Phone);

public sealed record AboutUsSection(string Title, string Summary, string[] Highlights, string[] Principles);

public sealed record StudentTimetable(string ClassName, string[] PeriodLabels, IReadOnlyList<StudentTimetableDay> Days);

public sealed record StudentTimetableDay(string Day, string[] Lessons);

public sealed record StudentCurriculum(string ClassName, string[] Subjects, IReadOnlyList<StudentCurriculumUnit> Units);

public sealed record StudentCurriculumUnit(string Subject, string[] Topics, string Outcome);

public sealed record StudentStudyContent(string ClassName, string[] Categories, IReadOnlyList<StudentStudyContentItem> Items);

public sealed record StudentStudyContentItem(string Category, string Title, string Detail, string ActionLabel, string Subject = "")
{
    public IReadOnlyList<StudentAssessmentQuestion> AssessmentQuestions { get; init; } = [];
}

public sealed record StudentAssessmentQuestion(
    int Number,
    string Prompt,
    int Marks,
    IReadOnlyList<string> Choices,
    string CorrectAnswer);

public sealed record StudentProgressSummary(
    string ClassName,
    string Attendance,
    string Status,
    string Remark,
    IReadOnlyList<StudentProgressSubject> Subjects);

public sealed record StudentProgressSubject(string Subject, string Score, string Remark);

public sealed record GalleryYearGroup(string PassedOutYear, string Title, string Description, IReadOnlyList<GalleryPhoto> Photos);

public sealed record GalleryPhoto(string Title, string Caption, string ImageUrl);
