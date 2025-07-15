using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class Classroom
{
    public int ClassId { get; set; }

    public string Name { get; set; } = null!;

    public int Number { get; set; }

    public int MaxCapacity { get; set; }

    public string? CourseId { get; set; }

    public int? LabId { get; set; }

    public virtual Course? Course { get; set; }

    public virtual Laboratory? Lab { get; set; }
}
