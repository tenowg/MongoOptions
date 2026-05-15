
Please visit my [MongoOptions.Blazor](https://github.com/tenowg/MongoOptions.Blazor) Github, it will be a usuable project to add Razor components to manage your Config files

# MongoOptions.Core 🍃

A high-performance, resilient configuration provider for .NET 10 that uses MongoDB as a backing store with built-in memory caching and data validation.
Update your config files without reloading you project.

## 🚀 Features

* **Fluent Configuration**: Set up in seconds with a clean, readable API.
* **Resilient Caching**: Powered by `IMemoryCache` with "Stale-on-Failure" protection.
* **Keyed/Named Options**: Support for multiple configuration instances (e.g., Tenant-specific settings).
* **Data Validation**: Built-in support for Data Annotations to keep your DB clean.
* **Management API**: Full CRUD support for managing configs via code (perfect for Blazor Admin UIs).
* **Metadata Search**: Config files can be stored with metadata, and you can retrieve Keys based on metadata
* **Distributed Locking** Write Lock objects with custom Timeouts

## 📦 Installation

```bash
dotnet add package Tenowg.MongoOptions
```

## Documentation ##
For a good quickstart and further documentation please head to [MongoOptions Documentation](https://tenowg.github.io/MongoOptions/index.html).

## Building Locally

This project uses .NET 10.
You will need to pull [Tenowg.MongoOptions.Generator](https://github.com/tenowg/MongoOptions.Generator) and put it next to this project.
There is a good chance you will need to edit the .csproj file to fix some file paths.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

If you encounter any issues or have questions, please open an issue on GitHub.