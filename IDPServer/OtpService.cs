using IDPServer.Models;
using IDPServer.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace IDPServer;

public sealed class OtpService : IOtpService
{
    // Custom purpose string so this token never collides with Identity's own
    // "change phone number" tokens, even though both use the same provider.
    private const string TokenPurpose = "otp-login";

    private static readonly TimeSpan ResendCooldown = TimeSpan.FromSeconds(60);
    private const int MaxVerifyAttempts = 5;

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ISmsSender _smsSender;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OtpService> _logger;

    public OtpService(
        UserManager<ApplicationUser> userManager,
        ISmsSender smsSender,
        IMemoryCache cache,
        ILogger<OtpService> logger)
    {
        _userManager = userManager;
        _smsSender = smsSender;
        _cache = cache;
        _logger = logger;
    }

    public async Task<OtpRequestResult> RequestOtpAsync(string phoneNumber, CancellationToken ct = default)
    {
        var cooldownKey = CooldownKey(phoneNumber);
        if (_cache.TryGetValue(cooldownKey, out DateTimeOffset lastSentAt))
        {
            var elapsed = DateTimeOffset.UtcNow - lastSentAt;
            if (elapsed < ResendCooldown)
            {
                return OtpRequestResult.Throttled((int)(ResendCooldown - elapsed).TotalSeconds);
            }
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, ct);

        if (user == null)
        {
            // First time this number is seen -> auto-provision a Customer account.
            // PhoneNumberConfirmed stays false until the code is actually verified,
            // so an abandoned request never looks like a real, usable account.
            user = new ApplicationUser
            {
                UserName = phoneNumber,
                PhoneNumber = phoneNumber,
                PhoneNumberConfirmed = false,
                UserType = UserType.Customer
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                _logger.LogError("Failed to provision customer for {PhoneNumber}: {Errors}", phoneNumber, errors);
                return OtpRequestResult.Failure("could_not_create_account");
            }
        }
        else if (user.IsBlocked)
        {
            return OtpRequestResult.Failure("account_blocked");
        }

        var code = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, TokenPurpose);

        await _smsSender.SendAsync(phoneNumber, $"Your verification code is: {code}", ct);

        _cache.Set(cooldownKey, DateTimeOffset.UtcNow, ResendCooldown);
        _cache.Remove(AttemptsKey(phoneNumber)); // a fresh code means a fresh attempt budget

        return OtpRequestResult.Success();
    }

    public async Task<OtpVerifyResult> VerifyOtpAsync(string phoneNumber, string code, CancellationToken ct = default)
    {
        var attemptsKey = AttemptsKey(phoneNumber);
        var attempts = _cache.GetOrCreate(attemptsKey, _ => 0);

        if (attempts >= MaxVerifyAttempts)
        {
            return OtpVerifyResult.Failure("too_many_attempts");
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, ct);
        if (user == null)
        {
            return OtpVerifyResult.Failure("invalid_code");
        }

        if (user.IsBlocked)
        {
            return OtpVerifyResult.Failure("account_blocked");
        }

        var isValid = await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultPhoneProvider, TokenPurpose, code);

        if (!isValid)
        {
            _cache.Set(attemptsKey, attempts + 1, TimeSpan.FromMinutes(10));
            return OtpVerifyResult.Failure("invalid_code");
        }

        _cache.Remove(attemptsKey);

        if (!user.PhoneNumberConfirmed)
        {
            user.PhoneNumberConfirmed = true;
            await _userManager.UpdateAsync(user);
        }

        return OtpVerifyResult.Success(user);
    }

    private static string CooldownKey(string phoneNumber) => $"otp:cooldown:{phoneNumber}";

    private static string AttemptsKey(string phoneNumber) => $"otp:attempts:{phoneNumber}";
}
