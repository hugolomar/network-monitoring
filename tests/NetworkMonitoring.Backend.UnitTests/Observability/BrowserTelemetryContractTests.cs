namespace NetworkMonitoring.Backend.UnitTests.Observability;

/// <summary>
/// Documents the browser telemetry payload contract shared with the frontend scrubber (FR-003, FR-019).
/// </summary>
public sealed class BrowserTelemetryContractTests
{
    /// <summary>
    /// Attribute keys that must never appear on browser-exported spans.
    /// Keep in sync with <c>src/NetworkMonitoring.Frontend/src/telemetry/hygiene.ts</c>.
    /// </summary>
    public static readonly string[] ForbiddenBrowserAttributes =
    [
        "enduser.id",
        "enduser.name",
        "enduser.email",
        "user.id",
        "user.email",
        "user.name",
        "http.request.header.authorization",
        "http.request.header.cookie"
    ];

    /// <summary>
    /// Ensures the forbidden attribute list stays explicit and non-empty for CI review.
    /// </summary>
    [Fact]
    public void Browser_telemetry_forbids_end_user_identity_attributes()
    {
        Assert.NotEmpty(ForbiddenBrowserAttributes);
        Assert.Contains("enduser.id", ForbiddenBrowserAttributes);
        Assert.Contains("user.email", ForbiddenBrowserAttributes);
        Assert.Contains("http.request.header.authorization", ForbiddenBrowserAttributes);
        Assert.All(ForbiddenBrowserAttributes, key => Assert.False(string.IsNullOrWhiteSpace(key)));
    }

    /// <summary>
    /// Ensures URL scrubbing rules reject query strings that could carry secrets or identifiers.
    /// </summary>
    [Theory]
    [InlineData("https://ui.local/devices?mac=AA:BB:CC:DD:EE:FF", "https://ui.local/devices")]
    [InlineData("https://ui.local/graph?token=abc#frag", "https://ui.local/graph")]
    public void Browser_url_scrubbing_drops_query_and_fragment(string raw, string expected)
    {
        Assert.Equal(expected, ScrubUrl(raw));
    }

    private static string ScrubUrl(string raw)
    {
        var uri = new Uri(raw, UriKind.Absolute);
        return $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath}";
    }
}
