using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Schema;

namespace ftss
{
    public readonly record struct TernaryTreeStats(
        int Size,
        int Nodes,
        bool IsCompact,
        int Depth,
        IEnumerable<int> Breadth,
        int MinCodePoint,
        int MaxCodePoint,
        int Surrogates
    );

    public class FastTernaryStringSet : IEnumerable<string>
    {
        /**
         * Constants
         */
        ///<summary>Node index indicating that no node is present.</summary>
        const int NUL = ~(1 << 31);
        /// <summary>First node index that would run off of the end of the array.</summary>
        const int NODE_CEILING = NUL - 3;
        /// <summary>End-of-string flag: set on node values when that node also marks the end of a string.</summary>
        const int EOS = 1 << 21;
        /// <summary>Mask to extract the code point from a node value, ignoring flags.</summary>
        const int CP_MASK = EOS - 1;
        /// <summary>Smallest code point that requires a surrogate pair.</summary>
        const int CP_MIN_SURROGATE = 0x10000;

        /**
         * <summary>
         * Tree data, an integer array laid out as follows:
         *
         * 1. `tree[n]`: code point of the character stored in this node, plus bit flags
         * 2. `tree[n+1]`: array index of the "less than" branch's child node
         * 3. `tree[n+2]`: array index of the "equal to" branch's child node
         * 4. `tree[n+3]`: array index of the "greater than" branch's child node
         * </summary>
         */
        protected IList<int> _tree;
        /// <summary>Tracks whether empty string is in the set as a special case.</summary>
        protected bool _hasEmpty;
        /**
         * <summary>
         * Tracks whether this tree has been compacted; if true this must be undone before mutating the tree.
         * </summary>
         */
        protected bool _compact;
        /// <summary>Tracks set size.</summary>
        protected int _size;

        /**
         * <summary>
         * Creates a new set. The set will be empty unless the optional iterable `source` object
         * is specified. If a `source` is provided, all of its elements will be added to the new set.
         * If `source` contains any element that would cause `add()` to throw an error, the constructor
         * will also throw an error for that element.
         *
         * **Note:** Since strings are iterable, passing a string to the constructor will create
         * a new set containing one string for each unique code point in the source string, and not
         * a singleton set containing just the source string as you might expect.
         * </summary>
         *
         * <param name="source">
         * An optional iterable object whose strings will be added to the new set.
         * </param>
         */
        public FastTernaryStringSet(IList<string>? source = null)
        {
            this.Clear();
            if (source is not null)
            {
                if (source.GetType() == typeof(FastTernaryStringSet))
                {
                    FastTernaryStringSet s = (FastTernaryStringSet)source;
                    _tree = s._tree is null ? [] : s._tree;
                    _hasEmpty = s._hasEmpty;
                    _compact = s._compact;
                    _size = s._size;
                }
                else
                {
                    this.AddAll(source);
                }
            }
        }

        /**
         * Public properties
         */
        public int Size { get { return _size; } }

        public TernaryTreeStats Stats
        {
            get
            {
                List<int> breadth = [];
                int nodes = _tree.Count / 4;
                int surrogates = 0;
                int minCodePoint = nodes > 0 ? 0x10ffff : 0;
                int maxCodePoint = 0;

                void traverse(int n, int d)
                {
                    if (n >= _tree.Count)
                    {
                        return;
                    }
                    while (d >= breadth.Count)
                    {
                        breadth.Add(0);
                    }
                    breadth[d]++;
                    int cp = _tree[n] & CP_MASK;
                    if (cp >= CP_MIN_SURROGATE)
                    {
                        surrogates++;
                    }
                    if (cp > maxCodePoint)
                    {
                        maxCodePoint = cp;
                    }
                    if (cp < minCodePoint)
                    {
                        minCodePoint = cp;
                    }

                    traverse(_tree[n + 1], d + 1);
                    traverse(_tree[n + 2], d + 1);
                    traverse(_tree[n + 3], d + 1);
                }
                traverse(0, 0);

                return new(
                    _size,
                    nodes,
                    _compact,
                    breadth.Count,
                    breadth,
                    minCodePoint,
                    maxCodePoint,
                    surrogates
                );
            }
        }

        /**
         * Public methods
         */
        public void Add(string s)
        {
            if (s.Length == 0)
            {
                if (!_hasEmpty)
                {
                    _hasEmpty = true;
                    _size++;
                }
            }
            else
            {
                if (_compact && !Has(s))
                {
                    this.Decompact();
                }
                _ = Add(0, s, 0, s[0]);
            }
        }

        /**
         * <summary>
         * Adds an entire array, or subarray, of strings to this set. By default,
         * the entire collection is added. If the `start` and/or `end` are specified,
         * only the elements in the specified range are added.
         *
         * If the collection is sorted in ascending order and no other strings have been
         * added to this set, the underlying tree is guaranteed to be balanced, ensuring
         * good search performance. If the collection is in random order, the tree is *likely*
         * to be nearly balanced.
         * </summary>
         *
         * <param name="source">The non-null collection of strings to add.</param>
         * <param name="start">The optional index of the first element to add (inclusive, default is 0).</param>
         * <param name="end">The optional index of the last element to add (exclusive, default is `source.length`)</param>
         */
        public void AddAll(IList<string>? source, int start = 0, int? end = null)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            int len = source.Count;
            end ??= len;
            if (start < 0 || start > len)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (end < 0 || end > len)
            {
                throw new ArgumentOutOfRangeException(nameof(end));
            }
            if (start < end)
            {
                AddAllInternal(source, start, end.Value);
            }
        }

        /// <summary>Removes all strings from this set.</summary>
        [MemberNotNull(nameof(_tree), nameof(_hasEmpty), nameof(_compact), nameof(_size))]
        public void Clear()
        {
            _tree = [];
            _hasEmpty = false;
            _compact = false;
            _size = 0;
        }

        /**
         * <summary>
         * Removes the specified string from this set, if it is present.
         * If it is not present, this has no effect.
         * Non-strings are accepted, but treated as if they are not present.
         * Returns true if the string was in this set; false otherwise.
         * </summary>
         *
         * <param name="s">The non-null string to delete.</param>
         */
        public bool Delete(string s)
        {
            if (s.Length == 0)
            {
                bool had = _hasEmpty;
                if (had)
                {
                    _hasEmpty = false;
                    _size--;
                }
                return had;
            }

            if (_compact&& Has(s))
            {
                Decompact();
            }
            return Delete(0, s, 0, s[0]);
        }

        /**
         * <summary>
         * Removes multiple elements from this set.
         * Returns true if every element was present and was removed.
         * </summary>
         * <param name="elements">The elements to remove.</param>
         */
        public bool DeleteAll(string[] elements)
        {
            if (elements is null)
            {
                return false;
            }
            bool allDeleted = true;
            foreach (string el in elements)
            {
                allDeleted = Delete(el) && allDeleted;
            }
            return allDeleted;
        }

        public static string FromCodePoints(IList<int> codepoints)
        {
            StringBuilder sb = new();
            foreach (int i in codepoints)
            {
                sb.Append((char)i);
            }
            return sb.ToString();
        }

        /**
         * <summary>
         * Returns all strings in this set that can be composed from combinations of the code points
         * in the specified string. Unlike an anagram, all of the code points need not to appear for a match
         * to count. For example, the pattern `"coat"` can match `"cat"` even though the *o* is not used.
         * However, characters cannot appear *more often* than they appear in the pattern string. The same
         * pattern `"coat"` cannot match `"tot"` since it includes only a single *t*.
         *
         * If this set contains the empty string, it is always included in results from this
         * method.
         * Returns A (possibly empty) array of strings from the set that can be composed from the
         *     pattern characters.
         * </summary>
         *
         * <param name="pattern">The non-null pattern string.</param>
         */
        public IList<string> GetArrangementsOf(string pattern)
        {
            if (pattern is null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            Dictionary<int, int> availChars = [];
            for (int i = 0; i < pattern.Length; )
            {
                int c = pattern[i++];
                if (c > CP_MIN_SURROGATE)
                {
                    i++;
                }
                if (availChars.ContainsKey(c))
                {
                    availChars[c]++;
                }
                else
                {
                    availChars.Add(c, 1);
                }
            }
            List<string> matches = _hasEmpty ? [""] : [];
            GetArrangementsOf(0, availChars, new StringBuilder(), matches);
            return matches;
        }

        /**
         * <summary>
         * Returns an array of possible completions for the specified prefix string.
         * That is, an array of all strings in the set that start with the prefix.
         * If the prefix itself is in the set, it is included as the first entry.
         * 
         * Returns a (possibly empty) array of all strings in the set for which the
         *     pattern is a prefix.
         * 
         * Throws `ArgumentNullException` if the prefix is null.
         * </summary>
         * <param name="prefix">The non-null pattern to find completions for.</param>
         */
        public IList<string> GetCompletionsOf(string prefix)
        {
            if (prefix is null)
            {
                throw new ArgumentNullException(nameof(prefix));
            }
            if (prefix.Length == 0)
            {
                return ToList();
            }

            IList<string> results = [];
            IList<int> pat = FastTernaryStringSet.ToCodePoints(prefix);
            int node = HasCodePoints(0, pat, 0);
            if (node < 0)
            {
                node = -node - 1;
                // Prefix is not in tree; no children are, either.
                if (node >= _tree.Count)
                {
                    return results;
                }
            }
            else
            {
                // Prefix is in tree and also in set.
                results.Add(prefix);
            }

            // Continue from end of prefix by taking equal branch.
            VisitCodePoints(_tree[node + 2], pat, (IList<int> s, int i) =>
            {
                results.Add(FastTernaryStringSet.FromCodePoints(s));
            });
            return results;
        }

        public IEnumerator<string> GetEnumerator()
        {
            if (_hasEmpty)
            {
                yield return string.Empty;
            }
            GetEnumerator(0, []);
            throw new NotImplementedException();
        }

        protected IEnumerator<string> GetEnumerator(int node, IList<int> prefix)
        {
            if (node < _tree.Count)
            {
                GetEnumerator(_tree[node + 1], prefix);
                prefix.Add(_tree[node] & CP_MASK);
                if ((_tree[node] & EOS) == EOS)
                {
                    yield return FastTernaryStringSet.FromCodePoints(prefix);
                }
                GetEnumerator(_tree[node + 2], prefix);
                prefix.RemoveAt(prefix.Count - 1);
                GetEnumerator(_tree[node + 3], prefix);
            }
        }

        /**
         * <summary>
         * Returns a new array of every element in the set. This is equivalent
         * to `Array.from(this)`, but this method is more efficient.
         * 
         * Returns A non-null array of the elements of the set in lexicographic order.
         * </summary>
         */
        public string[] ToArray()
        {
            return [.. ToList()];
        }

        public IList<string> ToList()
        {
            IList<string> a = _hasEmpty ? [string.Empty] : [];
            VisitCodePoints(0, [], (IList<int> s, int i) =>
            {
                a.Add(FastTernaryStringSet.FromCodePoints(s));
            });
            return a;
        }

        public static IList<int> ToCodePoints(string s)
        {
            IList<int> codepoints = [];
            for (int i = 0; i < s.Length; )
            {
                char c = s[i++];
                if (c >= CP_MIN_SURROGATE)
                {
                    ++i;
                }
                codepoints.Add(c);
            }
            return codepoints;
        }

        /**
         * <summary>
         * Returns whether this set contains the specified string.
         * If passed a non-string value, returns false.
         * Returns true if the string is present.
         * </summary>
         *
         * <param name="s">The non-null string to test for.</param>
         */
        public bool Has(string s)
        {
            if (s.Length == 0)
            {
                return _hasEmpty;
            }
            return Has(0, s, 0, s[0]);
        }

        /**
         * Protected methods
         */
        protected int Add(int node, string s, int i, char c)
        {
            _tree ??= [];
            if (node >= _tree.Count)
            {
                node = _tree.Count;
                if (node >= NODE_CEILING)
                {
                    throw new OutOfMemoryException("Add(): Cannot add more strings.");
                }
                _tree.Add(c);
                _tree.Add(NUL);
                _tree.Add(NUL);
                _tree.Add(NUL);
            }

            int treeChar = _tree[node] & CP_MASK;
            if (c < treeChar)
            {
                _tree[node + 1] = Add(_tree[node + 1], s, i, c);
            }
            else if (c > treeChar)
            {
                _tree[node + 3] = Add(_tree[node + 3], s, i, c);
            }
            else
            {
                i += c >= CP_MIN_SURROGATE ? 2 : 1;
                if (i >= s.Length)
                {
                    if ((_tree[node] & EOS) == 0)
                    {
                        _tree[node] |= EOS;
                        _size++;
                    }
                }
                else
                {
                    _tree[node + 2] = Add(_tree[node + 2], s, i, s[i]);
                }
            }
            return node;
        }

        protected void AddAllInternal(IList<string> source, int start, int end)
        {
            if (--end < start)
            {
                return;
            }

            /**
             * If the tree is empty and the list is sorted, then
             * insertion by repeated bifurcation ensures a balanced tree.
             * Inserting strings in sorted order is a degenerate case.
             */
            int mid = start + (end - start) / 2;
            Add(source[mid]);
            AddAllInternal(source, start, mid);
            AddAllInternal(source, mid + 1, end + 1);
        }

        protected void Decompact()
        {
            throw new NotImplementedException("Decompact()");
        }

        protected bool Delete(int node, string s, int i, char c)
        {
            if (node >= _tree.Count)
            {
                return false;
            }
            int treeChar = _tree[node] & CP_MASK;
            if (c < treeChar)
            {
                return Delete(_tree[node + 1], s, i, c);
            }
            else if (c > treeChar)
            {
                return Delete(_tree[node + 3], s, i, c);
            }
            else
            {
                i += c >= CP_MIN_SURROGATE ? 2 : 1;
                if (i >= s.Length)
                {
                    bool had = (_tree[node] & EOS) == EOS;
                    if (had)
                    {
                        _tree[node] &= CP_MASK;
                        _size--;
                    }
                    return had;
                }
                else
                {
                    return Delete(_tree[node + 2], s, i, s[i]);
                }
            }
        }

        protected void GetArrangementsOf(int node, IDictionary<int, int> availChars, StringBuilder sb, IList<string> matches)
        {
            if (node >= _tree.Count)
            {
                return;
            }
            GetArrangementsOf(_tree[node + 1], availChars, sb, matches);
            int c = _tree[node] & CP_MASK;
            if (availChars.TryGetValue(c, out int value) && value > 0)
            {
                availChars[c]--;
                sb.Append((char)c);
                if ((_tree[node] & EOS) == EOS)
                {
                    matches.Add(sb.ToString());
                }
                GetArrangementsOf(_tree[node + 2], availChars, sb, matches);
                sb.Remove(sb.Length - 1, 1);
                availChars[c]++;
            }
            GetArrangementsOf(_tree[node + 3], availChars, sb, matches);
        }

        protected bool Has(int node, string s, int i, char c)
        {
            _tree ??= [];
            if (node >= _tree.Count)
            {
                return false;
            }
            int treeChar = _tree[node] & CP_MASK;
            if (c < treeChar)
            {
                return Has(_tree[node + 1], s, i, c);
            }
            else if (c > treeChar)
            {
                return Has(_tree[node + 3], s, i, c);
            }
            else
            {
                i += c >= CP_MIN_SURROGATE ? 2 : 1;
                if (i >= s.Length)
                {
                    return (_tree[node] & EOS) == EOS;
                }
                else
                {
                    return this.Has(_tree[node + 2], s, i, s[i]);
                }
            }
        }

        protected int HasCodePoints(int node, IList<int> s, int i)
        {
            if (node >= _tree.Count)
            {
                return -node - 1;
            }
            int codepoint = s[i];
            int treeCodepoint = _tree[node] & CP_MASK;
            if (codepoint < treeCodepoint)
            {
                return HasCodePoints(_tree[node + 1], s, i);
            }
            else if (codepoint > treeCodepoint)
            {
                return HasCodePoints(_tree[node + 3], s, i);
            }
            else
            {
                if (++i >= s.Count)
                {
                    return (_tree[node] & EOS) == EOS ? node : -node - 1;
                }
                else
                {
                    return HasCodePoints(_tree[node + 2], s, i);
                }
            }
        }

        protected void VisitCodePoints(int node, IList<int> prefix, Action<IList<int>, int> visitFn)
        {
            if (node >= _tree.Count) { return; }
            VisitCodePoints(_tree[node + 1], prefix, visitFn);
            prefix.Add(_tree[node] & CP_MASK);
            if ((_tree[node] & EOS) == EOS)
            {
                visitFn(prefix, node);
            }
            VisitCodePoints(_tree[node + 2], prefix, visitFn);
            prefix.RemoveAt(prefix.Count - 1);
            VisitCodePoints(_tree[node + 3], prefix, visitFn);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
