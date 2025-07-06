using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class Professor
{
    public string ProfId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string Surname { get; set; } = null!;

    public DateOnly BirthDate { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string DepartmentId { get; set; } = null!;

    public DateOnly EnrollmentDate { get; set; }

    public virtual Department? Department { get; set; } = null!;

    public virtual ICollection<ProfCourseExam>? ProfCourseExams { get; set; } = new List<ProfCourseExam>();

    public virtual ICollection<ProfessorLab>? ProfessorLabs { get; set; } = new List<ProfessorLab>();
}
