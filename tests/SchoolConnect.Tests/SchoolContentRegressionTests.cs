using SchoolConnect.Shared.Services;

namespace SchoolConnect.Tests;

[TestFixture]
public sealed class SchoolContentRegressionTests
{
    [Test]
    public void GetProfile_RepeatedProjectionDoesNotDuplicateGeneratedAssessmentContent()
    {
        var service = new SchoolContentService(TestData.CreateStore());

        var first = service.GetProfile().StudentContents.Single();
        var second = service.GetProfile().StudentContents.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(first.Categories.Count(category => category == "Assessments"), Is.EqualTo(1));
            Assert.That(second.Categories.Count(category => category == "Assessments"), Is.EqualTo(1));
            Assert.That(second.Items.Count(item => item.Category == "Assessments"), Is.EqualTo(1));
            var firstQuestions = first.Items.Single(item => item.Category == "Assessments").AssessmentQuestions;
            var secondQuestions = second.Items.Single(item => item.Category == "Assessments").AssessmentQuestions;
            Assert.That(secondQuestions.Select(question => new
            {
                question.Number,
                question.Prompt,
                question.Marks,
                Choices = string.Join("|", question.Choices),
                question.CorrectAnswer,
                question.Explanation
            }), Is.EqualTo(firstQuestions.Select(question => new
            {
                question.Number,
                question.Prompt,
                question.Marks,
                Choices = string.Join("|", question.Choices),
                question.CorrectAnswer,
                question.Explanation
            })));
        }
    }

    [Test]
    public void GetProfile_AssessmentQuestionNumbersAnswersAndMarksRemainInternallyConsistent()
    {
        var service = new SchoolContentService(TestData.CreateStore());
        var questions = service.GetProfile().StudentContents.Single().Items
            .Single(item => item.Category == "Assessments").AssessmentQuestions;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(questions.Select(question => question.Number), Is.EqualTo(Enumerable.Range(1, questions.Count)));
            Assert.That(questions, Has.All.Matches<SchoolConnect.Shared.Models.StudentAssessmentQuestion>(question => question.Marks > 0));
            Assert.That(questions, Has.All.Matches<SchoolConnect.Shared.Models.StudentAssessmentQuestion>(question => question.Choices.Count >= 4));
            Assert.That(questions, Has.All.Matches<SchoolConnect.Shared.Models.StudentAssessmentQuestion>(question => question.Choices.Distinct(StringComparer.OrdinalIgnoreCase).Count() == question.Choices.Count));
            Assert.That(questions, Has.All.Matches<SchoolConnect.Shared.Models.StudentAssessmentQuestion>(question => question.Choices.Contains(question.CorrectAnswer)));
            Assert.That(questions, Has.All.Matches<SchoolConnect.Shared.Models.StudentAssessmentQuestion>(question => !string.IsNullOrWhiteSpace(question.Explanation)));
        }
    }

    [Test]
    public void GetProfile_MissingFacultyPhotoRemainsBackwardCompatible()
    {
        var faculty = new SchoolContentService(TestData.CreateStore()).GetProfile().FacultyContacts.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(faculty.ImageUrl, Is.Empty);
            Assert.That(faculty.Name, Is.Not.Empty);
            Assert.That(faculty.Phone, Is.Not.Empty);
        }
    }

    [Test]
    public void SaveEditableContent_WritesValidCompleteJsonAndLeavesNoTemporaryFile()
    {
        var contentFile = Path.Combine(Path.GetTempPath(), $"schoolconnect-regression-{Guid.NewGuid():N}.json");
        var overrides = new Dictionary<string, string?> { ["SchoolConnectContentFile"] = contentFile };

        try
        {
            var service = new SchoolContentService(TestData.CreateStore(overrides));
            var editable = service.GetEditableContent();
            editable.Notices[0] = editable.Notices[0] with { Title = "Regression save notice" };

            service.SaveEditableContent(editable);

            var json = File.ReadAllText(contentFile);
            var roundTrip = System.Text.Json.JsonSerializer.Deserialize<SchoolConnect.Shared.Configuration.SchoolConnectOptions>(json);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(roundTrip, Is.Not.Null);
                Assert.That(roundTrip!.Notices.Single().Title, Is.EqualTo("Regression save notice"));
                Assert.That(roundTrip.School.Name, Is.EqualTo("Sri Venkateswara Convent"));
                Assert.That(roundTrip.PortalAuth.Admin.Password, Is.Empty);
                Assert.That(File.Exists(contentFile + ".tmp"), Is.False);
            }
        }
        finally
        {
            File.Delete(contentFile);
            File.Delete(contentFile + ".tmp");
        }
    }

    [Test]
    public void GetProfile_EarlyYearsPaintingCategoryIsAddedOnlyOnce()
    {
        var overrides = new Dictionary<string, string?>
        {
            ["SchoolConnect:StudentContents:0:ClassName"] = "Nursery",
            ["SchoolConnect:StudentContents:0:Categories:1"] = "Painting & Drawing",
            ["SchoolConnect:StudentCurricula:0:ClassName"] = "Nursery"
        };
        var content = new SchoolContentService(TestData.CreateStore(overrides)).GetProfile().StudentContents.Single();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(content.Categories.Count(category => category == "Painting & Drawing"), Is.EqualTo(1));
            Assert.That(content.Items.Count(item => item.Category == "Painting & Drawing"), Is.EqualTo(1));
            Assert.That(content.Items, Has.None.Property("Category").EqualTo("Assessments"));
        }
    }

    [TestCase("https://cdn.example.edu/faculty/photo.jpg", true)]
    [TestCase("_content/SchoolConnect.Shared/photo.jpg", true)]
    [TestCase("/images/faculty/photo.jpg", true)]
    [TestCase("images/faculty/photo.jpg", true)]
    [TestCase("http://example.edu/photo.jpg", false)]
    [TestCase("javascript:alert(1)", false)]
    [TestCase("data:image/svg+xml,<svg></svg>", false)]
    [TestCase("//tracker.example/photo.jpg", false)]
    [TestCase("../private/photo.jpg", false)]
    [TestCase("images\\photo.jpg", false)]
    public void SafeImageUrl_AllowsOnlyHttpsOrSafeLocalPaths(string url, bool expected)
    {
        Assert.That(SafeImageUrl.IsAllowed(url), Is.EqualTo(expected));
    }
}
