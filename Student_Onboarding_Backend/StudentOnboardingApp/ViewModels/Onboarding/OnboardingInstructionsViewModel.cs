using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Onboarding;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels.Onboarding;

public partial class OnboardingInstructionsViewModel : BaseViewModel
{
    private readonly IOnboardingService _onboardingService;

    public OnboardingInstructionsViewModel(IOnboardingService onboardingService)
    {
        _onboardingService = onboardingService;
        Title = "Onboarding Instructions";
    }

    [ObservableProperty]
    private bool _isAgreed;

    public ObservableCollection<OnboardingInstructionDto> Instructions { get; } = [];

    [RelayCommand]
    private async Task LoadInstructionsAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _onboardingService.GetInstructionsAsync();
            if (result.Success && result.Data != null)
            {
                Instructions.Clear();
                foreach (var instruction in result.Data.OrderBy(i => i.Order))
                    Instructions.Add(instruction);
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
    }

    [RelayCommand]
    private async Task AcceptAndContinueAsync()
    {
        if (!IsAgreed) return;

        await ExecuteAsync(async () =>
        {
            var result = await _onboardingService.AcceptOnboardingAsync();
            if (result.Success)
            {
                await Shell.Current.GoToAsync("//main/dashboard");
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
    }
}
