using Dapper;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Interfaces.Repositories;
using SchoolERP.Application.Modules.Academics;
using static SchoolERP.Application.Interfaces.Repositories.IClassQueryRepository;

namespace SchoolERP.Infrastructure.Repositories.Queries;

public sealed class ClassQueryRepository(IDapperContext dapper) : QueryRepository(dapper), IClassQueryRepository
{
    public Task<List<ClassListRow>> ListAsync(CancellationToken ct = default) => QueryAsync<ClassListRow>("""
        SELECT c.id AS Id, c.name AS Name, c.branch_id AS BranchId, c.class_teacher_id AS ClassTeacherId, c.board_id AS BoardId,
               c.board AS Board, c.room AS Room, c.section AS Section, c.authadd AS Authadd, c.authedit AS Authedit,
               c.authdel AS Authdel, c.addondt AS Addondt, c.editondt AS Editondt, c.delondt AS Delondt, c.delstatus AS Delstatus,
               (SELECT COUNT(*) FROM students s WHERE s.class_id = c.id AND s.delstatus = 0) AS StudentCount,
               t.first_name + ' ' + t.last_name AS ClassTeacherName
        FROM classes c
        LEFT JOIN staff t ON t.id = c.class_teacher_id AND t.delstatus = 0
        WHERE c.delstatus = 0
        ORDER BY c.name, c.section
        """, null, ct);

    public Task<OverviewData> OverviewDataAsync(CancellationToken ct = default) => QueryMultipleAsync("""
        SELECT class_id AS ClassId, subject_id AS SubjectId, staff_id AS StaffId FROM class_subjects WHERE delstatus = 0;
        SELECT class_id AS ClassId, subject_id AS SubjectId FROM timetable_slots WHERE delstatus = 0;
        SELECT id AS Id, designation AS Designation FROM staff WHERE delstatus = 0 AND status = 'active';
        SELECT COUNT(*) FROM subjects WHERE delstatus = 0;
        """, null, async grid => new OverviewData(
            (await grid.ReadAsync<SubjectLink>()).AsList(),
            (await grid.ReadAsync<SlotSubject>()).AsList(),
            (await grid.ReadAsync<StaffDesignation>()).AsList(),
            await grid.ReadSingleAsync<int>()), ct);

    public Task<ClassMapData> ClassMapDataAsync(int classId, int? classTeacherId, CancellationToken ct = default) => QueryMultipleAsync("""
        SELECT first_name + ' ' + last_name FROM staff WHERE id = @classTeacherId AND delstatus = 0;
        SELECT COUNT(*) FROM students WHERE class_id = @classId AND status = 'active' AND delstatus = 0;
        SELECT id AS Id, title AS Title, amount AS Amount, due_date AS DueDate
            FROM fee_structures WHERE class_id = @classId AND delstatus = 0 ORDER BY id;
        SELECT TOP 20 id AS Id, title AS Title, kind AS Kind, start_date AS StartDate, status AS Status, class_id AS ClassId, fee AS Fee
            FROM activities
            WHERE delstatus = 0 AND status IN ('planned', 'open') AND (class_id IS NULL OR class_id = @classId)
            ORDER BY start_date;
        SELECT COUNT(*) FROM student_transport st
            JOIN students s ON s.id = st.student_id AND s.delstatus = 0
            WHERE st.delstatus = 0 AND s.class_id = @classId AND s.status = 'active';
        SELECT r.id AS Id, r.name AS Name, COUNT(*) AS Riders
            FROM student_transport st
            JOIN students s ON s.id = st.student_id AND s.delstatus = 0
            JOIN transport_routes r ON r.id = st.route_id AND r.delstatus = 0
            WHERE st.delstatus = 0 AND s.class_id = @classId AND s.status = 'active'
            GROUP BY r.id, r.name ORDER BY r.id;
        SELECT COUNT(*) FROM hostel_allocations h
            JOIN students s ON s.id = h.student_id AND s.delstatus = 0
            WHERE h.delstatus = 0 AND s.class_id = @classId AND s.status = 'active';
        SELECT COUNT(*) FROM timetable_slots WHERE class_id = @classId AND delstatus = 0;
        """, new { classId, classTeacherId }, async grid => new ClassMapData(
            await grid.ReadSingleOrDefaultAsync<string>(),
            await grid.ReadSingleAsync<int>(),
            (await grid.ReadAsync<FeeItem>()).AsList(),
            (await grid.ReadAsync<OpenActivity>()).AsList(),
            await grid.ReadSingleAsync<int>(),
            (await grid.ReadAsync<RouteRiders>()).AsList(),
            await grid.ReadSingleAsync<int>(),
            await grid.ReadSingleAsync<int>()), ct);
}
