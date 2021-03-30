using System.ComponentModel;

namespace UniversitySchedule.Core
{
    public enum ClassType
    {
        [Description("Лекция")]
        Lecture = 0,
        [Description("Семинар")]
        Seminar = 1,
        [Description("Контрольная работа")]
        Test = 3,
        [Description("Экзамен")]
        Exam = 4
    }
}
