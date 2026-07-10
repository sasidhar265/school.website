using SchoolConnect.Shared.Models;
using SchoolConnect.Shared.Configuration;

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
            StudentModules: options.StudentModules.Select(module => new RoleModule(
                module.Title,
                module.Description,
                module.Tasks.ToArray())).ToArray(),
            TeacherModules: options.TeacherModules.Select(module => new RoleModule(
                module.Title,
                module.Description,
                module.Tasks.ToArray())).ToArray(),
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
            StudentContents: BuildStudentContents(options),
            StudentProgresses: options.StudentProgresses.Select(progress => new StudentProgressSummary(
                progress.ClassName,
                progress.Attendance,
                progress.Status,
                progress.Remark,
                progress.Subjects.Select(subject => new StudentProgressSubject(
                    subject.Subject,
                    subject.Score,
                    subject.Remark)).ToArray())).ToArray(),
            GalleryYearGroups: options.GalleryYearGroups.Select(group => new GalleryYearGroup(
                group.PassedOutYear,
                group.Title,
                group.Description,
                group.Photos.Select(photo => new GalleryPhoto(
                    photo.Title,
                    photo.Caption,
                    photo.ImageUrl)).ToArray())).ToArray());
    }

    private static IReadOnlyList<StudentStudyContent> BuildStudentContents(SchoolConnectOptions options)
    {
        return options.StudentContents.Select(content =>
        {
            var items = content.Items.Select(item => new StudentStudyContentItem(
                item.Category,
                item.Title,
                item.Detail,
                item.ActionLabel,
                item.Subject)).ToList();

            var curriculum = options.StudentCurricula.FirstOrDefault(item =>
                string.Equals(item.ClassName, content.ClassName, StringComparison.OrdinalIgnoreCase));

            if (curriculum is not null)
            {
                foreach (var subject in curriculum.Subjects.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    var units = curriculum.Units
                        .Where(unit => string.Equals(unit.Subject, subject, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                    var topics = units.SelectMany(unit => unit.Topics).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                    var outcome = units.Select(unit => unit.Outcome).FirstOrDefault(text => !string.IsNullOrWhiteSpace(text));
                    var isEarlyYears = content.ClassName is "Nursery" or "LKG" or "UKG";
                    var format = isEarlyYears
                        ? "Activity-based observation • Teacher rubric • 20-25 minutes"
                        : "Formative assessment • 20 marks • 30 minutes";
                    var scope = topics.Length > 0 ? string.Join(", ", topics) : $"Core {subject} skills for {content.ClassName}";

                    var questions = BuildAssessmentQuestions(subject, topics, isEarlyYears);

                    if (questions.Length == 0)
                    {
                        var isTelugu = string.Equals(subject, "Telugu", StringComparison.OrdinalIgnoreCase);
                        var correctAnswer = isTelugu ? "తెలుగు పాఠ్యాంశం" : subject;
                        questions = [new StudentAssessmentQuestion(
                            1,
                            isTelugu ? "ఈ మూల్యాంకనం ఏ పాఠ్యాంశానికి సంబంధించినది?" : $"Which subject does this assessment cover?",
                            20,
                            isTelugu ? ["తెలుగు పాఠ్యాంశం", "గణితం", "విజ్ఞాన శాస్త్రం", "క్రీడలు"] : [subject, "Physical Education", "Music", "General Knowledge"],
                            correctAnswer)];
                    }

                    items.Add(new StudentStudyContentItem(
                        "Assessments",
                        $"{subject} Assessment",
                        $"{format}. Scope: {scope}. Learning check: {outcome ?? $"Demonstrate grade-appropriate understanding of {subject}."}",
                        "Start assessment",
                        subject)
                    {
                        AssessmentQuestions = questions
                    });
                }
            }

            var categories = content.Categories
                .Append("Assessments")
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return new StudentStudyContent(content.ClassName, categories, items);
        }).ToArray();
    }

    private static StudentAssessmentQuestion[] BuildAssessmentQuestions(string subject, string[] topics, bool isEarlyYears)
    {
        var isTelugu = string.Equals(subject, "Telugu", StringComparison.OrdinalIgnoreCase);
        var localizedTopics = isTelugu ? topics.Select(TranslateTeluguTopic).ToArray() : topics;
        var fallbackChoices = isTelugu
            ? new[] { "గణిత గణనలు", "క్రీడా నియమాలు", "సంగీత సాధన", "ఆంగ్ల వ్యాకరణం" }
            : new[] { "Sports training", "Music practice", "School transport", "Free play" };

        return localizedTopics.Take(5).Select((topic, index) =>
        {
            var otherTopics = localizedTopics.Where(candidate => !string.Equals(candidate, topic, StringComparison.OrdinalIgnoreCase));
            var choices = new[] { topic }
                .Concat(otherTopics)
                .Concat(fallbackChoices)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(4)
                .OrderBy(choice => StableChoiceOrder(choice, index))
                .ToArray();

            var prompt = isTelugu
                ? $"కింది వాటిలో తెలుగు పాఠ్యాంశానికి సంబంధించిన అంశాన్ని ఎంచుకోండి. (ప్రశ్న {index + 1})"
                : isEarlyYears
                    ? $"Which activity belongs to this {subject} learning section?"
                    : $"Which topic is included in this {subject} assessment section?";

            return new StudentAssessmentQuestion(index + 1, prompt, 4, choices, topic);
        }).ToArray();
    }

    private static int StableChoiceOrder(string choice, int questionIndex) =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(choice + questionIndex.ToString(System.Globalization.CultureInfo.InvariantCulture));

    private static string TranslateTeluguTopic(string topic) => topic switch
    {
        "Letter recognition and formation" => "అక్షరాల గుర్తింపు మరియు రాత",
        "Simple two-letter words" => "సరళమైన రెండక్షరాల పదాలు",
        "Picture vocabulary" => "చిత్ర పదజాలం",
        "Rhymes and oral expression" => "పద్యాలు మరియు మౌఖిక వ్యక్తీకరణ",
        "అచ్చులు and హల్లులు" => "అచ్చులు మరియు హల్లులు",
        "అచ్చులు, హల్లులు and గుణింతాలు" => "అచ్చులు, హల్లులు మరియు గుణింతాలు",
        "Simple words and sentences" => "సరళమైన పదాలు మరియు వాక్యాలు",
        "Picture-based vocabulary" => "చిత్ర ఆధారిత పదజాలం",
        "Short poems and stories" => "చిన్న పద్యాలు మరియు కథలు",
        "Copywriting and dictation" => "చూసి రాయడం మరియు శ్రుతలేఖనం",
        "గుణింతాలు and ఒత్తులు" => "గుణింతాలు మరియు ఒత్తులు",
        "Fluent word and sentence reading" => "పదాలు మరియు వాక్యాలను ధారాళంగా చదవడం",
        "Vocabulary and grammar foundations" => "పదజాలం మరియు వ్యాకరణ పునాదులు",
        "Poems and short prose" => "పద్యాలు మరియు చిన్న గద్య భాగాలు",
        "Dictation and guided composition" => "శ్రుతలేఖనం మరియు మార్గదర్శక రచన",
        "Prose, poetry and reading fluency" => "గద్యం, పద్యం మరియు పఠన నైపుణ్యం",
        "Vocabulary, synonyms and antonyms" => "పదజాలం, పర్యాయపదాలు మరియు వ్యతిరేక పదాలు",
        "Basic grammar and sentence structure" => "ప్రాథమిక వ్యాకరణం మరియు వాక్య నిర్మాణం",
        "Comprehension and oral expression" => "అవగాహన మరియు మౌఖిక వ్యక్తీకరణ",
        "Paragraph and letter writing" => "పేరా మరియు లేఖా రచన",
        "Prose and poetry appreciation" => "గద్య పద్యాల అవగాహన",
        "Grammar, verb forms and sentence types" => "వ్యాకరణం, క్రియా రూపాలు మరియు వాక్య రకాలు",
        "Idioms and vocabulary development" => "జాతీయాలు మరియు పదజాల అభివృద్ధి",
        "Unseen passage comprehension" => "అపరిచిత గద్య భాగ అవగాహన",
        "Essay, letter and creative writing" => "వ్యాసం, లేఖ మరియు సృజనాత్మక రచన",
        "Advanced prose and poetry comprehension" => "ఉన్నత గద్య పద్య అవగాహన",
        "Grammar and sentence analysis" => "వ్యాకరణం మరియు వాక్య విశ్లేషణ",
        "Proverbs, idioms and vocabulary" => "సామెతలు, జాతీయాలు మరియు పదజాలం",
        "Summary and unseen passage work" => "సారాంశం మరియు అపరిచిత గద్య భాగం",
        "Essays, formal letters and creative composition" => "వ్యాసాలు, అధికారిక లేఖలు మరియు సృజనాత్మక రచన",
        _ => topic.Replace(" and ", " మరియు ", StringComparison.OrdinalIgnoreCase)
    };
}
