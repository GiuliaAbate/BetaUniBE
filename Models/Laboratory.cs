using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class Laboratory
{
    public int LabId { get; set; }

    public string Name { get; set; } = null!;

    public string Attendance { get; set; } = null!;

    public string DepartmentId { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public virtual ICollection<Classroom>? Classrooms { get; set; } = new List<Classroom>();

    public virtual Department? Department { get; set; } = null!;

    public virtual ICollection<ProfessorLab>? ProfessorLabs { get; set; } = new List<ProfessorLab>();

    public virtual ICollection<StudentLab>? StudentLabs { get; set; } = new List<StudentLab>();
}
