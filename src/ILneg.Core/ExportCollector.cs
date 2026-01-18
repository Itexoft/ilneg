// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace Itexoft.ILneg;

public static class ExportCollector
{
    public static ExportSet Collect(string assemblyPath)
    {
        if (string.IsNullOrWhiteSpace(assemblyPath))
            throw new ArgumentException("Assembly path is required.", nameof(assemblyPath));

        if (!File.Exists(assemblyPath))
            throw new FileNotFoundException("Assembly not found.", assemblyPath);

        using var stream = File.OpenRead(assemblyPath);
        using var peReader = new PEReader(stream);
        var reader = peReader.GetMetadataReader();

        var assemblyName = reader.GetAssemblyDefinition().GetAssemblyName().Name;

        if (string.IsNullOrWhiteSpace(assemblyName))
            assemblyName = Path.GetFileNameWithoutExtension(assemblyPath);

        var methodsByType = new SortedDictionary<string, SortedSet<string>>(StringComparer.Ordinal);

        foreach (var typeHandle in reader.TypeDefinitions)
        {
            var typeDef = reader.GetTypeDefinition(typeHandle);
            var typeName = MetadataNameReader.GetTypeFullName(reader, typeHandle);

            if (typeName == "<Module>")
                continue;

            foreach (var methodHandle in typeDef.GetMethods())
            {
                var methodDef = reader.GetMethodDefinition(methodHandle);

                if (!MetadataNameReader.HasUnmanagedCallersOnly(reader, methodDef))
                    continue;

                var methodName = reader.GetString(methodDef.Name);

                if (!methodsByType.TryGetValue(typeName, out var methods))
                {
                    methods = new SortedSet<string>(StringComparer.Ordinal);
                    methodsByType[typeName] = methods;
                }

                methods.Add(methodName);
            }
        }

        return new ExportSet(assemblyName, methodsByType);
    }
}
