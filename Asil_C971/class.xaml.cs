using Microsoft.Maui.Controls;
using System;

namespace Asil_C971
{
    public partial class Class : ContentPage
    {
        private readonly dbService _db;
        private course _thisCourse;

        public Class(course c)
        {
            InitializeComponent();
            _db = new dbService();
            _thisCourse = c;
            this.BindingContext = _thisCourse;
        }

        private async void save(object sender, EventArgs e)
        {
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
    }
}