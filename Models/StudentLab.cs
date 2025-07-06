using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class StudentLab
{
    public int Id { get; set; }

    public string StudId { get; set; } = null!;

    public int LabId { get; set; }

    public string DepartmentId { get; set; } = null!;

    public virtual Department? Department { get; set; } = null!;

    public virtual Laboratory? Lab { get; set; } = null!;

    public virtual Student? Stud { get; set; } = null!;
}
