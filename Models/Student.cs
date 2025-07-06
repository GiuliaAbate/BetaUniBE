using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class Student
{
    public string StudId { get; set; } = null!;

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

    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; } = new List<ExamRegistration>();

    public virtual ICollection<StudentCourse>? StudentCourses { get; set; } = new List<StudentCourse>();

    public virtual ICollection<StudentLab>? StudentLabs { get; set; } = new List<StudentLab>();
}
