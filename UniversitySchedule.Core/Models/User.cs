using System;

namespace UniversitySchedule.Core
{
    public class User
    {
        public static int UserId { get; set; } = 1;
        public string Email { get; set; }
        public string Password { get; set; }
        public User()
        {
            IncreaseId();
        }

        static void IncreaseId()
        {
            UserId++;
        }
    }

}
