using SchoolConnect.Shared.Models;
using SchoolConnect.Shared.Services;

namespace SchoolConnect.Web;

public static class SchoolApiEndpoints
{
    public static IEndpointRouteBuilder MapSchoolApi(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api")
            .WithTags("SchoolConnect API");

        api.MapGet("/health", () => Results.Ok(new
        {
            status = "healthy",
            service = "SchoolConnect",
            timestampUtc = DateTimeOffset.UtcNow
        }));

        api.MapGet("/school", (SchoolContentService content) =>
        {
            var school = content.GetProfile();
            return Results.Ok(new
            {
                school.Name,
                school.ShortName,
                school.Tagline,
                school.Location,
                school.Phone,
                school.Email,
                school.AdmissionsBanner,
                school.AdmissionsHeadline,
                school.CampusLabel,
                school.CampusMapTitle,
                school.BoardLabel,
                school.StateLabel,
                school.Languages
            });
        });

        api.MapGet("/notices", (SchoolContentService content) => Results.Ok(content.GetProfile().Notices));
        api.MapGet("/events", (SchoolContentService content) => Results.Ok(content.GetProfile().AcademicEvents));
        api.MapGet("/faculty", (SchoolContentService content) => Results.Ok(content.GetProfile().FacultyContacts));

        api.MapGet("/classes", (SchoolContentService content) =>
        {
            var school = content.GetProfile();
            var classes = school.StudentCurricula.Select(item => item.ClassName)
                .Concat(school.StudentContents.Select(item => item.ClassName))
                .Concat(school.StudentTimetables.Select(item => item.ClassName))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(ClassSortKey)
                .ThenBy(item => item, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            return Results.Ok(classes);
        });

        api.MapGet("/classes/{className}/study-content", (string className, SchoolContentService content) =>
        {
            var studyContent = content.GetProfile().StudentContents.FirstOrDefault(item =>
                string.Equals(item.ClassName, className, StringComparison.OrdinalIgnoreCase));

            return studyContent is null
                ? Results.NotFound(new ApiError("class_not_found", "No study content was found for the requested class."))
                : Results.Ok(ToStudentSafeContent(studyContent));
        });

        api.MapGet("/classes/{className}/curriculum", (string className, SchoolContentService content) =>
        {
            var curriculum = content.GetProfile().StudentCurricula.FirstOrDefault(item =>
                string.Equals(item.ClassName, className, StringComparison.OrdinalIgnoreCase));

            return curriculum is null
                ? Results.NotFound(new ApiError("class_not_found", "No curriculum was found for the requested class."))
                : Results.Ok(curriculum);
        });

        return endpoints;
    }

    private static int ClassSortKey(string className) => className switch
    {
        "Nursery" => 0,
        "LKG" => 1,
        "UKG" => 2,
        _ when className.StartsWith("Class ", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(className[6..], out var number) => number + 2,
        _ => int.MaxValue
    };

    private static object ToStudentSafeContent(StudentStudyContent content) => new
    {
        content.ClassName,
        content.Categories,
        Items = content.Items.Select(item => new
        {
            item.Category,
            item.Title,
            item.Detail,
            item.ActionLabel,
            item.Subject,
            AssessmentQuestions = item.AssessmentQuestions.Select(question => new
            {
                question.Number,
                question.Prompt,
                question.Marks,
                question.Choices
            })
        })
    };

    private sealed record ApiError(string Code, string Message);
}
