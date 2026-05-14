using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Dashboard;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
    private readonly IDashboardService _dashboardService;
    private readonly ITokenStorageService _tokenStorage;

    public DashboardViewModel(IDashboardService dashboardService, ITokenStorageService tokenStorage)
    {
        _dashboardService = dashboardService;
        _tokenStorage = tokenStorage;
        Title = "Dashboard";
    }

    [ObservableProperty]
    private DashboardDto? _dashboard;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private bool _hasCourse;

    [ObservableProperty]
    private string _greeting = "Welcome!";

    [ObservableProperty]
    private string _userName = "Student";

    [ObservableProperty]
    private double _courseProgress;

    [ObservableProperty]
    private string _progressText = "";

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private bool _hasCompletedCourses;

    public ObservableCollection<CompletedCourseDto> CompletedCourses { get; } = [];

    [RelayCommand]
    private async Task LoadDashboardAsync()
    {
        await ExecuteAsync(async () =>
        {
            var user = await _tokenStorage.GetUserAsync();
            if (user != null)
            {
                UserName = user.FirstName;
                var hour = DateTime.Now.Hour;
                var timeGreeting = hour < 12 ? "Good Morning" : hour < 17 ? "Good Afternoon" : "Good Evening";
                Greeting = $"{timeGreeting}, {user.FirstName}!";
            }

            var result = await _dashboardService.GetDashboardAsync();
            if (result.Success && result.Data != null)
            {
                Dashboard = result.Data;
                HasCourse = !string.IsNullOrEmpty(result.Data.CourseName);
                CalculateProgress(result.Data);

                // Populate completed courses
                CompletedCourses.Clear();
                foreach (var c in result.Data.CompletedCourses)
                    CompletedCourses.Add(c);
                HasCompletedCourses = CompletedCourses.Count > 0;
            }
            else
            {
                Dashboard = null;
                HasCourse = false;
                CourseProgress = 0;
                ProgressText = "";
                IsCompleted = false;
                CompletedCourses.Clear();
                HasCompletedCourses = false;
            }
        });
        IsRefreshing = false;
    }

    private void CalculateProgress(DashboardDto data)
    {
        if (data.CourseStatus == "Completed")
        {
            CourseProgress = 1.0;
            ProgressText = "Completed";
            IsCompleted = true;
            return;
        }

        if (data.CourseStatus == "Pending Payment" || data.EnrolledDate == null || string.IsNullOrEmpty(data.CourseDuration))
        {
            CourseProgress = 0;
            ProgressText = data.CourseStatus == "Pending Payment" ? "Awaiting payment" : "";
            IsCompleted = false;
            return;
        }

        // Parse duration like "6 Months", "3 months", "1 Year", "12 Weeks"
        var totalDays = ParseDurationToDays(data.CourseDuration);
        if (totalDays <= 0)
        {
            CourseProgress = 0;
            ProgressText = "Ongoing";
            IsCompleted = false;
            return;
        }

        var enrolled = data.EnrolledDate.Value;
        var elapsed = (DateTime.Now - enrolled).TotalDays;
        var progress = Math.Clamp(elapsed / totalDays, 0, 1);

        CourseProgress = progress;
        IsCompleted = progress >= 1.0;

        if (IsCompleted)
        {
            ProgressText = "Completed";
        }
        else
        {
            var remaining = totalDays - elapsed;
            if (remaining > 30)
                ProgressText = $"{(int)Math.Ceiling(remaining / 30)} months remaining";
            else if (remaining > 7)
                ProgressText = $"{(int)Math.Ceiling(remaining / 7)} weeks remaining";
            else
                ProgressText = $"{(int)Math.Max(1, remaining)} days remaining";
        }
    }

    private static double ParseDurationToDays(string duration)
    {
        var match = Regex.Match(duration.Trim(), @"(\d+)\s*(month|year|week|day)", RegexOptions.IgnoreCase);
        if (!match.Success) return 0;

        var value = int.Parse(match.Groups[1].Value);
        var unit = match.Groups[2].Value.ToLower();

        return unit switch
        {
            "month" => value * 30.0,
            "year" => value * 365.0,
            "week" => value * 7.0,
            "day" => value,
            _ => 0
        };
    }

    [RelayCommand]
    private async Task GoToCoursesAsync()
    {
        await Shell.Current.GoToAsync("//main/courses");
    }
}
