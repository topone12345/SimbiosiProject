using System.Collections.Generic;

namespace SimbiosiClientLib.Entities.Tags
{
    internal class TagsCollectionBucket
    {

        internal TagsCollectionBucket()
        {
            NestedTable = new Dictionary<string, TagsCollectionBucket>();
            Tags =  new HashSet<SubscribeTag>();
        }

        /// <summary>
        /// Gets the nested table.
        /// </summary>
        /// <value>
        /// The nested table.
        /// </value>
        internal Dictionary<string, TagsCollectionBucket> NestedTable { get; }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <value>
        /// The tags.
        /// </value>
        internal HashSet<SubscribeTag> Tags { get; }


        /// <summary>
        /// Adds the tag to list.
        /// </summary>
        /// <param name="tag">The tag.</param>
        internal void AddTagToList(SubscribeTag tag)
        {
            if (!Tags.Contains(tag)) Tags.Add(tag);
        }

    }
}
