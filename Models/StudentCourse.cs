using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class StudentCourse
{
    public int Id { get; set; }

    public string StudId { get; set; } = null!;

    public string CourseId { get; set; } = null!;

    public string DepartmentId { get; set; } = null!;

    public virtual Course? Course { get; set; } = null!;

    public virtual Department? Department { get; set; } = null!;

    public virtual Student? Stud { get; set; } = null!;
}
