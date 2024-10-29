using System.Diagnostics.CodeAnalysis;

namespace typed_arrays;

public abstract class TypedArray
{
    protected int BYTES_PER_ELEMENT;
    protected ArrayBuffer _buffer;
    protected int _byteLength;
    protected int _byteOffset;
    protected int _length;
    protected Func<int, IList<int>> _pack;
    protected Func<IList<int>, int> _unpack;

    public TypedArray(int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(length);
        _length = length;
        _byteOffset = 0;
    }

    public TypedArray(IList<int> source)
    {
        int byteLength = source.Count * BYTES_PER_ELEMENT;
        _buffer = new ArrayBuffer(byteLength);
        _byteLength = byteLength;
        _byteOffset = 0;
        _length = source.Count;

        for (int i = 0; i < this._length; i++)
        {
            Set(i, source[i]);
        }
    }

    public TypedArray(ArrayBuffer source, int byteOffset = 0, int? length)
    {
        if (byteOffset > source.ByteLength)
        {
            throw new ArgumentException("Byte offset out of range.", nameof(byteOffset));
        }

        /**
         * The given byteOffset must be a multiple of the
         * element size of the specific type.
         */
        if (byteOffset % BYTES_PER_ELEMENT != 0)
        {
            throw new ArgumentException("Buffer length minus the byteOffset is not a multiple of the element size.",
                nameof(byteOffset));
        }

        int byteLength;
        if (!length.HasValue)
        {
            byteLength = source.ByteLength - byteOffset;
            if (byteLength % BYTES_PER_ELEMENT != 0)
            {
                throw new ArgumentException("Length of buffer minus byteOffset not a multiple of the element size.",
                    nameof(byteOffset));
            }
            length = byteLength / BYTES_PER_ELEMENT;
        }
        else
        {
            byteLength = length.Value * BYTES_PER_ELEMENT;
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
        int byteLength = source._length * BYTES_PER_ELEMENT;
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
    public int BytesPerElement { get { return BYTES_PER_ELEMENT; } }
    public int Length {  get { return _length; } }

    /**
     * Public methods
     */

    public int Get(int index)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);
        IList<int> bytes = new List<int>();
        for (
            int i = 0, o = _byteOffset + index * BYTES_PER_ELEMENT;
            i < BYTES_PER_ELEMENT;
            i++, o++
        )
        {
            bytes.Add(_buffer.Bytes[o]);
        }
        return _unpack(bytes);
    }

    public void Set(int index, int value)
    {
        if (index >= _length) { return; }

        IList<int> bytes = _pack(value);
        for (
            int i = 0, o = _byteOffset + index * BYTES_PER_ELEMENT;
            i < BYTES_PER_ELEMENT;
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

        int byteOffset = _byteOffset + offset * BYTES_PER_ELEMENT;
        int byteLength = source._length * BYTES_PER_ELEMENT;

        if (source.Buffer == _buffer)
        {
            IList<int> tmp = new List<int>();
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
}
