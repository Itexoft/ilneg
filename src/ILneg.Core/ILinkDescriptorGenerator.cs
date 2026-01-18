// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

using System.Xml.Linq;

namespace Itexoft.ILneg;

public sealed class LinkDescriptorGenerator : IExportGenerator
{
    public const string GeneratorName = "ilink";

    public string Name => GeneratorName;

    public string Description => "ILLink descriptor for UnmanagedCallersOnly exports.";

    public void Write(string outputPath, ExportSet exports)
    {
        if (string.IsNullOrWhiteSpace(outputPath))
            throw new ArgumentException("Output path is required.", nameof(outputPath));

        var directory = Path.GetDirectoryName(outputPath);

        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var document = CreateDocument(exports);
        document.Save(outputPath);
    }

    public void Write(TextWriter writer, ExportSet exports)
    {
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));

        var document = CreateDocument(exports);
        document.Save(writer);
    }

    private static XDocument CreateDocument(ExportSet exports)
    {
        if (exports is null)
            throw new ArgumentNullException(nameof(exports));

        var assemblyElement = new XElement("assembly", new XAttribute("fullname", exports.AssemblyName));

        foreach (var (typeName, methods) in exports.MethodsByType)
        {
            var typeElement = new XElement("type", new XAttribute("fullname", typeName));

            foreach (var method in methods)
                typeElement.Add(new XElement("method", new XAttribute("name", method)));

            assemblyElement.Add(typeElement);
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("linker", assemblyElement));
    }
}
