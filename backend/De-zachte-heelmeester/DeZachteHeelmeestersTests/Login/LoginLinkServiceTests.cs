using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Services;
using Microsoft.EntityFrameworkCore;

namespace DeZachteHeelmeestersTests.Login;

/// <summary>
/// Tests for LOG-04: logging in with a verification link that is valid for 5 minutes.
/// </summary>
[TestClass]
public class LoginLinkServiceTests
{
    private FakeClock _clock = null!;
    private ApplicationDbContext _context = null!;
    private LoginLinkService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        // Every test gets its own empty in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _clock = new FakeClock(new DateTimeOffset(2026, 11, 26, 12, 0, 0, TimeSpan.Zero));
        _service = new LoginLinkService(_context, _clock);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Dispose();
    }

    // AC1: the user clicks the link within 5 minutes and is logged in
    [TestMethod]
    public async Task VerifyLink_WithinFiveMinutes_ReturnsSuccess()
    {
        var token = await _service.CreateLinkAsync("user-1");
        _clock.Advance(TimeSpan.FromMinutes(4));

        var result = await _service.VerifyLinkAsync(token);

        Assert.AreEqual(LoginLinkStatus.Success, result.Status);
        Assert.AreEqual("user-1", result.UserId);
    }

    // AC2: the link is older than 5 minutes, so the user is not logged in
    [TestMethod]
    public async Task VerifyLink_AfterFiveMinutes_ReturnsExpired()
    {
        var token = await _service.CreateLinkAsync("user-1");
        _clock.Advance(TimeSpan.FromMinutes(5) + TimeSpan.FromSeconds(1));

        var result = await _service.VerifyLinkAsync(token);

        Assert.AreEqual(LoginLinkStatus.Expired, result.Status);
        Assert.IsNull(result.UserId);
    }

    // A link that does not exist is never accepted
    [TestMethod]
    public async Task VerifyLink_UnknownToken_ReturnsInvalid()
    {
        var result = await _service.VerifyLinkAsync("does-not-exist");

        Assert.AreEqual(LoginLinkStatus.Invalid, result.Status);
        Assert.IsNull(result.UserId);
    }

    // A link can only be used once
    [TestMethod]
    public async Task VerifyLink_UsedTwice_ReturnsInvalidSecondTime()
    {
        var token = await _service.CreateLinkAsync("user-1");
        await _service.VerifyLinkAsync(token);

        var result = await _service.VerifyLinkAsync(token);

        Assert.AreEqual(LoginLinkStatus.Invalid, result.Status);
    }
}

/// <summary>
/// A clock that the test can move forward, so we don't have to wait 5 real minutes.
/// </summary>
internal class FakeClock : TimeProvider
{
    private DateTimeOffset _now;

    public FakeClock(DateTimeOffset start)
    {
        _now = start;
    }

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan time)
    {
        _now = _now.Add(time);
    }
}
