using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class Department
{
    public string DepartmentId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public virtual ICollection<Course>? Courses { get; set; } = new List<Course>();

    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; } = new List<ExamRegistration>();

    public virtual ICollection<Exam>? Exams { get; set; } = new List<Exam>();

    public virtual ICollection<Laboratory>? Laboratories { get; set; } = new List<Laboratory>();

    public virtual ICollection<ProfCourseExam>? ProfCourseExams { get; set; } = new List<ProfCourseExam>();

    public virtual ICollection<ProfessorLab>? ProfessorLabs { get; set; } = new List<ProfessorLab>();

    public virtual ICollection<Professor>? Professors { get; set; } = new List<Professor>();

    public virtual ICollection<StudentCourse>? StudentCourses { get; set; } = new List<StudentCourse>();

    public virtual ICollection<StudentLab>? StudentLabs { get; set; } = new List<StudentLab>();
}
