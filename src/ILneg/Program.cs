// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

using Itexoft.ILneg.Properties;

namespace Itexoft.ILneg;

internal static class Program
{
    private const string toolName = "ilneg";

    public static int Main(string[] args)
    {
        var options = new Options();

        if (!TryParseArguments(args, options, out var error, out var showHelp, out var showVersion, out var showGenerators))
        {
            if (!string.IsNullOrEmpty(error))
                WriteError(error);

            PrintUsage(Console.Error);

            return ExitCodes.Usage;
        }

        if (showHelp)
        {
            PrintUsage(Console.Out);

            return ExitCodes.Success;
        }

        if (showVersion)
        {
            PrintVersion(Console.Out);

            return ExitCodes.Success;
        }

        if (showGenerators)
        {
            PrintGenerators(Console.Out);

            return ExitCodes.Success;
        }

        try
        {
            if (!ExportGeneratorRegistry.TryGet(options.GeneratorName!, out var generator))
            {
                WriteError($"Unknown generator: {options.GeneratorName}");

                return ExitCodes.Usage;
            }

            var exports = ExportCollector.Collect(options.AssemblyPath!);

            if (exports.MethodCount == 0)
            {
                WriteError("No UnmanagedCallersOnly methods found.");

                return ExitCodes.ProcessingError;
            }

            if (options.OutputToStdout)
            {
                generator.Write(Console.Out, exports);

                return ExitCodes.Success;
            }

            generator.Write(options.OutputPath!, exports);

            return ExitCodes.Success;
        }
        catch (Exception ex)
        {
            WriteError(ex.Message);

            return ExitCodes.ProcessingError;
        }
    }

    private static bool TryParseArguments(
        string[] args,
        Options options,
        out string error,
        out bool showHelp,
        out bool showVersion,
        out bool showGenerators)
    {
        error = string.Empty;
        showHelp = false;
        showVersion = false;
        showGenerators = false;

        if (args.Length == 0)
        {
            error = "No arguments provided.";

            return false;
        }

        var firstArg = args[0];

        if (firstArg is "--help" or "-h" or "-?")
        {
            showHelp = true;

            return true;
        }

        if (firstArg is "--version")
        {
            showVersion = true;

            return true;
        }

        if (firstArg is "--list-generators")
        {
            showGenerators = true;

            return true;
        }

        if (firstArg.StartsWith('-'))
        {
            error = "Generator name must be the first argument.";

            return false;
        }

        options.GeneratorName = firstArg;

        for (var i = 1; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg is "--help" or "-h" or "-?")
            {
                showHelp = true;

                return true;
            }

            if (arg is "--version")
            {
                showVersion = true;

                return true;
            }

            if (arg is "--list-generators")
            {
                showGenerators = true;

                return true;
            }

            if (arg.StartsWith('-'))
            {
                error = $"Unknown option: {arg}";

                return false;
            }

            if (string.IsNullOrEmpty(options.AssemblyPath))
            {
                options.AssemblyPath = arg;

                continue;
            }

            if (string.IsNullOrEmpty(options.OutputPath))
            {
                options.OutputPath = arg;

                continue;
            }

            error = "Too many positional arguments.";

            return false;
        }

        if (string.IsNullOrWhiteSpace(options.AssemblyPath))
        {
            error = "Assembly path is required.";

            return false;
        }

        if (string.IsNullOrWhiteSpace(options.OutputPath))
        {
            error = "Output path is required.";

            return false;
        }

        options.OutputToStdout = options.OutputPath == "-";

        return true;
    }

    private static void PrintUsage(TextWriter writer)
    {
        writer.WriteLine("ilneg - IL native exports generator");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  ilneg <generator> <assembly> <output>");
        writer.WriteLine("  ilneg --list-generators");
        writer.WriteLine();
        writer.WriteLine("Options:");
        writer.WriteLine("  <generator>           Generator name (first argument).");
        writer.WriteLine("  <assembly>            Input assembly path.");
        writer.WriteLine("  <output>              Output file path. Use '-' for stdout.");
        writer.WriteLine("  --list-generators      List supported generators.");
        writer.WriteLine("  --help, -h             Show this help.");
        writer.WriteLine("  --version              Show version.");
    }

    private static void PrintGenerators(TextWriter writer)
    {
        writer.WriteLine("Available generators:");

        foreach (var pair in ExportGeneratorRegistry.Generators.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            writer.WriteLine($"  {pair.Key} - {pair.Value.Description}");
    }

    private static void PrintVersion(TextWriter writer) =>
        writer.WriteLine(AssemblyMetadata.ProductVersion is null ? toolName : $"{toolName} {AssemblyMetadata.ProductVersion}");

    private static void WriteError(string message) => Console.Error.WriteLine($"{toolName}: error: {message}");

    private sealed class Options
    {
        public string? GeneratorName { get; set; }
        public string? AssemblyPath { get; set; }
        public string? OutputPath { get; set; }
        public bool OutputToStdout { get; set; }
    }

    private static class ExitCodes
    {
        public const int Success = 0;
        public const int Usage = 1;
        public const int ProcessingError = 2;
    }
}
