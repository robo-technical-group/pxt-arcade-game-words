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
using System.Numerics;

namespace typed_arrays;

public abstract class TypedArray
{
    protected ArrayBuffer _buffer;
    protected int _byteLength;
    protected int _byteOffset;
    protected int _length;

    public TypedArray(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _length = length;
        _byteOffset = 0;
        _byteLength = _length * BytesPerElement;
        _buffer = new ArrayBuffer(_byteLength);
    }

    public TypedArray(IList<int> source)
    {
        int byteLength = source.Count * BytesPerElement;
        _buffer = new ArrayBuffer(byteLength);
        _byteLength = byteLength;
        _byteOffset = 0;
        _length = source.Count;

        for (int i = 0; i < _length; i++)
        {
            Set(i, source[i]);
        }
    }

    public TypedArray(ArrayBuffer source, int byteOffset = 0, int? length = null)
    {
        if (byteOffset > source.ByteLength)
        {
            throw new ArgumentException("Byte offset out of range.", nameof(byteOffset));
        }

        /**
         * The given byteOffset must be a multiple of the
         * element size of the specific type.
         */
        if (byteOffset % BytesPerElement != 0)
        {
            throw new ArgumentException("Buffer length minus the byteOffset is not a multiple of the element size.",
                nameof(byteOffset));
        }

        int byteLength;
        if (!length.HasValue)
        {
            byteLength = source.ByteLength - byteOffset;
            if (byteLength % BytesPerElement != 0)
            {
                throw new ArgumentException("Length of buffer minus byteOffset not a multiple of the element size.",
                    nameof(byteOffset));
            }
            length = byteLength / BytesPerElement;
        }
        else
        {
            byteLength = length.Value * BytesPerElement;
        }

        if ((byteOffset + byteLength) > source.ByteLength)
        {
            throw new ArgumentException("byteOffset and length reference are an area beyond the end of the buffer.");
        }

        _buffer = source;
        _byteLength = byteLength;
        _byteOffset = byteOffset;
        _length = length.Value;
    }

    public TypedArray(TypedArray source)
    {
        int byteLength = source._length * BytesPerElement;
        _buffer = new ArrayBuffer(byteLength);
        _byteLength = byteLength;
        _byteOffset = 0;
        _length = source._length;

        for (int i = 0; i < _length; i++)
        {
            Set(i, source.Get(i));
        }
    }

    /**
     * Public properties
     */
    public ArrayBuffer Buffer { get { return _buffer; } }
    public int ByteLength { get { return _byteLength; } }
    public int ByteOffset { get { return _byteOffset; } }
    public abstract int BytesPerElement { get; }
    public int Length {  get { return _length; } }
    public abstract Func<int, IList<int>> Pack { get; }
    public abstract Func<IList<int>, int> Unpack { get; }

    /**
     * Public methods
     */

    public int Get(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);

        IList<int> bytes = [];
        for (
            int i = 0, o = _byteOffset + index * BytesPerElement;
            i < BytesPerElement;
            i++, o++
        )
        {
            bytes.Add(_buffer.Bytes[o]);
        }
        return Unpack(bytes);
    }

    public void Set(int index, int value)
    {
        if (index >= _length) { return; }

        IList<int> bytes = Pack(value);
        for (
            int i = 0, o = _byteOffset + index * BytesPerElement;
            i < BytesPerElement;
            i++, o++
        )
        {
            _buffer.Bytes[o] = bytes[i];
        }
    }

    public void Set(IList<int> source, int offset = 0)
    {
        int len = source.Count;
        if (offset + len > _length)
        {
            throw new ArgumentOutOfRangeException(nameof(source));
        }

        for (int i = 0; i < len; i++)
        {
            Set(offset + i, source[i]);
        }
    }

    public void Set(TypedArray source, int offset = 0)
    {
        if (offset + source.Length > _length)
        { 
            throw new ArgumentOutOfRangeException(nameof(source)); 
        }

        int byteOffset = _byteOffset + offset * BytesPerElement;
        int byteLength = source._length * BytesPerElement;

        if (source.Buffer == _buffer)
        {
            IList<int> tmp = [];
            for (
                int i = 0, s = source.ByteOffset;
                i < byteLength;
                i++, s++
            )
            {
                tmp.Add(source.Buffer.Bytes[s]);
            }
            for (
                int i = 0, d = byteOffset;
                i < byteLength;
                i++, d++
            )
            {
                _buffer.Bytes[d] = tmp[i];
            }
        }
        else
        {
            for (
                int i = 0, s = source.ByteOffset, d = byteOffset;
                i < byteLength;
                i++, s++, d++
            )
            {
                _buffer.Bytes[d] = source.Buffer.Bytes[s];
            }
        }
    }

    public IList<int> ToList() { return _buffer.ToList(); }
}
