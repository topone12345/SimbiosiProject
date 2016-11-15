using System;
using System.Collections.Generic;

namespace SimbiosiClientLib.Entities.Tags
{
    public class SubscribeTag : Tag
    {
        #region .Ctors
        /// <summary>
        /// Initializes a new instance of the <see cref="SubscribeTag"/> class.
        /// </summary>
        /// <param name="tokens">The tokens of the tag.</param>
        /// <example>MESSAGE.TRANSACTIONS.MASTERDATABASE</example>
        public SubscribeTag(List<string> tokens) : base(tokens)
        {
            CheckTokens();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubscribeTag"/> class.
        /// </summary>
        /// <param name="tagString">The tag string.</param>
        public SubscribeTag(string tagString) : base(tagString)
        {
            CheckTokens();
        }

        private void CheckTokens()
        {
            for (int i = 0; i < TokensCount; ++i)
            {
                if(this[i].Equals(WILDCARD_ENDWITH.ToString()) && i>0)
                    throw new ArgumentException("The wildcard for endwith \"<\" can be used only in the first token of the tag. Example <.TRANSACTIONS.152 (means all tag ends with TRANSACTIONS.152)");

                if (this[i].Equals(WILDCARD_STARTWITH.ToString()) && i < TokensCount-1)
                    throw new ArgumentException("The wildcard for startwith \">\" can be used only in the last token of the tag. Example DATA.TRANSACTIONS.> (means all tag starts with DATA.TRANSACTIONS)");


            }
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets a value indicating whether this instance is a start with subject.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is a start with subject; otherwise, <c>false</c>.
        /// </value>
        public bool IsAStartWithSubject => LastToken.Equals(WILDCARD_STARTWITH.ToString());

        /// <summary>
        /// Gets a value indicating whether this instance is an end with subject.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is an end with subject; otherwise, <c>false</c>.
        /// </value>
        public bool IsAnEndWithSubject => FirstToken.Equals(WILDCARD_ENDWITH.ToString());
        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the subscription tag match with the message tag otherwise return false
        /// </summary>
        /// <param name="messageTag">The message tag.</param>
        /// <returns>A boolean rappresenting the match</returns>
        public bool MatchWith(MessageTag messageTag)
        {
            if (Equals(messageTag))
                return true;

            if (IsAStartWithSubject && IsAnEndWithSubject) // ">" case only
                return true;

            if (IsAnEndWithSubject && TokensCount == 1) //"<" case only
                return true;


            int subscribeIndex = 0;
            for (int i = 0; i < messageTag.TokensCount; ++i)
            {
         

        


                if (i == 0 && IsAnEndWithSubject)
                {
                    bool found = false;
                    ++subscribeIndex;
                    for (int k = 0; k < messageTag.TokensCount; ++k)
                    {
                        if (messageTag[k] == this[subscribeIndex])
                        {
                            found = true;
                            i = k;

                            if (i == messageTag.TokensCount-1)
                                return true; //found on the last

                            break;
                        }
                    }

                    if(!found)
                        return false;
                }



                if (this[subscribeIndex] == WILDCARD_STARTWITH.ToString())
                    return true;

                if (messageTag[i] == this[subscribeIndex] || this[subscribeIndex] == WILDCARD_JOLLY.ToString())
                {
                    ++subscribeIndex;

                    if (i == messageTag.TokensCount-1 && subscribeIndex == TokensCount)
                        return true;


                    if (subscribeIndex == TokensCount)
                        return false;


                    continue;
                }

                return false;
            }


            return false;

        }

        #endregion
    }
}
