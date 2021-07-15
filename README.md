# Tiger.AppConfig

## What It Is

Tiger.AppConfig is a library integrating [AWS AppConfig][] values into
the [Microsoft.Extensions.Configuration][] configuration ecosystem.
Just as values from
[the command line][],
[environment variables][],
[JSON files][],
or [User Secrets][] <!-- or, like, a billion other sources -->
can be integrated into a unified, strongly-typed configuration, configuration values from AWS AppConfig can be, as well.

[AWS AppConfig]: https://docs.aws.amazon.com/appconfig/latest/userguide/what-is-appconfig.html
[Microsoft.Extensions.Configuration]: https://www.nuget.org/packages/Microsoft.Extensions.Configuration/
[the command line]: https://www.nuget.org/packages/Microsoft.Extensions.Configuration.CommandLine/
[environment variables]: https://www.nuget.org/packages/Microsoft.Extensions.Configuration.EnvironmentVariables/
[JSON files]: https://www.nuget.org/packages/Microsoft.Extensions.Configuration.Json/
[User Secrets]: https://www.nuget.org/packages/Microsoft.Extensions.Configuration.UserSecrets/

## How To Use It

Setup is performed in the same manner as any other Microsoft.Extensions.Configuration configuration. Ideally, this is performed in CloudFormation. If a required identifier is not present, the configuration retrieval will fail. The required identifiers are these:

- `Application`: The identifier of an `AWS::AppConfig::Application`
- `Environment`: The identifier of an `AWS::AppConfig::Environment`
- `ConfigurationProfile`: The identifier of an `AWS::AppConfig::ConfigurationProfile`

In CloudFormation, these values can be retrieved by applying the intrinsic function `!Ref` to resources of the specified resource type.

Add the AppConfig configuration to the application configuration with the extension method for `IConfigurationBuilder`, similar to any other configuration source. For example:

```csharp
hostBuilder.ConfigureAppConfiguration(b => b.AddAppConfig())
```

This should be applied to the host builder for the Lambda entry point of the application.

If the configuration identifiers are located under a section other than the default "AppConfig", that name can be provided as an argument to the call to `AddAppConfig`. Tiger.AppConfig reads the environment variables used to configure the AppConfig extension itself as necessary.

## Thank You

Seriously, though. Thank you for using this software. The author hopes it performs admirably for you.
