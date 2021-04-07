# Tiger.Secrets

## What It Is

Tiger.Secrets is a library integrating [AWS Secrets Manager][] secret values into
the [Microsoft.Extensions.Configuration][] configuration ecosystem.
Just as values from
[the command line][],
[environment variables][],
[JSON files][],
or [User Secrets][] <!-- or, like, a billion other sources -->
can be integrated into a unified, strongly-typed configuration, secret configuration values from AWS Secrets Manager can be, as well.

[AWS Secrets Manager]: https://aws.amazon.com/secrets-manager/
[Microsoft.Extensions.Configuration]: https://www.nuget.org/packages/Microsoft.Extensions.Configuration/
[the command line]: https://www.nuget.org/packages/Microsoft.Extensions.Configuration.CommandLine/
[environment variables]: https://www.nuget.org/packages/Microsoft.Extensions.Configuration.EnvironmentVariables/
[JSON files]: https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/
[User Secrets]: https://www.nuget.org/packages/Microsoft.Extensions.Configuration.UserSecrets/

## Why You Want It

Use of AWS Secrets Manager can be summed up in the following three basic points:

1. Don't check secret configuration values into code repositories.
2. Don't check secret configuration values into code repositories.
3. Don't check secret configuration values into code repositories.

There are a few ways to allow an application access to secret configuration values without falling afoul of the three points listed above. One is to check _encrypted_ secret configuration values into the code repository. This is safe, but requires a full deployment if any of these values change. Another is to redirect keys to secret values during deployment, such as with [CloudFormation dynamic references][]. This is also safe, but has the same deployment requirement. By retrieving the values at runtime (and combining them with safe configuration values), these can be changed while the application is running. This is a very nice feature for configuration values such as application-global log level, which – although not a _secret_ value – is useful to change on the fly for anomaly checking.

[CloudFormation dynamic references]: https://docs.aws.amazon.com/AWSCloudFormation/latest/UserGuide/dynamic-references.html

## How To Use It

In short, setup has two parts. One performed in the application (or, better, in CloudFormation), and one performed in Secrets Manager (which may _also_ be performed in CloudFormation).

In the application, the index values for the configuration key `Secrets:Ids` must be set. (That is, `Secrets:Ids:0`, `Secrets:Ids:1`, &c.) These are the identifiers which the library will look for in Secrets Manager. If a specified identifier is not present, the configuration will fail.

Add the Secrets Manager configuration to the application configuration with the extension method for `IConfiguration`, similar to any other configuration source. For example:

```csharp
config.AddAWSSecretsManager();
```

This can be added to a reuable host, one like `WebHost.CreateDefaultBuilder<TStartup>()` from ASP.NET Core. Or see if [your favorite hosting library][] already has one configured.

[your favorite hosting library]: https://github.com/Cimpress-MCP/Tiger-Lambda/

In Secrets Manager, the values associated with these identifiers must contain valid JSON strings. This library has no support for `SecretBinary` values. (If the default key/value user interface in the AWS console is used, Secrets Manager stores that as JSON behind the scenes, so it is valid.)

For further details, please consult [the wiki][]. (But don't forget to grant your application the IAM permissions to read these secret values!)

[the wiki]: https://github.com/Cimpress-MCP/Tiger-Secrets/wiki/How-To-Use-It-(Details)

## Thank You

Seriously, though. Thank you for using this software. The author hopes it performs admirably for you.
