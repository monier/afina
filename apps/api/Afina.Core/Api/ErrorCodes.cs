namespace Afina.Core.Api;

public static class ErrorCodes
{
    // Registration errors
    public const string USERNAME_REQUIRED = "USERNAME_REQUIRED";
    public const string PASSWORD_REQUIRED = "PASSWORD_REQUIRED";
    public const string USERNAME_ALREADY_EXISTS = "USERNAME_ALREADY_EXISTS";
    public const string USERNAME_INVALID = "USERNAME_INVALID";
    public const string PASSWORD_TOO_WEAK = "PASSWORD_TOO_WEAK";

    // Login errors
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string USER_NOT_FOUND = "USER_NOT_FOUND";
    public const string INVALID_REFRESH_TOKEN = "INVALID_REFRESH_TOKEN";

    // Authorization errors
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string USER_DELETED = "USER_DELETED";

    // General errors
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
}
