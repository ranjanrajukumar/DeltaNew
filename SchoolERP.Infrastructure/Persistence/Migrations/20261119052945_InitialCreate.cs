using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolERP.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academic_years",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_current = table.Column<bool>(type: "bit", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_academic_years", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "app_menu_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    audience = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    key = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    parent_key = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    kind = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    label = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    icon = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    icon_image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    icon_mime = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    roles = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    position = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_menu_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "application_documents",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    admission_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    original_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    size = table.Column<int>(type: "int", nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    uploaded_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_application_documents", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    user_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    method = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    resource = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    path = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    status = table.Column<int>(type: "int", nullable: false),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "biometric_devices",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    serial_no = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    location = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    api_key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'active'"),
                    last_seen_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biometric_devices", x => x.id);
                    table.CheckConstraint("CK_biometric_devices_status", "[status] IN (N'active', N'inactive')");
                });

            migrationBuilder.CreateTable(
                name: "boards",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    is_enabled = table.Column<bool>(type: "bit", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_boards", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    incharge = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    is_main = table.Column<bool>(type: "bit", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_branches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "calendar_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'event'"),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    audience = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'all'"),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calendar_events", x => x.id);
                    table.CheckConstraint("CK_calendar_events_audience", "[audience] IN (N'all', N'staff', N'parents')");
                    table.CheckConstraint("CK_calendar_events_type", "[type] IN (N'holiday', N'exam', N'event', N'meeting', N'other')");
                });

            migrationBuilder.CreateTable(
                name: "co_scholastic_areas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false, defaultValueSql: "N'Skills'"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_co_scholastic_areas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    head_staff_id = table.Column<int>(type: "int", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "designations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_designations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "document_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_templates", x => x.id);
                    table.CheckConstraint("CK_document_templates_type", "[type] IN (N'bonafide', N'transfer', N'character', N'fee')");
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    category = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    spent_on = table.Column<DateOnly>(type: "date", nullable: false),
                    method = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'cash'"),
                    reference = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.id);
                    table.CheckConstraint("CK_expenses_method", "[method] IN (N'cash', N'card', N'upi', N'bank')");
                });

            migrationBuilder.CreateTable(
                name: "fee_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "fee_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grade_bands",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    board = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'CBSE'"),
                    grade = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    min_percent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grade_bands", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "grading_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    pass_percent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grading_settings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hostels",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'boys'"),
                    warden_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    warden_phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hostels", x => x.id);
                    table.CheckConstraint("CK_hostels_type", "[type] IN (N'boys', N'girls', N'mixed')");
                });

            migrationBuilder.CreateTable(
                name: "incomes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    source = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    received_on = table.Column<DateOnly>(type: "date", nullable: false),
                    method = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'cash'"),
                    reference = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incomes", x => x.id);
                    table.CheckConstraint("CK_incomes_method", "[method] IN (N'cash', N'card', N'upi', N'bank')");
                });

            migrationBuilder.CreateTable(
                name: "inventory_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "leave_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    applies_to = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'staff'"),
                    yearly_days = table.Column<int>(type: "int", nullable: true),
                    paid = table.Column<bool>(type: "bit", nullable: false),
                    approver = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'admin'"),
                    auto_approve_days = table.Column<int>(type: "int", nullable: true),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_types", x => x.id);
                    table.CheckConstraint("CK_leave_types_applies_to", "[applies_to] IN (N'staff', N'student', N'both')");
                    table.CheckConstraint("CK_leave_types_approver", "[approver] IN (N'admin', N'class_teacher', N'department_head')");
                });

            migrationBuilder.CreateTable(
                name: "library_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_library_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "lms_tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "login_events",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    success = table.Column<bool>(type: "bit", nullable: false),
                    ip = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    user_agent = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_login_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "master_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    required = table.Column<bool>(type: "bit", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_master_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "message_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    channel = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_message_templates", x => x.id);
                    table.CheckConstraint("CK_message_templates_channel", "[channel] IN (N'email', N'sms')");
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    channel = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    kind = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'direct'"),
                    recipient = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    recipient_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    student_id = table.Column<int>(type: "int", nullable: true),
                    subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    provider = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    error = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    sent_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_messages", x => x.id);
                    table.CheckConstraint("CK_messages_channel", "[channel] IN (N'email', N'sms')");
                    table.CheckConstraint("CK_messages_kind", "[kind] IN (N'direct', N'broadcast', N'absence', N'fee_reminder')");
                    table.CheckConstraint("CK_messages_status", "[status] IN (N'sent', N'logged', N'failed')");
                });

            migrationBuilder.CreateTable(
                name: "notices",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    audience = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'all'"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notices", x => x.id);
                    table.CheckConstraint("CK_notices_audience", "[audience] IN (N'all', N'teachers', N'parents', N'students')");
                });

            migrationBuilder.CreateTable(
                name: "office_visitors",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    visitor_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    organisation = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    purpose = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    whom_to_meet = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    id_proof = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    badge_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    check_in = table.Column<DateTime>(type: "datetime2", nullable: false),
                    check_out = table.Column<DateTime>(type: "datetime2", nullable: true),
                    note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    recorded_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_office_visitors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "photos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    entity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    mime_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    size = table.Column<int>(type: "int", nullable: false),
                    original_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    uploaded_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_photos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "record_extras",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    entity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    entity_id = table.Column<int>(type: "int", nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_record_extras", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_folders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_folders", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "report_runs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    template_id = table.Column<int>(type: "int", nullable: true),
                    template_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    user_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    role = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    format = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'queued'"),
                    row_count = table.Column<int>(type: "int", nullable: true),
                    size_bytes = table.Column<int>(type: "int", nullable: true),
                    file_name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    stored_name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    error = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    started_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    finished_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_runs", x => x.id);
                    table.CheckConstraint("CK_report_runs_format", "[format] IN (N'pdf', N'xlsx')");
                    table.CheckConstraint("CK_report_runs_status", "[status] IN (N'queued', N'running', N'done', N'failed', N'expired')");
                });

            migrationBuilder.CreateTable(
                name: "role_rights",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    module = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    can_view = table.Column<bool>(type: "bit", nullable: false),
                    can_create = table.Column<bool>(type: "bit", nullable: false),
                    can_edit = table.Column<bool>(type: "bit", nullable: false),
                    can_delete = table.Column<bool>(type: "bit", nullable: false),
                    can_approve = table.Column<bool>(type: "bit", nullable: false),
                    can_export = table.Column<bool>(type: "bit", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_rights", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    base_role = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    permissions = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                    table.CheckConstraint("CK_roles_base_role", "[base_role] IN (N'admin', N'teacher', N'accountant')");
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    kind = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'classroom'"),
                    capacity = table.Column<int>(type: "int", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.id);
                    table.CheckConstraint("CK_rooms_kind", "[kind] IN (N'classroom', N'lab', N'hall', N'library', N'ground', N'other')");
                });

            migrationBuilder.CreateTable(
                name: "safety_inspections",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    inspection_date = table.Column<DateOnly>(type: "date", nullable: false),
                    area = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    inspector = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    result = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'pass'"),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    next_due = table.Column<DateOnly>(type: "date", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_safety_inspections", x => x.id);
                    table.CheckConstraint("CK_safety_inspections_result", "[result] IN (N'pass', N'needs_action', N'fail')");
                });

            migrationBuilder.CreateTable(
                name: "settings",
                columns: table => new
                {
                    key = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_settings", x => x.key);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subjects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    contact_person = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "support_tickets",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    subject = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    category = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'question'"),
                    message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'open'"),
                    created_by = table.Column<int>(type: "int", nullable: false),
                    created_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    responded_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_support_tickets", x => x.id);
                    table.CheckConstraint("CK_support_tickets_category", "[category] IN (N'question', N'problem', N'suggestion')");
                    table.CheckConstraint("CK_support_tickets_status", "[status] IN (N'open', N'in_progress', N'resolved')");
                });

            migrationBuilder.CreateTable(
                name: "user_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    reg_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    model = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    insurance_expiry = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'active'"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicles", x => x.id);
                    table.CheckConstraint("CK_vehicles_status", "[status] IN (N'active', N'inactive')");
                });

            migrationBuilder.CreateTable(
                name: "board_affiliations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    board_id = table.Column<int>(type: "int", nullable: false),
                    branch_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'unverified'"),
                    affiliation_no = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: true),
                    valid_from = table.Column<DateOnly>(type: "date", nullable: true),
                    valid_to = table.Column<DateOnly>(type: "date", nullable: true),
                    source_ref = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    verified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    verified_on = table.Column<DateOnly>(type: "date", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_board_affiliations", x => x.id);
                    table.CheckConstraint("CK_board_affiliations_status", "[status] IN (N'unverified', N'verified', N'lapsed')");
                    table.ForeignKey(
                        name: "FK_board_affiliations_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "board_rules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    board_id = table.Column<int>(type: "int", nullable: false),
                    rule_type = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    scope_key = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValueSql: "N'default'"),
                    effective_year_id = table.Column<int>(type: "int", nullable: true),
                    version = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'active'"),
                    payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    verified = table.Column<bool>(type: "bit", nullable: false),
                    source_ref = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    change_note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    verified_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    verified_on = table.Column<DateOnly>(type: "date", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_board_rules", x => x.id);
                    table.CheckConstraint("CK_board_rules_rule_type", "[rule_type] IN (N'subject_template', N'exam_pattern', N'grading', N'bell_schedule', N'promotion', N'report_card')");
                    table.CheckConstraint("CK_board_rules_status", "[status] IN (N'active', N'retired')");
                    table.ForeignKey(
                        name: "FK_board_rules_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_openings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    department_id = table.Column<int>(type: "int", nullable: true),
                    vacancies = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    closing_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'open'"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_openings", x => x.id);
                    table.CheckConstraint("CK_job_openings_status", "[status] IN (N'open', N'closed')");
                    table.ForeignKey(
                        name: "FK_job_openings_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    designation = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    department_id = table.Column<int>(type: "int", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    join_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'active'"),
                    staff_category = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    middle_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    marital_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    blood_group = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    national_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    pan_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    state = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    pincode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    qualification = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    experience_years = table.Column<decimal>(type: "decimal(4,1)", nullable: true),
                    employment_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    previous_employer = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    emergency_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    emergency_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    bank_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    bank_account_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ifsc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff", x => x.id);
                    table.CheckConstraint("CK_staff_status", "[status] IN (N'active', N'inactive')");
                    table.ForeignKey(
                        name: "FK_staff_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "fee_group_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    group_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_group_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_fee_group_items_fee_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "fee_groups",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "hostel_meals",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hostel_id = table.Column<int>(type: "int", nullable: false),
                    day = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    meal = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    items = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hostel_meals", x => x.id);
                    table.CheckConstraint("CK_hostel_meals_day", "[day] IN (N'mon', N'tue', N'wed', N'thu', N'fri', N'sat', N'sun')");
                    table.CheckConstraint("CK_hostel_meals_meal", "[meal] IN (N'breakfast', N'lunch', N'snacks', N'dinner')");
                    table.ForeignKey(
                        name: "FK_hostel_meals_hostels_hostel_id",
                        column: x => x.hostel_id,
                        principalTable: "hostels",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "hostel_rooms",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hostel_id = table.Column<int>(type: "int", nullable: false),
                    room_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    monthly_fee = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hostel_rooms", x => x.id);
                    table.ForeignKey(
                        name: "FK_hostel_rooms_hostels_hostel_id",
                        column: x => x.hostel_id,
                        principalTable: "hostels",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    asset_tag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    location = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    purchase_date = table.Column<DateOnly>(type: "date", nullable: true),
                    cost = table.Column<decimal>(type: "decimal(12,2)", nullable: false, defaultValueSql: "0"),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'in_use'"),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assets", x => x.id);
                    table.CheckConstraint("CK_assets_status", "[status] IN (N'in_use', N'in_store', N'under_repair', N'disposed')");
                    table.ForeignKey(
                        name: "FK_assets_inventory_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "inventory_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "inventory_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'pcs'"),
                    reorder_level = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_inventory_items_inventory_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "inventory_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "books",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    author = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    category_id = table.Column<int>(type: "int", nullable: true),
                    isbn = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    copies = table.Column<int>(type: "int", nullable: false),
                    available = table.Column<int>(type: "int", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_books", x => x.id);
                    table.ForeignKey(
                        name: "FK_books_library_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "library_categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "report_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    folder_id = table.Column<int>(type: "int", nullable: true),
                    name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    config = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    roles = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    pages = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    board = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    created_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_templates_report_folders_folder_id",
                        column: x => x.folder_id,
                        principalTable: "report_folders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    role_id = table.Column<int>(type: "int", nullable: true),
                    staff_id = table.Column<int>(type: "int", nullable: true),
                    must_change_password = table.Column<bool>(type: "bit", nullable: false),
                    role = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'admin'"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.CheckConstraint("CK_users_role", "[role] IN (N'admin', N'teacher', N'accountant', N'parent', N'student')");
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "room_bookings",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    period = table.Column<int>(type: "int", nullable: false),
                    purpose = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    booked_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_bookings", x => x.id);
                    table.ForeignKey(
                        name: "FK_room_bookings_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "purchase_orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    supplier_id = table.Column<int>(type: "int", nullable: false),
                    order_date = table.Column<DateOnly>(type: "date", nullable: false),
                    expected_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'ordered'"),
                    received_on = table.Column<DateOnly>(type: "date", nullable: true),
                    note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    created_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_orders", x => x.id);
                    table.CheckConstraint("CK_purchase_orders_status", "[status] IN (N'ordered', N'received', N'cancelled')");
                    table.ForeignKey(
                        name: "FK_purchase_orders_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "transport_routes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    vehicle_id = table.Column<int>(type: "int", nullable: true),
                    monthly_fee = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_routes", x => x.id);
                    table.ForeignKey(
                        name: "FK_transport_routes_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "transport_staff",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    license_no = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    license_expiry = table.Column<DateOnly>(type: "date", nullable: true),
                    vehicle_id = table.Column<int>(type: "int", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_staff", x => x.id);
                    table.CheckConstraint("CK_transport_staff_role", "[role] IN (N'driver', N'attendant')");
                    table.ForeignKey(
                        name: "FK_transport_staff_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "vehicle_locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vehicle_id = table.Column<int>(type: "int", nullable: false),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: false),
                    speed = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    recorded_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_locations", x => x.id);
                    table.ForeignKey(
                        name: "FK_vehicle_locations_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "vehicle_maintenance",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vehicle_id = table.Column<int>(type: "int", nullable: false),
                    service_date = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    cost = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    next_due = table.Column<DateOnly>(type: "date", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_maintenance", x => x.id);
                    table.ForeignKey(
                        name: "FK_vehicle_maintenance_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "vehicle_trackers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    vehicle_id = table.Column<int>(type: "int", nullable: false),
                    api_key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vehicle_trackers", x => x.id);
                    table.ForeignKey(
                        name: "FK_vehicle_trackers_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "job_applicants",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    opening_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    qualification = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'applied'"),
                    staff_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_applicants", x => x.id);
                    table.CheckConstraint("CK_job_applicants_status", "[status] IN (N'applied', N'shortlisted', N'interviewed', N'offered', N'hired', N'rejected')");
                    table.ForeignKey(
                        name: "FK_job_applicants_job_openings_opening_id",
                        column: x => x.opening_id,
                        principalTable: "job_openings",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "classes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    branch_id = table.Column<int>(type: "int", nullable: true),
                    class_teacher_id = table.Column<int>(type: "int", nullable: true),
                    board_id = table.Column<int>(type: "int", nullable: true),
                    board = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'CBSE'"),
                    room = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    section = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValueSql: "N'A'"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classes", x => x.id);
                    table.ForeignKey(
                        name: "FK_classes_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_classes_branches_branch_id",
                        column: x => x.branch_id,
                        principalTable: "branches",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_classes_staff_class_teacher_id",
                        column: x => x.class_teacher_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "leave_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    leave_type_id = table.Column<int>(type: "int", nullable: true),
                    from_date = table.Column<DateOnly>(type: "date", nullable: false),
                    to_date = table.Column<DateOnly>(type: "date", nullable: false),
                    days = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    day_part = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValueSql: "N'full'"),
                    from_time = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    to_time = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'pending'"),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    decided_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_leave_requests", x => x.id);
                    table.CheckConstraint("CK_leave_requests_status", "[status] IN (N'pending', N'approved', N'rejected', N'cancelled')");
                    table.ForeignKey(
                        name: "FK_leave_requests_leave_types_leave_type_id",
                        column: x => x.leave_type_id,
                        principalTable: "leave_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_leave_requests_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payslips",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    month = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    basic = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    allowances = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    deductions = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    net = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'draft'"),
                    paid_on = table.Column<DateOnly>(type: "date", nullable: true),
                    paid_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payslips", x => x.id);
                    table.CheckConstraint("CK_payslips_status", "[status] IN (N'draft', N'paid')");
                    table.ForeignKey(
                        name: "FK_payslips_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "performance_reviews",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    period = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    review_date = table.Column<DateOnly>(type: "date", nullable: false),
                    reviewer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    work = table.Column<int>(type: "int", nullable: false),
                    punctuality = table.Column<int>(type: "int", nullable: false),
                    teamwork = table.Column<int>(type: "int", nullable: false),
                    initiative = table.Column<int>(type: "int", nullable: false),
                    strengths = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    improvements = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'draft'"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_performance_reviews", x => x.id);
                    table.CheckConstraint("CK_performance_reviews_status", "[status] IN (N'draft', N'final')");
                    table.ForeignKey(
                        name: "FK_performance_reviews_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "staff_attendance",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_attendance", x => x.id);
                    table.CheckConstraint("CK_staff_attendance_status", "[status] IN (N'present', N'absent', N'late', N'leave')");
                    table.ForeignKey(
                        name: "FK_staff_attendance_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "staff_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    original_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    size = table.Column<int>(type: "int", nullable: false),
                    stored_name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    uploaded_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_files", x => x.id);
                    table.ForeignKey(
                        name: "FK_staff_files_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "staff_salaries",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    basic = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    allowances = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    deductions = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_salaries", x => x.id);
                    table.ForeignKey(
                        name: "FK_staff_salaries_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "staff_unavailability",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    staff_id = table.Column<int>(type: "int", nullable: false),
                    day = table.Column<int>(type: "int", nullable: false),
                    period = table.Column<int>(type: "int", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_staff_unavailability", x => x.id);
                    table.ForeignKey(
                        name: "FK_staff_unavailability_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "asset_maintenance",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    asset_id = table.Column<int>(type: "int", nullable: false),
                    service_date = table.Column<DateOnly>(type: "date", nullable: false),
                    description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    cost = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    next_due = table.Column<DateOnly>(type: "date", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asset_maintenance", x => x.id);
                    table.ForeignKey(
                        name: "FK_asset_maintenance_assets_asset_id",
                        column: x => x.asset_id,
                        principalTable: "assets",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "inventory_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    approved_quantity = table.Column<int>(type: "int", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'pending'"),
                    requested_by = table.Column<int>(type: "int", nullable: false),
                    requested_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    decided_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    decided_on = table.Column<DateOnly>(type: "date", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    movement_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory_requests", x => x.id);
                    table.CheckConstraint("CK_inventory_requests_status", "[status] IN (N'pending', N'approved', N'rejected', N'cancelled')");
                    table.ForeignKey(
                        name: "FK_inventory_requests_inventory_items_item_id",
                        column: x => x.item_id,
                        principalTable: "inventory_items",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_cost = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    supplier_id = table.Column<int>(type: "int", nullable: true),
                    order_id = table.Column<int>(type: "int", nullable: true),
                    party = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    created_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movements", x => x.id);
                    table.CheckConstraint("CK_stock_movements_type", "[type] IN (N'in', N'out')");
                    table.ForeignKey(
                        name: "FK_stock_movements_inventory_items_item_id",
                        column: x => x.item_id,
                        principalTable: "inventory_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_stock_movements_suppliers_supplier_id",
                        column: x => x.supplier_id,
                        principalTable: "suppliers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "parents",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    alternate_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    occupation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    employer = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    national_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    state = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    pincode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parents", x => x.id);
                    table.ForeignKey(
                        name: "FK_parents_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "push_subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    endpoint = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    p256dh = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    auth = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    user_agent = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_push_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_push_subscriptions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "user_group_members",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    group_id = table.Column<int>(type: "int", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_group_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_group_members_user_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "user_groups",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_user_group_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "purchase_order_lines",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    item_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_cost = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_purchase_order_lines", x => x.id);
                    table.ForeignKey(
                        name: "FK_purchase_order_lines_inventory_items_item_id",
                        column: x => x.item_id,
                        principalTable: "inventory_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_purchase_order_lines_purchase_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "purchase_orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "transport_stops",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    route_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    pickup_time = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_stops", x => x.id);
                    table.ForeignKey(
                        name: "FK_transport_stops_transport_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "transport_routes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    kind = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    venue = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    class_id = table.Column<int>(type: "int", nullable: true),
                    capacity = table.Column<int>(type: "int", nullable: true),
                    fee = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    requires_consent = table.Column<bool>(type: "bit", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'planned'"),
                    organizer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activities", x => x.id);
                    table.CheckConstraint("CK_activities_kind", "[kind] IN (N'event', N'celebration', N'competition', N'sport', N'field_trip', N'club')");
                    table.CheckConstraint("CK_activities_status", "[status] IN (N'planned', N'open', N'completed', N'cancelled')");
                    table.ForeignKey(
                        name: "FK_activities_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "admissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    applicant_name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    guardian_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    guardian_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    class_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'enquiry'"),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    student_id = table.Column<int>(type: "int", nullable: true),
                    source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'office'"),
                    test_date = table.Column<DateOnly>(type: "date", nullable: true),
                    test_marks = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    test_max_marks = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    interview_date = table.Column<DateOnly>(type: "date", nullable: true),
                    interview_time = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    interview_panel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    interview_result = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    offer_expiry = table.Column<DateOnly>(type: "date", nullable: true),
                    follow_up_on = table.Column<DateOnly>(type: "date", nullable: true),
                    visit_on = table.Column<DateOnly>(type: "date", nullable: true),
                    last_contacted_on = table.Column<DateOnly>(type: "date", nullable: true),
                    enquiry_on = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "CAST(SYSUTCDATETIME() AS date)"),
                    academic_year = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    assigned_to_id = table.Column<int>(type: "int", nullable: true),
                    preferred_contact = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    follow_up_type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    follow_up_note = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    enrolled_on = table.Column<DateOnly>(type: "date", nullable: true),
                    choices = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    referred_by = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    consent_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    middle_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    religion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    mother_tongue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    national_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    state = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    pincode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    father_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    father_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    father_occupation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    mother_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    mother_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    mother_occupation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    guardian_relation = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    guardian_email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    previous_school = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    previous_class = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    secondary_guardian_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    secondary_guardian_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    special_needs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    previous_tc_no = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    previous_percent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admissions", x => x.id);
                    table.CheckConstraint("CK_admissions_gender", "[gender] IN (N'male', N'female', N'other')");
                    table.CheckConstraint("CK_admissions_interview_result", "[interview_result] IN (N'pending', N'recommended', N'not_recommended')");
                    table.CheckConstraint("CK_admissions_status", "[status] IN (N'draft', N'enquiry', N'applied', N'waitlisted', N'approved', N'rejected', N'enrolled')");
                    table.ForeignKey(
                        name: "FK_admissions_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "class_subjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: false),
                    staff_id = table.Column<int>(type: "int", nullable: true),
                    component = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'theory'"),
                    is_elective = table.Column<bool>(type: "bit", nullable: false),
                    elective_group = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    periods_per_week = table.Column<int>(type: "int", nullable: true),
                    practical_share = table.Column<int>(type: "int", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_class_subjects", x => x.id);
                    table.ForeignKey(
                        name: "FK_class_subjects_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_class_subjects_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_class_subjects_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "exam_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exam_groups", x => x.id);
                    table.ForeignKey(
                        name: "FK_exam_groups_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "fee_structures",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_structures", x => x.id);
                    table.ForeignKey(
                        name: "FK_fee_structures_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "homework",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    assigned_on = table.Column<DateOnly>(type: "date", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: false),
                    max_marks = table.Column<int>(type: "int", nullable: true),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework", x => x.id);
                    table.ForeignKey(
                        name: "FK_homework_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_homework_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_courses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    certificate_enabled = table.Column<bool>(type: "bit", nullable: false),
                    tags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    banner_image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    published = table.Column<bool>(type: "bit", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_courses", x => x.id);
                    table.ForeignKey(
                        name: "FK_lms_courses_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lms_courses_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "online_classes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: true),
                    class_date = table.Column<DateOnly>(type: "date", nullable: false),
                    start_time = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    end_time = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    link = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    teacher_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_online_classes", x => x.id);
                    table.ForeignKey(
                        name: "FK_online_classes_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_online_classes_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    admission_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    first_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    dob = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    guardian_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    guardian_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    class_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'active'"),
                    registration_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'complete'"),
                    middle_name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    admission_date = table.Column<DateOnly>(type: "date", nullable: true),
                    roll_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    nationality = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    religion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    category = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    mother_tongue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    national_id = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    city = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    state = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    pincode = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    father_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    father_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    father_occupation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    mother_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    mother_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    mother_occupation = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    guardian_relation = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    guardian_email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    previous_school = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    previous_class = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    secondary_guardian_name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    secondary_guardian_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    special_needs = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    previous_tc_no = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    previous_percent = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_students", x => x.id);
                    table.CheckConstraint("CK_students_gender", "[gender] IN (N'male', N'female', N'other')");
                    table.CheckConstraint("CK_students_status", "[status] IN (N'active', N'inactive')");
                    table.ForeignKey(
                        name: "FK_students_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "study_materials",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_materials", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_materials_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_study_materials_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "syllabus_topics",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: false),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    objective = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    planned_date = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'not_started'"),
                    completed_on = table.Column<DateOnly>(type: "date", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_syllabus_topics", x => x.id);
                    table.CheckConstraint("CK_syllabus_topics_status", "[status] IN (N'not_started', N'in_progress', N'completed')");
                    table.ForeignKey(
                        name: "FK_syllabus_topics_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_syllabus_topics_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "timetable_slots",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    day = table.Column<int>(type: "int", nullable: false),
                    period = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: false),
                    staff_id = table.Column<int>(type: "int", nullable: true),
                    room_id = table.Column<int>(type: "int", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_timetable_slots", x => x.id);
                    table.ForeignKey(
                        name: "FK_timetable_slots_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_timetable_slots_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_timetable_slots_staff_staff_id",
                        column: x => x.staff_id,
                        principalTable: "staff",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_timetable_slots_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    max_marks = table.Column<int>(type: "int", nullable: false),
                    exam_group_id = table.Column<int>(type: "int", nullable: true),
                    weight = table.Column<int>(type: "int", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exams", x => x.id);
                    table.ForeignKey(
                        name: "FK_exams_classes_class_id",
                        column: x => x.class_id,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_exams_exam_groups_exam_group_id",
                        column: x => x.exam_group_id,
                        principalTable: "exam_groups",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_announcements",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    published = table.Column<bool>(type: "bit", nullable: false),
                    created_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_announcements", x => x.id);
                    table.ForeignKey(
                        name: "FK_lms_announcements_lms_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "lms_courses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_assignments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    due_on = table.Column<DateOnly>(type: "date", nullable: true),
                    max_score = table.Column<int>(type: "int", nullable: false),
                    published = table.Column<bool>(type: "bit", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_lms_assignments_lms_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "lms_courses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_materials",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    kind = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'file'"),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    attachment_key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    attachment_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    attachment_mime = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    attachment_size = table.Column<int>(type: "int", nullable: true),
                    published = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_materials", x => x.id);
                    table.CheckConstraint("CK_lms_materials_kind", "[kind] IN (N'file', N'link', N'video')");
                    table.ForeignKey(
                        name: "FK_lms_materials_lms_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "lms_courses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_modules",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    published = table.Column<bool>(type: "bit", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_modules", x => x.id);
                    table.ForeignKey(
                        name: "FK_lms_modules_lms_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "lms_courses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_quizzes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    questions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    max_attempts = table.Column<int>(type: "int", nullable: false),
                    due_on = table.Column<DateOnly>(type: "date", nullable: true),
                    published = table.Column<bool>(type: "bit", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_quizzes", x => x.id);
                    table.ForeignKey(
                        name: "FK_lms_quizzes_lms_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "lms_courses",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "absence_alerts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    method = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    note = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    notified_by = table.Column<int>(type: "int", nullable: true),
                    notified_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_absence_alerts", x => x.id);
                    table.CheckConstraint("CK_absence_alerts_method", "[method] IN (N'call', N'sms', N'whatsapp', N'in_person', N'portal')");
                    table.ForeignKey(
                        name: "FK_absence_alerts_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "activity_participants",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    activity_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'registered'"),
                    consent = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'not_required'"),
                    result = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_participants", x => x.id);
                    table.CheckConstraint("CK_activity_participants_consent", "[consent] IN (N'not_required', N'pending', N'given', N'declined')");
                    table.CheckConstraint("CK_activity_participants_status", "[status] IN (N'registered', N'attended', N'absent')");
                    table.ForeignKey(
                        name: "FK_activity_participants_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_activity_participants_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "attendance",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    arrival_time = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    out_time = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    note = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    saved_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance", x => x.id);
                    table.CheckConstraint("CK_attendance_status", "[status] IN (N'present', N'absent', N'late')");
                    table.ForeignKey(
                        name: "FK_attendance_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "attendance_corrections",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    old_status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    new_status = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    corrected_by = table.Column<int>(type: "int", nullable: true),
                    corrected_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_attendance_corrections", x => x.id);
                    table.ForeignKey(
                        name: "FK_attendance_corrections_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "behavior_records",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    points = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    recorded_by = table.Column<int>(type: "int", nullable: true),
                    recorded_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_behavior_records", x => x.id);
                    table.CheckConstraint("CK_behavior_records_type", "[type] IN (N'merit', N'demerit', N'incident', N'note')");
                    table.ForeignKey(
                        name: "FK_behavior_records_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "biometric_punches",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    device_id = table.Column<int>(type: "int", nullable: false),
                    enroll_id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: true),
                    punched_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_biometric_punches", x => x.id);
                    table.ForeignKey(
                        name: "FK_biometric_punches_biometric_devices_device_id",
                        column: x => x.device_id,
                        principalTable: "biometric_devices",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_biometric_punches_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "book_issues",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    book_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    issued_on = table.Column<DateOnly>(type: "date", nullable: false),
                    due_on = table.Column<DateOnly>(type: "date", nullable: false),
                    returned_on = table.Column<DateOnly>(type: "date", nullable: true),
                    fine = table.Column<decimal>(type: "decimal(8,2)", nullable: false, defaultValueSql: "0"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_book_issues", x => x.id);
                    table.ForeignKey(
                        name: "FK_book_issues_books_book_id",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_book_issues_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "book_reservations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    book_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    reserved_on = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'active'"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_book_reservations", x => x.id);
                    table.CheckConstraint("CK_book_reservations_status", "[status] IN (N'active', N'fulfilled', N'cancelled')");
                    table.ForeignKey(
                        name: "FK_book_reservations_books_book_id",
                        column: x => x.book_id,
                        principalTable: "books",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_book_reservations_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "clinic_visits",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    visit_date = table.Column<DateOnly>(type: "date", nullable: false),
                    complaint = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    treatment = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    medication = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    outcome = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'returned_to_class'"),
                    parent_informed = table.Column<bool>(type: "bit", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinic_visits", x => x.id);
                    table.CheckConstraint("CK_clinic_visits_outcome", "[outcome] IN (N'returned_to_class', N'rested', N'sent_home', N'referred')");
                    table.ForeignKey(
                        name: "FK_clinic_visits_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "co_scholastic_grades",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    area_id = table.Column<int>(type: "int", nullable: false),
                    exam_group_id = table.Column<int>(type: "int", nullable: false),
                    grade = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    remark = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_co_scholastic_grades", x => x.id);
                    table.ForeignKey(
                        name: "FK_co_scholastic_grades_co_scholastic_areas_area_id",
                        column: x => x.area_id,
                        principalTable: "co_scholastic_areas",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_co_scholastic_grades_exam_groups_exam_group_id",
                        column: x => x.exam_group_id,
                        principalTable: "exam_groups",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_co_scholastic_grades_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "document_checks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'pending'"),
                    note = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    checked_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    checked_on = table.Column<DateTime>(type: "datetime2", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_checks", x => x.id);
                    table.CheckConstraint("CK_document_checks_status", "[status] IN (N'pending', N'verified', N'rejected')");
                    table.ForeignKey(
                        name: "FK_document_checks_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "document_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    purpose = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'requested'"),
                    certificate_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    issued_on = table.Column<DateOnly>(type: "date", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    snapshot = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    requested_by = table.Column<int>(type: "int", nullable: true),
                    decided_by = table.Column<int>(type: "int", nullable: true),
                    requested_by_role = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    decided_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    decided_on = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document_requests", x => x.id);
                    table.CheckConstraint("CK_document_requests_status", "[status] IN (N'requested', N'issued', N'rejected')");
                    table.CheckConstraint("CK_document_requests_type", "[type] IN (N'bonafide', N'transfer', N'character', N'fee')");
                    table.ForeignKey(
                        name: "FK_document_requests_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "feedback",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ticket_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    kind = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    subject = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    rating = table.Column<int>(type: "int", nullable: true),
                    anonymous = table.Column<bool>(type: "bit", nullable: false),
                    submitted_by_user_id = table.Column<int>(type: "int", nullable: false),
                    submitted_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    submitted_by_role = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'open'"),
                    priority = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'normal'"),
                    response = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    responded_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    responded_on = table.Column<DateTime>(type: "datetime2", nullable: true),
                    level = table.Column<int>(type: "int", nullable: false),
                    last_action_on = table.Column<DateTime>(type: "datetime2", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback", x => x.id);
                    table.CheckConstraint("CK_feedback_kind", "[kind] IN (N'feedback', N'grievance')");
                    table.CheckConstraint("CK_feedback_priority", "[priority] IN (N'low', N'normal', N'high')");
                    table.CheckConstraint("CK_feedback_status", "[status] IN (N'open', N'in_progress', N'resolved', N'closed')");
                    table.ForeignKey(
                        name: "FK_feedback_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "fees",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    paid_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false, defaultValueSql: "0"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fees", x => x.id);
                    table.ForeignKey(
                        name: "FK_fees_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "homework_submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    homework_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'pending'"),
                    marks = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_homework_submissions", x => x.id);
                    table.CheckConstraint("CK_homework_submissions_status", "[status] IN (N'pending', N'submitted', N'late', N'missing')");
                    table.ForeignKey(
                        name: "FK_homework_submissions_homework_homework_id",
                        column: x => x.homework_id,
                        principalTable: "homework",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_homework_submissions_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "hostel_allocations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    room_id = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    bed_no = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    meal_plan = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    local_guardian_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    local_guardian_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    local_guardian_relation = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    security_deposit = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    consent_received = table.Column<bool>(type: "bit", nullable: false),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hostel_allocations", x => x.id);
                    table.ForeignKey(
                        name: "FK_hostel_allocations_hostel_rooms_room_id",
                        column: x => x.room_id,
                        principalTable: "hostel_rooms",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_hostel_allocations_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "hostel_attendance",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hostel_attendance", x => x.id);
                    table.CheckConstraint("CK_hostel_attendance_status", "[status] IN (N'present', N'absent', N'leave')");
                    table.ForeignKey(
                        name: "FK_hostel_attendance_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "hostel_visitors",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    visitor_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    relation = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    purpose = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    check_in = table.Column<DateTime>(type: "datetime2", nullable: false),
                    check_out = table.Column<DateTime>(type: "datetime2", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hostel_visitors", x => x.id);
                    table.ForeignKey(
                        name: "FK_hostel_visitors_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "incidents",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    incident_date = table.Column<DateOnly>(type: "date", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: true),
                    location = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    severity = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'minor'"),
                    description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    action_taken = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'open'"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_incidents", x => x.id);
                    table.CheckConstraint("CK_incidents_severity", "[severity] IN (N'minor', N'moderate', N'serious')");
                    table.CheckConstraint("CK_incidents_status", "[status] IN (N'open', N'resolved')");
                    table.ForeignKey(
                        name: "FK_incidents_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "library_memberships",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    card_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    member_since = table.Column<DateOnly>(type: "date", nullable: false),
                    valid_until = table.Column<DateOnly>(type: "date", nullable: true),
                    max_books = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'active'"),
                    notes = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_library_memberships", x => x.id);
                    table.CheckConstraint("CK_library_memberships_status", "[status] IN (N'active', N'suspended')");
                    table.ForeignKey(
                        name: "FK_library_memberships_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_enrollments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    active = table.Column<bool>(type: "bit", nullable: false),
                    enrolled_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_enrollments", x => x.id);
                    table.ForeignKey(
                        name: "FK_lms_enrollments_lms_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "lms_courses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lms_enrollments_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "parent_students",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    parent_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    relation = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValueSql: "N'Guardian'"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parent_students", x => x.id);
                    table.ForeignKey(
                        name: "FK_parent_students_parents_parent_id",
                        column: x => x.parent_id,
                        principalTable: "parents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_parent_students_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "period_attendance",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    class_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    period = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_period_attendance", x => x.id);
                    table.CheckConstraint("CK_period_attendance_status", "[status] IN (N'present', N'absent', N'late')");
                    table.ForeignKey(
                        name: "FK_period_attendance_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_accounts", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_accounts_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_student_accounts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_board_registrations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    board_id = table.Column<int>(type: "int", nullable: false),
                    registration_no = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    roll_no = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    exam_year = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'pending'"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_board_registrations", x => x.id);
                    table.CheckConstraint("CK_student_board_registrations_status", "[status] IN (N'pending', N'submitted', N'registered')");
                    table.ForeignKey(
                        name: "FK_student_board_registrations_boards_board_id",
                        column: x => x.board_id,
                        principalTable: "boards",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_student_board_registrations_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    kind = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    original_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    mime_type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    size = table.Column<int>(type: "int", nullable: false),
                    stored_name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    uploaded_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_files", x => x.id);
                    table.CheckConstraint("CK_student_files_kind", "[kind] IN (N'photo', N'document')");
                    table.ForeignKey(
                        name: "FK_student_files_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_health",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    blood_group = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    height_cm = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    weight_kg = table.Column<decimal>(type: "decimal(5,1)", nullable: true),
                    allergies = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    conditions = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    medications = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    emergency_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    emergency_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    doctor_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    doctor_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_health", x => x.id);
                    table.CheckConstraint("CK_student_health_blood_group", "[blood_group] IN (N'A+', N'A-', N'B+', N'B-', N'AB+', N'AB-', N'O+', N'O-')");
                    table.ForeignKey(
                        name: "FK_student_health_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_leaves",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    leave_type_id = table.Column<int>(type: "int", nullable: true),
                    from_date = table.Column<DateOnly>(type: "date", nullable: false),
                    to_date = table.Column<DateOnly>(type: "date", nullable: false),
                    days = table.Column<decimal>(type: "decimal(4,1)", nullable: false),
                    day_part = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false, defaultValueSql: "N'full'"),
                    from_time = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    to_time = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'pending'"),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    requested_by = table.Column<int>(type: "int", nullable: true),
                    decided_by = table.Column<int>(type: "int", nullable: true),
                    requested_by_role = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    decided_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    decided_on = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_leaves", x => x.id);
                    table.CheckConstraint("CK_student_leaves_status", "[status] IN (N'pending', N'approved', N'rejected', N'cancelled')");
                    table.ForeignKey(
                        name: "FK_student_leaves_leave_types_leave_type_id",
                        column: x => x.leave_type_id,
                        principalTable: "leave_types",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_student_leaves_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_subjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    class_subject_id = table.Column<int>(type: "int", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_subjects", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_subjects_class_subjects_class_subject_id",
                        column: x => x.class_subject_id,
                        principalTable: "class_subjects",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_student_subjects_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "student_transport",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    route_id = table.Column<int>(type: "int", nullable: false),
                    stop_id = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    trip_type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValueSql: "N'both'"),
                    authorized_pickup_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authorized_pickup_phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_transport", x => x.id);
                    table.ForeignKey(
                        name: "FK_student_transport_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_student_transport_transport_routes_route_id",
                        column: x => x.route_id,
                        principalTable: "transport_routes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_student_transport_transport_stops_stop_id",
                        column: x => x.stop_id,
                        principalTable: "transport_stops",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "transport_attendance",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    trip = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_attendance", x => x.id);
                    table.CheckConstraint("CK_transport_attendance_status", "[status] IN (N'boarded', N'absent')");
                    table.CheckConstraint("CK_transport_attendance_trip", "[trip] IN (N'morning', N'evening')");
                    table.ForeignKey(
                        name: "FK_transport_attendance_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "substitutions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    slot_id = table.Column<int>(type: "int", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    substitute_id = table.Column<int>(type: "int", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    created_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_substitutions", x => x.id);
                    table.ForeignKey(
                        name: "FK_substitutions_staff_substitute_id",
                        column: x => x.substitute_id,
                        principalTable: "staff",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_substitutions_timetable_slots_slot_id",
                        column: x => x.slot_id,
                        principalTable: "timetable_slots",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "marks",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    exam_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: false),
                    marks = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    practical_marks = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marks", x => x.id);
                    table.ForeignKey(
                        name: "FK_marks_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_marks_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_marks_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "revaluation_requests",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    exam_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    subject_id = table.Column<int>(type: "int", nullable: false),
                    original_marks = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    new_marks = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    reason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'pending'"),
                    remarks = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    requested_by = table.Column<int>(type: "int", nullable: true),
                    decided_by = table.Column<int>(type: "int", nullable: true),
                    decided_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revaluation_requests", x => x.id);
                    table.CheckConstraint("CK_revaluation_requests_status", "[status] IN (N'pending', N'accepted', N'rejected')");
                    table.ForeignKey(
                        name: "FK_revaluation_requests_exams_exam_id",
                        column: x => x.exam_id,
                        principalTable: "exams",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_revaluation_requests_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_revaluation_requests_subjects_subject_id",
                        column: x => x.subject_id,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_submissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    assignment_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    response = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    resource_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    attachment_data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    attachment_key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    attachment_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    attachment_mime = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    attachment_size = table.Column<int>(type: "int", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    score = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    feedback = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    graded_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    graded_by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_lms_submissions_lms_assignments_assignment_id",
                        column: x => x.assignment_id,
                        principalTable: "lms_assignments",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lms_submissions_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_lessons",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    module_id = table.Column<int>(type: "int", nullable: true),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    video_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    resource_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    attachment_data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    attachment_key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    attachment_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    attachment_mime = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    attachment_size = table.Column<int>(type: "int", nullable: true),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    published = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_lessons", x => x.id);
                    table.ForeignKey(
                        name: "FK_lms_lessons_lms_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "lms_courses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lms_lessons_lms_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "lms_modules",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_quiz_attempts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    quiz_id = table.Column<int>(type: "int", nullable: false),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    attempt_no = table.Column<int>(type: "int", nullable: false),
                    answers = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    score = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    max_score = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    review_status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValueSql: "N'reviewed'"),
                    feedback = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    question_scores = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_quiz_attempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_lms_quiz_attempts_lms_quizzes_quiz_id",
                        column: x => x.quiz_id,
                        principalTable: "lms_quizzes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lms_quiz_attempts_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "feedback_logs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    feedback_id = table.Column<int>(type: "int", nullable: false),
                    action = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    level = table.Column<int>(type: "int", nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    by_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feedback_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_feedback_logs_feedback_feedback_id",
                        column: x => x.feedback_id,
                        principalTable: "feedback",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "fee_adjustments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fee_id = table.Column<int>(type: "int", nullable: false),
                    kind = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fee_adjustments", x => x.id);
                    table.CheckConstraint("CK_fee_adjustments_kind", "[kind] IN (N'discount', N'fine')");
                    table.ForeignKey(
                        name: "FK_fee_adjustments_fees_fee_id",
                        column: x => x.fee_id,
                        principalTable: "fees",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fee_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    method = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'cash'"),
                    receipt_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    paid_on = table.Column<DateOnly>(type: "date", nullable: false),
                    received_by = table.Column<int>(type: "int", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.id);
                    table.CheckConstraint("CK_payments_method", "[method] IN (N'cash', N'card', N'upi', N'bank')");
                    table.ForeignKey(
                        name: "FK_payments_fees_fee_id",
                        column: x => x.fee_id,
                        principalTable: "fees",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_lesson_parts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<int>(type: "int", nullable: false),
                    lesson_id = table.Column<int>(type: "int", nullable: false),
                    kind = table.Column<string>(type: "nvarchar(20)", nullable: false, defaultValueSql: "N'text'"),
                    title = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    attachment_key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    attachment_name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    attachment_mime = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    attachment_size = table.Column<int>(type: "int", nullable: true),
                    sequence = table.Column<int>(type: "int", nullable: false),
                    published = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_lesson_parts", x => x.id);
                    table.CheckConstraint("CK_lms_lesson_parts_kind", "[kind] IN (N'text', N'video', N'link', N'file')");
                    table.ForeignKey(
                        name: "FK_lms_lesson_parts_lms_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "lms_courses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lms_lesson_parts_lms_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lms_lessons",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "lms_progress",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_id = table.Column<int>(type: "int", nullable: false),
                    lesson_id = table.Column<int>(type: "int", nullable: false),
                    viewed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    view_count = table.Column<int>(type: "int", nullable: false, defaultValueSql: "0"),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lms_progress", x => x.id);
                    table.ForeignKey(
                        name: "FK_lms_progress_lms_lessons_lesson_id",
                        column: x => x.lesson_id,
                        principalTable: "lms_lessons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_lms_progress_students_student_id",
                        column: x => x.student_id,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    payment_id = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    reason = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    refunded_on = table.Column<DateOnly>(type: "date", nullable: false),
                    created_by = table.Column<int>(type: "int", nullable: true),
                    authadd = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authedit = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    authdel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    addondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    editondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delondt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    delstatus = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refunds", x => x.id);
                    table.ForeignKey(
                        name: "FK_refunds_payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "payments",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_absence_alerts_student_id",
                table: "absence_alerts",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_academic_years_name",
                table: "academic_years",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_activities_class_id",
                table: "activities",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_participants_student_id",
                table: "activity_participants",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "uq_activity_student",
                table: "activity_participants",
                columns: new[] { "activity_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_admissions_class_id",
                table: "admissions",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "uq_app_menu_key",
                table: "app_menu_items",
                columns: new[] { "audience", "key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_asset_maintenance_asset_id",
                table: "asset_maintenance",
                column: "asset_id");

            migrationBuilder.CreateIndex(
                name: "IX_assets_asset_tag",
                table: "assets",
                column: "asset_tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_assets_category_id",
                table: "assets",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "uq_att",
                table: "attendance",
                columns: new[] { "student_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_attendance_corrections_student_id",
                table: "attendance_corrections",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "idx_audit_created",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_audit_resource",
                table: "audit_logs",
                column: "resource");

            migrationBuilder.CreateIndex(
                name: "IX_behavior_records_student_id",
                table: "behavior_records",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_biometric_devices_api_key",
                table: "biometric_devices",
                column: "api_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_biometric_devices_serial_no",
                table: "biometric_devices",
                column: "serial_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_biometric_punches_student_id",
                table: "biometric_punches",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "ix_punch_time",
                table: "biometric_punches",
                column: "punched_at");

            migrationBuilder.CreateIndex(
                name: "uq_punch",
                table: "biometric_punches",
                columns: new[] { "device_id", "enroll_id", "punched_at" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_board_affiliations_board_id",
                table: "board_affiliations",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "uq_board_rule_version",
                table: "board_rules",
                columns: new[] { "board_id", "rule_type", "scope_key", "effective_year_id", "version" },
                unique: true,
                filter: "[effective_year_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_boards_code",
                table: "boards",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_boards_name",
                table: "boards",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_book_issues_book_id",
                table: "book_issues",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "IX_book_issues_student_id",
                table: "book_issues",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_book_reservations_book_id",
                table: "book_reservations",
                column: "book_id");

            migrationBuilder.CreateIndex(
                name: "IX_book_reservations_student_id",
                table: "book_reservations",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_books_category_id",
                table: "books",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_books_isbn",
                table: "books",
                column: "isbn",
                unique: true,
                filter: "[isbn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_branches_code",
                table: "branches",
                column: "code",
                unique: true,
                filter: "[code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_branches_name",
                table: "branches",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_class_subjects_staff_id",
                table: "class_subjects",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_class_subjects_subject_id",
                table: "class_subjects",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "uq_class_subject",
                table: "class_subjects",
                columns: new[] { "class_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_classes_board_id",
                table: "classes",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "IX_classes_branch_id",
                table: "classes",
                column: "branch_id");

            migrationBuilder.CreateIndex(
                name: "IX_classes_class_teacher_id",
                table: "classes",
                column: "class_teacher_id");

            migrationBuilder.CreateIndex(
                name: "uq_class",
                table: "classes",
                columns: new[] { "name", "section" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clinic_visits_student_id",
                table: "clinic_visits",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_co_scholastic_areas_name",
                table: "co_scholastic_areas",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_co_scholastic_grades_area_id",
                table: "co_scholastic_grades",
                column: "area_id");

            migrationBuilder.CreateIndex(
                name: "IX_co_scholastic_grades_exam_group_id",
                table: "co_scholastic_grades",
                column: "exam_group_id");

            migrationBuilder.CreateIndex(
                name: "uq_cosch",
                table: "co_scholastic_grades",
                columns: new[] { "student_id", "area_id", "exam_group_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departments_name",
                table: "departments",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_designations_name",
                table: "designations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_document_check",
                table: "document_checks",
                columns: new[] { "student_id", "title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_document_requests_certificate_no",
                table: "document_requests",
                column: "certificate_no",
                unique: true,
                filter: "[certificate_no] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_document_requests_student_id",
                table: "document_requests",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_templates_type",
                table: "document_templates",
                column: "type",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_exam_group",
                table: "exam_groups",
                columns: new[] { "class_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_exams_class_id",
                table: "exams",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_exams_exam_group_id",
                table: "exams",
                column: "exam_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_fee_adjustments_fee_id",
                table: "fee_adjustments",
                column: "fee_id");

            migrationBuilder.CreateIndex(
                name: "IX_fee_group_items_group_id",
                table: "fee_group_items",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_fee_groups_name",
                table: "fee_groups",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fee_structures_class_id",
                table: "fee_structures",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_fee_types_name",
                table: "fee_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_feedback_status",
                table: "feedback",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_student_id",
                table: "feedback",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_feedback_ticket_no",
                table: "feedback",
                column: "ticket_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_feedback_user",
                table: "feedback",
                column: "submitted_by_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_fblog_feedback",
                table: "feedback_logs",
                column: "feedback_id");

            migrationBuilder.CreateIndex(
                name: "IX_fees_student_id",
                table: "fees",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_class_id",
                table: "homework",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_subject_id",
                table: "homework",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_homework_submissions_student_id",
                table: "homework_submissions",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "uq_homework_student",
                table: "homework_submissions",
                columns: new[] { "homework_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hostel_allocations_room_id",
                table: "hostel_allocations",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_hostel_allocations_student_id",
                table: "hostel_allocations",
                column: "student_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_hostel_att",
                table: "hostel_attendance",
                columns: new[] { "student_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_hostel_meal",
                table: "hostel_meals",
                columns: new[] { "hostel_id", "day", "meal" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_hostel_room",
                table: "hostel_rooms",
                columns: new[] { "hostel_id", "room_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hostel_visitors_student_id",
                table: "hostel_visitors",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_hostels_name",
                table: "hostels",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_incidents_student_id",
                table: "incidents",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_categories_name",
                table: "inventory_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_items_category_id",
                table: "inventory_items",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_items_name",
                table: "inventory_items",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_requests_item_id",
                table: "inventory_requests",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_applicants_opening_id",
                table: "job_applicants",
                column: "opening_id");

            migrationBuilder.CreateIndex(
                name: "IX_job_openings_department_id",
                table: "job_openings",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_leave_type_id",
                table: "leave_requests",
                column: "leave_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_leave_requests_staff_id",
                table: "leave_requests",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_leave_types_name",
                table: "leave_types",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_library_categories_name",
                table: "library_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_library_memberships_card_no",
                table: "library_memberships",
                column: "card_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_library_memberships_student_id",
                table: "library_memberships",
                column: "student_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lms_announcements_course_id",
                table: "lms_announcements",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_assignments_course_id",
                table: "lms_assignments",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_courses_class_id",
                table: "lms_courses",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_courses_subject_id",
                table: "lms_courses",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_enrollments_student_id",
                table: "lms_enrollments",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "lms_enrollment_course_student",
                table: "lms_enrollments",
                columns: new[] { "course_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lms_lesson_parts_course_id",
                table: "lms_lesson_parts",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_lesson_parts_lesson_id",
                table: "lms_lesson_parts",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_lessons_course_id",
                table: "lms_lessons",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_lessons_module_id",
                table: "lms_lessons",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_materials_course_id",
                table: "lms_materials",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_modules_course_id",
                table: "lms_modules",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_progress_lesson_id",
                table: "lms_progress",
                column: "lesson_id");

            migrationBuilder.CreateIndex(
                name: "lms_progress_student_lesson",
                table: "lms_progress",
                columns: new[] { "student_id", "lesson_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lms_quiz_attempts_student_id",
                table: "lms_quiz_attempts",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "lms_quiz_attempt_no",
                table: "lms_quiz_attempts",
                columns: new[] { "quiz_id", "student_id", "attempt_no" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lms_quizzes_course_id",
                table: "lms_quizzes",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_lms_submissions_student_id",
                table: "lms_submissions",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "lms_submission_student",
                table: "lms_submissions",
                columns: new[] { "assignment_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_lms_tags_name",
                table: "lms_tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_login_events_created",
                table: "login_events",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_login_events_user",
                table: "login_events",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_marks_student_id",
                table: "marks",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_marks_subject_id",
                table: "marks",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "uq_mark",
                table: "marks",
                columns: new[] { "exam_id", "student_id", "subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_master_item",
                table: "master_items",
                columns: new[] { "type", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_message_templates_name",
                table: "message_templates",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_messages_kind_student",
                table: "messages",
                columns: new[] { "kind", "student_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_office_visitor_in",
                table: "office_visitors",
                column: "check_in");

            migrationBuilder.CreateIndex(
                name: "IX_online_classes_class_id",
                table: "online_classes",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_online_classes_subject_id",
                table: "online_classes",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_parent_students_student_id",
                table: "parent_students",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "uq_parent_student",
                table: "parent_students",
                columns: new[] { "parent_id", "student_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_parents_user_id",
                table: "parents",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_fee_id",
                table: "payments",
                column: "fee_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_receipt_no",
                table: "payments",
                column: "receipt_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_payslip",
                table: "payslips",
                columns: new[] { "staff_id", "month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_performance_reviews_staff_id",
                table: "performance_reviews",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "uq_period_att",
                table: "period_attendance",
                columns: new[] { "student_id", "date", "period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_photo_owner",
                table: "photos",
                columns: new[] { "entity", "entity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_item_id",
                table: "purchase_order_lines",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_order_lines_order_id",
                table: "purchase_order_lines",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_orders_supplier_id",
                table: "purchase_orders",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_push_subscriptions_endpoint",
                table: "push_subscriptions",
                column: "endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_push_subscriptions_user_id",
                table: "push_subscriptions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_record_extras",
                table: "record_extras",
                columns: new[] { "entity", "entity_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_refunds_payment_id",
                table: "refunds",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_folders_name",
                table: "report_folders",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_report_run_status",
                table: "report_runs",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_report_run_user",
                table: "report_runs",
                columns: new[] { "user_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "uq_report_template",
                table: "report_templates",
                columns: new[] { "folder_id", "name" },
                unique: true,
                filter: "[folder_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_revaluation_requests_exam_id",
                table: "revaluation_requests",
                column: "exam_id");

            migrationBuilder.CreateIndex(
                name: "IX_revaluation_requests_student_id",
                table: "revaluation_requests",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_revaluation_requests_subject_id",
                table: "revaluation_requests",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "uq_role_right",
                table: "role_rights",
                columns: new[] { "role", "module" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_room_booking",
                table: "room_bookings",
                columns: new[] { "room_id", "date", "period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_name",
                table: "rooms",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_department_id",
                table: "staff",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_staff_no",
                table: "staff",
                column: "staff_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_staff_att",
                table: "staff_attendance",
                columns: new[] { "staff_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_files_staff_id",
                table: "staff_files",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_staff_files_stored_name",
                table: "staff_files",
                column: "stored_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_salaries_staff_id",
                table: "staff_salaries",
                column: "staff_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_staff_unavail",
                table: "staff_unavailability",
                columns: new[] { "staff_id", "day", "period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_item_id",
                table: "stock_movements",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_supplier_id",
                table: "stock_movements",
                column: "supplier_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_accounts_student_id",
                table: "student_accounts",
                column: "student_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_accounts_user_id",
                table: "student_accounts",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_board_registrations_board_id",
                table: "student_board_registrations",
                column: "board_id");

            migrationBuilder.CreateIndex(
                name: "uq_student_board_reg",
                table: "student_board_registrations",
                columns: new[] { "student_id", "board_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_files_stored_name",
                table: "student_files",
                column: "stored_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_files_student_id",
                table: "student_files",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_health_student_id",
                table: "student_health",
                column: "student_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_leaves_leave_type_id",
                table: "student_leaves",
                column: "leave_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_leaves_student_id",
                table: "student_leaves",
                column: "student_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_subjects_class_subject_id",
                table: "student_subjects",
                column: "class_subject_id");

            migrationBuilder.CreateIndex(
                name: "uq_student_subject",
                table: "student_subjects",
                columns: new[] { "student_id", "class_subject_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_student_transport_route_id",
                table: "student_transport",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_transport_stop_id",
                table: "student_transport",
                column: "stop_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_transport_student_id",
                table: "student_transport",
                column: "student_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_admission_no",
                table: "students",
                column: "admission_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_students_class_id",
                table: "students",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_materials_class_id",
                table: "study_materials",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_materials_subject_id",
                table: "study_materials",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_subjects_code",
                table: "subjects",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_substitutions_substitute_id",
                table: "substitutions",
                column: "substitute_id");

            migrationBuilder.CreateIndex(
                name: "uq_substitution",
                table: "substitutions",
                columns: new[] { "slot_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_name",
                table: "suppliers",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_syllabus_topics_class_id",
                table: "syllabus_topics",
                column: "class_id");

            migrationBuilder.CreateIndex(
                name: "IX_syllabus_topics_subject_id",
                table: "syllabus_topics",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_slots_room_id",
                table: "timetable_slots",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_slots_staff_id",
                table: "timetable_slots",
                column: "staff_id");

            migrationBuilder.CreateIndex(
                name: "IX_timetable_slots_subject_id",
                table: "timetable_slots",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "uq_slot",
                table: "timetable_slots",
                columns: new[] { "class_id", "day", "period" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_transport_att",
                table: "transport_attendance",
                columns: new[] { "student_id", "date", "trip" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transport_routes_name",
                table: "transport_routes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transport_routes_vehicle_id",
                table: "transport_routes",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_transport_staff_vehicle_id",
                table: "transport_staff",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_transport_stops_route_id",
                table: "transport_stops",
                column: "route_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_group_members_user_id",
                table: "user_group_members",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "uq_group_member",
                table: "user_group_members",
                columns: new[] { "group_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_groups_name",
                table: "user_groups",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_vloc",
                table: "vehicle_locations",
                columns: new[] { "vehicle_id", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_maintenance_vehicle_id",
                table: "vehicle_maintenance",
                column: "vehicle_id");

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_trackers_api_key",
                table: "vehicle_trackers",
                column: "api_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicle_trackers_vehicle_id",
                table: "vehicle_trackers",
                column: "vehicle_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vehicles_reg_no",
                table: "vehicles",
                column: "reg_no",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "absence_alerts");

            migrationBuilder.DropTable(
                name: "academic_years");

            migrationBuilder.DropTable(
                name: "activity_participants");

            migrationBuilder.DropTable(
                name: "admissions");

            migrationBuilder.DropTable(
                name: "app_menu_items");

            migrationBuilder.DropTable(
                name: "application_documents");

            migrationBuilder.DropTable(
                name: "asset_maintenance");

            migrationBuilder.DropTable(
                name: "attendance");

            migrationBuilder.DropTable(
                name: "attendance_corrections");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "behavior_records");

            migrationBuilder.DropTable(
                name: "biometric_punches");

            migrationBuilder.DropTable(
                name: "board_affiliations");

            migrationBuilder.DropTable(
                name: "board_rules");

            migrationBuilder.DropTable(
                name: "book_issues");

            migrationBuilder.DropTable(
                name: "book_reservations");

            migrationBuilder.DropTable(
                name: "calendar_events");

            migrationBuilder.DropTable(
                name: "clinic_visits");

            migrationBuilder.DropTable(
                name: "co_scholastic_grades");

            migrationBuilder.DropTable(
                name: "designations");

            migrationBuilder.DropTable(
                name: "document_checks");

            migrationBuilder.DropTable(
                name: "document_requests");

            migrationBuilder.DropTable(
                name: "document_templates");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "fee_adjustments");

            migrationBuilder.DropTable(
                name: "fee_group_items");

            migrationBuilder.DropTable(
                name: "fee_structures");

            migrationBuilder.DropTable(
                name: "fee_types");

            migrationBuilder.DropTable(
                name: "feedback_logs");

            migrationBuilder.DropTable(
                name: "grade_bands");

            migrationBuilder.DropTable(
                name: "grading_settings");

            migrationBuilder.DropTable(
                name: "homework_submissions");

            migrationBuilder.DropTable(
                name: "hostel_allocations");

            migrationBuilder.DropTable(
                name: "hostel_attendance");

            migrationBuilder.DropTable(
                name: "hostel_meals");

            migrationBuilder.DropTable(
                name: "hostel_visitors");

            migrationBuilder.DropTable(
                name: "incidents");

            migrationBuilder.DropTable(
                name: "incomes");

            migrationBuilder.DropTable(
                name: "inventory_requests");

            migrationBuilder.DropTable(
                name: "job_applicants");

            migrationBuilder.DropTable(
                name: "leave_requests");

            migrationBuilder.DropTable(
                name: "library_memberships");

            migrationBuilder.DropTable(
                name: "lms_announcements");

            migrationBuilder.DropTable(
                name: "lms_enrollments");

            migrationBuilder.DropTable(
                name: "lms_lesson_parts");

            migrationBuilder.DropTable(
                name: "lms_materials");

            migrationBuilder.DropTable(
                name: "lms_progress");

            migrationBuilder.DropTable(
                name: "lms_quiz_attempts");

            migrationBuilder.DropTable(
                name: "lms_submissions");

            migrationBuilder.DropTable(
                name: "lms_tags");

            migrationBuilder.DropTable(
                name: "login_events");

            migrationBuilder.DropTable(
                name: "marks");

            migrationBuilder.DropTable(
                name: "master_items");

            migrationBuilder.DropTable(
                name: "message_templates");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "notices");

            migrationBuilder.DropTable(
                name: "office_visitors");

            migrationBuilder.DropTable(
                name: "online_classes");

            migrationBuilder.DropTable(
                name: "parent_students");

            migrationBuilder.DropTable(
                name: "payslips");

            migrationBuilder.DropTable(
                name: "performance_reviews");

            migrationBuilder.DropTable(
                name: "period_attendance");

            migrationBuilder.DropTable(
                name: "photos");

            migrationBuilder.DropTable(
                name: "purchase_order_lines");

            migrationBuilder.DropTable(
                name: "push_subscriptions");

            migrationBuilder.DropTable(
                name: "record_extras");

            migrationBuilder.DropTable(
                name: "refunds");

            migrationBuilder.DropTable(
                name: "report_runs");

            migrationBuilder.DropTable(
                name: "report_templates");

            migrationBuilder.DropTable(
                name: "revaluation_requests");

            migrationBuilder.DropTable(
                name: "role_rights");

            migrationBuilder.DropTable(
                name: "room_bookings");

            migrationBuilder.DropTable(
                name: "safety_inspections");

            migrationBuilder.DropTable(
                name: "settings");

            migrationBuilder.DropTable(
                name: "staff_attendance");

            migrationBuilder.DropTable(
                name: "staff_files");

            migrationBuilder.DropTable(
                name: "staff_salaries");

            migrationBuilder.DropTable(
                name: "staff_unavailability");

            migrationBuilder.DropTable(
                name: "stock_movements");

            migrationBuilder.DropTable(
                name: "student_accounts");

            migrationBuilder.DropTable(
                name: "student_board_registrations");

            migrationBuilder.DropTable(
                name: "student_files");

            migrationBuilder.DropTable(
                name: "student_health");

            migrationBuilder.DropTable(
                name: "student_leaves");

            migrationBuilder.DropTable(
                name: "student_subjects");

            migrationBuilder.DropTable(
                name: "student_transport");

            migrationBuilder.DropTable(
                name: "study_materials");

            migrationBuilder.DropTable(
                name: "substitutions");

            migrationBuilder.DropTable(
                name: "support_tickets");

            migrationBuilder.DropTable(
                name: "syllabus_topics");

            migrationBuilder.DropTable(
                name: "transport_attendance");

            migrationBuilder.DropTable(
                name: "transport_staff");

            migrationBuilder.DropTable(
                name: "user_group_members");

            migrationBuilder.DropTable(
                name: "vehicle_locations");

            migrationBuilder.DropTable(
                name: "vehicle_maintenance");

            migrationBuilder.DropTable(
                name: "vehicle_trackers");

            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "biometric_devices");

            migrationBuilder.DropTable(
                name: "books");

            migrationBuilder.DropTable(
                name: "co_scholastic_areas");

            migrationBuilder.DropTable(
                name: "fee_groups");

            migrationBuilder.DropTable(
                name: "feedback");

            migrationBuilder.DropTable(
                name: "homework");

            migrationBuilder.DropTable(
                name: "hostel_rooms");

            migrationBuilder.DropTable(
                name: "job_openings");

            migrationBuilder.DropTable(
                name: "lms_lessons");

            migrationBuilder.DropTable(
                name: "lms_quizzes");

            migrationBuilder.DropTable(
                name: "lms_assignments");

            migrationBuilder.DropTable(
                name: "parents");

            migrationBuilder.DropTable(
                name: "purchase_orders");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "report_folders");

            migrationBuilder.DropTable(
                name: "exams");

            migrationBuilder.DropTable(
                name: "inventory_items");

            migrationBuilder.DropTable(
                name: "leave_types");

            migrationBuilder.DropTable(
                name: "class_subjects");

            migrationBuilder.DropTable(
                name: "transport_stops");

            migrationBuilder.DropTable(
                name: "timetable_slots");

            migrationBuilder.DropTable(
                name: "user_groups");

            migrationBuilder.DropTable(
                name: "library_categories");

            migrationBuilder.DropTable(
                name: "hostels");

            migrationBuilder.DropTable(
                name: "lms_modules");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DropTable(
                name: "fees");

            migrationBuilder.DropTable(
                name: "exam_groups");

            migrationBuilder.DropTable(
                name: "inventory_categories");

            migrationBuilder.DropTable(
                name: "transport_routes");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "lms_courses");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "vehicles");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "classes");

            migrationBuilder.DropTable(
                name: "boards");

            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "departments");
        }
    }
}
