using System.Collections.Generic;

namespace UniversitySchedule.Core
{
    public class Group
    {
        public string NameOfGroup { get; set; }
        public List<Student> Students { get; set; }
        public override string ToString() => NameOfGroup;
    }
}
