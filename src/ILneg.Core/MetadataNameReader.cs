// Copyright (c) 2011-2026 Denis Kudelin
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.

using System.Reflection.Metadata;

namespace Itexoft.ILneg;

internal static class MetadataNameReader
{
    private const string unmanagedCallersOnlyFullName = "System.Runtime.InteropServices.UnmanagedCallersOnlyAttribute";

    public static bool HasUnmanagedCallersOnly(MetadataReader reader, MethodDefinition methodDef)
    {
        foreach (var attrHandle in methodDef.GetCustomAttributes())
        {
            var attr = reader.GetCustomAttribute(attrHandle);
            var fullName = GetAttributeTypeFullName(reader, attr);

            if (fullName == unmanagedCallersOnlyFullName)
                return true;
        }

        return false;
    }

    public static string GetTypeFullName(MetadataReader reader, EntityHandle handle) => handle.Kind switch
    {
        HandleKind.TypeReference => GetTypeFullName(reader, reader.GetTypeReference((TypeReferenceHandle)handle)),
        HandleKind.TypeDefinition => GetTypeFullName(reader, (TypeDefinitionHandle)handle),
        _ => string.Empty,
    };

    private static string GetAttributeTypeFullName(MetadataReader reader, CustomAttribute attr)
    {
        var ctor = attr.Constructor;

        return ctor.Kind switch
        {
            HandleKind.MemberReference => GetTypeFullName(reader, reader.GetMemberReference((MemberReferenceHandle)ctor).Parent),
            HandleKind.MethodDefinition => GetTypeFullName(reader, reader.GetMethodDefinition((MethodDefinitionHandle)ctor).GetDeclaringType()),
            _ => string.Empty,
        };
    }

    private static string GetTypeFullName(MetadataReader reader, TypeReference typeRef)
    {
        var name = reader.GetString(typeRef.Name);
        var ns = reader.GetString(typeRef.Namespace);

        if (typeRef.ResolutionScope.Kind == HandleKind.TypeReference)
        {
            var parent = reader.GetTypeReference((TypeReferenceHandle)typeRef.ResolutionScope);

            return GetTypeFullName(reader, parent) + "/" + name;
        }

        if (!string.IsNullOrEmpty(ns))
            return ns + "." + name;

        return name;
    }

    private static string GetTypeFullName(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var typeDef = reader.GetTypeDefinition(handle);
        var name = reader.GetString(typeDef.Name);
        var ns = reader.GetString(typeDef.Namespace);
        var declaring = typeDef.GetDeclaringType();

        if (!declaring.IsNil)
            return GetTypeFullName(reader, declaring) + "/" + name;

        if (!string.IsNullOrEmpty(ns))
            return ns + "." + name;

        return name;
    }
}
