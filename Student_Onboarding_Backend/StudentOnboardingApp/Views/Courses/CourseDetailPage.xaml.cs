using StudentOnboardingApp.ViewModels;

namespace StudentOnboardingApp.Views.Courses;

public partial class CourseDetailPage : ContentPage
{
    private bool _navigatedAway;

    public CourseDetailPage(CourseDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        _navigatedAway = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // If the page is disappearing but not because of a back navigation
        // (e.g., tab switch), remove it from the stack so the courses list
        // shows next time the user visits the Courses tab
        if (!_navigatedAway)
        {
            var nav = Shell.Current.Navigation;
            if (nav.NavigationStack.Contains(this))
            {
                try { nav.RemovePage(this); }
                catch { /* ignore */ }
            }
        }
    }
}
