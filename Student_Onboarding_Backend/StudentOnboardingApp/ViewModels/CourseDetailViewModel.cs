using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Course;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels;

[QueryProperty(nameof(CourseId), "courseId")]
public partial class CourseDetailViewModel : BaseViewModel
{
    private readonly ICourseService _courseService;

    public CourseDetailViewModel(ICourseService courseService)
    {
        _courseService = courseService;
        Title = "Course Details";
    }

    [ObservableProperty]
    private string _courseId = string.Empty;

    [ObservableProperty]
    private CourseDetailDto? _course;

    [ObservableProperty]
    private double _averageRating;

    [ObservableProperty]
    private int _totalReviews;

    [ObservableProperty]
    private int _selectedRating;

    [ObservableProperty]
    private string _reviewRemarks = string.Empty;

    [ObservableProperty]
    private string? _reviewSuccessMessage;

    [ObservableProperty]
    private bool _isSubmittingReview;

    [ObservableProperty]
    private bool _canReview;

    [ObservableProperty]
    private bool _hasReviewed;

    public ObservableCollection<CourseReviewDto> Reviews { get; } = [];

    public string Star1Color => SelectedRating >= 1 ? "#F59E0B" : "#D4D2EC";
    public string Star2Color => SelectedRating >= 2 ? "#F59E0B" : "#D4D2EC";
    public string Star3Color => SelectedRating >= 3 ? "#F59E0B" : "#D4D2EC";
    public string Star4Color => SelectedRating >= 4 ? "#F59E0B" : "#D4D2EC";
    public string Star5Color => SelectedRating >= 5 ? "#F59E0B" : "#D4D2EC";

    partial void OnSelectedRatingChanged(int value)
    {
        OnPropertyChanged(nameof(Star1Color));
        OnPropertyChanged(nameof(Star2Color));
        OnPropertyChanged(nameof(Star3Color));
        OnPropertyChanged(nameof(Star4Color));
        OnPropertyChanged(nameof(Star5Color));
    }

    partial void OnCourseIdChanged(string value)
    {
        if (Guid.TryParse(value, out _))
        {
            LoadCourseCommand.ExecuteAsync(null);
            LoadReviewsCommand.ExecuteAsync(null);
        }
    }

    [RelayCommand]
    private async Task LoadCourseAsync()
    {
        await ExecuteAsync(async () =>
        {
            if (Guid.TryParse(CourseId, out var id))
            {
                var result = await _courseService.GetCourseDetailAsync(id);
                if (result.Success && result.Data != null)
                {
                    Course = result.Data;
                    Title = result.Data.Name;
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
        });
    }

    [RelayCommand]
    private async Task LoadReviewsAsync()
    {
        if (!Guid.TryParse(CourseId, out var id)) return;

        try
        {
            var result = await _courseService.GetCourseReviewsAsync(id);
            if (result.Success && result.Data != null)
            {
                AverageRating = result.Data.AverageRating;
                TotalReviews = result.Data.TotalReviews;
                CanReview = result.Data.CanReview;
                HasReviewed = result.Data.HasReviewed;
                Reviews.Clear();
                foreach (var r in result.Data.Reviews)
                    Reviews.Add(r);
            }
        }
        catch { /* silently fail */ }
    }

    [RelayCommand]
    private void SetRating(string rating)
    {
        if (int.TryParse(rating, out var r))
            SelectedRating = r;
    }

    [RelayCommand]
    private async Task SubmitReviewAsync()
    {
        if (SelectedRating < 1 || SelectedRating > 5)
        {
            ErrorMessage = "Please select a rating (1-5 stars).";
            return;
        }

        if (!Guid.TryParse(CourseId, out var id)) return;

        IsSubmittingReview = true;
        ErrorMessage = null;
        ReviewSuccessMessage = null;

        try
        {
            var request = new SubmitReviewRequest
            {
                Rating = SelectedRating,
                Remarks = string.IsNullOrWhiteSpace(ReviewRemarks) ? null : ReviewRemarks.Trim()
            };

            var result = await _courseService.SubmitReviewAsync(id, request);
            if (result.Success)
            {
                ReviewSuccessMessage = "Review submitted! Thank you.";
                SelectedRating = 0;
                ReviewRemarks = string.Empty;
                CanReview = false;
                HasReviewed = true;
                await LoadReviewsAsync();
                _ = ClearReviewSuccessAfterDelayAsync();
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsSubmittingReview = false;
        }
    }

    private async Task ClearReviewSuccessAfterDelayAsync()
    {
        await Task.Delay(5000);
        ReviewSuccessMessage = null;
    }

    [ObservableProperty]
    private bool _applicationSubmitted;

    [ObservableProperty]
    private bool _isApplying;

    [RelayCommand]
    private async Task ApplyAsync()
    {
        if (Course == null || IsApplying) return;

        IsApplying = true;
        ErrorMessage = null;

        try
        {
            var request = new CourseApplicationRequest { CourseId = Course.Id };
            var result = await _courseService.ApplyForCourseAsync(request);

            if (result.Success)
            {
                ApplicationSubmitted = true;
                await Task.Delay(3000);
                try
                {
                    await Shell.Current.GoToAsync("//main/courses");
                }
                catch
                {
                    // Fallback: navigate back if route fails
                    await Shell.Current.Navigation.PopToRootAsync();
                }
            }
            else
            {
                ErrorMessage = result.Message;
            }
        }
        catch
        {
            ErrorMessage = "Unable to apply for this course. Please try again.";
        }
        finally
        {
            IsApplying = false;
        }
    }
}
