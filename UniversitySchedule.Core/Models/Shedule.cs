using System;
using System.Collections.ObjectModel;

namespace UniversitySchedule.Core
{
    public class Shedule
    {
        public static int SheduleId { get; set; } = 1;
        private string _onlineUrl;
        public string NameOfClass { get; set; }
        public ClassType ClassType { get; set; }
        public DateTime TimeOfClass { get; set; }
        public Teacher Teacher { get; set; }

        /// <summary>
        /// If the class is going for several groups
        /// </summary>
        public ObservableCollection<Group> Groups { get; set; }
        public bool Online { get; set; }
        public string OnlineUrl
        {
            get
            {
                if (Online)
                    return _onlineUrl;
                else
                    return string.Empty;
            }
            set
            {
                if (!Online && !string.IsNullOrEmpty(value))
                {
                    Online = true;
                }
                _onlineUrl = value;
            }
        }
        public Shedule()
        {
            IncreaseId();
        }
        static void IncreaseId()
        {
            SheduleId++;
        }
    }
}
