namespace SchoolConnect.Tests;

[TestFixture]
public sealed class SchoolContentServiceTests
{
    [Test]
    public void GetProfile_ProjectsConfiguredSchoolIdentityAndContactDetails()
    {
        var service = new SchoolConnect.Shared.Services.SchoolContentService(TestData.CreateStore());

        var profile = service.GetProfile();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(profile.Name, Is.EqualTo("Sri Venkateswara Convent"));
            Assert.That(profile.ShortName, Is.EqualTo("SVC"));
            Assert.That(profile.Location, Is.EqualTo("Bayyavaram, Andhra Pradesh"));
            Assert.That(profile.Phone, Is.EqualTo("+91 98765 43210"));
            Assert.That(profile.Email, Is.EqualTo("office@example.edu"));
            Assert.That(profile.AdmissionsHeadline, Does.Contain("2026-2027"));
            Assert.That(profile.BoardLabel, Is.EqualTo("State Board"));
            Assert.That(profile.Languages, Is.EquivalentTo(new[] { "English Medium" }));
        }
    }

    [Test]
    public void GetProfile_ProjectsHomepageBusinessSections()
    {
        var service = new SchoolConnect.Shared.Services.SchoolContentService(TestData.CreateStore());

        var profile = service.GetProfile();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(profile.Notices.Single().Title, Is.EqualTo("Unit Test Timetable Published"));
            Assert.That(profile.AcademicEvents.Single().Title, Is.EqualTo("Science Lab Orientation"));
            Assert.That(profile.QuickActions.Single().Route, Is.EqualTo("/admissions"));
            Assert.That(profile.StudentModules.Single().Title, Is.EqualTo("Student Profile"));
            Assert.That(profile.TeacherModules.Single().Title, Is.EqualTo("Teacher Profile"));
            Assert.That(profile.FacultyContacts.Single().Role, Is.EqualTo("Principal"));
            Assert.That(profile.AboutUs.Highlights, Does.Contain("35+ years of excellence"));
        }
    }

    [Test]
    public void GetProfile_ProjectsStudentAcademicDataWithoutSharingMutableOptionLists()
    {
        var service = new SchoolConnect.Shared.Services.SchoolContentService(TestData.CreateStore());

        var profile = service.GetProfile();

        var timetable = profile.StudentTimetables.Single();
        var curriculum = profile.StudentCurricula.Single();
        var content = profile.StudentContents.Single();
        var progress = profile.StudentProgresses.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(timetable.ClassName, Is.EqualTo("Class 5"));
            Assert.That(timetable.PeriodLabels, Is.EqualTo(new[] { "Period 1", "Period 2" }));
            Assert.That(timetable.Days.Single().Lessons, Is.EqualTo(new[] { "Maths", "Science" }));

            Assert.That(curriculum.Subjects, Is.EqualTo(new[] { "Science" }));
            Assert.That(curriculum.Units.Single().Topics, Is.EqualTo(new[] { "Plants" }));
            Assert.That(curriculum.Units.Single().Outcome, Is.EqualTo("Understand plant life."));

            Assert.That(content.Categories, Is.EqualTo(new[] { "Worksheet" }));
            Assert.That(content.Items.Single().ActionLabel, Is.EqualTo("Open"));

            Assert.That(progress.Attendance, Is.EqualTo("96%"));
            Assert.That(progress.Subjects.Single().Score, Is.EqualTo("A"));
        }
    }

    [Test]
    public void GetProfile_ProjectsGalleryYearGroupsForAlumniMemories()
    {
        var service = new SchoolConnect.Shared.Services.SchoolContentService(TestData.CreateStore());

        var galleryGroup = service.GetProfile().GalleryYearGroups.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(galleryGroup.PassedOutYear, Is.EqualTo("2025"));
            Assert.That(galleryGroup.Title, Is.EqualTo("Batch 2025"));
            Assert.That(galleryGroup.Photos.Single().ImageUrl, Is.EqualTo("annual-day.jpg"));
        }
    }
}
