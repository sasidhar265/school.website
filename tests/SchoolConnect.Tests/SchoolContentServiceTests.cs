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

            Assert.That(content.Categories, Is.EqualTo(new[] { "Worksheet", "Assessments" }));
            Assert.That(content.Items.Single(item => item.Category == "Worksheet").ActionLabel, Is.EqualTo("Open"));
            var assessment = content.Items.Single(item => item.Category == "Assessments");
            Assert.That(assessment.Subject, Is.EqualTo("Science"));
            Assert.That(assessment.Detail, Does.Contain("Plants"));
            Assert.That(assessment.Detail, Does.Contain("20 marks"));

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

    [Test]
    public void GetProfile_GeneratesTenExplainedQuestionsAndTwentyMarksForFiveTopicSubject()
    {
        var overrides = new Dictionary<string, string?>
        {
            ["SchoolConnect:StudentCurricula:0:Units:0:Topics:1"] = "Animals",
            ["SchoolConnect:StudentCurricula:0:Units:0:Topics:2"] = "Habitats",
            ["SchoolConnect:StudentCurricula:0:Units:0:Topics:3"] = "Food chains",
            ["SchoolConnect:StudentCurricula:0:Units:0:Topics:4"] = "The environment"
        };
        var service = new SchoolConnect.Shared.Services.SchoolContentService(TestData.CreateStore(overrides));

        var assessment = service.GetProfile().StudentContents.Single().Items
            .Single(item => item.Category == "Assessments");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(assessment.AssessmentQuestions, Has.Count.EqualTo(10));
            Assert.That(assessment.AssessmentQuestions.Sum(question => question.Marks), Is.EqualTo(20));
            Assert.That(assessment.AssessmentQuestions.Select(question => question.Number), Is.EqualTo(Enumerable.Range(1, 10)));
            Assert.That(assessment.AssessmentQuestions, Has.All.Property(nameof(SchoolConnect.Shared.Models.StudentAssessmentQuestion.Explanation)).Not.Empty);
            Assert.That(assessment.AssessmentQuestions, Has.All.Matches<SchoolConnect.Shared.Models.StudentAssessmentQuestion>(question => question.Choices.Contains(question.CorrectAnswer)));
        }
    }

    [TestCase("Nursery")]
    [TestCase("LKG")]
    [TestCase("UKG")]
    public void GetProfile_ReplacesEarlyYearsAssessmentsWithPaintingAndDrawing(string className)
    {
        var overrides = new Dictionary<string, string?>
        {
            ["SchoolConnect:StudentCurricula:1:ClassName"] = className,
            ["SchoolConnect:StudentCurricula:1:Subjects:0"] = "Creative Play",
            ["SchoolConnect:StudentCurricula:1:Units:0:Subject"] = "Creative Play",
            ["SchoolConnect:StudentCurricula:1:Units:0:Topics:0"] = "Colour exploration",
            ["SchoolConnect:StudentContents:1:ClassName"] = className,
            ["SchoolConnect:StudentContents:1:Categories:0"] = "Worksheet",
            ["SchoolConnect:StudentContents:1:Categories:1"] = "Assessments",
            ["SchoolConnect:StudentContents:1:Items:0:Category"] = "Worksheet",
            ["SchoolConnect:StudentContents:1:Items:0:Title"] = "Tracing sheet",
            ["SchoolConnect:StudentContents:1:Items:0:ActionLabel"] = "Open"
        };
        var service = new SchoolConnect.Shared.Services.SchoolContentService(TestData.CreateStore(overrides));

        var content = service.GetProfile().StudentContents.Single(item => item.ClassName == className);
        var painting = content.Items.Single(item => item.Category == "Painting & Drawing");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content.Categories, Does.Contain("Painting & Drawing"));
            Assert.That(content.Categories, Does.Not.Contain("Assessments"));
            Assert.That(content.Items, Has.None.Property("Category").EqualTo("Assessments"));
            Assert.That(painting.Title, Is.EqualTo("Online Creative Canvas"));
            Assert.That(painting.ActionLabel, Is.EqualTo("Start drawing"));
            Assert.That(painting.AssessmentQuestions, Is.Empty);
        }
    }

    [Test]
    public void GetProfile_ProjectsFacultyPhotoWithContactDetails()
    {
        var service = new SchoolConnect.Shared.Services.SchoolContentService(TestData.CreateStore(
            new Dictionary<string, string?>
            {
                ["SchoolConnect:FacultyContacts:0:ImageUrl"] = "https://images.example.edu/principal.jpg"
            }));

        var faculty = service.GetProfile().FacultyContacts.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(faculty.Name, Is.EqualTo("S. Priya"));
            Assert.That(faculty.Role, Is.EqualTo("Principal"));
            Assert.That(faculty.Phone, Is.EqualTo("+91 90000 10003"));
            Assert.That(faculty.ImageUrl, Is.EqualTo("https://images.example.edu/principal.jpg"));
        }
    }

    [Test]
    public void SaveEditableContent_WithoutDatabase_PersistsLocallyAndKeepsConfiguredAuthentication()
    {
        var contentFile = Path.Combine(Path.GetTempPath(), $"schoolconnect-content-{Guid.NewGuid():N}.json");
        var overrides = new Dictionary<string, string?> { ["SchoolConnectContentFile"] = contentFile };

        try
        {
            var firstService = new SchoolConnect.Shared.Services.SchoolContentService(TestData.CreateStore(overrides));
            var edited = firstService.GetEditableContent();
            edited.School.Name = "Locally Edited School";
            edited.FacultyContacts[0] = edited.FacultyContacts[0] with { ImageUrl = "faculty/principal.jpg" };

            firstService.SaveEditableContent(edited);

            var reloadedStore = TestData.CreateStore(overrides);
            var reloadedService = new SchoolConnect.Shared.Services.SchoolContentService(reloadedStore);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(File.Exists(contentFile), Is.True);
                Assert.That(reloadedService.GetProfile().Name, Is.EqualTo("Locally Edited School"));
                Assert.That(reloadedService.GetProfile().FacultyContacts.Single().ImageUrl, Is.EqualTo("faculty/principal.jpg"));
                Assert.That(reloadedStore.Options.PortalAuth.Admin.Pin, Is.EqualTo("ADMIN"));
                Assert.That(reloadedStore.Options.PortalAuth.Admin.Password, Is.EqualTo("admin-test-password"));
                Assert.That(File.ReadAllText(contentFile), Does.Not.Contain("admin-test-password"));
            }
        }
        finally
        {
            File.Delete(contentFile);
            File.Delete(contentFile + ".tmp");
        }
    }
}
