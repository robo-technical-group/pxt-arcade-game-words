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
public class DataView
{
    protected ArrayBuffer _buffer;
    protected int _byteLength;
    protected int _byteOffset;

    public DataView(
        ArrayBuffer buffer,
        int byteOffset = 0,
        int? byteLength = null
    )
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteOffset);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(byteOffset, buffer.ByteLength);

        if (byteLength is null)
        {
            byteLength = buffer.ByteLength - byteOffset;
        }
        else
        {
            ArgumentOutOfRangeException.ThrowIfNegative(byteLength.Value);
        }

        if ((byteOffset + byteLength) > buffer.ByteLength)
        {
            throw new ArgumentOutOfRangeException(nameof(byteOffset), 
                "byteOffset and length reference an area beyond the end of the buffer.");
        }

        _buffer = buffer;
        _byteLength = byteLength.Value;
        _byteOffset = byteOffset;
    }

    /**
     * Public properties.
     */
    public ArrayBuffer Buffer {  get { return _buffer; } }
    public int ByteLength { get { return _byteLength; } }
    public int ByteOffset { get { return _byteOffset; } }
    public static bool IS_BIG_ENDIAN
    {
        get
        {
            TypedArray<ushort> u16 = new([0x1234,]);
            TypedArray<byte> u8 = new(u16.Buffer);
            return u8.Get(0) == 0x12;
        }
    }

    /**
     * Public methods.
     */
    /**
     * <summary>
     * Gets the Float32 (float) value at the specified byte offset from the start of the view. There is
     * no alignment constraint; multi-byte values may be fetched from any offset.
     * </summary>
     * <param name="byteOffset">The place in the buffer at which the value should be retrieved.</param>
     * <param name="IsLittleEndian">If false, a big-endian value should be read.</param>
     */
    public float GetFloat32(int byteOffset, bool IsLittleEndian = false)
    {
        throw new NotImplementedException();
    }

    /**
     * <summary>
     * Gets the Float64 (double) value at the specified byte offset from the start of the view. There is
     * no alignment constraint; multi-byte values may be fetched from any offset.
     * </summary>
     * <param name="byteOffset">The place in the buffer at which the value should be retrieved.</param>
     * <param name="IsLittleEndian">If false, a big-endian value should be read.</param>
     */
    public double GetFloat64(int byteOffset, bool IsLittleEndian = false)
    {
        throw new NotImplementedException();
    }

    /**
     * <summary>
     * Gets the Int8 (sbyte) value at the specified byte offset from the start of the view. There is
     * no alignment constraint; multi-byte values may be fetched from any offset.
     * </summary>
     * <param name="byteOffset">The place in the buffer at which the value should be retrieved.</param>
     * <param name="IsLittleEndian">If false, a big-endian value should be read.</param>
     */
    public sbyte GetInt8(int byteOffset)
    {
        TypedArray<sbyte> r = new();
        return Getter(r, byteOffset);
    }

    /**
     * <summary>
     * Gets the Int16 (short) value at the specified byte offset from the start of the view. There is
     * no alignment constraint; multi-byte values may be fetched from any offset.
     * </summary>
     * <param name="byteOffset">The place in the buffer at which the value should be retrieved.</param>
     * <param name="IsLittleEndian">If false, a big-endian value should be read.</param>
     */
    public short GetInt16(int byteOffset, bool IsLittleEndian = false)
    {
        TypedArray<Int16> r = new();
        return Getter(r, byteOffset, IsLittleEndian);
    }

    /**
     * <summary>
     * Gets the Int32 (int) value at the specified byte offset from the start of the view. There is
     * no alignment constraint; multi-byte values may be fetched from any offset.
     * </summary>
     * <param name="byteOffset">The place in the buffer at which the value should be retrieved.</param>
     * <param name="IsLittleEndian">If false, a big-endian value should be read.</param>
     */
    public int GetInt32(int byteOffset, bool IsLittleEndian = false)
    {
        TypedArray<int> r = new();
        return Getter(r, byteOffset, IsLittleEndian);
    }

    /**
     * <summary>
     * Gets the UInt8 (byte) value at the specified byte offset from the start of the view. There is
     * no alignment constraint; multi-byte values may be fetched from any offset.
     * </summary>
     * <param name="byteOffset">The place in the buffer at which the value should be retrieved.</param>
     * <param name="IsLittleEndian">If false, a big-endian value should be read.</param>
     */
    public byte GetUInt8(int byteOffset)
    {
        TypedArray<byte> r = new();
        return Getter(r, byteOffset);
    }

    /**
     * <summary>
     * Gets the UInt16 (ushort) value at the specified byte offset from the start of the view. There is
     * no alignment constraint; multi-byte values may be fetched from any offset.
     * </summary>
     * <param name="byteOffset">The place in the buffer at which the value should be retrieved.</param>
     * <param name="IsLittleEndian">If false, a big-endian value should be read.</param>
     */
    public ushort GetUInt16(int byteOffset, bool IsLittleEndian = false)
    {
        TypedArray<UInt16> r = new();
        return Getter(r, byteOffset, IsLittleEndian);
    }

    /**
     * <summary>
     * Gets the UInt32 (uint) value at the specified byte offset from the start of the view. There is
     * no alignment constraint; multi-byte values may be fetched from any offset.
     * </summary>
     * <param name="byteOffset">The place in the buffer at which the value should be retrieved.</param>
     * <param name="IsLittleEndian">If false, a big-endian value should be read.</param>
     */
    public uint GetUInt32(int byteOffset, bool IsLittleEndian = false)
    {
        TypedArray<uint> r = new();
        return Getter(r, byteOffset, IsLittleEndian);
    }

    /**
     * Stores a Float32 (float) value at the specified byte offset from the start of the view.
     * @param byteOffset The place in the buffer at which the value should be set.
     * @param value The value to set.
     * @param littleEndian If false or undefined, a big-endian value should be written.
     */
    public void SetFloat32(int byteOffset, float value, bool IsLittleEndian = false)
    {
        throw new NotImplementedException();
    }

    /**
     * Stores a Float64 (double) value at the specified byte offset from the start of the view.
     * @param byteOffset The place in the buffer at which the value should be set.
     * @param value The value to set.
     * @param littleEndian If false or undefined, a big-endian value should be written.
     */
    public void SetFloat64(int byteOffset, double value, bool IsLittleEndian = false)
    {
        throw new NotImplementedException();
    }

    /**
     * Stores an Int8 (sbyte) value at the specified byte offset from the start of the view.
     * @param byteOffset The place in the buffer at which the value should be set.
     * @param value The value to set.
     * @param littleEndian If false or undefined, a big-endian value should be written.
     */
    public void SetInt8(int byteOffset, sbyte value)
    {
        TypedArray<sbyte> r = new();
        Setter(r, byteOffset, value);
    }

    /**
     * Stores an Int16 (short) value at the specified byte offset from the start of the view.
     * @param byteOffset The place in the buffer at which the value should be set.
     * @param value The value to set.
     * @param littleEndian If false or undefined, a big-endian value should be written.
     */
    public void SetInt16(int byteOffset, short value, bool IsLittleEndian = false)
    {
        TypedArray<Int16> r = new();
        Setter(r, byteOffset, value, IsLittleEndian);
    }

    /**
     * Stores an Int32 (int) value at the specified byte offset from the start of the view.
     * @param byteOffset The place in the buffer at which the value should be set.
     * @param value The value to set.
     * @param littleEndian If false or undefined, a big-endian value should be written.
     */
    public void SetInt32(int byteOffset, int value, bool IsLittleEndian = false)
    {
        TypedArray<int> r = new();
        Setter(r, byteOffset, value, IsLittleEndian);
    }

    /**
     * Stores a UInt8 (byte) value at the specified byte offset from the start of the view.
     * @param byteOffset The place in the buffer at which the value should be set.
     * @param value The value to set.
     * @param littleEndian If false or undefined, a big-endian value should be written.
     */
    public void SetUInt8(int byteOffset, byte value)
    {
        TypedArray<byte> r = new();
        Setter(r, byteOffset, value);
    }

    /**
     * Stores a UInt16 (ushort) value at the specified byte offset from the start of the view.
     * @param byteOffset The place in the buffer at which the value should be set.
     * @param value The value to set.
     * @param littleEndian If false or undefined, a big-endian value should be written.
     */
    public void SetUInt16(int byteOffset, ushort value, bool IsLittleEndian = false)
    {
        TypedArray<UInt16> r = new();
        Setter(r, byteOffset, value, IsLittleEndian);
    }

    /**
     * Stores a UInt32 (uint) value at the specified byte offset from the start of the view.
     * @param byteOffset The place in the buffer at which the value should be set.
     * @param value The value to set.
     * @param littleEndian If false or undefined, a big-endian value should be written.
     */
    public void SetUInt32(int byteOffset, uint value, bool IsLittleEndian = false)
    {
        TypedArray<uint> r = new();
        Setter(r, byteOffset, value, IsLittleEndian);
    }

    /**
     * Protected methods.
     */
    protected T Getter<T>(
        TypedArray<T> r,
        int byteOffset,
        bool IsLittleEndian = false
    ) where T : IBinaryInteger<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteOffset);
        if (byteOffset + r.BytesPerElement > _byteLength)
        {
            throw new ArgumentOutOfRangeException(nameof(byteOffset));
        }

        byteOffset += _byteOffset;

        TypedArray<byte> u8 = new(_buffer, byteOffset, r.BytesPerElement);
        List<byte> bytes = [];
        for (int i = 0; i < r.BytesPerElement; i++)
        {
            bytes.Add(u8.Get(i));
        }
        if (IsLittleEndian == IS_BIG_ENDIAN)
        {
            bytes.Reverse();
        }
        TypedArray<byte> r8 = new(bytes);
        r = new(r8.Buffer);
        return r.Get(0);
    }

    protected void Setter<T>(
        TypedArray<T> r,
        int byteOffset,
        T value,
        bool IsLittleEndian = false
    ) where T: IBinaryInteger<T>
    {
        ArgumentOutOfRangeException.ThrowIfNegative(byteOffset);
        if (byteOffset + r.BytesPerElement > _byteLength)
        {
            throw new ArgumentOutOfRangeException(nameof(byteOffset));
        }

        // Get bytes.
        r = new([value,]);
        TypedArray<byte> byteArray = new(r.Buffer);
        List<byte> bytes = [];

        for (int i = 0; i < r.BytesPerElement; i++)
        {
            bytes.Add(byteArray.Get(i));
        }

        // Flip if necessary.
        if (IsLittleEndian == IS_BIG_ENDIAN)
        {
            bytes.Reverse();
        }

        // Write them.
        TypedArray<byte> byteView = new(_buffer, byteOffset, r.BytesPerElement);
        byteView.Set(bytes);
    }
}
