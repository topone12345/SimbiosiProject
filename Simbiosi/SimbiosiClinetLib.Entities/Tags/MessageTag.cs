using System;
using System.Collections.Generic;

namespace SimbiosiClientLib.Entities.Tags
{
    /// <summary>
    /// Defines a Tag of a specific message
    /// </summary>
    /// <seealso cref="Tag" />
    public class MessageTag : Tag
    {

        #region .Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageTag"/> class.
        /// </summary>
        /// <param name="tokens">The tokens of the tag.</param>
        /// <example>MESSAGE.TRANSACTIONS.MASTERDATABASE</example>
        public MessageTag(List<string> tokens) : base(tokens)
        {
            if(HasWildcards)
                throw new ArgumentException("A message tag can not contain wildcard chars *, <, >", nameof(tokens));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageTag"/> class.
        /// </summary>
        /// <param name="tagString">The tag string.</param>
        public MessageTag(string tagString) : base(tagString)
        {
            if (HasWildcards)
                throw new ArgumentException("A message tag can not contain wildcard chars *, <, >", nameof(tagString));
        }
        #endregion
    }
}
