using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using UniversitySchedule.Core;

namespace UniversitySchedule
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly SheduleManager SheduleManager = new SheduleManager();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = SheduleManager;
        }

        #region Some ClickMethods and UI events
        #region LoginMethods
        private void PasswordBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!SheduleManager.CheckStringsNonEmpty(new string[] { PasswordB.Password }))
            {
                tbPassword.Text = "";
            }

        }
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!SheduleManager.CheckStringsNonEmpty(new string[] { PasswordB.Password }))
            {
                tbPassword.Text = "Пароль";
            }
        }
        private void EnterButton_Click(object sender, RoutedEventArgs e)
        {
            User us = new User()
            {
                Email = TbLogin.Text,
                Password = PasswordB.Password
            };
            if (SheduleManager.Authorize(us))
                MainPageOpen();
            else
                WrongPasswordtb.Visibility = Visibility.Visible;
        }
        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            SetRegistrationPageOn();
        }
        private void TeacherButton_click(object sender, RoutedEventArgs e)
        {
            ChoseTypeSP.Visibility = Visibility.Collapsed;
            RegistrationPageGrid.Visibility = Visibility.Visible;
            TeacherRegGrid.Visibility = Visibility.Visible;

        }
        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            ChoseTypeSP.Visibility = Visibility.Collapsed;
            RegistrationPageGrid.Visibility = Visibility.Visible;
            StudentRegGrid.Visibility = Visibility.Visible;
        }

        private void StudentRegOk_Click(object sender, RoutedEventArgs e)
        {
            string[] inputs = new string[3]
            {
                RegStudentNameTb.Text,
                RegStudentLoginTb.Text,
                RegStudentPasswordTb.Text
            };
            const int nameIndex = 0;
            const int loginIndex = 1;
            const int passwordIndex = 2;
            if (SheduleManager.CheckStringsNonEmpty(inputs) && GroupSelectionBox.SelectedItem != null)
            {
                Student s = new Student()
                {
                    Name = inputs[nameIndex].Trim(),
                    User = new User()
                    {
                        Email = inputs[loginIndex],
                        Password = inputs[passwordIndex]
                    }
                };
                SheduleManager.AddStudent(GroupSelectionBox.SelectedItem as Group, s);
                SheduleManager.Authorize(s.User);
                MainPageOpen();
            }

        }
        private void TeacherRegOk_Click(object sender, RoutedEventArgs e)
        {
            string[] inputs = new string[4]
            {
                TeacherRegNameTb.Text,
                TeacherRegFacultyTb.Text,
                TeacherRegLoginTb.Text,
                TeacherRegPasswordTb.Text
            };
            const int nameIndex = 0;
            const int facultyIndex = 1;
            const int loginIndex = 2;
            const int passwordIndex = 3;
            if (SheduleManager.CheckStringsNonEmpty(inputs))
            {
                Teacher t = new Teacher()
                {
                    Name = inputs[nameIndex].Trim(),
                    Faculty = inputs[facultyIndex].Trim(),
                    User = new User()
                    {
                        Email = inputs[loginIndex],
                        Password = inputs[passwordIndex]
                    }
                };
                SheduleManager.RegistrateTeacher(t);
                MainPageOpen();
            }
        }
        private void MainPageOpen()
        {
            MainGrid.Visibility = Visibility.Visible;
            LoginGrid.Visibility = Visibility.Collapsed;
            RegistrationPageGrid.Visibility = Visibility.Collapsed;
            SheduleManager.SaveData();
            //ListBoxOfMainShedules.ItemsSource = SheduleManager.CurrentShedules;
            this.MaxHeight = 800;
            this.MaxWidth = 1400;
            this.Height = MaxHeight;
            this.Width = MaxWidth;

        }
        private void SetRegistrationPageOn()
        {
            LoginGrid.Visibility = Visibility.Collapsed;
            MainGrid.Visibility = Visibility.Collapsed;
            RegistrationPageGrid.Visibility = Visibility.Visible;
        }

        #endregion

        private void AddSheduleGrid_Click(object sender, RoutedEventArgs e)
        {
            AddSheduleGrid.Visibility = Visibility.Visible;

            SearchSheduleGrid.Visibility = Visibility.Collapsed;
            DeleteSheduleGrid.Visibility = Visibility.Collapsed;
        }


        private void DeleteSheduleGrid_Click(object sender, RoutedEventArgs e)
        {
            DeleteSheduleGrid.Visibility = Visibility.Visible;

            SearchSheduleGrid.Visibility = Visibility.Collapsed;
            AddSheduleGrid.Visibility = Visibility.Collapsed;
        }

        private void SeacrhSheduleGrid_Click(object sender, RoutedEventArgs e)
        {
            SearchSheduleGrid.Visibility = Visibility.Visible;

            DeleteSheduleGrid.Visibility = Visibility.Collapsed;
            AddSheduleGrid.Visibility = Visibility.Collapsed;
        }
        private void AddSheduleButton_Click(object sender, RoutedEventArgs e)
        {
            string[] inputs;
            if ((bool)IsOnline.IsChecked)
            {
                inputs = new string[3]
                 {
                    NameOfClassTb.Text,
                    UrlOnlineClassTb.Text,
                    TimeOfClass.Text,
                 };

            }
            else
            {
                inputs = new string[2]
                {                  
                    NameOfClassTb.Text,
                    TimeOfClass.Text,
                };
            }
            if (SheduleManager.CheckStringsNonEmpty(inputs)
                && TeachersChooseBox.SelectedItem != null
                && DateOfClass.SelectedDate.HasValue
                && DateOfClass.SelectedDate.Value > DateTime.Now
                && ListViewOfGroups.SelectedItems.Count != 0)
            {
                DateTime? sheduleDatetime = SheduleManager.GetDateTimeByDateAndStringTime(DateOfClass.SelectedDate.Value, TimeOfClass.Text);
                if (sheduleDatetime != null)
                {
                    Shedule sh = new Shedule()
                    {
                        TimeOfClass = (DateTime)sheduleDatetime,
                        OnlineUrl = UrlOnlineClassTb.Text,
                        NameOfClass = NameOfClassTb.Text,
                        Groups = new ObservableCollection<Group>(),
                        Teacher = (Teacher)TeachersChooseBox.SelectedItem,
                        ClassType = SheduleManager.LastAddedClassType
                    };
                    foreach (Group item in ListViewOfGroups.SelectedItems)
                    {
                        sh.Groups.Add(item);
                    }
                    var checkIsAdd = SheduleManager.Shedules.Count;
                    SheduleManager.AddShedule(sh);
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        inputs[i] = string.Empty;
                    }
                    if (checkIsAdd < SheduleManager.Shedules.Count)
                        MessageBox.Show("Занятие успешно добавлено");
                }
                else
                {
                    MessageBox.Show("Время введено неправильно");
                }

            }
        }

        private void AddNewGroup_Click(object sender, RoutedEventArgs e)
        {
            SheduleManager.AddGroup(AddGroupNameTb.Text);
        }

        private void SearchForShedules_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            SheduleManager.SearchShedulesByTeachersAndClassNames((sender as TextBox).Text);
        }
        private void DeleteShedules_Click(object sender, RoutedEventArgs e)
        {
            var shedules = new List<Shedule>();
            foreach (var item in DeleteShedulesListBox.SelectedItems)
            {
                shedules.Add((Shedule)item);
            }
            SheduleManager.DeleteShedules(shedules);
        }
        #endregion
    }
}
