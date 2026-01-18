// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

namespace Itexoft.ILneg;

public static class ExportGeneratorRegistry
{
    public const string DefaultGenerator = LinkDescriptorGenerator.GeneratorName;

    public static IReadOnlyDictionary<string, IExportGenerator> Generators { get; } =
        new Dictionary<string, IExportGenerator>(StringComparer.OrdinalIgnoreCase)
        {
            [LinkDescriptorGenerator.GeneratorName] = new LinkDescriptorGenerator(),
        };

    public static bool TryGet(string name, out IExportGenerator generator)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            generator = null!;

            return false;
        }

        return Generators.TryGetValue(name, out generator!);
    }
}
