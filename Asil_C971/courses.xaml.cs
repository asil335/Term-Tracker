using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Asil_C971
{
    public partial class courses : ContentPage
    {
        private readonly dbService _db;
        public ObservableCollection<course> courseList { get; set; } = new ObservableCollection<course>();
        private readonly List<course> selectedCourse = new List<course>();
        private RadioButton _radio;

        private term _thisTerm;

        public courses(term t)
        {
            InitializeComponent();
            _db = new dbService();
            _thisTerm = t;
            this.BindingContext = _thisTerm;
            coursesView.ItemsSource = courseList;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await coursesLoad();
        }

        private async Task coursesLoad()
        {
            courseList.Clear();
            selectedCourse.Clear();

            var t = await _db.getTermCourses(_thisTerm.Id);
            foreach (var c in t)
            {
                courseList.Add(c);
            }
        }

        private async void AddClick(object sender, EventArgs e)
        {
            selectedCourse.Clear();
            int n = courseList.Count + 1;
            if (n == 7)
            {
                await DisplayAlert("Failed", "Terms can only contain 6 courses.", "Ok");
                return;
            }
            var newCourse = new course
            {
                Title = $"Course {n}",
                Start = DateTime.Now.AddMonths(n - 1),
                End = DateTime.Now.AddMonths(n),
                termId = _thisTerm.Id
            };

            await _db.saveCourse(newCourse);
            courseList.Add(newCourse);
        }

        private void Checked(object sender, CheckedChangedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            if (radioButton.BindingContext is not course c) return;

            if (e.Value)
            {
                if (_radio != null && _radio != radioButton)
                {
                    _radio.IsChecked = false;
                }

                _radio = radioButton;
                selectedCourse.Clear();
                selectedCourse.Add(c);
            }
        }

        private async void DelClick(object sender, EventArgs e)
        {
            foreach (var c in new List<course>(selectedCourse))
            {
                await _db.delCourse(c);
                courseList.Remove(c);
            }
            selectedCourse.Clear();
        }

        private async void courseEdited(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            if (element.BindingContext is course c)
            {
                if (c.Start > c.End)
                {
                    await DisplayAlert("Invalid Entry", "Start date cannot be after the end date.", "OK");
                    await coursesLoad();
                    return;
                }
                await _db.saveCourse(c);
            }
        }
        private async void termEdited(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            if (element.BindingContext is term t)
            {
                if (t.Start > t.End)
                {
                    await DisplayAlert("Invalid Entry", "Start date cannot be after the end date.", "OK");
                    return;
                }
                await _db.saveTerm(t);
            }
        }
        private async void classClick(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (button.CommandParameter is course t)
            {
                await Navigation.PushAsync(new Class(t));
            }
        }
    }
}