// Copyright (c) Lucas Girouard-Stranks. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;
using Metal;
using ObjCRuntime;
using static ObjCRuntime.Messaging;

namespace CoreAnimation
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct CAMetalDrawable
    {
        public readonly IntPtr Handle;

        public MTLTexture texture => t_objc_msgSend<MTLTexture>(Handle, sel_texture);

        public static implicit operator IntPtr(CAMetalDrawable value)
        {
            return value.Handle;
        }

        private static readonly Selector sel_texture = "texture";
    }
}
