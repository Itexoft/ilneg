# ilneg

`ilneg` is a small IL native exports generator. It scans a managed assembly and emits artifacts based on
`UnmanagedCallersOnly` exports. The output format is selected by the generator name (first argument).

## Usage

```bash
ilneg ilink path/to/YourAssembly.dll out.xml
ilneg ilink path/to/YourAssembly.dll -
ilneg --list-generators
```

### Generators

- `ilink` — emits an ILLink descriptor that keeps only `UnmanagedCallersOnly` entry points and their dependencies.

### Output format (ilink)

```xml
<linker>
  <assembly fullname="YourAssembly">
    <type fullname="Your.Namespace.Exports">
      <method name="Foo" />
    </type>
  </assembly>
</linker>
```

## Build

```bash
dotnet publish -c Release -r <rid> src/ILneg/ILneg.csproj
```

## Package layout

Native binaries are packed into:

```
runtimes/<rid>/native/ilneg[.exe]
```

## License

MPL-2.0-no-copyleft-exception. See `LICENSE.md`.
