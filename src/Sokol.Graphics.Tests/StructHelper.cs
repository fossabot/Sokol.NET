﻿// Copyright (c) Lucas Girouard-Stranks. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Sokol.Graphics.Tests
{
    [SuppressMessage("ReSharper", "SA1600", Justification = "Tests")]
    public static class StructHelper
    {
        private static readonly AssemblyName StructGeneratorAssemblyName = new AssemblyName("GeneratedStructsAssembly");
        private static readonly ModuleBuilder ModuleBuilder;
        private static readonly Dictionary<Type, Type> StructGeneratedTypesByStructTypes = new Dictionary<Type, Type>();

        static StructHelper()
        {
            var assemblyBuilder =
                AssemblyBuilder.DefineDynamicAssembly(StructGeneratorAssemblyName, AssemblyBuilderAccess.Run);
            ModuleBuilder = assemblyBuilder.DefineDynamicModule(StructGeneratorAssemblyName.Name!);
        }

        public static Type CStructType(this Type structType)
        {
            if (StructGeneratedTypesByStructTypes.TryGetValue(structType, out var generatedStructType))
            {
                return generatedStructType;
            }

            if (structType.IsPointer)
            {
                var pointerType = typeof(IntPtr);
                StructGeneratedTypesByStructTypes[structType] = pointerType;
                return pointerType;
            }

            if (structType.IsEnum)
            {
                var baseType = Enum.GetUnderlyingType(structType);
                StructGeneratedTypesByStructTypes[structType] = baseType;
                return baseType;
            }

            if (structType.FullName?.StartsWith("System.") ?? false)
            {
                StructGeneratedTypesByStructTypes[structType] = structType;
                return structType;
            }

            if (structType.Name.StartsWith("<", StringComparison.Ordinal))
            {
                StructGeneratedTypesByStructTypes[structType] = structType;
                return structType;
            }

            var generatedStructName = $"{structType.Name}<C>";

            TypeBuilder generatedStructTypeBuilder;
            try
            {
                generatedStructTypeBuilder = ModuleBuilder.DefineType(
                    generatedStructName,
                    TypeAttributes.Public | TypeAttributes.Sealed | TypeAttributes.SequentialLayout | TypeAttributes.Serializable | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit,
                    typeof(ValueType));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            var fields = structType.GetTypeInfo().DeclaredFields;
            var currentFieldPosition = -1;

            foreach (var field in fields)
            {
                if (field.IsStatic)
                {
                    continue;
                }

                var fieldType = field.FieldType;
                var structTypeC = CStructType(fieldType);

                var fieldOffsetAttribute = field.GetCustomAttribute<FieldOffsetAttribute>();
                if (fieldOffsetAttribute?.Value == currentFieldPosition)
                {
                    continue;
                }

                if (fieldOffsetAttribute != null)
                {
                    currentFieldPosition = fieldOffsetAttribute.Value;
                }

                generatedStructTypeBuilder.DefineField(field.Name, structTypeC, FieldAttributes.Public);
            }

            generatedStructType = generatedStructTypeBuilder.CreateType();
            StructGeneratedTypesByStructTypes[structType] = generatedStructType!;

            return generatedStructType!;
        }
    }
}
