>**Important Notice**
> Starting from version 1.1.0, BSP no longer stores the session ID in `localStorage`.

# Introduction
`BlazorSessionProvider` is a C# .NET library for Blazor Server applications that allows you to store, retrieve, and manage server-side session state, where client–server interaction and persistent state are separate.

### Features
- 📍 Stateful (server) — the session lives on the server
- 🔒 Data **never travels to the client**, preventing XSS
- 🧠 Full server-side control (you can invalidate, renew, or redirect sessions)
- 🔁 The server decides when a session expires, based on the configured settings
- 🕒 | **NEW** | Automatic cleanup of expired sessions
- ⚙️ Simple configuration
- ✅ Easy API for handling session data
- 📡 Native integration with `SignalR` (keeps the session alive through the connection)
- 👤 `ClaimsPrincipal` synchronized in real time (reactive to changes)
- 🧱 State persists even if the connection reloads, or until the server closes it

### Use cases for BSP
There are many use cases for BSP. Here are the most common:

- 👤 **Custom session after login** — Stores user data (ID, role, permissions) in the session after authentication.
- ⏰ **Session timeout control** — Automatically expires after a defined period and redirects the user to an expired-session page.
- 🧩 **Step-by-step wizard** — Preserves data between steps of a multi-page form without relying on parameters or `LocalStorage`.
- 🛒 **Temporary shopping cart** — Keeps the cart data on the server before checkout, avoiding exposure on the client side.
- 🔗 **Shared context data across pages** — Shares information between components (such as `ActiveDocumentId`) without using `QueryString` or `CascadingParameters`.
- 🧠 **Control of multiple SignalR connections** — Detects if a user opens multiple tabs with the same session.

# Requirements

- This library works on .NET 8 and above.

# Quick Install
1. Install the package via NuGet

```shell
dotnet add package BlazorSessionProvider
```

2. Import the library in your `Program.cs` file, and add the session service after created the builder

```cs
using BlazorSessionProvider;

var builder = WebApplication.CreateBuilder(args);
...
builder.Services.AddSessionProvider(config => {
  config.TimeDelay           = new TimeSpan(0, 0, 30);
  config.SessionExpiredUrl   = "/logout";
  config.SessionNotFoundUrl  = "/";
});
```

The session provider uses a configuration object:

- `TimeDelay` is the time interval for each session to be expired
- `SessionExpiredUrl` is the URL of the application to which it will redirect when the session has expired.
- `SessionNotFoundUrl` is the URL of the application to which it will redirect when the application does not find the session key.

Import the library and the injection dependency on `_Imports.cs`

```cs
@using BlazorSessionProvider.Sessions
@inject ISessionProvider SESS
```

Use "InteractiveServer" on each page you want to manage your sessions:

```cs
@rendermode InteractiveServer
```

Or, you can use the render mode in the `App.razor`:

```html
</head>
<body>
  <Routes @rendermode="InteractiveServer" />
  ...
</body>
```

See the [Wikia](https://github.com/oscardsoto/BlazorSessionProvider/wiki). for more details.

---
Made with ❤️ by Oscar D. Soto