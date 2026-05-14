using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudentOnboardingApp.Models.Profile;
using StudentOnboardingApp.Services.Interfaces;

namespace StudentOnboardingApp.ViewModels;

public partial class EditProfileViewModel : BaseViewModel
{
    private readonly IProfileService _profileService;

    public EditProfileViewModel(IProfileService profileService)
    {
        _profileService = profileService;
        Title = "Edit Profile";
    }

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    private DateTime? _dateOfBirth;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _education = string.Empty;

    [RelayCommand]
    private async Task LoadProfileAsync()
    {
        await ExecuteAsync(async () =>
        {
            var result = await _profileService.GetProfileAsync();
            if (result.Success && result.Data != null)
            {
                FirstName = result.Data.FirstName;
                LastName = result.Data.LastName;
                PhoneNumber = result.Data.PhoneNumber ?? string.Empty;
                DateOfBirth = result.Data.DateOfBirth;
                Address = result.Data.Address ?? string.Empty;
                Education = result.Data.Education ?? string.Empty;
            }
        });
    }

    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        await ExecuteAsync(async () =>
        {
            var request = new UpdateProfileRequest
            {
                FirstName = FirstName,
                LastName = LastName,
                PhoneNumber = PhoneNumber,
                DateOfBirth = DateOfBirth,
                Address = Address,
                Education = Education
            };

            var result = await _profileService.UpdateProfileAsync(request);

            if (result.Success)
            {
                await Shell.Current.DisplayAlert("Success", "Profile updated successfully.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                ErrorMessage = result.Message;
            }
        });
    }

    [RelayCommand]
    private async Task UploadDocumentAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select Document",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.Android, new[] { "application/pdf", "image/*" } },
                    { DevicePlatform.iOS, new[] { "public.pdf", "public.image" } },
                })
            });

            if (result != null)
            {
                IsBusy = true;
                using var stream = await result.OpenReadAsync();
                var docType = await Shell.Current.DisplayPromptAsync(
                    "Document Type", "Enter the document type (e.g., ID Proof, Address Proof):");

                if (!string.IsNullOrEmpty(docType))
                {
                    var uploadResult = await _profileService.UploadDocumentAsync(stream, result.FileName, docType);
                    if (uploadResult.Success)
                    {
                        await Shell.Current.DisplayAlert("Success", "Document uploaded successfully.", "OK");
                    }
                    else
                    {
                        ErrorMessage = uploadResult.Message;
                    }
                }
                IsBusy = false;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }
}
