using System;
using System.Collections.Generic;

namespace BetaUni.Models;

public partial class Course
{
    public string CourseId { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string DepartmentId { get; set; } = null!;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public virtual ICollection<Classroom>? Classrooms { get; set; } = new List<Classroom>();

    public virtual Department? Department { get; set; } = null!;

    public virtual ICollection<ExamRegistration>? ExamRegistrations { get; set; } = new List<ExamRegistration>();

    public virtual ICollection<Exam>? Exams { get; set; } = new List<Exam>();

    public virtual ICollection<ProfCourseExam>? ProfCourseExams { get; set; } = new List<ProfCourseExam>();

    public virtual ICollection<StudentCourse>? StudentCourses { get; set; } = new List<StudentCourse>();
}
