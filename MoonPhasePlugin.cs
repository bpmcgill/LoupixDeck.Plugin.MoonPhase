using LoupixDeck.PluginSdk;

namespace LoupixDeck.Plugin.MoonPhase;

/// <summary>
/// LoupixDeck plugin that shows the current lunar phase on a touch button —
/// as a drawn moon image and as text (phase name + illumination %).
/// </summary>
public sealed class MoonPhasePlugin : LoupixPlugin
{
    private IPluginHost _host = null!;

    public override PluginMetadata Metadata { get; } = new()
    {
        Id = "moonphase",
        Name = "Moon Phase",
        Version = new Version(1, 0, 0),
        SdkVersion = SdkInfo.Version,
        Author = "Ben McGill",
        Description = "Current lunar phase as a moon image or text readout."
    };

    public override void Initialize(IPluginHost host)
    {
        _host = host;
        MoonPhaseImageCommand.LoadFrames(typeof(MoonPhasePlugin).Assembly);
        _host.Logger.Info($"Moon Phase plugin loaded ({MoonPhaseImageCommand.FrameCount} frames).");
    }

    public override IEnumerable<IPluginCommand> GetCommands()
    {
        yield return new MoonPhaseTextCommand();
        yield return new MoonPhaseImageCommand();
    }
}