using Clinexa.Services.Interfaces;

public class EgyptDateTimeService : IDateTimeService
{
    private readonly TimeZoneInfo _egyptTimeZone;

    public EgyptDateTimeService()
    {
        _egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows()
                ? "Egypt Standard Time"
                : "Africa/Cairo");
    }

    public DateTime Now =>
        TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            _egyptTimeZone);

    public DateTime UtcNow =>
        DateTime.UtcNow;
}