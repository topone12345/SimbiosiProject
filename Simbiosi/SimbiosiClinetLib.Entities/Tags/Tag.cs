using System;
using System.Collections;
using System.Collections.Generic;

namespace SimbiosiClientLib.Entities.Tags
{
    /// <summary>
    /// Defines a tag used for message tagging or subscribing
    /// </summary>
    /// <seealso cref="string" />
    /// <seealso cref="Tag" />
    public abstract  class Tag : IEnumerable<string>, IEquatable<Tag>
    {



        #region fields

        private readonly List<string> _tokens;
        private bool? _hasWildcards;
        private readonly string _tagString;
        #endregion

        #region Constants

        protected const char WILDCARD_JOLLY = '*';
        protected const char WILDCARD_STARTWITH = '>';
        protected internal const char WILDCARD_ENDWITH = '<';
        protected const string RESERVED_USE_TOKEN = "$";
        protected static List<string> WILDCARDS = new List<string> {"*", "<", ">"};

        #endregion

        #region .Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        /// <param name="tokens">The tokens of the tag.</param>
        /// <example>MESSAGE.TRANSACTIONS.MASTERDATABASE</example>
        protected Tag(List<string> tokens)
        {
            _tokens = tokens;
            _tagString = string.Join(".", _tokens);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        /// <param name="tagString">The tag string.</param>
        protected Tag(string tagString)
        {
            _tokens = new List<string>(tagString.Split('.'));
            _tagString = tagString;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return _tagString;
        }



        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)_tokens).GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<string>)_tokens).GetEnumerator();
        }

        #endregion


        #region Properties

        /// <summary>
        /// Gets the token <see cref="System.String"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="System.String"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The token at the specified index</returns>
        public string this[int index] => _tokens[index];


        /// <summary>
        /// Gets a value indicating whether this instance is a subscribe tag.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is subscribe tag otherwise, <c>false</c>. If this is not a valid subsribe tag can be a message tag
        /// </value>
        public bool HasWildcards
        {
            get
            {
                if (_hasWildcards.HasValue)
                    return _hasWildcards.Value;

                foreach (var token in _tokens)
                {
                    if (WILDCARDS.Contains(token))
                    {
                        _hasWildcards = true;
                        return true;
                    }
                }

                _hasWildcards = false;
                return false;
            }
        }


        /// <summary>
        /// Gets a value indicating whether this instance is reserved use tag.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is reserved use tag; otherwise, <c>false</c>.
        /// </value>
        public bool IsReservedUseTag => this[0].Equals(RESERVED_USE_TOKEN);


        /// <summary>
        /// Gets the tokens count.
        /// </summary>
        /// <value>
        /// The tokens count.
        /// </value>
        public int TokensCount => _tokens.Count;


        /// <summary>
        /// Gets the last token.
        /// </summary>
        /// <value>
        /// The last token.
        /// </value>
        public string LastToken => _tokens[_tokens.Count - 1];

        /// <summary>
        /// Gets the first token.
        /// </summary>
        /// <value>
        /// The first token.
        /// </value>
        public string FirstToken => _tokens[0];
        #endregion

        #region Equality
        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Tag other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(_tagString, other._tagString);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            var other = obj as Tag;
            return other != null && Equals(other);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return _tagString?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Tag left, Tag right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Tag left, Tag right)
        {
            return !Equals(left, right);
        }
        #endregion
    }
}
