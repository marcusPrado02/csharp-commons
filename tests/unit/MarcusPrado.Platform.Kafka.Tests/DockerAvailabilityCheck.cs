namespace MarcusPrado.Platform.Kafka.Tests;

internal static class DockerAvailabilityCheck
{
    private static readonly Lazy<bool> IsDockerAvailable = new(() =>
    {
        const string SocketPath = "/var/run/docker.sock";
        return System.IO.File.Exists(SocketPath);
    });

    public static void SkipIfDockerNotAvailable()
    {
        if (!IsDockerAvailable.Value)
        {
            throw Xunit.Sdk.SkipException.ForSkip("Docker is not available");
        }
    }
}
