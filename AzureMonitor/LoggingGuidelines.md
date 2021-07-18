## 1. Follow Message Template Syntax and Recommendations

### Message Template Syntax
The string above `"Disk quota {Quota} exceeded by {User}"` is a Serilog _message template_. Message templates are a superset of standard .NET format strings, so any format string acceptable to `string.Format()` will also be correctly processed by Serilog.

* Property names are written between `{` and `}` brackets
* Property names must be valid C# identifiers, for example `FooBar`, but **not** `Foo.Bar` or `Foo-Bar`
* Brackets can be escaped by doubling them, e.g. `{{` will be rendered as `{`
parameters by treating the property names as indexes; this is identical to `string.Format()`'s behaviour

### Message Template Recommendations
**Fluent Style Guideline** - use the names of properties as content within the message as in the `User` example above. This improves readability and makes events more compact.

**Sentences vs. Fragments** - log event messages are fragments, not sentences; for consistency with other libraries that use Serilog, avoid a trailing period/full stop when possible.

**Templates vs. Messages** - Serilog events have a message template associated, _not_ a message. Internally, Serilog parses and caches every template (up to a fixed size limit). Treating the string parameter to log methods as a message, as in the case below, will degrade performance and consume cache memory.

```csharp
// Don't:
_logger.LogInformation("The time is " + DateTime.Now);

// Don't:
_logger.LogInformation($"The time is {DateTime.Now}");
```

Instead, _always_ use template properties to include variables in messages:

```csharp
// Do:
_logger.LogInformation("The time is {Now}", DateTime.Now);
```

**Property Naming** - Property names should use `PascalCase` for consistency with other code and libraries from the Serilog ecosystem.

The message template syntax is specified at [messagetemplates.org](https://messagetemplates.org/).

## 2. Always pass exception as first parameter
To log an exception, always pass the exception object as first argument:

```csharp
logger.LogWarning(exception, "An exception occured")
```
Otherwise it is treated as custom property and if it has no placeholder in the message it will not end up in the log. Also the formatting and storage is exception specific if you use the correct method overload.

## 3. Use structured logs
It is recommended to always use semantic/structured logs so that the logging backend receives the string with placeholders and its values separately. It is then able to just replace them in its UI on demand. This way each log statement preserves all associated properties and a template hash (in AI “MessageTemplate”) which allows the backend to apply advanced filtering or search for all logs of a given type.

```csharp
logger.LogWarning("The person {PersonId} could not be found", personId);
```
The previous statement will log the following properties to Application Insights:

Message: `The person 5 could not be found`
MessageTemplate: `The person {PersonId} could not be found`
PersonId: `5`

## 4. Use scopes to add custom properties to multiple log entries
Use scopes to transparently add custom properties to all logs in a given execution context.

## 5. User Log Event Levels properly

The following table lists the [LogLevel](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel) values, the convenience `Log{LogLevel}` extension method, and the suggested usage:

| LogLevel | Method | Description |
|--|--|--|
| [Trace](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel) | [Microsoft.Extensions.Logging.LoggerExtensions.LogTrace](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.logtrace) | Contain the most detailed messages. These messages may contain sensitive app data. These messages are disabled by default and should ***not*** be enabled in production. |
| [Debug](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel) | [Microsoft.Extensions.Logging.LoggerExtensions.LogDebug](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.logdebug) | For debugging and development. Use with caution in production due to the high volume. |
| [Information](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel) | [Microsoft.Extensions.Logging.LoggerExtensions.LogInformation](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.loginformation) | Tracks the general flow of the app. May have long-term value. |
| [Warning](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel) | [Microsoft.Extensions.Logging.LoggerExtensions.LogWarning](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.logwarning) | For abnormal or unexpected events. Typically includes errors or conditions that don't cause the app to fail. |
| [Error](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel) | [Microsoft.Extensions.Logging.LoggerExtensions.LogError](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.logerror) | For errors and exceptions that cannot be handled. These messages indicate a failure in the current operation or request, not an app-wide failure. |
| [Critical](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel) | [Microsoft.Extensions.Logging.LoggerExtensions.LogCritical](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerextensions.logcritical) | For failures that require immediate attention. Examples: data loss scenarios, out of disk space. |

# References: 
1. https://github.com/serilog/serilog/wiki/Writing-Log-Events
1. https://docs.microsoft.com/en-us/dotnet/core/extensions/logging#log-level
1. https://blog.rsuter.com/logging-with-ilogger-recommendations-and-best-practices
