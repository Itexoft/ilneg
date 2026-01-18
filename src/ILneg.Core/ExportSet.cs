// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

namespace Itexoft.ILneg;

public sealed class ExportSet
{
    private readonly SortedDictionary<string, IReadOnlyCollection<string>> methodsByType;

    public ExportSet(string assemblyName, SortedDictionary<string, SortedSet<string>> methodsByType)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
            throw new ArgumentException("Assembly name is required.", nameof(assemblyName));

        if (methodsByType is null)
            throw new ArgumentNullException(nameof(methodsByType));

        this.AssemblyName = assemblyName;
        this.methodsByType = new SortedDictionary<string, IReadOnlyCollection<string>>(StringComparer.Ordinal);

        var methodCount = 0;

        foreach (var (typeName, methods) in methodsByType)
        {
            if (methods.Count == 0)
                continue;

            this.methodsByType[typeName] = methods;
            methodCount += methods.Count;
        }

        this.MethodCount = methodCount;
    }

    public string AssemblyName { get; }

    public IReadOnlyDictionary<string, IReadOnlyCollection<string>> MethodsByType => this.methodsByType;

    public int MethodCount { get; }
}
