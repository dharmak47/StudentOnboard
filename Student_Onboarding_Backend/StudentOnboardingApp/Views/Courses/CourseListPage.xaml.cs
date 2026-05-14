using StudentOnboardingApp.ViewModels;

namespace StudentOnboardingApp.Views.Courses;

public partial class CourseListPage : ContentPage
{
    private readonly CourseListViewModel _viewModel;

    public CourseListPage(CourseListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Hide entire collection
        CoursesCollection.Opacity = 0;
        CoursesCollection.TranslationY = 10;

        // Load data
        await _viewModel.LoadCoursesCommand.ExecuteAsync(null);

        // Fade in everything at once
        await Task.WhenAll(
            CoursesCollection.FadeTo(1, 300, Easing.CubicOut),
            CoursesCollection.TranslateTo(0, 0, 300, Easing.CubicOut)
        );
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        CoursesCollection.Opacity = 0;
    }
}
