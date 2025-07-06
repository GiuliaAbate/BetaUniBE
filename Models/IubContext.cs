using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BetaUni.Models;

public partial class IubContext : DbContext
{
    public IubContext()
    {
    }

    public IubContext(DbContextOptions<IubContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Classroom> Classrooms { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<ExamRegistration> ExamRegistrations { get; set; }

    public virtual DbSet<Laboratory> Laboratories { get; set; }

    public virtual DbSet<ProfCourseExam> ProfCourseExams { get; set; }

    public virtual DbSet<Professor> Professors { get; set; }

    public virtual DbSet<ProfessorLab> ProfessorLabs { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentCourse> StudentCourses { get; set; }

    public virtual DbSet<StudentLab> StudentLabs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.HasKey(e => e.ClassId);

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.CourseId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("CourseID");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Course).WithMany(p => p.Classrooms)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Classrooms_Departments");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71873B0E9EB4");

            entity.Property(e => e.CourseId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("CourseID");
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Department).WithMany(p => p.Courses)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_Departments");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.Property(e => e.Cfu).HasColumnName("CFU");
            entity.Property(e => e.CourseId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("CourseID");
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Type).HasMaxLength(50);

            entity.HasOne(d => d.Course).WithMany(p => p.Exams)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exams_Courses");

            entity.HasOne(d => d.Department).WithMany(p => p.Exams)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Exams_Departments");
        });

        modelBuilder.Entity<ExamRegistration>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CourseId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("CourseID");
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.Property(e => e.StudId)
                .HasMaxLength(6)
                .IsFixedLength()
                .HasColumnName("StudID");

            entity.HasOne(d => d.Course).WithMany(p => p.ExamRegistrations)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamRegistrations_Courses");

            entity.HasOne(d => d.Department).WithMany(p => p.ExamRegistrations)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamRegistrations_Departments");

            entity.HasOne(d => d.Exam).WithMany(p => p.ExamRegistrations)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamRegistrations_Exams");

            entity.HasOne(d => d.Stud).WithMany(p => p.ExamRegistrations)
                .HasForeignKey(d => d.StudId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExamRegistrations_Students");
        });

        modelBuilder.Entity<Laboratory>(entity =>
        {
            entity.HasKey(e => e.LabId);

            entity.Property(e => e.LabId).HasColumnName("LabID");
            entity.Property(e => e.Attendance)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.Name).HasMaxLength(100);

            entity.HasOne(d => d.Department).WithMany(p => p.Laboratories)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Laboratories_Departments");
        });

        modelBuilder.Entity<ProfCourseExam>(entity =>
        {
            entity.ToTable("ProfCourseExam");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CourseId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("CourseID");
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.ExamId).HasColumnName("ExamID");
            entity.Property(e => e.ProfId)
                .HasMaxLength(6)
                .IsFixedLength()
                .HasColumnName("ProfID");

            entity.HasOne(d => d.Course).WithMany(p => p.ProfCourseExams)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProfCourseExam_Courses");

            entity.HasOne(d => d.Department).WithMany(p => p.ProfCourseExams)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProfCourseExam_Departments");

            entity.HasOne(d => d.Exam).WithMany(p => p.ProfCourseExams)
                .HasForeignKey(d => d.ExamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProfCourseExam_Exams");

            entity.HasOne(d => d.Prof).WithMany(p => p.ProfCourseExams)
                .HasForeignKey(d => d.ProfId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProfCourseExam_Professors");
        });

        modelBuilder.Entity<Professor>(entity =>
        {
            entity.HasKey(e => e.ProfId);

            entity.Property(e => e.ProfId)
                .HasMaxLength(6)
                .IsFixedLength()
                .HasColumnName("ProfID");
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(30);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Salt).HasMaxLength(100);
            entity.Property(e => e.Surname).HasMaxLength(30);

            entity.HasOne(d => d.Department).WithMany(p => p.Professors)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Professors_Departments");
        });

        modelBuilder.Entity<ProfessorLab>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.LabId).HasColumnName("LabID");
            entity.Property(e => e.ProfId)
                .HasMaxLength(6)
                .IsFixedLength()
                .HasColumnName("ProfID");

            entity.HasOne(d => d.Department).WithMany(p => p.ProfessorLabs)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProfessorLabs_Departments");

            entity.HasOne(d => d.Lab).WithMany(p => p.ProfessorLabs)
                .HasForeignKey(d => d.LabId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProfessorLabs_Laboratories");

            entity.HasOne(d => d.Prof).WithMany(p => p.ProfessorLabs)
                .HasForeignKey(d => d.ProfId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProfessorLabs_Professors");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudId);

            entity.Property(e => e.StudId)
                .HasMaxLength(6)
                .IsFixedLength()
                .HasColumnName("StudID");
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(30);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(10)
                .IsFixedLength();
            entity.Property(e => e.Salt).HasMaxLength(100);
            entity.Property(e => e.Surname).HasMaxLength(30);
        });

        modelBuilder.Entity<StudentCourse>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CourseId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("CourseID");
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.StudId)
                .HasMaxLength(6)
                .IsFixedLength()
                .HasColumnName("StudID");

            entity.HasOne(d => d.Course).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentCourses_Courses");

            entity.HasOne(d => d.Department).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentCourses_Departments");

            entity.HasOne(d => d.Stud).WithMany(p => p.StudentCourses)
                .HasForeignKey(d => d.StudId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentCourses_Students");
        });

        modelBuilder.Entity<StudentLab>(entity =>
        {
            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.DepartmentId)
                .HasMaxLength(4)
                .IsFixedLength()
                .HasColumnName("DepartmentID");
            entity.Property(e => e.LabId).HasColumnName("LabID");
            entity.Property(e => e.StudId)
                .HasMaxLength(6)
                .IsFixedLength()
                .HasColumnName("StudID");

            entity.HasOne(d => d.Department).WithMany(p => p.StudentLabs)
                .HasForeignKey(d => d.DepartmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentLabs_Departments");

            entity.HasOne(d => d.Lab).WithMany(p => p.StudentLabs)
                .HasForeignKey(d => d.LabId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentLabs_Laboratories");

            entity.HasOne(d => d.Stud).WithMany(p => p.StudentLabs)
                .HasForeignKey(d => d.StudId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_StudentLabs_Students");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
