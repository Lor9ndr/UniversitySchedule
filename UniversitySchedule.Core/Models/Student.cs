namespace UniversitySchedule.Core
{
    public class Student
    {
        public string Name { get; set; }
        public User User { get; set; }
        public override string ToString() => Name;
    }
}