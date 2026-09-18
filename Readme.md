# Importing Files Demo API + Angular UI

Brief instructions to run the solution (server + client) and how to expose Swagger.

## Prerequisites
- .NET 10 SDK installed
- Node.js and npm
- (Optional) Trust the .NET dev certificate for HTTPS: `dotnet dev-certs https --trust`

## Run the API (server)
1. From the solution root you can run the server project directly:
   - dotnet restore
   - dotnet build
   - dotnet run --project ImportingFilesDemoAPIAngularUI.Server
   or open the solution in Visual Studio and start the Server profile.

2. Default dev ports used by this project (check ImportingFilesDemoAPIAngularUI.Server/Properties/launchSettings.json):
   - HTTPS API port: https://localhost:7161
   - HTTP API port: http://localhost:5109

## Run the client (Angular SPA)
1. Change to the client folder and install dependencies:
   - cd importingfilesdemoapiangularui.client
   - npm install

2. Start the dev server (on Windows `npm start` will run the appropriate script):
   - npm start

3. The SPA dev server runs on port 63874 by default: https://localhost:63874

## Accessing Swagger
- API Swagger UI (served by the API project):
  - https://localhost:7161/swagger

- To access Swagger through the SPA dev server (so the URL is https://localhost:63874/swagger) make sure the client dev server proxies `/swagger` and the OpenAPI JSON (`/openapi`) to the API. This project includes `src/proxy.conf.js` and the client package.json start scripts already reference the proxy, but verify:
  - importingfilesdemoapiangularui.client/src/proxy.conf.js contains `/swagger` and `/openapi` in its context array
  - importingfilesdemoapiangularui.client/package.json start scripts include `--proxy-config src/proxy.conf.js`
  - restart the SPA dev server after changes

## Adding or enabling Swagger in the API project
1. Add the package (if not present):
   - Microsoft.AspNetCore.OpenApi (use a version that targets .NET 10)

2. Register OpenAPI services and middleware in Program.cs:

```csharp
// Register the OpenAPI generator
builder.Services.AddOpenApi();

// Map and expose the OpenAPI document and Swagger UI (example below shows always-on; remove the `if` check to expose in non-development)
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
	options.SwaggerEndpoint("/openapi/v1.json", "ImportingFilesDemoAPIAngularUI v1");
	options.RoutePrefix = "swagger";
});
```

3. Notes:
   - By default the template in this repo registers MapOpenApi() and UseSwaggerUI(...) only when the environment is Development. If you want Swagger available in other environments, move those calls out of the `if (app.Environment.IsDevelopment())` block.
   - The OpenAPI generator used by the ASP.NET templates exposes the document at `/openapi/v1.json` and the UI at `/swagger`.

## Troubleshooting
- If navigating to `https://localhost:63874/swagger` shows the Angular router error (NG04002), your SPA dev server is not proxying that path and is serving index.html instead. Restart the client dev server with the proxy enabled and confirm proxy.conf.js includes `/swagger` and `/openapi`.
- If the browser blocks requests due to certificate issues, either trust the dev certificate (see prerequisites) or set `secure: false` in the proxy config for local development (the client proxy file in this repo already uses `secure: false`).

If you want, I can add or update the proxy and package.json scripts for you, or move Swagger out of the Development-only block in Program.cs — tell me which change you prefer.
