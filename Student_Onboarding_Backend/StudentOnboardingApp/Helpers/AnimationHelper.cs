namespace StudentOnboardingApp.Helpers;

public static class AnimationHelper
{
    public static async Task FadeUpAsync(VisualElement element, uint duration = 450, uint delay = 0)
    {
        element.Opacity = 0;
        element.TranslationY = 18;

        if (delay > 0) await Task.Delay((int)delay);

        await Task.WhenAll(
            element.FadeToAsync(1, duration, Easing.CubicOut),
            element.TranslateToAsync(0, 0, duration, Easing.CubicOut)
        );
    }

    public static async Task StaggerFadeUpAsync(IEnumerable<VisualElement> elements, uint staggerDelay = 50)
    {
        uint delay = 0;
        var tasks = new List<Task>();
        foreach (var element in elements)
        {
            tasks.Add(FadeUpAsync(element, delay: delay));
            delay += staggerDelay;
        }
        await Task.WhenAll(tasks);
    }

    public static async Task SlideInRightAsync(VisualElement element, uint duration = 350, uint delay = 0)
    {
        element.Opacity = 0;
        element.TranslationX = 24;

        if (delay > 0) await Task.Delay((int)delay);

        await Task.WhenAll(
            element.FadeToAsync(1, duration, Easing.CubicOut),
            element.TranslateToAsync(0, 0, duration, Easing.CubicOut)
        );
    }

    public static async Task BounceAsync(VisualElement element)
    {
        await element.ScaleToAsync(0.95, 80, Easing.CubicOut);
        await element.ScaleToAsync(1.0, 160, new Easing(t =>
        {
            return 1 + 2.56 * Math.Pow(t - 1, 3) + 1.56 * Math.Pow(t - 1, 2);
        }));
    }

    public static async Task PulseAsync(VisualElement element, uint count = 1)
    {
        for (uint i = 0; i < count; i++)
        {
            await element.FadeToAsync(0.5, 500, Easing.CubicInOut);
            await element.FadeToAsync(1.0, 500, Easing.CubicInOut);
        }
    }

    public static async Task ShakeAsync(VisualElement element)
    {
        await element.TranslateToAsync(-8, 0, 50);
        await element.TranslateToAsync(8, 0, 50);
        await element.TranslateToAsync(-6, 0, 50);
        await element.TranslateToAsync(6, 0, 50);
        await element.TranslateToAsync(-3, 0, 50);
        await element.TranslateToAsync(0, 0, 50);
    }
}
