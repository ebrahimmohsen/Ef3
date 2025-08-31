using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Emit;

namespace EF3
{

    public class Student
    {
        public int ID { get; set; }
        [Required, MaxLength(50)]
        public string FName { get; set; } = string.Empty;
        [Required, MaxLength(50)]
        public string LName { get; set; } = string.Empty;
        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;
        public int Age { get; set; }

        public int Dep_Id { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<Stud_Course> Stud_Courses { get; set; } = new List<Stud_Course>();
    }

    public class Instructor
    {
        public int ID { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public decimal Bouns { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Salary { get; set; }
        [MaxLength(100)]
        public string Address { get; set; } = string.Empty;
        public decimal HourRate { get; set; }

        public int Dept_ID { get; set; }
        public Department Department { get; set; } = null!;

        public ICollection<Course_Inst> Course_Insts { get; set; } = new List<Course_Inst>();
    }

    public class Course
    {
        public int ID { get; set; }
        public int Duration { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(250)]
        public string Description { get; set; } = string.Empty;

        public int Top_ID { get; set; }
        public Topic Topic { get; set; } = null!;

        public ICollection<Stud_Course> Stud_Courses { get; set; } = new List<Stud_Course>();
        public ICollection<Course_Inst> Course_Insts { get; set; } = new List<Course_Inst>();
    }

    public class Department
    {
        public int ID { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public int Ins_ID { get; set; }
        public Instructor Instructor { get; set; } = null!;

        public DateTime HiringDate { get; set; }

        public ICollection<Student> Students { get; set; } = new List<Student>();
    }

    public class Topic
    {
        public int ID { get; set; }
        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Course> Courses { get; set; } = new List<Course>();
    }

    public class Stud_Course
    {
        public int stud_ID { get; set; }
        public Student Student { get; set; } = null!;
        public int Course_ID { get; set; }
        public Course Course { get; set; } = null!;
        public int Grade { get; set; }
    }

    public class Course_Inst
    {
        public int inst_ID { get; set; }
        public Instructor Instructor { get; set; } = null!;
        public int Course_ID { get; set; }
        public Course Course { get; set; } = null!;
        public string? Evaluate { get; set; }
    }


    public class ITIDbContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Topic> Topics { get; set; }
        public DbSet<Stud_Course> Stud_Courses { get; set; }
        public DbSet<Course_Inst> Course_Insts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=.;Database=ITIDb;Trusted_Connection=True;TrustServerCertificate=True");
        }
        #region Q1 Map All the Relationships

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Student - Department (Many-to-One)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Students)
                .HasForeignKey(s => s.Dep_Id);

            // Department - Instructor (One-to-One)
            modelBuilder.Entity<Department>()
                .HasOne(d => d.Instructor)
                .WithMany()
                .HasForeignKey(d => d.Ins_ID)
                .OnDelete(DeleteBehavior.NoAction);

            // Course - Topic (Many-to-One)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Topic)
                .WithMany(t => t.Courses)
                .HasForeignKey(c => c.Top_ID);

            // Student - Course (Many-to-Many)
            modelBuilder.Entity<Stud_Course>()
                .HasKey(sc => new { sc.stud_ID, sc.Course_ID });

            modelBuilder.Entity<Stud_Course>()
                .HasOne(sc => sc.Student)
                .WithMany(s => s.Stud_Courses)
                .HasForeignKey(sc => sc.stud_ID);

            modelBuilder.Entity<Stud_Course>()
                .HasOne(sc => sc.Course)
                .WithMany(c => c.Stud_Courses)
                .HasForeignKey(sc => sc.Course_ID);

            // Instructor - Course (Many-to-Many)
            modelBuilder.Entity<Course_Inst>()
                .HasKey(ci => new { ci.inst_ID, ci.Course_ID });

            modelBuilder.Entity<Course_Inst>()
                .HasOne(ci => ci.Instructor)
                .WithMany(i => i.Course_Insts)
                .HasForeignKey(ci => ci.inst_ID);

            modelBuilder.Entity<Course_Inst>()
                .HasOne(ci => ci.Course)
                .WithMany(c => c.Course_Insts)
                .HasForeignKey(ci => ci.Course_ID);
        }
    }
    #endregion

    #region Q1  (CRUD + DEMO Examples) 

    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new ITIDbContext())
            {
                Console.WriteLine("=== EF Core ITI Project (EF1) ===");

                // Insert Student
                var student = new Student
                {
                    FName = "Ali",
                    LName = "Hassan",
                    Age = 22,
                    Address = "Cairo",
                    Dep_Id = 1
                };
                db.Students.Add(student);
                db.SaveChanges();
                Console.WriteLine("Student Inserted.");

                // Read All Students
                var students = db.Students.ToList();
                Console.WriteLine("Students in DB:");
                foreach (var s in students)
                    Console.WriteLine($"{s.ID} - {s.FName} {s.LName}");

                // Update Student
                var firstStudent = db.Students.FirstOrDefault();
                if (firstStudent != null)
                {
                    firstStudent.Address = "Alexandria";
                    db.SaveChanges();
                    Console.WriteLine("Student Updated.");
                }

                // Delete Student
                var delStudent = db.Students.FirstOrDefault();
                if (delStudent != null)
                {
                    db.Students.Remove(delStudent);
                    db.SaveChanges();
                    Console.WriteLine("Student Deleted.");
                }

                // ==== LINQ DEMO ====
                Console.WriteLine("LINQ Examples:");

                // Get all instructors with salary > 5000
                var highSalaryInstructors = db.Instructors
                                              .Where(i => i.Salary > 5000)
                                              .ToList();
                Console.WriteLine("Instructors with high salary:");
                foreach (var i in highSalaryInstructors)
                    Console.WriteLine($"{i.Name} - {i.Salary}");

                // Join Student with Department
                var studentDept = db.Students
                                    .Join(db.Departments,
                                          s => s.Dep_Id,
                                          d => d.ID,
                                          (s, d) => new { s.FName, d.Name })
                                    .ToList();
                Console.WriteLine("Students with Department:");
                foreach (var sd in studentDept)
                    Console.WriteLine($"{sd.FName} - {sd.Name}");

                // Group Courses by Topic
                var coursesByTopic = db.Courses
                                       .GroupBy(c => c.Top_ID)
                                       .Select(g => new { TopicID = g.Key, Count = g.Count() })
                                       .ToList();
                Console.WriteLine("Courses per Topic:");
                foreach (var g in coursesByTopic)
                    Console.WriteLine($"Topic {g.TopicID} has {g.Count} courses");
            }

            Console.WriteLine("=== Done ===");
        }
    }
}
#endregion