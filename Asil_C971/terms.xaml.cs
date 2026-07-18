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
        private RadioButton _radio;

        public terms()
        {
            InitializeComponent();
            _db = new dbService();
            termsView.ItemsSource = termList;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await termsLoad();
        }

        private async Task termsLoad()
        {
            termList.Clear();
            selectedTerm.Clear();
            var list = await _db.getTerms();
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
        }

        private void Checked(object sender, CheckedChangedEventArgs e)
        {
            var radioButton = (RadioButton)sender;
            var t = (term)radioButton.BindingContext;

            if (e.Value)
            {
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
        }

        private async void termEdited(object sender, EventArgs e)
        {
            var element = (BindableObject)sender;
            if (element.BindingContext is term t)
            {
                await _db.saveTerm(t);
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