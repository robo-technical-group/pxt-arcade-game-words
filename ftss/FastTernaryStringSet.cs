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
            ArgumentNullException.ThrowIfNull(pattern);
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
            IList<int> pat = FastTernaryStringSet.ToCodePoints(suffix);

            VisitCodePoints(0, [], (IList<int> s, int q) =>
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
            FastTernaryStringSet patterns = new([pattern,]);
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
                            int[] delete1 = new int[cp.Count - 1];
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
        protected int AddCodePoints(int node, IList<int> s, int i)
        {
            int cp = s[i];
            if (node >= _tree.Count)
            {
                node = _tree.Count;
                ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(node, NODE_CEILING);
            }
            _tree.Add(cp);
            _tree.Add(NUL);
            _tree.Add(NUL);
            _tree.Add(NUL);

            int treeCp = _tree[node] & CP_MASK;
            if (cp < treeCp)
            {
                _tree[node + 1] = AddCodePoints(_tree[node + 1], s, i);
            }
            else if (cp > treeCp)
            {
                _tree[node + 3] = AddCodePoints(_tree[node + 3], s, i);
            }
            else
            {
                i += (cp >= CP_MIN_SURROGATE ? 2 : 1);
                if (i >= s.Count)
                {
                    if ((_tree[node] & EOS) == 0)
                    {
                        _tree[node] |= EOS;
                        _size++;
                    }
                }
                else
                {
                    _tree[node + 2] = AddCodePoints(_tree[node + 2], s, i);
                }
            }

            return node;
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

        protected void GetPartialMatches(
            int node, string pattern, int i, char dc, IList<int> prefix, IList<string> matches)
        {
            if (node >= _tree.Count) { return; }

            char cp = pattern[i];
            int treeCp = _tree[node] & CP_MASK;
            if (cp < treeCp || cp == dc)
            {
                GetPartialMatches(_tree[node + 1], pattern, i, dc, prefix, matches);
            }
            if (cp == treeCp || cp == dc)
            {
                int i_ = i + (cp >= CP_MIN_SURROGATE ? 2 : 1);
                prefix.Add(treeCp);
                if (i_ >= pattern.Length)
                {
                    if ((_tree[node] & EOS) == EOS)
                    {
                        matches.Add(FromCodePoints(prefix));
                    }
                }
                else
                {
                    GetPartialMatches(_tree[node + 2], pattern, i_, dc, prefix, matches);
                }
                prefix.RemoveAt(prefix.Count - 1);
            }
            if (cp > treeCp || cp == dc)
            {
                GetPartialMatches(_tree[node + 3], pattern, i, dc, prefix, matches);
            }
        }

        protected void GetWithinEditDistance(int node, IList<int> pat, int i, int dist, IList<int> prefix, FastTernaryStringSet o)
        {
            if (node >= _tree.Count || dist < 0) { return; }
            int treeCp = _tree[node] & CP_MASK;
            int eos = _tree[node] & EOS;

            if (i < pat.Count)
            {
                int cp = pat[i];
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
                    GetWithinEditDistance(_tree[node + 2], pat, i_, dist, prefix, o);
                    prefix.Remove(prefix.Count - 1);
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
                    GetWithinEditDistance(_tree[node + 2], pat, i, dist_, prefix, o);
                    // Substitute the tree'd code point with the pattern's
                    GetWithinEditDistance(_tree[node + 2], pat, i_, dist_, prefix, o);
                    prefix.Remove(prefix.Count - 1);
                }

                if (cp < treeCp || dist > 0)
                {
                    GetWithinEditDistance(_tree[node + 1], pat, i, dist, prefix, o);
                }
                if (cp > treeCp || dist > 0)
                {
                    GetWithinEditDistance(_tree[node + 3], pat, i, dist, prefix, o);
                }
            }
            else if (dist > 0)
            {
                prefix.Add(treeCp);
                if (eos == EOS)
                {
                    o.AddCodePoints(0, prefix, 0);
                }
                GetWithinEditDistance(_tree[node + 2], pat, i, dist - 1, prefix, o);
                prefix.Remove(prefix.Count - 1);
                GetWithinEditDistance(_tree[node + 1], pat, i, dist, prefix, o);
                GetWithinEditDistance(_tree[node + 3], pat, i, dist, prefix, o);
            }
        }

        protected void GetWithinHammingDistance(int node, string pat, int i, int dist, IList<int> prefix, IList<string> o)
        {
            if (node >= _tree.Count || dist < 0) { return; }
            char cp = pat[i];
            int treeCp = _tree[node] & CP_MASK;
            if (cp < treeCp || dist > 0)
            {
                GetWithinHammingDistance(_tree[node + 1], pat, i, dist, prefix, o);
            }

            prefix.Add(treeCp);
            if ((_tree[node] & EOS) == EOS && pat.Length == prefix.Count)
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
                GetWithinHammingDistance(_tree[node + 2], pat, i_, dist_, prefix, o);
            }
            prefix.RemoveAt(prefix.Count - 1);

            if (cp > treeCp || dist > 0)
            {
                GetWithinHammingDistance(_tree[node + 3], pat, i, dist, prefix, o);
            }
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
