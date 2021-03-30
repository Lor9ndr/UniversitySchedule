using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace UniversitySchedule.Core
{
    public sealed class SheduleManager : INotifyPropertyChanged
    {
        #region Public Properties
        public Student CurrentStudent { get; set; }
        public Teacher CurrentTeacher { get; set; }
        public ObservableCollection<Group> CurrentGroups { get; set; }
        public User CurrentUser { get; set; } = new User();
        private ObservableCollection<Shedule> _currentShedules;
        public ObservableCollection<Shedule> CurrentShedules
        {
            get => _currentShedules;
            set
            {
                _currentShedules = value;
                OnPropertyChanged("CurrentShedules");
            }
        }
        public ObservableCollection<Shedule> Shedules { get; set; }
        public ObservableCollection<Group> Groups { get; set; }
        public ObservableCollection<Teacher> Teachers { get; set; }
        public ObservableCollection<Student> Students { get; set; }

        public List<ClassType> ClassTypes { get; } = new List<ClassType>()
        {
            ClassType.Exam,
            ClassType.Lecture,
            ClassType.Seminar,
            ClassType.Test
        };
        public bool IsSearchingForCurrentShedules
        {
            get => _isSearchingForCurrentShedules;
            set
            {
                if (value != _isSearchingForCurrentShedules)
                {
                    _isSearchingForCurrentShedules = value;
                    OnPropertyChanged("IsSearchingForCurrentShedules");
                    if (value == true)
                        GetCurrentShedules();
                    else
                        if (Shedules != null && Shedules.Count > 0)
                        CurrentShedules = Shedules;
                    OnPropertyChanged("CurrentShedules");
                }
            }
        }
        public bool IsSheduleIsEmpty
        {
            get => _isSheduleIsEmpty;
            set
            {
                _isSheduleIsEmpty = value;
                OnPropertyChanged("IsSheduleIsEmpty");
            }
        }
        public ClassType LastAddedClassType
        {
            get => _lastAddedClassType; set
            {
                _lastAddedClassType = value;
                OnPropertyChanged("LastAddedClassType");
            }
        }
        public bool IsUserStudent
        {
            get => _isStudent; set
            {
                _isStudent = value;
                OnPropertyChanged("IsUserStudent");
            }
        }
        #endregion

        #region Private Properites
        private bool _isSearchingForCurrentShedules = true;
        private bool _isSheduleIsEmpty;
        private bool _isStudent;
        private ClassType _lastAddedClassType;


        private readonly string fileName = "../../../Data/Data.json";

        public event PropertyChangedEventHandler PropertyChanged;

        private class AllData
        {
            public ObservableCollection<Shedule> Shedules { get; set; }
            public ObservableCollection<Group> Groups { get; set; }
            public ObservableCollection<Teacher> Teachers { get; set; }
            public ObservableCollection<Student> Students { get; set; }
        }
        #endregion

        #region Constructor
        public SheduleManager()
        {
            LoadData();
        }
        #endregion

        #region Public Methods
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public void LoadData()
        {
            AllData data = Deserialize<AllData>(fileName);
            if (data != null)
            {
                Shedules = data.Shedules;
                Groups = data.Groups;
                Teachers = data.Teachers;
                Students = data.Students;
                Shedules.OrderBy(s => s.TimeOfClass);
                ClearPastShedules();
            }
            else
            {
                Shedules = new ObservableCollection<Shedule>();
                Groups = new ObservableCollection<Group>();
                Teachers = new ObservableCollection<Teacher>();
                Students = new ObservableCollection<Student>();
            }
        }
        public void SaveData()
        {
            AllData data = new AllData()
            {
                Shedules = this.Shedules,
                Groups = this.Groups,
                Teachers = this.Teachers,
                Students = this.Students,

            };
            Serialize(fileName, data);
        }
        /// <summary>
        /// Authorization of user
        /// </summary>
        /// <param name="us">User by input</param>
        /// <returns></returns>
        public bool Authorize(User us)
        {

            bool found;
            if (!Teachers.Any(s => s.User.Email == us.Email && s.User.Password == us.Password))
                found = Students.Any(s => s.User.Email == us.Email && s.User.Password == us.Password);
            else
                found = true;
            if (found)
            {
                IsStudent(us);
                SetCurrentUser(us);
            }
            return found;
        }
        public void RegistrateTeacher(Teacher t)
        {
            Teachers.Add(t);
            CurrentTeacher = t;
            IsStudent(t.User);
            SaveData();
        }

        public void AddStudent(Group group, Student student)
        {
            Groups.Where(s => s == group).ToList()[0].Students.Add(student);
            Students.Add(student);
        }
        /// <summary>
        /// if any of strings is empty or null then it returns false
        /// </summary>
        /// <param name="s">array of strings</param>
        /// <returns></returns>
        public bool CheckStringsNonEmpty(string[] s)
        {
            foreach (var item in s)
            {
                if (string.IsNullOrEmpty(item) && string.IsNullOrWhiteSpace(item))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// this function convert to <see cref="DateTime"/> from string time 
        /// and <see cref="DateTime"/> date to one object
        /// </summary>
        /// <param name="date">input date</param>
        /// <param name="time">string time which converts to one object</param>
        /// <returns></returns>
        public DateTime? GetDateTimeByDateAndStringTime(DateTime date, string time)
        {
            try
            {
                DateTime stringTime = Convert.ToDateTime(time);
                return new DateTime(date.Year, date.Month, date.Day, stringTime.Hour, stringTime.Minute, 0);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Function adds to current shedule and overall shedule if there is no shedules in that time and with these groups
        /// </summary>
        /// <param name="sh"> which shedule to add</param>
        public void AddShedule(Shedule sh)
        {
            bool currentIsTeacher = false;
            if (sh.Teacher == CurrentTeacher && CurrentShedules.All(s => s.TimeOfClass != sh.TimeOfClass))
                currentIsTeacher = true;
            bool isAbleToAdd = true;
            foreach (var shedule in Shedules)
            {
                foreach (var group in sh.Groups)
                {
                    isAbleToAdd = shedule.Groups.All(s => s.NameOfGroup != group.NameOfGroup) || sh.TimeOfClass != shedule.TimeOfClass;
                    if (isAbleToAdd == false)
                        break;
                }
                if (isAbleToAdd == false)
                    break;
            }
            if (isAbleToAdd)
            {
                if (currentIsTeacher)
                {
                    CurrentShedules.Add(sh);
                    Shedules.Add(sh);
                }
                else
                    Shedules.Add(sh);
            }
            if (CurrentStudent != null && CurrentShedules.Count > 0)
                IsSheduleIsEmpty = true;
            SaveData();
        }
        public void AddGroup(string nameOfGroup)
        {
            if (CheckStringsNonEmpty(new string[] { nameOfGroup }) && !Groups.Any(s => s.NameOfGroup == nameOfGroup))
            {
                Groups.Add(new Group
                {
                    NameOfGroup = nameOfGroup,
                    Students = new List<Student>()
                });
                SaveData();
            }
        }
        public void AddTeacher(Teacher t)
        {
            Teachers.Add(t);
            IsStudent(t.User);
            SetCurrentUser(t.User);
            SaveData();
        }
        /// <summary>
        /// Search for shedules by typing shedule name or teacher name
        /// </summary>
        /// <param name="inputString">search by that string</param>
        public void SearchShedulesByTeachersAndClassNames(string inputString)
        {
            List<Shedule> shedules;
            LoadShedules();
            if (!CheckStringsNonEmpty(new string[] { inputString }))
            {
                if (IsSearchingForCurrentShedules)
                    GetCurrentShedules();
                else
                {
                    CurrentShedules.Clear();
                    foreach (var item in Shedules)
                    {
                        CurrentShedules.Add(item);
                    }
                }
            }
            else
            {
                if (IsSearchingForCurrentShedules)
                    shedules = CurrentShedules.Where(s => s.Teacher.Name.StartsWith(inputString) || s.NameOfClass.StartsWith(inputString)).ToList();
                else
                    shedules = Shedules.Where(s => s.Teacher.Name.StartsWith(inputString) || s.NameOfClass.StartsWith(inputString)).ToList();
                if (shedules.Count > 0)
                    CurrentShedules.Clear();
                foreach (var item in shedules)
                {
                    CurrentShedules.Add(item);
                }
            }
            CheckShedulesIsEmpty();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="shedules"></param>
        public void DeleteShedules(List<Shedule> shedules)
        {
            foreach (var item in shedules)
            {
                Shedules.Remove(item);
            }
            GetCurrentShedules();
            SaveData();
            CheckShedulesIsEmpty();
        }

        #endregion

        #region Private Methods
        private T Deserialize<T>(string fileName)
        {
            using var sr = new StreamReader(fileName);
            using var jsonReader = new JsonTextReader(sr);
            var serializer = new JsonSerializer();
            return serializer.Deserialize<T>(jsonReader);
        }

        private void Serialize<T>(string fileName, T data)
        {
            using var sw = new StreamWriter(fileName);
            using var jsonWriter = new JsonTextWriter(sw);
            var serializer = new JsonSerializer();
            serializer.Serialize(jsonWriter, data);
        }
        private void LoadShedules()
        {
            AllData data = Deserialize<AllData>(fileName);
            if (data != null)
            {
                Shedules = data.Shedules;
            }
            Shedules.OrderBy(s => s.TimeOfClass);
        }
        private void IsStudent(User us)
        {
            IsUserStudent = Students.Any(s => s.User?.Email == us.Email && s.User.Password == us.Password);
        }
        private void SetCurrentUser(User us)
        {
            if (IsUserStudent)
            {
                CurrentStudent = Students.First(s => s.User.Email == us.Email && s.User.Password == us.Password);
                CurrentUser = CurrentStudent.User;
            }
            else
            {
                CurrentTeacher = Teachers.First(s => s.User.Email == us.Email && s.User.Password == us.Password);
                CurrentUser = CurrentTeacher.User;
            }
            GetCurrentShedules();
        }
        private void GetCurrentShedules()
        {
            CurrentShedules = new ObservableCollection<Shedule>();
            if (IsUserStudent)
            {
                SetCurrentGroup();
                foreach (var group in CurrentGroups)
                {
                    foreach (var item in Shedules)
                    {
                        if (item.Groups.Any(s => s.NameOfGroup == group.NameOfGroup))
                        {
                            CurrentShedules.Add(item);
                        }
                    }
                }
            }
            else
            {
                if (IsSearchingForCurrentShedules)
                {
                    foreach (var item in Shedules)
                    {
                        if (item.Teacher.User.Email == CurrentTeacher.User.Email && item.Teacher.User.Password == CurrentTeacher.User.Password)
                        {
                            CurrentShedules.Add(item);
                        }
                    }
                }
                else
                    CurrentShedules = Shedules;
            }
            CheckShedulesIsEmpty();
            CurrentShedules.OrderBy(s => s.TimeOfClass);
        }
        private void CheckShedulesIsEmpty()
        {
            if (CurrentShedules.Count == 0)
                IsSheduleIsEmpty = true;
            else
                IsSheduleIsEmpty = false;
        }
        private void SetCurrentGroup()
        {
            CurrentGroups = new ObservableCollection<Group>();
            foreach (var item in Groups.Where(s => s.Students.Any(x => x.User.Password == CurrentUser.Password && x.User.Email == CurrentUser.Email)))
            {
                CurrentGroups.Add(item);
            }
        }
        /// <summary>
        /// If <see cref="DateTime.Now"/> more than some of shedules, they will be deleted
        /// </summary>
        private void ClearPastShedules()
        {
            List<Shedule> pastShedules =  Shedules.Where(s => s.TimeOfClass < DateTime.Now).ToList();
            foreach (var item in pastShedules)
            {
                Shedules.Remove(item);
            }
        }
        #endregion
    }
}
