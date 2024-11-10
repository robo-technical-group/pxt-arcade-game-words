/*
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
public class ArrayBuffer
{
    protected readonly int B64_PARTITION_SIZE = 80;
    protected int _byteLength;
    protected IList<uint> _bytes;

    public ArrayBuffer(int length = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _byteLength = length;
        _bytes = [];
        for (int i = 0; i < length; i++)
        {
            _bytes.Add(0);
        }
    }

    /**
     * Public properties
     */
    public int ByteLength { get { return _byteLength; } }
    public IList<uint> Bytes { get { return _bytes; } }

    /**
     * Public methods
     */
    public ArrayBuffer Slice(int from, int? to = null)
    {
        int length = _byteLength;
        int begin = Clamp(from, length);
        int end = length;

        if (to != null)
        {
            end = Clamp(to.Value, length);
        }

        if (begin > end)
        {
            return new ArrayBuffer(0);
        }

        int num = end - begin;
        ArrayBuffer target = new(num);
        TypedArray<byte> targetArray = new(target);
        TypedArray<byte> sourceArray = new(this, begin, num);
        targetArray.Set(sourceArray);

        return target;
    }

    public IEnumerable<string> ToBase64StringSet()
    {
        TypedArray<byte> array = new(this);
        string r = Convert.ToBase64String(array.ToArray());
        return r.Partition(B64_PARTITION_SIZE);
    }

    public IList<uint> ToList() { return [.. _bytes]; }

    /**
     * Protected methods
     */
    protected static int Clamp(int val, int length)
    {
        if (val < 0)
        {
            return Math.Max(val + length, 0);
        }
        return Math.Min(val, length);
    }
}
