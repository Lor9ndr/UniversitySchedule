namespace UniversitySchedule.Core
{
    public class Teacher
    {

        public string Name { get; set; }
        public string Faculty { get; set; }
        public User User { get; set; }
        public override string ToString() => Name;

    }
}
