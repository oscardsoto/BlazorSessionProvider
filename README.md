# BlazorSessionProvider

## Introduction
BlazorSessionProvider is a Blazor Server library that handle sessions inside the application. Simple to install and configure.

## Requirements
- This library works under .NET 8 and higher

## Quick Install
1. Install the package via NuGet:
  ```console
  dotnet add package BlazorSessionProvider --version 1.0.0
  ```

2. Add the next code line in your Blazor project, on `Program.cs`:
  ```csharp
  using BlazorSessionProvider;
  var builder = WebApplication.CreateBuilder(args);
  ...
  builder.Services.AddSessionProvider(config => {
      config.TimeDelay           = new TimeSpan(0, 0, 30);
      config.SessionExpiredUrl   = "/logout";
      config.SessionNotFoundUrl  = "/";
  });
  ```
  Where:
  - `config.TimeDelay` is the time it will take for a session to expire from when it is initialized within the application.
  - `config.SessionExpiredUrl` is the URL of the application to which it will redirect when the session has expired.
  - `config.SessionNotFoundUrl` is the URL of the application to which it will redirect when the application does not find the session key.

3. Add these lines in your `_Imports.razor` file:
  ```csharp
  @using BlazorSessionProvider.Sessions
  @inject ISessionProvider SESS
  ```

Make sure to put the render mode in "InteractiveServer" on each page you want to manage your servers.
```csharp
@rendermode InteractiveServer
```

## Quick Usage

1. To store a session, add this line in your page/component:
  ```csharp
  SESS.CreateNewSession(new KeyValuePair("Key", Value));
  ```
  You can use the same line to replace the value for "Key" session.

2. To get it, use this line in your page/component:
  ```csharp
  var mySession = await SESS.GetSession<object>("Key");
  ```
  If you want to delete the session, just add `true` in the `removeIt` property:
  ```csharp
  var mySession = await SESS.GetSession<object>("Key", true);
  ```

3. If you want to delete the actual session manually, use:
  ```csharp
  SESS.RemoveSession();
  ```
  This will NOT delete all the sessions stored in the application.

You can't manage sessions in the method `OnInitialized` or `OnInitializedAsync`, because BlazorSessionProvider use JavaScriptInterop to get the key session in the browser.

```csharp
protected override async Task OnInitializedAsync()
{
	string errorSess = await SESS.GetSession<string>("your_session_name");
}

/*
	This will throw the next exception:
	InvalidOperationException: JavaScript interop calls cannot be issued at this time...
*/
```

Manage your sessions inside (or after) `OnAfterRender` or `OnAfterRenderAsync` to avoid this error

```csharp
protected override async Task OnAfterRenderAsync(bool firstRender)
{
	...
	string goodSess = await SESS.GetSession<string>("your_session_name");
	...
}
```
## Docs
For more information, see the Wikia.
***
Thank you so much for using BlazorSessionProvider.

Made with ❤️ by Oscar D. Soto