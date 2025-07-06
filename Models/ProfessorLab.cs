using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class ProfessorLab
{
    public int Id { get; set; }

    public string ProfId { get; set; } = null!;

    public int LabId { get; set; }

    public string DepartmentId { get; set; } = null!;

    public virtual Department? Department { get; set; } = null!;

    public virtual Laboratory? Lab { get; set; } = null!;

    public virtual Professor? Prof { get; set; } = null!;
}
