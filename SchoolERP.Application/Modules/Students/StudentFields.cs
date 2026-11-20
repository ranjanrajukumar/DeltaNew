namespace SchoolERP.Application.Modules.Students;

/// <summary>
/// Every detail kept on a student's registration, in the order the form shows them (the Node app's academics/studentFields.js).
/// The same list drives validation, the API and the profile page. type: text | date | number; Max is the column length for text.
/// </summary>
public static class StudentFields
{
    public sealed record Field(string Key, string Label, string Type, int Max = 0);

    public static readonly IReadOnlyList<Field> Details =
    [
        new("middleName", "Middle name", "text", 80),
        new("admissionDate", "Date of admission", "date"),
        new("rollNo", "Roll number", "text", 20),
        new("nationality", "Nationality", "text", 50),
        new("religion", "Religion", "text", 50),
        new("category", "Category", "text", 30),
        new("motherTongue", "Mother tongue", "text", 50),
        new("nationalId", "Aadhaar / national ID no", "text", 20),
        new("email", "Student email", "text", 150),
        new("phone", "Student phone", "text", 20),
        new("address", "Address", "text", 255),
        new("city", "City / town", "text", 80),
        new("state", "State", "text", 80),
        new("pincode", "PIN / postal code", "text", 12),
        new("fatherName", "Father's name", "text", 120),
        new("fatherPhone", "Father's phone", "text", 20),
        new("fatherOccupation", "Father's occupation", "text", 100),
        new("motherName", "Mother's name", "text", 120),
        new("motherPhone", "Mother's phone", "text", 20),
        new("motherOccupation", "Mother's occupation", "text", 100),
        new("guardianRelation", "Guardian relation", "text", 40),
        new("guardianEmail", "Guardian email", "text", 150),
        new("previousSchool", "Previous school", "text", 160),
        new("previousClass", "Class last attended", "text", 50),
        new("secondaryGuardianName", "Secondary guardian", "text", 120),
        new("secondaryGuardianPhone", "Secondary contact", "text", 20),
        new("specialNeeds", "Known learning or medical needs", "text", 255),
        new("previousTcNo", "Transfer certificate no", "text", 40),
        new("previousPercent", "Last result (%)", "number"),
        new("remarks", "Remarks", "text", 500),
    ];

    /// <summary>The writable student fields, fixed in code and never taken from the request.</summary>
    public static readonly IReadOnlyList<string> Writable =
    [
        "admissionNo", "firstName", "lastName", "dob", "gender", "guardianName", "guardianPhone", "classId", "status",
        "registrationStatus", .. Details.Select(f => f.Key),
    ];
}
