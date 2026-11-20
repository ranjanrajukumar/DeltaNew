using System.Text.Json.Nodes;

namespace SchoolERP.Application.Modules.Boards;

/// <summary>
/// Starter values used ONLY to fill an empty rules table the first time (the Node app's boards/boardStarterRules.js and
/// academics/subjectStarters.js). They are not official board rules: they are saved as unverified, with no source, and an
/// administrator replaces them (and records the source) under Board rules.
/// </summary>
public static class BoardStarters
{
    public const string StarterNote = "Starter value, not verified against the board";

    /// <summary>[name, periodsPerWeek, practical?] per stage, per board.</summary>
    private static readonly Dictionary<string, Dictionary<string, (string Name, int Periods, bool Practical)[]>> Subjects = new()
    {
        ["CBSE"] = new()
        {
            ["pre"] = [("English", 5, false), ("Hindi", 5, false), ("Mathematics", 5, false), ("Environmental Studies", 4, false), ("Rhymes and Story", 3, false), ("Art and Craft", 3, false), ("Play and Sports", 3, false)],
            ["lower"] = [("English", 6, false), ("Hindi", 5, false), ("Mathematics", 6, false), ("Environmental Studies", 4, false), ("Computer Science", 2, false), ("Art Education", 2, false), ("Physical Education", 3, false), ("General Knowledge", 2, false)],
            ["upper"] = [("English", 6, false), ("Hindi", 5, false), ("Mathematics", 6, false), ("Science", 5, false), ("Social Science", 5, false), ("Computer Science", 2, false), ("Art Education", 2, false), ("Physical Education", 3, false), ("Value Education", 1, false)],
            ["mid"] = [("English", 6, false), ("Hindi", 5, false), ("Mathematics", 6, false), ("Science", 6, false), ("Social Science", 5, false), ("Sanskrit", 3, false), ("Computer Science", 2, false), ("Art Education", 1, false), ("Physical Education", 2, false), ("Skill Education", 2, false)],
            ["sec"] = [("English", 6, false), ("Hindi", 5, false), ("Mathematics", 7, false), ("Science", 6, false), ("Social Science", 6, false), ("Information Technology", 2, false), ("Physical Education", 2, false), ("Science Practical", 2, true), ("Art Education", 2, false)],
            ["sr"] = [("English Core", 6, false), ("Physics", 5, false), ("Chemistry", 5, false), ("Mathematics", 6, false), ("Physics Practical", 2, true), ("Chemistry Practical", 2, true), ("Computer Science", 4, false), ("Physical Education", 2, false), ("General Studies", 2, false)],
        },
        ["UP Board"] = new()
        {
            ["pre"] = [("Hindi", 5, false), ("English", 5, false), ("Mathematics", 5, false), ("Environmental Studies", 4, false), ("Rhymes and Story", 3, false), ("Drawing", 3, false), ("Play and Sports", 3, false)],
            ["lower"] = [("Hindi", 6, false), ("English", 5, false), ("Mathematics", 6, false), ("Environmental Studies", 4, false), ("Drawing", 2, false), ("Physical Education", 3, false), ("Moral Education", 2, false), ("Computer", 2, false)],
            ["upper"] = [("Hindi", 6, false), ("English", 5, false), ("Mathematics", 6, false), ("Science", 5, false), ("Social Science", 5, false), ("Sanskrit", 3, false), ("Drawing", 2, false), ("Physical Education", 3, false), ("Computer", 2, false)],
            ["mid"] = [("Hindi", 6, false), ("English", 5, false), ("Mathematics", 6, false), ("Science", 6, false), ("Social Science", 6, false), ("Sanskrit", 4, false), ("Drawing", 2, false), ("Physical Education", 2, false), ("Computer", 2, false), ("Home Science", 2, false)],
            ["sec"] = [("Hindi", 6, false), ("English", 5, false), ("Mathematics", 7, false), ("Science", 6, false), ("Social Science", 6, false), ("Sanskrit", 4, false), ("Drawing", 2, false), ("Physical Education", 2, false), ("Science Practical", 2, true)],
            ["sr"] = [("General Hindi", 5, false), ("English", 5, false), ("Physics", 5, false), ("Chemistry", 5, false), ("Mathematics", 6, false), ("Physics Practical", 2, true), ("Chemistry Practical", 2, true), ("Physical Education", 2, false), ("Computer", 3, false)],
        },
    };

    public static IEnumerable<string> BoardNames => Subjects.Keys;

    /// <summary>{ subjects: [{ name, periodsPerWeek, component? }] } for one board and stage.</summary>
    public static JsonObject SubjectTemplate(string board, string stage)
    {
        var list = new JsonArray();
        foreach (var (name, periods, practical) in Subjects[board][stage])
        {
            var item = new JsonObject { ["name"] = name, ["periodsPerWeek"] = periods };
            if (practical) item["component"] = "practical"; // [name, p] leaves component undefined, as the Node starter did
            list.Add(item);
        }
        return new JsonObject { ["subjects"] = list };
    }

    private static JsonObject Exam(string name, int max, int weight) => new() { ["name"] = name, ["maxMarks"] = max, ["weight"] = weight };

    public static JsonObject ExamPattern(string board) => board == "CBSE"
        ? new JsonObject
        {
            ["groupName"] = "Annual result", ["skipStages"] = new JsonArray("pre"),
            ["exams"] = new JsonArray(Exam("Periodic Test 1", 20, 10), Exam("Half-Yearly Exam", 80, 30), Exam("Periodic Test 2", 20, 10), Exam("Annual Exam", 80, 50)),
        }
        : new JsonObject
        {
            ["groupName"] = "Annual result", ["skipStages"] = new JsonArray("pre"),
            ["exams"] = new JsonArray(Exam("Quarterly (Trimasik)", 100, 20), Exam("Half-Yearly (Ardhvarshik)", 100, 30), Exam("Annual (Varshik)", 100, 50)),
        };

    private static JsonObject Band(string grade, int min) => new() { ["grade"] = grade, ["min"] = min };

    public static JsonObject Grading(string board) => board == "CBSE"
        ? new JsonObject
        {
            ["passPercent"] = 33,
            ["bands"] = new JsonArray(Band("A1", 91), Band("A2", 81), Band("B1", 71), Band("B2", 61), Band("C1", 51), Band("C2", 41), Band("D", 33), Band("E1", 21), Band("E2", 0)),
        }
        : new JsonObject
        {
            ["passPercent"] = 33,
            ["bands"] = new JsonArray(Band("Dist.", 75), Band("I Div", 60), Band("II Div", 45), Band("III Div", 33), Band("Fail", 0)),
        };

    /// <summary>Promotion thresholds. A null limit means that check is not applied.</summary>
    public static JsonObject Promotion() => new() { ["minOverallPercent"] = 33, ["requirePass"] = true, ["minAttendancePercent"] = 75 };

    /// <summary>What a board's report card shows. The wording and fields are the school's to set; nothing here is an official format.</summary>
    public static JsonObject ReportCard() => new()
    {
        ["title"] = "Report card", ["showPercentage"] = true, ["showGrade"] = true, ["showRank"] = true,
        ["showAttendance"] = true, ["showAffiliation"] = true, ["footerNote"] = "",
    };
}
