namespace Zergatul.Obs.InputOverlay
{
    public sealed record ApplicationOptions(
        string Host,
        int Port,
        bool EnableDebugLog);
}