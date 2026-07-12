using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using SchoolConnect.Shared.Configuration;
using SchoolConnect.Shared.Models;
using SchoolConnect.Shared.Services;

namespace SchoolConnect.Tests;

internal static class TestData
{
    public static SchoolContentStore CreateStore(IReadOnlyDictionary<string, string?>? overrides = null)
    {
        var values = CreateConfiguration();
        if (overrides is not null)
        {
            foreach (var item in overrides)
            {
                values[item.Key] = item.Value;
            }
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new SchoolContentStore(configuration, NullLogger<SchoolContentStore>.Instance);
    }

    public static PortalSessionService CreatePortalSession()
    {
        return new PortalSessionService(
            CreateStore(),
            new PasswordHasher<PortalAccountOptions>(),
            new EphemeralDataProtectionProvider(),
            new LoginAttemptGuard(),
            NullLogger<PortalSessionService>.Instance);
    }

    private static Dictionary<string, string?> CreateConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["SchoolConnect:School:Name"] = "Sri Venkateswara Convent",
            ["SchoolConnect:School:ShortName"] = "SVC",
            ["SchoolConnect:School:Tagline"] = "Connected school portal",
            ["SchoolConnect:School:Location"] = "Bayyavaram, Andhra Pradesh",
            ["SchoolConnect:School:Phone"] = "+91 98765 43210",
            ["SchoolConnect:School:Email"] = "office@example.edu",
            ["SchoolConnect:School:AdmissionsBanner"] = "Admissions open",
            ["SchoolConnect:School:AdmissionsHeadline"] = "Ready to join the 2026-2027 academic year?",
            ["SchoolConnect:School:CampusLabel"] = "Bayyavaram Campus",
            ["SchoolConnect:School:CampusMapTitle"] = "Visit Sri Venkateswara Convent",
            ["SchoolConnect:School:BoardLabel"] = "State Board",
            ["SchoolConnect:School:StateLabel"] = "Andhra Pradesh",

            ["SchoolConnect:PortalAuth:Student:Pin"] = "STU1001",
            ["SchoolConnect:PortalAuth:Student:Password"] = "student@123",
            ["SchoolConnect:PortalAuth:Student:DisplayName"] = "Student User",
            ["SchoolConnect:PortalAuth:Student:ClassName"] = "Class 5",
            ["SchoolConnect:PortalAuth:Student:Gender"] = "Female",
            ["SchoolConnect:PortalAuth:Student:GuardianName"] = "Parent User",
            ["SchoolConnect:PortalAuth:Student:MobileNumber"] = "+91 90000 10001",
            ["SchoolConnect:PortalAuth:Student:ClassTeacher"] = "K. Ravi",
            ["SchoolConnect:PortalAuth:Student:SchoolJoinedYear"] = "2021",

            ["SchoolConnect:PortalAuth:Teacher:Pin"] = "TCH1001",
            ["SchoolConnect:PortalAuth:Teacher:Password"] = "teacher@123",
            ["SchoolConnect:PortalAuth:Teacher:DisplayName"] = "Teacher User",
            ["SchoolConnect:PortalAuth:Teacher:Gender"] = "Male",
            ["SchoolConnect:PortalAuth:Teacher:ClassDealingWith"] = "Class 5",
            ["SchoolConnect:PortalAuth:Teacher:Subject"] = "Science",
            ["SchoolConnect:PortalAuth:Teacher:Qualification"] = "B.Ed",

            ["SchoolConnect:PortalAuth:Admin:Pin"] = "ADMIN",
            ["SchoolConnect:PortalAuth:Admin:Password"] = "admin-test-password",
            ["SchoolConnect:PortalAuth:Admin:DisplayName"] = "School Administrator",
            ["SchoolConnect:PortalAuth:Admin:ClassName"] = "Administration",

            ["SchoolConnect:Notices:0:Date"] = "08 Jul 2026",
            ["SchoolConnect:Notices:0:Audience"] = "Students",
            ["SchoolConnect:Notices:0:Priority"] = "High",
            ["SchoolConnect:Notices:0:Title"] = "Unit Test Timetable Published",
            ["SchoolConnect:Notices:0:Summary"] = "Students can review timetable portions.",

            ["SchoolConnect:AcademicEvents:0:Date"] = "15 Jul",
            ["SchoolConnect:AcademicEvents:0:Title"] = "Science Lab Orientation",
            ["SchoolConnect:AcademicEvents:0:Description"] = "Hands-on safety briefing.",

            ["SchoolConnect:QuickActions:0:Title"] = "Admission Form",
            ["SchoolConnect:QuickActions:0:Description"] = "Submit an admission enquiry.",
            ["SchoolConnect:QuickActions:0:Route"] = "/admissions",
            ["SchoolConnect:QuickActions:0:Metric"] = "Apply",

            ["SchoolConnect:StudentModules:0:Title"] = "Student Profile",
            ["SchoolConnect:StudentModules:0:Description"] = "Profile and school details.",
            ["SchoolConnect:StudentModules:0:Tasks:0"] = "View profile",

            ["SchoolConnect:TeacherModules:0:Title"] = "Teacher Profile",
            ["SchoolConnect:TeacherModules:0:Description"] = "Teacher dashboard.",
            ["SchoolConnect:TeacherModules:0:Tasks:0"] = "Review class",

            ["SchoolConnect:BusRoutes:0:Name"] = "Route A",
            ["SchoolConnect:BusRoutes:0:StartsAt"] = "8:00 AM",
            ["SchoolConnect:BusRoutes:0:Areas"] = "Bayyavaram",
            ["SchoolConnect:BusRoutes:0:Driver"] = "Driver User",

            ["SchoolConnect:FacultyContacts:0:Name"] = "S. Priya",
            ["SchoolConnect:FacultyContacts:0:Role"] = "Principal",
            ["SchoolConnect:FacultyContacts:0:Phone"] = "+91 90000 10003",

            ["SchoolConnect:AboutUs:Title"] = "Welcome to Our School",
            ["SchoolConnect:AboutUs:Summary"] = "A legacy of primary education.",
            ["SchoolConnect:AboutUs:Highlights:0"] = "35+ years of excellence",
            ["SchoolConnect:AboutUs:Principles:0"] = "Discipline",

            ["SchoolConnect:StudentTimetables:0:ClassName"] = "Class 5",
            ["SchoolConnect:StudentTimetables:0:PeriodLabels:0"] = "Period 1",
            ["SchoolConnect:StudentTimetables:0:PeriodLabels:1"] = "Period 2",
            ["SchoolConnect:StudentTimetables:0:Days:0:Day"] = "Monday",
            ["SchoolConnect:StudentTimetables:0:Days:0:Lessons:0"] = "Maths",
            ["SchoolConnect:StudentTimetables:0:Days:0:Lessons:1"] = "Science",

            ["SchoolConnect:StudentCurricula:0:ClassName"] = "Class 5",
            ["SchoolConnect:StudentCurricula:0:Subjects:0"] = "Science",
            ["SchoolConnect:StudentCurricula:0:Units:0:Subject"] = "Science",
            ["SchoolConnect:StudentCurricula:0:Units:0:Topics:0"] = "Plants",
            ["SchoolConnect:StudentCurricula:0:Units:0:Outcome"] = "Understand plant life.",

            ["SchoolConnect:StudentContents:0:ClassName"] = "Class 5",
            ["SchoolConnect:StudentContents:0:Categories:0"] = "Worksheet",
            ["SchoolConnect:StudentContents:0:Items:0:Category"] = "Worksheet",
            ["SchoolConnect:StudentContents:0:Items:0:Title"] = "Science Worksheet",
            ["SchoolConnect:StudentContents:0:Items:0:Detail"] = "Practice questions.",
            ["SchoolConnect:StudentContents:0:Items:0:ActionLabel"] = "Open",

            ["SchoolConnect:StudentProgresses:0:ClassName"] = "Class 5",
            ["SchoolConnect:StudentProgresses:0:Attendance"] = "96%",
            ["SchoolConnect:StudentProgresses:0:Status"] = "On track",
            ["SchoolConnect:StudentProgresses:0:Remark"] = "Consistent performance.",
            ["SchoolConnect:StudentProgresses:0:Subjects:0:Subject"] = "Science",
            ["SchoolConnect:StudentProgresses:0:Subjects:0:Score"] = "A",
            ["SchoolConnect:StudentProgresses:0:Subjects:0:Remark"] = "Strong",

            ["SchoolConnect:GalleryYearGroups:0:PassedOutYear"] = "2025",
            ["SchoolConnect:GalleryYearGroups:0:Title"] = "Batch 2025",
            ["SchoolConnect:GalleryYearGroups:0:Description"] = "Class memories.",
            ["SchoolConnect:GalleryYearGroups:0:Photos:0:Title"] = "Annual Day",
            ["SchoolConnect:GalleryYearGroups:0:Photos:0:Caption"] = "Students on stage.",
            ["SchoolConnect:GalleryYearGroups:0:Photos:0:ImageUrl"] = "annual-day.jpg"
        };
    }
}
