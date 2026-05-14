using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Course;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels;

public partial class CourseListViewModel : BaseViewModel
{
    private readonly ICourseService _courseService;

    public CourseListViewModel(ICourseService courseService)
    {
        _courseService = courseService;
        Title = "Courses";
    }

    [ObservableProperty]
    private bool _isRefreshing;

    public ObservableCollection<CourseDto> Courses { get; } = [];

    [RelayCommand]
    private async Task LoadCoursesAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _courseService.GetCoursesAsync();
            if (result.Success && result.Data != null)
            {
                Courses.Clear();
                foreach (var course in result.Data)
                    Courses.Add(course);
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task GoToCourseDetailAsync(CourseDto course)
    {
        await Shell.Current.GoToAsync(
            $"{Constants.Routes.CourseDetail}?courseId={course.Id}");
    }
}
