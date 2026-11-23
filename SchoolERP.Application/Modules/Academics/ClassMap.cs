namespace SchoolERP.Application.Modules.Academics;

/// <summary>A class's subject map: each subject with its teacher, how it is taught and how many students take it.</summary>
public sealed record ClassMap(int ClassId, string ClassName, int Students, IReadOnlyList<SubjectRow> Subjects);
