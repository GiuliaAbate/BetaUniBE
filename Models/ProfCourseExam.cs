using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class ProfCourseExam
{
    public int Id { get; set; }

    public string ProfId { get; set; } = null!;

    public string CourseId { get; set; } = null!;

    public int ExamId { get; set; }

    public string DepartmentId { get; set; } = null!;

    public virtual Course? Course { get; set; } = null!;

    public virtual Department? Department { get; set; } = null!;

    public virtual Exam? Exam { get; set; } = null!;

    public virtual Professor? Prof { get; set; } = null!;
}
