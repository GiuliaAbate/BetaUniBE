using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class Exam
{
    public int ExamId { get; set; }

    public string Name { get; set; } = null!;

    public int Cfu { get; set; }

    public string Type { get; set; } = null!;

    public DateOnly Date { get; set; }

    public string CourseId { get; set; } = null!;

    public string DepartmentId { get; set; } = null!;

    public virtual Course? Course { get; set; } = null!;

    public virtual Department? Department { get; set; } = null!;

    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; } = new List<ExamRegistration>();

    public virtual ICollection<ProfCourseExam>? ProfCourseExams { get; set; } = new List<ProfCourseExam>();
}
