﻿/*
 Copyright (c) 2010, Linden Research, Inc.
 Copyright (c) 2014, Joshua Bell

 Permission is hereby granted, free of charge, to any person obtaining a copy
 of this software and associated documentation files (the "Software"), to deal
 in the Software without restriction, including without limitation the rights
 to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the Software is
 furnished to do so, subject to the following conditions:

 The above copyright notice and this permission notice shall be included in
 all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 THE SOFTWARE.
 $/LicenseInfo$
 */

// Original can be found at:
//   https://bitbucket.org/lindenlab/llsd
// Modifications by Joshua Bell inexorabletash@gmail.com
//   https://github.com/inexorabletash/polyfill

// ES3/ES5 implementation of the Krhonos Typed Array Specification
//   Ref: http://www.khronos.org/registry/typedarray/specs/latest/
//   Date: 2011-02-01
//
// Variations:
//  * Allows typed_array.get/set() as alias for subscripts (typed_array[])
//  * Gradually migrating structure from Khronos spec to ES2015 spec
//  * slice() implemention from https://github.com/ttaubert/node-arraybuffer-slice/
//  * Base64 conversions from https://github.com/rrhett/typescript-base64-arraybuffer

namespace typed_arrays;

public class Uint8Array : TypedArray
{
    public Uint8Array(int length = 0) : base(length) { }

    public Uint8Array(IList<int> bytes) : base(bytes) { }
    public Uint8Array(ArrayBuffer source, int byteOffset = 0, int? length = null) :
        base(source, byteOffset, length)
    { }
    public Uint8Array(TypedArray source) : base(source) { }

    public override int BytesPerElement => 1;
    public override Func<int, IList<int>> Pack => value => Convert.PackU8(value);
    public override Func<IList<int>, int> Unpack => bytes => Convert.UnpackU8(bytes);
}
