using System.Collections.ObjectModel;

namespace Asil_C971
{
    public class Result
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public object Source { get; set; }
    }

    public partial class terms : ContentPage
    {
        private readonly dbService _db;
        public ObservableCollection<term> termList { get; set; } = new ObservableCollection<term>();
        public ObservableCollection<Result> searchList { get; set; } = new ObservableCollection<Result>();

        private readonly List<term> selectedTerm = new List<term>();
        private RadioButton? _radio;

        public terms()
        {
            InitializeComponent();
            _db = new dbService();
            termsView.ItemsSource = termList;
            searchResultsView.ItemsSource = searchList;
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
                    Title = "Example Term",
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
                Start = DateTime.Now.AddMonths(6 * (n - 1)),
                End = DateTime.Now.AddMonths(6 * (n - 1) + 6)
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
        private bool filters = false;
        private void filterToggle(object sender, EventArgs e)
        {
            filters = !filters;
            filterMenu.IsVisible = filters;
            reportStamp.IsVisible = filters;
            reportStamp.Text = $"Report generated: {DateTime.Now}";
            refresh(appSearchBar.Text);
        }

        private void filtered(object sender, EventArgs e)
        {
            if (appSearchBar == null) return;
            refresh(appSearchBar.Text);
        }

        private void onSrch(object sender, TextChangedEventArgs e)
        {
            refresh(e.NewTextValue);
        }

        private async void refresh(string query)
        {
            if (termsContainer == null || searchContainer == null || returnBtn == null) return; query = query?.ToLower() ?? "";

            if (string.IsNullOrWhiteSpace(query) && !filters)
            {
                termsContainer.IsVisible = true;
                searchContainer.IsVisible = false;
                returnBtn.IsVisible = false;
                filterMenu.IsVisible = false;
                searchList.Clear();
                return;
            }

            termsContainer.IsVisible = false;
            searchContainer.IsVisible = true;
            returnBtn.IsVisible = true;
            searchList.Clear();

            bool showTerms = chkTerms?.IsChecked ?? true;
            bool showCourses = chkCourses?.IsChecked ?? true;
            bool showAssessments = chkAssessments?.IsChecked ?? true;
            string selectedStatus = pckStatus?.SelectedItem?.ToString() ?? "All";

            if (showTerms && selectedStatus == "All")
            {
                var allTerms = await _db.getTerms();
                var filteredTerms = allTerms.Where(t => t.Title != null && t.Title.ToLower().Contains(query)).ToList();
                foreach (var t in filteredTerms)
                {
                    searchList.Add(new Result { Title = t.Title, Subtitle = "Term", Source = t });
                }
            }

            if (showCourses)
            {
                var Courses = await _db.getCourses();
                var filtered = Courses.Where(c => c.Title != null && c.Title.ToLower().Contains(query)).ToList();

                if (selectedStatus != "All")
                {
                    filtered = filtered.Where(c => c.Status == selectedStatus).ToList();
                }

                foreach (var c in filtered)
                {
                    searchList.Add(new Result { Title = c.Title, Subtitle = $"Course ({c.Status})", Source = c });
                }
            }

            if (showAssessments && selectedStatus == "All")
            {
                var allTests = await _db.getTests();
                var filteredTests = allTests.Where(t => t.Title != null && t.Title.ToLower().Contains(query)).ToList();
                foreach (var t in filteredTests)
                {
                    string typeString = t.Type ? "OA" : "PA";
                    searchList.Add(new Result { Title = t.Title, Subtitle = $"Assessment ({typeString})", Source = t });
                }
            }
        }

        private async void srchClick(object sender, EventArgs e)
        {
            var button = (Button)sender;

            if (button.CommandParameter is term t)
            {
                await Navigation.PushAsync(new courses(t));
            }
            else if (button.CommandParameter is course c)
            {
                await Navigation.PushAsync(new Class(c));
            }
            else if (button.CommandParameter is test testObj)
            {
                var parentCourseList = await _db.getCourses();
                var parentCourse = parentCourseList.FirstOrDefault(c => c.Id == testObj.classId);

                if (parentCourse != null)
                {
                    await Navigation.PushAsync(new Class(parentCourse));
                }
            }
        }

        private void rtrnClick(object sender, EventArgs e)
        {
            appSearchBar.Text = string.Empty;
            termsContainer.IsVisible = true;
            searchContainer.IsVisible = false;
            returnBtn.IsVisible = false;
            filters = false;
            filterMenu.IsVisible = false;
            searchList.Clear();
        }
    }
}