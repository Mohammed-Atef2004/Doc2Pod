using System.Text.RegularExpressions;
using Domain.Common.Domain.Common;
using  Domain.SharedKernel;
using  Domain.Users.Errors;
namespace  Domain.Users.ValueObjects;


public sealed class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase,
        matchTimeout: TimeSpan.FromMilliseconds(250));

    public string Value { get; private set; }

    private Email() { }
    private Email(string value) => Value = value;

    public static Result<Email> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<Email>.Failure(EmailErrors.Empty);

        var trimmed = value.Trim();

        if (trimmed.Length > 254)
            return Result<Email>.Failure(EmailErrors.TooLong);

        if (!EmailRegex.IsMatch(trimmed))
            return Result<Email>.Failure(EmailErrors.InvalidFormat);

        return Result<Email>.Success(new Email(trimmed.ToLowerInvariant()));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
    public static Email FromDatabase(string value)
    {
        return new Email(value);
    }
}

