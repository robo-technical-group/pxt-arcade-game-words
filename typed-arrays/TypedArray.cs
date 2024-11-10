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
using System.Diagnostics;
using System.Numerics;
namespace typed_arrays;
public class TypedArray<T> : IEnumerable<T> where T : IBinaryInteger<T>
{
    protected ArrayBuffer _buffer;
    protected int _byteLength;
    protected int _byteOffset;
    protected int _length;

    public TypedArray(int length = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _length = length;
        _byteOffset = 0;
        _byteLength = length * BytesPerElement;
        _buffer = new ArrayBuffer(_byteLength);
    }

    public TypedArray(IList<T> source)
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
        ArgumentOutOfRangeException.ThrowIfNegative(byteOffset);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(byteOffset, source.ByteLength);

        /**
         * The given byteOffset must be a multiple of the
         * element size of the specific type.
         */
        if (byteOffset % BytesPerElement != 0)
        {
            throw new ArgumentOutOfRangeException("Buffer length minus the byteOffset is not a multiple of the element size.",
                nameof(byteOffset));
        }

        int byteLength;
        if (!length.HasValue)
        {
            byteLength = source.ByteLength - byteOffset;
            if (byteLength % BytesPerElement != 0)
            {
                throw new ArgumentOutOfRangeException("Length of buffer minus byteOffset not a multiple of the element size.",
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
            throw new ArgumentOutOfRangeException("byteOffset and length reference are an area beyond the end of the buffer.");
        }

        _buffer = source;
        _byteLength = byteLength;
        _byteOffset = byteOffset;
        _length = length.Value;
    }

    public static TypedArray<T> FromBase64StringSet(IEnumerable<string> strings)
    {
        string s = String.Join(string.Empty, strings);
        // Debug.WriteLine(s);
        TypedArray<byte> b = new(Convert.FromBase64String(s));
        return FromTypedArray(b);
    }

    public static TypedArray<T> FromTypedArray<U>(TypedArray<U> source) where U : IBinaryInteger<U>
    {
        TypedArray<T> r = new(source._length);
        for (int i = 0; i < r._length; i++)
        {
            r.Set(i, source.Get(i));
        }
        return r;
    }

    /**
     * Public properties.
     */

    public T this[int index]
    {
        get { return Get(index); }
        set { Set(index, value); }
    }

    public ArrayBuffer Buffer { get { return _buffer; } }
    public int ByteLength { get { return _byteLength; } }
    public int ByteOffset { get { return _byteOffset; } }
    public int BytesPerElement
    {
        get
        {
            return T.Zero switch
            {
                sbyte or byte => 1,
                Int16 or UInt16 => 2,
                int or UInt32 => 4,
                _ => throw new NotImplementedException(),
            };
        }
    }
    public int Length { get { return _length; } }

    /**
     * Public methods.
     */

    public T Get(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);
        IList<uint> bytes = [];
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

    public IList<uint> Pack(T value)
    {
        return value switch
        {
            sbyte n => [(uint)(n & 0xff),],
            byte n => [(uint)(n & 0xff),],
            Int16 n => [(uint)(n & 0xff), (uint)((n >> 8) & 0xff),],
            UInt16 n => [(uint)(n & 0xff), (uint)((n >> 8) & 0xff),],
            int n => [(uint)(n & 0xff), (uint)((n >> 8) & 0xff), (uint)((n >> 16) & 0xff), (uint)((n >> 24) & 0xff),],
            uint n => [n & 0xff, (n >> 8) & 0xff, (n >> 16) & 0xff, (n >> 24) & 0xff,],
            _ => throw new NotImplementedException()
        };
    }

    public void Set<U>(int index, U value) where U : IBinaryInteger<U>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);
        T v = T.CreateTruncating(value);
        IList<uint> bytes = Pack(v);
        for (
            int i = 0, o = _byteOffset + index * BytesPerElement;
            i < BytesPerElement;
            i++, o++
        )
        {
            _buffer.Bytes[o] = bytes[i];
        }
    }

    public void Set(IList<T> source, int offset = 0)
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

    public void Set<U>(TypedArray<U> source, int offset = 0) where U : IBinaryInteger<U>
    {
        if (offset + source.Length > _length)
        {
            throw new ArgumentOutOfRangeException(nameof(source));
        }

        int byteOffset = _byteOffset + offset * BytesPerElement;
        int byteLength = source.Length * BytesPerElement;

        if (source.Buffer == _buffer)
        {
            IList<uint> tmp = [];
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

    public T Unpack(IList<uint> bytes)
    {
        return T.Zero switch
        {
            sbyte => AsSigned(bytes[0], 8),
            byte => AsUnsigned(bytes[0], 8),
            Int16 => AsSigned(bytes[1] << 8 | bytes[0], 16),
            UInt16 => AsUnsigned(bytes[1] << 8 | bytes[0], 16),
            int => AsSigned(bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0], 32),
            uint => AsUnsigned(bytes[3] << 24 | bytes[2] << 16 | bytes[1] << 8 | bytes[0], 32),
            _ => throw new NotImplementedException()
        };
    }

    /**
     * Protected methods.
     */

    protected static T AsSigned(uint value, int bits)
    {
        int s = 32 - bits;
        return T.CreateTruncating((value << s) >> s);
    }

    protected static T AsUnsigned(uint value, int bits)
    {
        int s = 32 - bits;
        return T.CreateTruncating((value << s) >>> s);
    }

    #region Enumerator<T>
    public IEnumerator<T> GetEnumerator()
    {
        return new CustomEnum(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    protected class CustomEnum(TypedArray<T> array) : IEnumerator<T>
    {
        private int _index = -1;

        public T Current => array.Get(_index);

        object System.Collections.IEnumerator.Current => Current;

        public bool MoveNext()
        {
            _index++;
            return _index < array.Length;
        }

        public void Reset()
        {
            _index = -1;
        }

        public void Dispose() { }
    }
    #endregion
}
