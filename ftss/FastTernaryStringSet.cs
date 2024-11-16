using NAryDict;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using typed_arrays;

namespace ftss
{
    public record struct DecodedBuffer(
        byte Version,
        bool HasEmpty,
        bool Compact,
        bool V2B16,
        uint Size,
        IList<uint> Tree
    );

    public readonly record struct TernaryTreeStats(
        uint Size,
        int Nodes,
        bool IsCompact,
        int Depth,
        IEnumerable<int> Breadth,
        uint MinCodePoint,
        uint MaxCodePoint,
        int Surrogates
    );

    public class FastTernaryStringSet : IEnumerable<string>
    {
        /**
         * Constants
         */
        /// <summary>Node index indicating that no node is present.</summary>
        const uint NUL = ~(1 << 31);
        /// <summary>First node index that would run off of the end of the array.</summary>
        const uint NODE_CEILING = NUL - 3;
        /// <summary>End-of-string flag: set on node values when that node also marks the end of a string.</summary>
        const uint EOS = 1 << 21;
        /// <summary>Mask to extract the code point from a node value, ignoring flags.</summary>
        const uint CP_MASK = EOS - 1;
        /// <summary>Smallest code point that requires a surrogate pair.</summary>
        const uint CP_MIN_SURROGATE = 0x10000;
        /// <summary>Version number for the data buffer format.</summary>
        const int BUFF_VERSION = 3;
        /// <summary>Magic number used by buffer data format.</summary>
        const int BUFF_MAGIC = 84;
        /// <summary>Buffer format header size (4 bytes magic/properties + 4 bytes node count).</summary>
        const int BUFF_HEAD_SIZE = 8;
        /// <summary>Buffer format flag bit: set has empty string</summary>
        const byte BF_HAS_EMPTY = 1;
        /// <summary>Buffer format flag bit: set is compact</summary>
        const byte BF_COMPACT = 2;
        /// <summary>Buffer format flag bit: v2 file uses 16-bit integers for branch pointers.</summary>
        const byte BF_BRANCH16 = 4;

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
        protected IList<uint> _tree;
        /// <summary>Tracks whether empty string is in the set as a special case.</summary>
        protected bool _hasEmpty;
        /**
         * <summary>
         * Tracks whether this tree has been compacted; if true this must be undone before mutating the tree.
         * </summary>
         */
        protected bool _compact;
        /// <summary>Tracks set size.</summary>
        protected uint _size;

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

        public FastTernaryStringSet(FastTernaryStringSet? source)
        {
            this.Clear();
            if (source is not null)
            {
                _tree = new List<uint>(source._tree);
                _hasEmpty = source._hasEmpty;
                _compact = source._compact;
                _size = source._size;
            }
        }

        /**
         * <summary>
         * Creates a new string set from data in a buffer previously created with `toBuffer`.
         * Buffers created by an older version of this library can be deserialized by newer versions
         * of this library.
         * The reverse may or may not be true, depending on the specific versions involved.
         * Throws `ArgumentNullError` if the specified buffer is null.
         * Throws `TypeError` if the buffer data is invalid or from an unsupported version.
         *
         * Returns A new set that recreates the original set that was stored in the buffer.
         * </summary>
         * <param name="buffer">The buffer to recreate the set from.</param>
         */
        public FastTernaryStringSet(ArrayBuffer buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            DecodedBuffer h = Decode(buffer);
            _hasEmpty = h.HasEmpty;
            _compact = h.Compact;
            _size = h.Size;
            _tree = h.Tree;
        }

        /**
         * Public properties
         */
        public string this[int index] { get { return Get(index); } }

        public bool Compacted { get { return _compact; } }

        public IList<string> Keys { get { return ToList(); } }

        public uint Size { get { return _size; } }

        public TernaryTreeStats Stats
        {
            get
            {
                List<int> breadth = [];
                int nodes = _tree.Count / 4;
                int surrogates = 0;
                uint minCodePoint = nodes > 0 ? (uint)0x10ffff : 0;
                uint maxCodePoint = 0;

                void traverse(uint n, int d)
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
                    uint cp = _tree[(int)n] & CP_MASK;
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

                    traverse(_tree[(int)n + 1], d + 1);
                    traverse(_tree[(int)n + 2], d + 1);
                    traverse(_tree[(int)n + 3], d + 1);
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

        // For testing only
        // public IList<uint> Tree {  get { return _tree; } }

        public IList<string> Values { get { return ToList(); } }

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

        /**
         * <summary>
         * Balances the tree structure, minimizing the depth of the tree.
         * This may improve search performance, especially after adding or deleting a large
         * number of strings.
         *
         * It is not normally necessary to call this method as long as care was taken not
         * to add large numbers of strings in lexicographic order. That said, two scenarios
         * where this methof may be particularly useful are:
         *  - If the set will be used in two phases, with strings being added in one phase
         *    followed by a phase of extensive search operations.
         *  - If the string is about to be serialized to a buffer for future use.
         *
         * As detailed under `addAll`, if the entire contents of the set were added by a single
         * call to `addAll` using a sorted array, the tree is already balanced and calling this
         * method will have no benefit.
         *
         * **Note:** This method undoes the effect of `compact()`. If you want to balance and
         * compact the tree, be sure to balance it first.
         * </summary>
         */
        public void Balance()
        {
            _tree = new FastTernaryStringSet(ToList())._tree;
            _compact = false;
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
         * Compacts the set to reduce its memory footprint and improve search performance.
         * Compaction allows certain nodes of the underlying tree to be shared, effectively
         * converting it to a graph. For large sets, the result is typically a *significantly*
         * smaller footprint. The tradeoff is that compacted sets cannot be mutated.
         * Any attempt to do so, such as adding or deleting a string, will automatically
         * decompact the set to a its standard tree form, if necessary, before performing
         * the requested operation.
         *
         * Compaction is an excellent option if the primary purpose of a set is matching
         * or searching against a fixed string collection. Since compaction and decompaction
         * are both expensive operations, it may not be suitable if the set is expected to
         * be modified intermittently.
         */
        public void Compact()
        {
            if (_compact || _tree.Count == 0) { return; }

            /*

            Theory of operation:

            In a ternary tree, all strings with the same prefix share the nodes
            that make up that prefix. The compact operation does much the same thing,
            but for suffixes. It does this by deduplicating identical tree nodes.
            For example, every string that ends in "e" and is not a prefix of any other
            strings looks the same: an "e" node with three NUL child branch pointers.
            But these can be distributed throughout the tree. Consider a tree containing only
            "ape" and "haze": we could save space by having only a single copy of the "e" node
            and pointing to it from both the "p" node and the "z" node.

            So: to compact the tree, we iterate over each node and build a map of all unique nodes.
            The first time we come across a node, we add it to the map, mapping the node to
            a number which is the next available slot in the new, compacted, output array we will write.

            Once we have built the map, we iterate over the nodes again. This time we look up each node
            in the previously built map to find the slot it was assigned in the output array. If the
            slot is past the end of the array, then we haven't added it to the output yet. We can
            write the node's value unchanged, but the three pointers to the child branches need to
            be rewritten to point to the new, deduplicated equivalent of the nodes that they point to now.
            Thus for each branch, if the pointer is NUL we write it unchanged. Otherwise we look up the node
            that the branch points to in our unique node map to get its new slot number (i.e. array offset)
            and write the translated address.

            After doing this once, we will have deduplicated just the leaf nodes. In the original tree,
            only nodes with no children can be duplicates, because their branches are all NUL.
            But after rewriting the tree, some of the parents of those leaf nodes may now point to
            *shared* leaf nodes, so they themselves might now have duplicates in other parts of the tree.
            So, we can repeat the rewriting step above to remove these newly generated duplicates as well.
            This may again lead to new duplicates, and so on: rewriting continues until the output
            doesn't shrink anymore.

            */

            IList<uint> source = _tree;
            _tree = [];
            int pass = 0;
            while (true)
            {
                pass++;
                // Debug.WriteLine($"Compaction pass #{pass}");
                IList<uint> compacted = CompactionPass(source);
                if (compacted.Count == source.Count)
                {
                    _tree = compacted;
                    break;
                }
                source = compacted;
            }

            _compact = true;
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

            if (_compact && Has(s))
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

        /**
         * <summary>
         * Calls the specified callback function once for each string in this set.
         * </summary>
         * 
         * @param callbackFn The function to call for each string.
         */
        public void ForEach(Action<string> callback)
        {
            if (this._hasEmpty)
            {
                callback(string.Empty);
            }
            VisitCodePoints(0, [], (prefix, q) =>
            {
                string s = FromCodePoints(prefix);
                callback(s);
            });
        }

        public static string FromCodePoints(IList<uint> codepoints)
        {
            StringBuilder sb = new();
            foreach (uint i in codepoints)
            {
                sb.Append((char)i);
            }
            return sb.ToString();
        }

        public string Get(int index)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(index, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, (int)_size);
            if (_hasEmpty && index == 0)
            {
                return String.Empty;
            }
            string toReturn = String.Empty;
            int count = _hasEmpty ? 1 : 0;
            SearchCodePoints(0, [], (prefix, node) =>
            {
                count++;
                if (count == index)
                {
                    toReturn = FromCodePoints(prefix);
                    return true;
                }
                return false;
            });
            return toReturn;
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
            ArgumentNullException.ThrowIfNull(pattern);
            Dictionary<uint, uint> availChars = [];
            for (int i = 0; i < pattern.Length;)
            {
                uint c = pattern[i++];
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
         * Returns an array of the strings that are completed by the specified suffix string.
         * That is, an array of all strings in the set that end with the suffix,
         * including the suffix itself if appropriate.
         * 
         * Returns a (possibly empty) array of all strings in the set for which the
         *     pattern is a suffix.
         *     
         * Throws `ArgumentNullException` if the suffix is null.
         * </summary>
         * <param name="suffix">The non-null pattern to find completions for.</param>
         */
        public IList<string> GetCompletedBy(string suffix)
        {
            ArgumentNullException.ThrowIfNull(suffix);
            if (suffix.Length == 0)
            {
                return ToList();
            }
            IList<string> results = [];
            IList<uint> pat = FastTernaryStringSet.ToCodePoints(suffix);

            VisitCodePoints(0, [], (IList<uint> s, uint q) =>
            {
                if (s.Count >= pat.Count)
                {
                    for (int i = 1; i <= pat.Count; i++)
                    {
                        if (s[s.Count - i] != pat[pat.Count - i])
                        {
                            return;
                        }
                    }
                    results.Add(FastTernaryStringSet.FromCodePoints(s));
                }
            });
            return results;
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
            ArgumentNullException.ThrowIfNull(prefix);
            if (prefix.Length == 0)
            {
                return ToList();
            }

            IList<string> results = [];
            IList<uint> pat = FastTernaryStringSet.ToCodePoints(prefix);
            uint node = HasCodePoints(0, pat, 0);
            if ((int)node < 0)
            {
                node = uint.MaxValue - node;
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
            VisitCodePoints(_tree[(int)node + 2], pat, (IList<uint> s, uint i) =>
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

        /**
         * <summary>
         * Returns all strings that match the pattern. The pattern may include zero or
         * more "don't care" characters that can match any code point. By default this
         * character is `"."`, but any valid code point can be used. For example, the
         * pattern `"c.t"` would match any of `"cat"`, `"cot"`, or `"cut"`, but not `"cup"`.
         * Returns A (possibly empty) array of strings that match the pattern string.
         * Throws `ArgumentNullException` if the pattern or don't care string is null.
         * Throws `ArgumentNullException` if the don't care string is empty.
         * </summary>
         * <param name="pattern">A pattern string matched against the strings in the set.</param>
         * <param name="dontCareChar">
         * The character that can stand in for any character in the pattern.
         *     Only the first code point is used. (Default is `"."`.)
         * </param>
         */
        public IList<string> GetPartialMatchesOf(string pattern, string dontCareChar = ".")
        {
            ArgumentNullException.ThrowIfNull(pattern);
            if (string.IsNullOrEmpty(dontCareChar))
            {
                throw new ArgumentNullException(nameof(dontCareChar));
            }

            if (pattern.Length == 0)
            {
                return _hasEmpty ? [""] : [];
            }

            IList<string> matches = [];
            GetPartialMatches(0, pattern, 0, dontCareChar[0], [], matches);
            return matches;
        }

        /**
         * <summary>
         * Returns an array of all strings in the set that are within the specified edit distance
         * of the given pattern string. A string is within edit distance *n* of the pattern if
         * it can be transformed into the pattern with no more than *n* insertions, deletions,
         * or substitutions. For example:
         *  - `cat` is edit distance 0 from itself;
         *  - `at` is edit distance 1 from `cat` (1 deletion);
         *  - `cot` is edit distance 1 from `cat` (1 substitution); and
         *  - `coats` is edit distance 2 from `cat` (2 insertions).
         * Returns A (possibly empty) array of strings from the set that match the pattern.
         * Throws `ArgumentNullException` if the pattern is null.
         * Throws `ArgumentOutOfRangeException` if the distance is negative.
         * </summary>
         * <param name="pattern">A pattern string matched against the strings in the set.</param>
         * <param name="distance">
         * The maximum number of edits to apply to the pattern string.
         *   May be Infinity to allow any number of edits.
         * </param>
         */
        public IList<string> GetWithinEditDistanceOf(string pattern, int distance)
        {
            ArgumentNullException.ThrowIfNull(pattern);
            ArgumentOutOfRangeException.ThrowIfNegative(distance);
            if (distance < 1)
            {
                return Has(pattern) ? [pattern] : [];
            }

            /**
             * Once we start inserting and deleting characters,
             * a standard traversal no longer guarantees
             * sorted order. So, instead of collecting results
             * in a list, we collect them in a temporary set.
             */
            FastTernaryStringSet results = [];

            // Add empty string if we can delete the pattern down to it.
            if (_hasEmpty && pattern.Length <= distance)
            {
                results.Add("");
            }

            /**
             * We avoid redundant work by computing possible deletions
             * ahead of time. (For example, aaa deletes to aa 3 different ways.)
             */
            FastTernaryStringSet patterns = new((string[])[pattern,]);
            for (int d = distance; d >= 0; d--)
            {
                FastTernaryStringSet reducedPatterns = [];
                if (patterns._hasEmpty)
                {
                    GetWithinEditDistance(0, [], 0, d, [], results);
                }

                /**
                 * Make patterns for the next iteration by
                 * deleting each character in turn
                 * from this iteration's patterns.
                 * abc => ab ac bc => a b c => empty string
                 */
                patterns.VisitCodePoints(0, [], (cp, q) =>
                {
                    GetWithinEditDistance(0, cp, 0, d, [], results);
                    if (d > 0 && cp.Count > 0)
                    {
                        if (cp.Count == 1)
                        {
                            reducedPatterns._hasEmpty = true;
                        }
                        else
                        {
                            uint[] delete1 = new uint[cp.Count - 1];
                            for (int i = 0; i < cp.Count; i++)
                            {
                                for (int j = 0; j < i; j++)
                                {
                                    delete1[j] = cp[j];
                                }
                                for (int j = i + 1; j < cp.Count; j++)
                                {
                                    delete1[j - 1] = cp[j];
                                }
                                reducedPatterns.AddCodePoints(0, delete1, 0);
                            }
                        }
                    }
                });
                if (patterns._hasEmpty)
                {
                    GetWithinEditDistance(0, [], 0, d, [], results);
                }
                patterns = reducedPatterns;
            }
            return results.ToList();
        }

        /**
         * <summary>
         * Returns an array of all strings in the set that are within the specified Hamming distance
         * of the given pattern string. A string is within Hamming distance *n* of the pattern if at
         * most *n* of its code points are different from those of the pattern. For example:
         *  - `cat` is Hamming distance 0 from itself;
         *  - `cot` is Hamming distance 1 from `cat`;
         *  - `cop` is Hamming distance 2 from `cat`; and
         *  - `top` is Hamming distance 3 from `cat`.
         * Returns A (possibly empty) array of strings from the set that match the pattern.
         * Throws `ArgumentNullException` if the pattern is null.
         * Throws `ArgumentOutOfRangeException` if the distance is negative.
         * </summary>
         * <param name="pattern">A pattern string matched against the strings in the set.</param>
         * <param name="distance">
         * The maximum number of code point deviations to allow from the pattern string.
         *     May be Infinity to allow any number.
         * </param>
         */
        public IList<string> GetWithinHammingDistanceOf(string pattern, int distance)
        {
            ArgumentNullException.ThrowIfNull(pattern);
            ArgumentOutOfRangeException.ThrowIfNegative(distance);

            if (distance < 1 || pattern.Length == 0)
            {
                return Has(pattern) ? [pattern] : [];
            }

            IList<string> matches = [];

            // Optimize case where any string the same length as the pattern will match.
            if (distance >= pattern.Length)
            {
                VisitCodePoints(0, [], (prefix, i) =>
                {
                    if (prefix.Count == pattern.Length)
                    {
                        matches.Add(FromCodePoints(prefix));
                    }
                });
                return matches;
            }

            GetWithinHammingDistance(0, pattern, 0, distance, [], matches);
            return matches;
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

        /**
         * <summary>
         * Returns a buffer whose contents can be used to recreate this set.
         * The returned data is independent of the platform on which it is created.
         * The buffer content and length will depend on the state of the set's
         * underlying structure. For this reason you may wish to `balance()`
         * and/or `compact()` the set first.
         *
         * Returns A non-null buffer.
         * </summary>
         */
        public ArrayBuffer ToBuffer()
        {
            return Encode(
                _size, _hasEmpty, _compact, _tree
            );
        }

        public IList<string> ToList()
        {
            IList<string> a = _hasEmpty ? [string.Empty,] : [];
            VisitCodePoints(0, [], (IList<uint> s, uint i) =>
            {
                a.Add(FastTernaryStringSet.FromCodePoints(s));
            });
            return a;
        }

        public static IList<uint> ToCodePoints(string s)
        {
            IList<uint> codepoints = [];
            for (int i = 0; i < s.Length;)
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
        protected uint Add(uint node, string s, int i, char c)
        {
            _tree ??= [];
            if (node >= _tree.Count)
            {
                node = (uint)_tree.Count;
                if (node >= NODE_CEILING)
                {
                    throw new OutOfMemoryException("Add(): Cannot add more strings.");
                }
                _tree.Add(c);
                _tree.Add(NUL);
                _tree.Add(NUL);
                _tree.Add(NUL);
            }

            uint treeChar = _tree[(int)node] & CP_MASK;
            if (c < treeChar)
            {
                _tree[(int)node + 1] = Add(_tree[(int)node + 1], s, i, c);
            }
            else if (c > treeChar)
            {
                _tree[(int)node + 3] = Add(_tree[(int)node + 3], s, i, c);
            }
            else
            {
                i += c >= CP_MIN_SURROGATE ? 2 : 1;
                if (i >= s.Length)
                {
                    if ((_tree[(int)node] & EOS) == 0)
                    {
                        _tree[(int)node] |= EOS;
                        _size++;
                    }
                }
                else
                {
                    _tree[(int)node + 2] = Add(_tree[(int)node + 2], s, i, s[i]);
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

        /**
         * <summary>
         * Adds a string described as an array of numeric code points.
         * Does not handle adding empty strings.
         * Does not check if the tree needs to be decompacted.
         * </summary>
         * <param name="node">The subtree from which to begin adding (0 for root).</param>
         * <param name="s">The non-null array of code points to add.</param>
         * <param name="i">The array index of the code point to start from (0 to add entire string).</param>
         */
        protected uint AddCodePoints(uint node, IList<uint> s, int i)
        {
            uint cp = s[i];
            if (node >= _tree.Count)
            {
                node = (uint)_tree.Count;
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(node, NODE_CEILING);
            }
            _tree.Add(cp);
            _tree.Add(NUL);
            _tree.Add(NUL);
            _tree.Add(NUL);

            uint treeCp = _tree[(int)node] & CP_MASK;
            if (cp < treeCp)
            {
                _tree[(int)node + 1] = AddCodePoints(_tree[(int)node + 1], s, i);
            }
            else if (cp > treeCp)
            {
                _tree[(int)node + 3] = AddCodePoints(_tree[(int)node + 3], s, i);
            }
            else
            {
                i += (cp >= CP_MIN_SURROGATE ? 2 : 1);
                if (i >= s.Count)
                {
                    if ((_tree[(int)node] & EOS) == 0)
                    {
                        _tree[(int)node] |= EOS;
                        _size++;
                    }
                }
                else
                {
                    _tree[(int)node + 2] = AddCodePoints(_tree[(int)node + 2], s, i);
                }
            }

            return node;
        }

        protected static IList<uint> CompactionPass(IList<uint> tree)
        {
            // Nested lists are used to map node offsets ("pointers")
            // in the original list to "slots" (a node's index in the new list).
            uint nextSlot = 0;
            NAryDictionary<uint, uint, uint, uint, uint> nodeMap = [];
            
            // If a node has already been assigned in a slot, then return that slot.
            // Otherwise, assign it the next available slot and return that.
            Func<uint, uint> mapping = new((i) =>
            {
                uint[] val = new uint[4];
                if (i >= tree.Count - 3)
                {
                    val[0] = val[1] = val[2] = val[3] = UInt32.MaxValue;
                }
                else
                {
                    val[0] = tree[(int)i];
                    val[1] = tree[(int)i + 1];
                    val[2] = tree[(int)i + 2];
                    val[3] = tree[(int)i + 3];
                }
                if (!nodeMap.TryGetValue(val[0], out NAryDictionary<uint, uint, uint, uint>? ltMap))
                {
                    ltMap = [];
                    nodeMap.Add(val[0], ltMap);
                }
                if (!ltMap.TryGetValue(val[1], out NAryDictionary<uint, uint, uint>? eqMap))
                {
                    eqMap = [];
                    ltMap.Add(val[1], eqMap);
                }
                if (!eqMap.TryGetValue(val[2], out NAryDictionary<uint, uint>? gtMap))
                {
                    gtMap = [];
                    eqMap.Add(val[2], gtMap);
                }
                if (!gtMap.TryGetValue(val[3], out uint slot))
                {
                    slot = nextSlot;
                    gtMap.Add(val[3], slot);
                    nextSlot += 4;
                }
                return slot;
            });

            // Create a map of unique nodes.
            for (uint i = 0; i < tree.Count; i += 4)
            {
                mapping(i);
            }

            // Check if tree would shrink before bothering to rewrite it.
            if (nextSlot == tree.Count)
            {
                return tree;
            }

            // Rewrite tree.
            List<uint> compactTree = [];
            for (uint i = 0; i < tree.Count; i += 4)
            {
                uint slot = mapping(i);

                // If the unique version of the node hasn't been written yet,
                // then append it to the output array.
                if (slot >= compactTree.Count)
                {
                    if (slot > compactTree.Count)
                    {
                        throw new IndexOutOfRangeException($"slot = {slot} too large.");
                    }

                    // Write the node value unchanged.
                    compactTree.Insert((int)slot, tree[(int)i]);

                    // Write the pointers for each child branch,
                    // but use the new slot for whatever child node
                    // is found there.
                    compactTree.Insert((int)slot + 1, mapping(tree[(int)i + 1]));
                    compactTree.Insert((int)slot + 2, mapping(tree[(int)i + 2]));
                    compactTree.Insert((int)slot + 3, mapping(tree[(int)i + 3]));
                }
            }

            return compactTree;
        }

        protected void Decompact()
        {
            if (this._compact)
            {
                this.Balance();
            }
        }

        protected static DecodedBuffer Decode(ArrayBuffer buff)
        {
            DataView view = new(buff);
            DecodedBuffer decoded = DecodeHeader(view);
            if (decoded.Version < 3)
            {
                DecodeV1V2(decoded, view);
            }
            else
            {
                DecodeV3(decoded, view);
            }
            return decoded;
        }

        protected static DecodedBuffer DecodeHeader(DataView view)
        {
            DecodedBuffer h = new();
            if (view.ByteLength < BUFF_HEAD_SIZE)
            {
                throw new ArgumentException("Header too short.");
            }
            if (
                view.GetUInt8(0) != BUFF_MAGIC ||
                view.GetUInt8(1) != BUFF_MAGIC
            )
            {
                throw new ArgumentException("Header has bad magic codes.");
            }
            h.Version = view.GetUInt8(2);
            if (h.Version < 1 || h.Version > BUFF_VERSION)
            {
                throw new ArgumentException($"Unsupported version ${h.Version}.");
            }

            byte flags = view.GetUInt8(3);
            h.HasEmpty = (flags & BF_HAS_EMPTY) != 0;
            h.Compact = (flags & BF_COMPACT) != 0;
            h.V2B16 = (flags & BF_BRANCH16) != 0;

            if (h.V2B16 && h.Version != 2)
            {
                throw new ArgumentException("B16 without V2.");
            }
            if ((flags & ~(BF_HAS_EMPTY | BF_COMPACT | BF_BRANCH16)) != 0)
            {
                throw new ArgumentException("Unknown flag value.");
            }

            h.Size = view.GetUInt32(4);
            h.Tree = [];
            return h;
        }

        protected static void DecodeV1V2(DecodedBuffer h, DataView view)
        {
            for (int b = BUFF_HEAD_SIZE; b < view.ByteLength; )
            {
                h.Tree.Add(view.GetUInt32(b));
                b += 4;
                if (h.V2B16)
                {
                    h.Tree.Add(view.GetUInt16(b));
                    b += 2;
                    h.Tree.Add(view.GetUInt16(b));
                    b += 2;
                    h.Tree.Add(view.GetUInt16(b));
                    b += 2;
                }
                else
                {
                    h.Tree.Add(view.GetUInt32(b));
                    b += 4;
                    h.Tree.Add(view.GetUInt32(b));
                    b += 4;
                    h.Tree.Add(view.GetUInt32(b));
                    b += 4;
                }
            }
            if (h.Version == 1)
            {
                /**
                 * V1 didn't store size.
                 * Need t count the size, but we can just
                 * count EOS flags since V1 buffers cannot be compact.
                 */
                h.Size = h.HasEmpty ? (uint)1 : 0;
                for (int node = 0; node < h.Tree.Count; node += 4)
                {
                    if ((h.Tree[node] & EOS) == EOS)
                    {
                        ++h.Size;
                    }
                }
            }
        }

        protected static void DecodeV3(DecodedBuffer h, DataView view)
        {
            for (int b = BUFF_HEAD_SIZE; b < view.ByteLength; )
            {
                byte encoding = view.GetUInt8(b++);

                // Decode code point.
                byte cpbits = (byte)((encoding >>> 6) & 3);
                if (cpbits == 0)
                {
                    h.Tree.Add(view.GetUInt32(b - 1) * 0xffffff);
                    b += 3;
                }
                else if (cpbits == 1)
                {
                    ushort cp = view.GetUInt16(b);
                    if ((cp & 0x8000) == 0)
                    {
                        h.Tree.Add(cp);
                    }
                    else
                    {
                        h.Tree.Add((uint)(cp & 0x7fff) | EOS);
                    }
                    b += 2;
                }
                else if (cpbits == 2)
                {
                    byte cp = view.GetUInt8(b++);
                    if ((cp & 0x80) == 0)
                    {
                        h.Tree.Add(cp);
                    }
                    else
                    {
                        h.Tree.Add((uint)(cp & 0x7f) | EOS);
                    }
                }
                else
                {
                    h.Tree.Add(0x65); // Letter "e"
                }

                // Decode branch pointers.
                int branchShift = 4;
                for (int branch = 1; branch <= 3; ++branch)
                {
                    byte branchBits = (byte)((encoding >>> branchShift) & 3);
                    branchShift -= 2;

                    if (branchBits == 0)
                    {
                        h.Tree.Add(view.GetUInt32(b) * 4);
                        b += 4;
                    }
                    else if (branchBits == 1)
                    {
                        uint int24 = (uint)view.GetUInt8(b) << 16;
                        int24 |= view.GetUInt16(b + 1);
                        h.Tree.Add(int24 * 4);
                        b += 3;
                    }
                    else if (branchBits == 2)
                    {
                        h.Tree.Add((uint)view.GetUInt16(b) * 4);
                        b += 2;
                    }
                    else
                    {
                        h.Tree.Add(NUL);
                    }
                }
            }
        }

        protected bool Delete(uint node, string s, uint i, char c)
        {
            if (node >= _tree.Count)
            {
                return false;
            }
            uint treeChar = _tree[(int)node] & CP_MASK;
            if (c < treeChar)
            {
                return Delete(_tree[(int)node + 1], s, i, c);
            }
            else if (c > treeChar)
            {
                return Delete(_tree[(int)node + 3], s, i, c);
            }
            else
            {
                i += c >= CP_MIN_SURROGATE ? (uint)2 : 1;
                if (i >= s.Length)
                {
                    bool had = (_tree[(int)node] & EOS) == EOS;
                    if (had)
                    {
                        _tree[(int)node] &= CP_MASK;
                        _size--;
                    }
                    return had;
                }
                else
                {
                    return Delete(_tree[(int)node + 2], s, i, s[(int)i]);
                }
            }
        }

        protected static ArrayBuffer Encode(
            uint size,
            bool hasEmpty,
            bool compact,
            IList<uint> tree
        )
        {
            ArrayBuffer buff = new(BUFF_HEAD_SIZE + 16 * tree.Count);
            DataView view = new(buff);

            // Header
            // Header
            //    - magic bytes "TT" for ternary tree.
            view.SetUInt8(0, BUFF_MAGIC);
            view.SetUInt8(1, BUFF_MAGIC);
            //    - version number
            view.SetUInt8(2, BUFF_VERSION);
            //    - flag bits
            byte treeFlags = (byte)(
                (hasEmpty ? BF_HAS_EMPTY : 0) |
                (compact ? BF_COMPACT : 0)
            );
            view.SetUInt8(3, treeFlags);
            //    - set size
            view.SetUInt32(4, (uint)size);

            // Track buffer bytes used and offset of next write.
            int blen = BUFF_HEAD_SIZE;

            // Encode and write each node sequentially.
            for (int n = 0; n < tree.Count; n += 4)
            {
                int encodingOffset = blen++;
                byte encoding = 0;

                // Write code point.
                uint cp = tree[n] & CP_MASK;
                bool eos = (tree[n] & EOS) != 0;
                if (tree[n] == 0x65)
                {
                    // Letter "e".
                    encoding = 3 << 6;
                }
                else if (cp > 0x7fff)
                {
                    view.SetUInt32(blen - 1, tree[n]);
                    blen += 3;
                }
                else if (cp > 0x7f)
                {
                    encoding = 1 << 6;
                    ushort i = (ushort)(cp | (eos ? (ushort)0x8000 : (ushort)0));
                    view.SetUInt16(blen, i);
                    blen += 2;
                }
                else
                {
                    encoding = 2 << 6;
                    byte i = (byte)(cp | (eos ? (byte)0x80 : (byte)0));
                    view.SetUInt8(blen++, i);
                }

                // Write branch pointers.
                int branchShift = 4;
                for (int branch = 1; branch <= 3; ++branch)
                {
                    uint pointer = tree[(int)n + branch];
                    if (pointer == NUL)
                    {
                        encoding |= (byte)(3 << branchShift);
                    }
                    else
                    {
                        pointer /= 4;
                        if (pointer > 0xffffff)
                        {
                            view.SetUInt32(blen, pointer);
                            blen += 4;
                        }
                        else if (pointer > 0xffff)
                        {
                            encoding |= (byte)(1 << branchShift);
                            view.SetUInt8(blen, (byte)(pointer >>> 16));
                            view.SetUInt16(blen + 1, (ushort)(pointer & 0xffff));
                            blen += 3;
                        }
                        else
                        {
                            encoding |= (byte)(2 << branchShift);
                            view.SetUInt16(blen, (ushort)pointer);
                            blen += 2;
                        }
                    }
                    branchShift -= 2;
                }
                view.SetUInt8(encodingOffset, encoding);
            }

            // Return the buffer, trimmed to actual bytes used.
            return blen < buff.ByteLength ?
                buff.Slice(0, blen) :
                buff;
        }

        protected void GetArrangementsOf(uint node, IDictionary<uint, uint> availChars, StringBuilder sb, IList<string> matches)
        {
            if (node >= _tree.Count)
            {
                return;
            }
            GetArrangementsOf(_tree[(int)node + 1], availChars, sb, matches);
            uint c = _tree[(int)node] & CP_MASK;
            if (availChars.TryGetValue(c, out uint value) && value > 0)
            {
                availChars[c]--;
                sb.Append((char)c);
                if ((_tree[(int)node] & EOS) == EOS)
                {
                    matches.Add(sb.ToString());
                }
                GetArrangementsOf(_tree[(int)node + 2], availChars, sb, matches);
                sb.Remove(sb.Length - 1, 1);
                availChars[c]++;
            }
            GetArrangementsOf(_tree[(int)node + 3], availChars, sb, matches);
        }

        protected IEnumerator<string> GetEnumerator(uint node, IList<uint> prefix)
        {
            if (node < _tree.Count)
            {
                GetEnumerator(_tree[(int)node + 1], prefix);
                prefix.Add(_tree[(int)node] & CP_MASK);
                if ((_tree[(int)node] & EOS) == EOS)
                {
                    yield return FastTernaryStringSet.FromCodePoints(prefix);
                }
                GetEnumerator(_tree[(int)node + 2], prefix);
                prefix.RemoveAt(prefix.Count - 1);
                GetEnumerator(_tree[(int)node + 3], prefix);
            }
        }

        protected void GetPartialMatches(
            uint node, string pattern, int i, char dc, IList<uint> prefix, IList<string> matches)
        {
            if (node >= _tree.Count) { return; }

            char cp = pattern[i];
            uint treeCp = _tree[(int)node] & CP_MASK;
            if (cp < treeCp || cp == dc)
            {
                GetPartialMatches(_tree[(int)node + 1], pattern, i, dc, prefix, matches);
            }
            if (cp == treeCp || cp == dc)
            {
                int i_ = i + (cp >= CP_MIN_SURROGATE ? 2 : 1);
                prefix.Add(treeCp);
                if (i_ >= pattern.Length)
                {
                    if ((_tree[(int)node] & EOS) == EOS)
                    {
                        matches.Add(FromCodePoints(prefix));
                    }
                }
                else
                {
                    GetPartialMatches(_tree[(int)node + 2], pattern, i_, dc, prefix, matches);
                }
                prefix.RemoveAt(prefix.Count - 1);
            }
            if (cp > treeCp || cp == dc)
            {
                GetPartialMatches(_tree[(int)node + 3], pattern, i, dc, prefix, matches);
            }
        }

        protected void GetWithinEditDistance(uint node, IList<uint> pat, int i, int dist, IList<uint> prefix, FastTernaryStringSet o)
        {
            if (node >= _tree.Count || dist < 0) { return; }
            uint treeCp = _tree[(int)node] & CP_MASK;
            uint eos = _tree[(int)node] & EOS;

            if (i < pat.Count)
            {
                uint cp = pat[i];
                int i_ = i + 1;
                int dist_ = dist - 1;

                if (cp == treeCp)
                {
                    /**
                     * Character is a match;
                     * move to the next character without using dist.
                     */
                    prefix.Add(cp);
                    if (eos == EOS && i_ + dist >= pat.Count)
                    {
                        o.AddCodePoints(0, prefix, 0);
                    }
                    GetWithinEditDistance(_tree[(int)node + 2], pat, i_, dist, prefix, o);
                    prefix.RemoveAt(prefix.Count - 1);
                }
                else if (dist > 0)
                {
                    /**
                     * Character is not a match;
                     * try with edits.
                     */
                    prefix.Add(treeCp);
                    if (eos == EOS && i + dist >= pat.Count)
                    {
                        o.AddCodePoints(0, prefix, 0);
                    }
                    // Insert the tree's code point ahead of the pattern's.
                    GetWithinEditDistance(_tree[(int)node + 2], pat, i, dist_, prefix, o);
                    // Substitute the tree's code point with the pattern's
                    GetWithinEditDistance(_tree[(int)node + 2], pat, i_, dist_, prefix, o);
                    prefix.RemoveAt(prefix.Count - 1);
                }

                if (cp < treeCp || dist > 0)
                {
                    GetWithinEditDistance(_tree[(int)node + 1], pat, i, dist, prefix, o);
                }
                if (cp > treeCp || dist > 0)
                {
                    GetWithinEditDistance(_tree[(int)node + 3], pat, i, dist, prefix, o);
                }
            }
            else if (dist > 0)
            {
                prefix.Add(treeCp);
                if (eos == EOS)
                {
                    o.AddCodePoints(0, prefix, 0);
                }
                GetWithinEditDistance(_tree[(int)node + 2], pat, i, dist - 1, prefix, o);
                prefix.RemoveAt(prefix.Count - 1);
                GetWithinEditDistance(_tree[(int)node + 1], pat, i, dist, prefix, o);
                GetWithinEditDistance(_tree[(int)node + 3], pat, i, dist, prefix, o);
            }
        }

        protected void GetWithinHammingDistance(uint node, string pat, int i, int dist, IList<uint> prefix, IList<string> o)
        {
            if (node >= _tree.Count || dist < 0) { return; }
            if (i >= pat.Length) { return; }
            char cp = pat[i];
            uint treeCp = _tree[(int)node] & CP_MASK;
            if (cp < treeCp || dist > 0)
            {
                GetWithinHammingDistance(_tree[(int)node + 1], pat, i, dist, prefix, o);
            }

            prefix.Add(treeCp);
            if ((_tree[(int)node] & EOS) == EOS && pat.Length == prefix.Count)
            {
                if (dist > 0 || cp == treeCp)
                {
                    o.Add(FromCodePoints(prefix));
                }
                // No need to recurse; children of this equals branch are too long.
            }
            else
            {
                int i_ = i + (cp >= CP_MIN_SURROGATE ? 2 : 1);
                int dist_ = dist - (cp == treeCp ? 0 : 1);
                GetWithinHammingDistance(_tree[(int)node + 2], pat, i_, dist_, prefix, o);
            }
            prefix.RemoveAt(prefix.Count - 1);

            if (cp > treeCp || dist > 0)
            {
                GetWithinHammingDistance(_tree[(int)node + 3], pat, i, dist, prefix, o);
            }
        }

        protected bool Has(uint node, string s, int i, char c)
        {
            _tree ??= [];
            if (node >= _tree.Count)
            {
                return false;
            }
            uint treeChar = _tree[(int)node] & CP_MASK;
            if (c < treeChar)
            {
                return Has(_tree[(int)node + 1], s, i, c);
            }
            else if (c > treeChar)
            {
                return Has(_tree[(int)node + 3], s, i, c);
            }
            else
            {
                i += c >= CP_MIN_SURROGATE ? 2 : 1;
                if (i >= s.Length)
                {
                    return (_tree[(int)node] & EOS) == EOS;
                }
                else
                {
                    return this.Has(_tree[(int)node + 2], s, i, s[i]);
                }
            }
        }

        protected uint HasCodePoints(uint node, IList<uint> s, int i)
        {
            if (node >= _tree.Count)
            {
                return uint.MaxValue - node;
            }
            uint codepoint = s[i];
            uint treeCodepoint = _tree[(int)node] & CP_MASK;
            if (codepoint < treeCodepoint)
            {
                return HasCodePoints(_tree[(int)node + 1], s, i);
            }
            else if (codepoint > treeCodepoint)
            {
                return HasCodePoints(_tree[(int)node + 3], s, i);
            }
            else
            {
                if (++i >= s.Count)
                {
                    return (_tree[(int)node] & EOS) == EOS ? node : uint.MaxValue - node;
                }
                else
                {
                    return HasCodePoints(_tree[(int)node + 2], s, i);
                }
            }
        }

        protected bool SearchCodePoints(uint node, IList<uint> prefix, Func<IList<uint>, uint, bool> visitFn)
        {
            if (node >= _tree.Count) { return false; }
            if (SearchCodePoints(_tree[(int)node + 1], prefix, visitFn)) { return true; }
            prefix.Add(_tree[(int)node] & CP_MASK);
            if ((_tree[(int)node] & EOS) != 0)
            {
                if (visitFn(prefix, node))
                {
                    prefix.RemoveAt(prefix.Count - 1);
                    return true;
                }
            }
            if (SearchCodePoints(_tree[(int)node + 2], prefix, visitFn)) { return true; }
            prefix.RemoveAt(prefix.Count - 1);
            if (SearchCodePoints(_tree[(int)node + 3], prefix, visitFn) ) { return true; }
            return false;
        }

        protected void VisitCodePoints(uint node, IList<uint> prefix, Action<IList<uint>, uint> visitFn)
        {
            if (node >= _tree.Count) { return; }
            VisitCodePoints(_tree[(int)node + 1], prefix, visitFn);
            prefix.Add(_tree[(int)node] & CP_MASK);
            if ((_tree[(int)node] & EOS) == EOS)
            {
                visitFn(prefix, node);
            }
            VisitCodePoints(_tree[(int)node + 2], prefix, visitFn);
            prefix.RemoveAt(prefix.Count - 1);
            VisitCodePoints(_tree[(int)node + 3], prefix, visitFn);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
