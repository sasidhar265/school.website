using SchoolConnect.Shared.Models;

namespace SchoolConnect.Shared.Services;

public sealed class SchoolContentService
{
    private readonly SchoolContentStore store;

    public SchoolContentService(SchoolContentStore store)
    {
        this.store = store;
    }

    public SchoolProfile GetProfile()
    {
        var options = store.Options;
        var school = options.School;

        return new SchoolProfile(
            Name: school.Name,
            ShortName: school.ShortName,
            Tagline: school.Tagline,
            Location: school.Location,
            Phone: school.Phone,
            Email: school.Email,
            AdmissionsBanner: school.AdmissionsBanner,
            AdmissionsHeadline: school.AdmissionsHeadline,
            CampusLabel: school.CampusLabel,
            CampusMapTitle: school.CampusMapTitle,
            BoardLabel: school.BoardLabel,
            StateLabel: school.StateLabel,
            Languages: ["English Medium"],
            Notices: options.Notices,
            AcademicEvents: options.AcademicEvents,
            QuickActions: options.QuickActions,
            StudentModules: options.StudentModules,
            TeacherModules: options.TeacherModules,
            BusRoutes: options.BusRoutes,
            FacultyContacts: options.FacultyContacts,
            AboutUs: new AboutUsSection(
                options.AboutUs.Title,
                options.AboutUs.Summary,
                options.AboutUs.Highlights.ToArray(),
                options.AboutUs.Principles.ToArray()),
            StudentTimetables: options.StudentTimetables.Select(tt => new StudentTimetable(
                tt.ClassName,
                tt.PeriodLabels.ToArray(),
                tt.Days.Select(day => new StudentTimetableDay(day.Day, day.Lessons.ToArray())).ToArray())).ToArray(),
            StudentCurricula: options.StudentCurricula.Select(curriculum => new StudentCurriculum(
                curriculum.ClassName,
                curriculum.Subjects.ToArray(),
                curriculum.Units.Select(unit => new StudentCurriculumUnit(
                    unit.Subject,
                    unit.Topics.ToArray(),
                    unit.Outcome)).ToArray())).ToArray(),
            StudentContents: options.StudentContents.Select(content => new StudentStudyContent(
                content.ClassName,
                content.Categories.ToArray(),
                content.Items.Select(item => new StudentStudyContentItem(
                    item.Category,
                    item.Title,
                    item.Detail,
                    item.ActionLabel)).ToArray())).ToArray(),
            StudentProgresses: options.StudentProgresses.Select(progress => new StudentProgressSummary(
                progress.ClassName,
                progress.Attendance,
                progress.Status,
                progress.Remark,
                progress.Subjects.Select(subject => new StudentProgressSubject(
                    subject.Subject,
                    subject.Score,
                    subject.Remark)).ToArray())).ToArray());
    }
}
