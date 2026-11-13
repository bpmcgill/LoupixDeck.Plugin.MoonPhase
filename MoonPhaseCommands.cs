using System.Reflection;
using System.Text.RegularExpressions;
using LoupixDeck.PluginSdk;

namespace LoupixDeck.Plugin.MoonPhase;

/// <summary>Lunar phase calculation (synodic-month approximation).</summary>
internal static class MoonMath
{
    // Mean synodic month (days) and a known new-moon epoch (2000-01-06 18:14 UTC).
    public const double SynodicMonthDays = 29.530588853;
    private static readonly DateTime Epoch = new(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);

    /// <summary>Lunation age in days (0 = new moon).</summary>
    public static double AgeDays(DateTime nowUtc)
    {
        double age = (nowUtc - Epoch).TotalDays % SynodicMonthDays;
        return age < 0 ? age + SynodicMonthDays : age;
    }

    /// <summary>Illuminated fraction, 0 (new) .. 1 (full).</summary>
    public static double IlluminationFraction(double age)
    {
        double phase = 2.0 * Math.PI * age / SynodicMonthDays;
        return (1.0 - Math.Cos(phase)) / 2.0;
    }

    /// <summary>The eight named phases, boundaries at 1/16..15/16 of the cycle.</summary>
    public static string PhaseName(double age)
    {
        double[] bounds = { 1.84566, 5.53699, 9.22831, 12.91963, 16.61096, 20.30228, 23.99361, 27.68493 };
        string[] names =
        {
            "New Moon", "Waxing Crescent", "First Quarter", "Waxing Gibbous",
            "Full Moon", "Waning Gibbous", "Last Quarter", "Waning Crescent",
        };
        for (int i = 0; i < bounds.Length; i++)
            if (age < bounds[i])
                return names[i];
        return "New Moon";
    }

    /// <summary>Which pre-rendered frame represents this age.</summary>
    public static int FrameIndex(double age, int frameCount)
    {
        int idx = (int)Math.Round(age / SynodicMonthDays * frameCount) % frameCount;
        return idx < 0 ? idx + frameCount : idx;
    }
}

/// <summary>Text readout: phase name + illumination percentage.</summary>
internal sealed class MoonPhaseTextCommand : IDisplayCommand
{
    public CommandDescriptor Descriptor { get; } = new()
    {
        CommandName = "Moon.Phase",
        DisplayName = "Moon phase (text)",
        Group = "Moon Phase",
    };

    public ButtonTargets SupportedTargets => ButtonTargets.TouchButton;
    public TimeSpan UpdateInterval => TimeSpan.FromMinutes(10);

    public string GetText(CommandContext ctx)
    {
        double age = MoonMath.AgeDays(DateTime.UtcNow);
        int pct = (int)Math.Round(MoonMath.IlluminationFraction(age) * 100.0);
        return $"{MoonMath.PhaseName(age)}\n{pct}%";
    }

    public Task Execute(CommandContext ctx) => Task.CompletedTask;
}

/// <summary>Renders the current moon phase image on a touch button.</summary>
internal sealed class MoonPhaseImageCommand : IDisplayImageCommand
{
    private static byte[][] _frames = [];

    public static int FrameCount => _frames.Length;

    public CommandDescriptor Descriptor { get; } = new()
    {
        CommandName = "Moon.Image",
        DisplayName = "Moon phase (image)",
        Group = "Moon Phase",
        ButtonLayout = new ButtonLayoutDescriptor { Mode = ButtonLayoutMode.None },
    };

    public ButtonTargets SupportedTargets => ButtonTargets.TouchButton;
    public TimeSpan UpdateInterval => TimeSpan.FromMinutes(10);

    /// <summary>Loads the embedded moon_XX.png frames, indexed by position.</summary>
    public static void LoadFrames(Assembly assembly)
    {
        var regex = new Regex(@"moon_(\d+)\.png$", RegexOptions.Compiled);
        var frames = new List<(int Index, byte[] Data)>();

        foreach (string name in assembly.GetManifestResourceNames())
        {
            Match m = regex.Match(name);
            if (!m.Success)
                continue;

            using Stream s = assembly.GetManifestResourceStream(name)!;
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            frames.Add((int.Parse(m.Groups[1].Value), ms.ToArray()));
        }

        if (frames.Count == 0)
            return;

        int max = frames.Max(f => f.Index);
        var ordered = new byte[max + 1][];
        foreach (var (index, data) in frames)
            ordered[index] = data;

        _frames = ordered;
    }

    public bool RenderImage(CommandContext ctx, IRenderCanvas canvas)
    {
if (_frames.Length == 0)
            return false;

        double age = MoonMath.AgeDays(DateTime.UtcNow);
        int idx = MoonMath.FrameIndex(age, _frames.Length);

        canvas.DrawImage(_frames[idx], 0, 0, canvas.Width, canvas.Height);
        return true;
    }

    public Task Execute(CommandContext ctx) => Task.CompletedTask;
}