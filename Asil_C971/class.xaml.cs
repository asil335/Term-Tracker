using Microsoft.Maui.Controls;
using Plugin.LocalNotification;
using System;
using System.Collections.ObjectModel;

namespace Asil_C971
{
    public partial class Class : ContentPage
    {
        private readonly dbService _db;
        private course _thisCourse;
        public ObservableCollection<test> testList { get; set; } = new ObservableCollection<test>();
        private readonly List<test> thisTest = new List<test>();
        private RadioButton _radio;
        public Class(course c)
        {
            InitializeComponent();
            _db = new dbService();
            _thisCourse = c;
            this.BindingContext = _thisCourse;
            testsView.ItemsSource = testList;
        }

        private async void save(object sender, EventArgs e)
        {
            if (_thisCourse.Start > _thisCourse.End)
            {
                await DisplayAlert("Invalid Entry", "Start date cannot be after the end date.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(_thisCourse.ProfName))
            {
                await DisplayAlert("Error", "Empty instructor name.", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(_thisCourse.profEmail))
            {
                await DisplayAlert("Error", "Empty email", "OK");
                return;
            }
            if (string.IsNullOrWhiteSpace(_thisCourse.profPhone))
            {
                await DisplayAlert("Error", "Empty phone number", "OK");
                return;
            }
            if (!_thisCourse.profEmail.Contains("@"))
            {
                await DisplayAlert("Error", "Invalid email.", "OK");
                return;
            }

            await _db.saveCourse(_thisCourse);
        }
        private async void shareClick(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_thisCourse.Notes))
            {
                return;
            }
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = "Share",
                Text = _thisCourse.Notes,
                Subject = $"{_thisCourse.Title} notes"
            });
        }
        private async void notifyMe(object sender, EventArgs e)
        {
            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
                var granted = await LocalNotificationCenter.Current.RequestNotificationPermission();
                if (!granted)
                {
                    await DisplayAlert("Permission Denied", "Notification permission required", "OK");
                    return;
                }
            }
            string action = await DisplayActionSheet("Select Notifications", "Cancel", null,
                "Class Start and End Dates",
                "Assessment Due Dates",
                "Both");

            if (action == "Cancel" || string.IsNullOrEmpty(action)) return;

            bool priornotif = false;

            if (action == "Class Start and End Dates" || action == "Both")
            {
                LocalNotificationCenter.Current.Cancel(_thisCourse.Id);
                LocalNotificationCenter.Current.Cancel(_thisCourse.Id + 100);

                var notif = new NotificationRequest
                {
                    NotificationId = _thisCourse.Id,
                    Title = "Course starting soon",
                    Description = $"{_thisCourse.Title} starts this week",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = _thisCourse.Start.AddDays(-7)
                        //NotifyTime = DateTime.Now.AddSeconds(5) // debug
                    }
                };

                var endNotif = new NotificationRequest
                {
                    NotificationId = _thisCourse.Id + 100,
                    Title = "Course ending soon",
                    Description = $"{_thisCourse.Title} ends this week",
                    Schedule = new NotificationRequestSchedule
                    {
                        NotifyTime = _thisCourse.End.AddDays(-7)
                        //NotifyTime = DateTime.Now.AddSeconds(7) // debug
                    }
                };

                await LocalNotificationCenter.Current.Show(notif);
                await LocalNotificationCenter.Current.Show(endNotif);
                priornotif = true;
            }

            if (action == "Assessment Due Dates" || action == "Both")
            {
                foreach (var t in testList)
                {
                    LocalNotificationCenter.Current.Cancel(t.Id + 200);
                    LocalNotificationCenter.Current.Cancel(t.Id + 300);

                    var testEndNotification = new NotificationRequest
                    {
                        NotificationId = t.Id + 200,
                        Title = "Assessment Due",
                        Description = $"{t.Title} is due this week",
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = t.End.AddDays(-7)
                            //NotifyTime = DateTime.Now.AddSeconds(10) // debug
                        }
                    };

                    var testStartNotification = new NotificationRequest
                    {
                        NotificationId = t.Id + 300,
                        Title = "Assessment starting soon", // almost forgot
                        Description = $"{t.Title} starts this week",
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = t.Start.AddDays(-7)
                            //NotifyTime = DateTime.Now.AddSeconds(12) // debug
                        }
                    };

                    await LocalNotificationCenter.Current.Show(testEndNotification);
                    await LocalNotificationCenter.Current.Show(testStartNotification);
                }
                priornotif = true;
            }

            if (priornotif)
            {
                await DisplayAlert("Reminder set", "You will be notified a week before", "OK");
            }
        }
        private async void add(object sender, EventArgs e)
        {
            if (testList.Count >= 2) return;
            bool newtype = false;
            if (testList.Count == 1)
            {
                newtype = !testList[0].Type;
            }

            var newTest = new test
            {
                Type = newtype,
                Title = newtype ? "Objective Assessment" : "Performance Assessment",
                Start = _thisCourse.Start.AddSeconds(1),
                End = _thisCourse.End.AddSeconds(-1),
                classId = _thisCourse.Id
            };

            await _db.saveTests(newTest);
            testList.Add(newTest);
        }
        private void Checked(object sender, CheckedChangedEventArgs e)
        {
            var radiobtn = (RadioButton)sender;
            if (radiobtn.BindingContext is not test t) return;

            if (e.Value)
            {
                if (_radio != null && _radio != radiobtn)
                {
                    _radio.IsChecked = false;
                }

                _radio = radiobtn;
                thisTest.Clear();
                thisTest.Add(t);
            }
        }

        private async void delbtn(object sender, EventArgs e)
        {
            foreach (var t in new List<test>(thisTest))
            {
                await _db.delTests(t);
                testList.Remove(t);
            }
            thisTest.Clear();
        }

        private async void testEdited(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            if (element.BindingContext is test t)
            {
                if ((_thisCourse.End < t.End) || (_thisCourse.Start > t.End) || (_thisCourse.Start > t.Start) || (_thisCourse.End < t.Start))
                {
                    await DisplayAlert("Invalid Entry", "Assessment due dates must be within class date range", "OK");
                    await testsLoad();
                    return;
                }
                await _db.saveTests(t);
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await testsLoad();
        }

        private async Task testsLoad()
        {
            testList.Clear();
            thisTest.Clear();
            var list = await _db.getTests(_thisCourse.Id);
            foreach (var t in list)
            {
                testList.Add(t);
            }
        }
    }
}