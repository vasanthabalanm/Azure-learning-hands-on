# API Documentation: Scalar & ReDoc Guide

Starting with .NET 9, Swagger UI is no longer included in default templates. This guide covers two modern alternatives now implemented in your MsalDemo API.

---

## Quick Access URLs

After running the API (`dotnet run`), access documentation at:

| Tool | URL | Interactive Testing |
|------|-----|---------------------|
| **Swagger UI** | `https://localhost:7xxx/swagger` | ✅ Yes |
| **Scalar** | `https://localhost:7xxx/scalar/v1` | ✅ Yes |
| **ReDoc** | `https://localhost:7xxx/redoc` | ❌ No (read-only) |

---

## Scalar

### What is Scalar?

Scalar is a modern, interactive API documentation tool that provides a superior developer experience compared to traditional Swagger UI.

### Key Features

| Feature | Description |
|---------|-------------|
| **Dark Mode** | Built-in dark/light theme toggle |
| **Code Examples** | Auto-generated code snippets in 10+ languages |
| **Interactive Testing** | Try API calls directly from the documentation |
| **Search** | Powerful search across all endpoints |
| **Authentication** | Easy Bearer token input for protected APIs |
| **Modern UI** | Clean, responsive design |

### Theme Options

Scalar supports multiple themes. Current implementation uses `BluePlanet`:

```csharp
// Available themes in ScalarTheme enum:
ScalarTheme.Default
ScalarTheme.Alternate
ScalarTheme.Moon
ScalarTheme.Purple
ScalarTheme.Solarized
ScalarTheme.BluePlanet  // ← Currently used
ScalarTheme.Saturn
ScalarTheme.Kepler
ScalarTheme.Mars
ScalarTheme.DeepSpace
ScalarTheme.None
```

### Changing Default Code Language

The implementation defaults to C# HttpClient examples:

```csharp
.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient)
```

Other options include:

```csharp
// Languages (ScalarTarget)
ScalarTarget.CSharp, ScalarTarget.JavaScript, ScalarTarget.Python,
ScalarTarget.Java, ScalarTarget.Go, ScalarTarget.Ruby, ScalarTarget.PHP,
ScalarTarget.Shell, ScalarTarget.Swift, ScalarTarget.Kotlin

// HTTP Clients (ScalarClient) - varies by language
// C#: HttpClient, RestSharp
// JavaScript: Fetch, Axios, jQuery
// Python: Requests, HttpClient
// etc.
```

---

## ReDoc

### What is ReDoc?

ReDoc generates beautiful, clean API documentation with a focus on readability. It's ideal for sharing with external stakeholders or embedding in developer portals.

### Key Features

| Feature | Description |
|---------|-------------|
| **Three-Panel Layout** | Navigation, content, and code samples |
| **Deep Linking** | Direct links to any section |
| **Responsive Design** | Works on all devices |
| **Search** | Find endpoints quickly |
| **Print-Friendly** | Generate PDF documentation |
| **No Interactive Testing** | Documentation only (use Scalar/Swagger for testing) |

### Configuration Options

```csharp
app.UseReDoc(options =>
{
    options.SpecUrl = "/swagger/v1/swagger.json";  // OpenAPI spec location
    options.RoutePrefix = "redoc";                 // URL path
    options.DocumentTitle = "API Documentation";  // Browser tab title
    
    // Layout options
    options.ExpandResponses("200,201");    // Auto-expand these responses
    options.RequiredPropsFirst();          // Show required fields first
    options.PathInMiddlePanel();           // Show path in middle panel
    options.SortPropsAlphabetically();     // Alphabetize properties
    
    // UI options
    options.HideHostname();                // Hide base URL
    options.HideDownloadButton();          // Hide "Download" button
    options.HideLoading();                 // No loading spinner
    options.NativeScrollbars();            // Use native scrollbars
    options.NoAutoAuth();                  // Don't auto-fill auth
});
```

---

## Comparison: Swagger vs Scalar vs ReDoc

| Aspect | Swagger UI | Scalar | ReDoc |
|--------|------------|--------|-------|
| **Interactive Testing** | ✅ Yes | ✅ Yes | ❌ No |
| **Dark Mode** | ❌ No | ✅ Yes | ❌ No |
| **Code Generation** | ❌ No | ✅ Multi-language | ❌ No |
| **Modern UI** | ⚠️ Dated | ✅ Modern | ✅ Clean |
| **Search** | ⚠️ Basic | ✅ Powerful | ✅ Good |
| **Three-Panel Layout** | ❌ No | ✅ Yes | ✅ Yes |
| **Print/PDF Export** | ❌ No | ❌ No | ✅ Yes |
| **Deep Linking** | ⚠️ Limited | ✅ Yes | ✅ Yes |
| **Best For** | Quick testing | Developers | Documentation |

---

## When to Use Each

### Use Swagger UI When

- You need quick, no-frills API testing
- Working with legacy teams familiar with Swagger
- Minimal configuration needed

### Use Scalar When

- Building modern APIs with developer experience focus
- Need multi-language code examples
- Want dark mode and modern UI
- Primary use is interactive API exploration

### Use ReDoc When

- Creating public API documentation
- Sharing with external partners/customers
- Need print-friendly documentation
- Embedding in developer portals
- Documentation-first approach

---

## Implementation Details

### NuGet Packages

```xml
<!-- In your .csproj -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="Scalar.AspNetCore" Version="2.3.0" />
<PackageReference Include="Swashbuckle.AspNetCore.ReDoc" Version="10.2.3" />
```

### Required Using Statement

```csharp
using Scalar.AspNetCore;
```

### Minimal Setup

```csharp
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // OpenAPI JSON (required for all tools)
    app.UseSwagger();
    
    // Swagger UI
    app.UseSwaggerUI();
    
    // Scalar - Modern interactive docs
    app.MapScalarApiReference();
    
    // ReDoc - Beautiful static docs
    app.UseReDoc(c => c.SpecUrl = "/swagger/v1/swagger.json");
}
```

---

## Production Considerations

### Security

In production, you may want to:

1. **Disable documentation entirely**:
   ```csharp
   if (app.Environment.IsDevelopment())
   {
       // Only enable in development
   }
   ```

2. **Require authentication**:
   ```csharp
   app.MapScalarApiReference()
      .RequireAuthorization("AdminOnly");
   ```

3. **Use a separate documentation endpoint**:
   ```csharp
   // Expose only ReDoc for external users (no testing capability)
   if (app.Environment.IsProduction())
   {
       app.UseReDoc();  // Read-only documentation
   }
   ```

---

## Troubleshooting

### Scalar Not Loading?

1. Ensure `app.UseSwagger()` is called before `app.MapScalarApiReference()`
2. Check the OpenAPI route pattern matches your Swagger setup
3. Verify the package is installed: `dotnet list package`

### ReDoc Shows Empty Page?

1. Verify `options.SpecUrl` points to correct swagger.json path
2. Check browser console for CORS errors
3. Ensure Swagger is generating valid OpenAPI spec

### Code Examples Not Showing in Scalar?

- Ensure your API endpoints have proper documentation attributes:
  ```csharp
  [HttpGet]
  [ProducesResponseType(typeof(UserProfile), StatusCodes.Status200OK)]
  [ProducesResponseType(StatusCodes.Status401Unauthorized)]
  public IActionResult GetUser() { }
  ```

---

## Next Learning Steps

Now that you have API documentation set up, consider learning:

1. **XML Documentation** - Add `///` comments to your controllers for richer docs
2. **OpenAPI Annotations** - Use `[SwaggerOperation]` attributes for descriptions
3. **API Versioning** - Document multiple API versions
4. **Custom Themes** - Customize Scalar/ReDoc appearance
5. **CI/CD Integration** - Auto-generate OpenAPI specs in pipelines

---

## Resources

- [Scalar Documentation](https://scalar.com/docs)
- [ReDoc GitHub](https://github.com/Redocly/redoc)
- [OpenAPI Specification](https://swagger.io/specification/)
- [Swashbuckle Documentation](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
