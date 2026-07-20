using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Asil_C971
{
    public partial class terms : ContentPage
    {
        private readonly dbService _db;
        public ObservableCollection<term> termList { get; set; } = new ObservableCollection<term>();
        private readonly List<term> selectedTerm = new List<term>();
        private RadioButton? _radio;

        public terms()
        {
            InitializeComponent();
            _db = new dbService();
            termsView.ItemsSource = termList;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            try
            {
                await termsLoad();
            }
            catch (Exception ex)
            {
                await DisplayAlert("error", $"{ex.Message}", "OK");
            }
        }

        private async Task termsLoad()
        {
            termList.Clear();
            selectedTerm.Clear();
            var list = await _db.getTerms();
            if (list.Count == 0)
            {
                var Term5 = new term
                {
                    Title = "Term 5",
                    Start = DateTime.Now.AddMonths(-5),
                    End = DateTime.Now.AddMonths(1)
                };
                await _db.saveTerm(Term5);

                var Capstone = new course
                {
                    termId = Term5.Id,
                    Title = "BSSE Capstone",
                    Start = DateTime.Now,
                    End = DateTime.Now.AddMonths(1),
                    Status = "In Progress",
                    ProfName = "Anika Patel",
                    profPhone = "555-123-4567",
                    profEmail = "anika.patel@strimeuniversity.edu",
                };
                await _db.saveCourse(Capstone);

                var pa = new test
                {
                    classId = Capstone.Id,
                    Title = "PA: Application Deployment",
                    Type = false,
                    Start = DateTime.Now,
                    End = DateTime.Now.AddDays(14)
                };
                await _db.saveTests(pa);

                var oa = new test
                {
                    classId = Capstone.Id,
                    Title = "OA: Proposal and Waivers",
                    Type = true,
                    Start = DateTime.Now.AddDays(15),
                    End = DateTime.Now.AddDays(30)
                };
                await _db.saveTests(oa);

                list = await _db.getTerms();
            }

            foreach (var t in list)
            {
                termList.Add(t);
            }
        }

        private async void AddClick(object sender, EventArgs e)
        {
            selectedTerm.Clear();
            int n = termList.Count + 1;
            var newTerm = new term
            {
                Title = $"Term {n}",
                Start = DateTime.Now.AddMonths(6* (n - 1)),
                End = DateTime.Now.AddMonths(6*(n - 1)+6)
            };

            await _db.saveTerm(newTerm);
            termList.Add(newTerm);
            if (_radio != null)
            {
                _radio.IsChecked = false;
                _radio = null;
            }
        }

        private void Checked(object sender, CheckedChangedEventArgs e)
        {
            var radioButton = (RadioButton)sender;

            if (radioButton.BindingContext is not term t) return;

            if (e.Value)
            {
                if (_radio != null && _radio != radioButton)
                {
                    _radio.IsChecked = false;
                }

                _radio = radioButton;
                selectedTerm.Clear();
                selectedTerm.Add(t);
            }
        }

        private async void DelClick(object sender, EventArgs e)
        {
            foreach (var t in new List<term>(selectedTerm))
            {
                await _db.delTerm(t);
                termList.Remove(t);
            }
            selectedTerm.Clear();
            if (_radio != null)
            {
                _radio.IsChecked = false;
                _radio = null;
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
                    await termsLoad();
                    return;
                }
                await _db.saveTerm(t);
            }
            if (_radio != null)
            {
                _radio.IsChecked = false;
                _radio = null;
            }
        }

        private async void coursesClick(object sender, EventArgs e)
        {
            var button = (Button)sender;
            if (button.CommandParameter is term t)
            {
                await Navigation.PushAsync(new courses(t));
            }
        }
        }
    }