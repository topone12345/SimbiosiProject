using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimbiosiClientLib.Entities.Tags
{
    public class TagsCollection : ICollection<SubscribeTag>
    {
        #region fields
        private TagsCollectionBucket _table;
        private List<SubscribeTag> _innerList;
        #endregion

        #region .Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="TagsCollection"/> class.
        /// </summary>
        public TagsCollection()
        {
            _table = new TagsCollectionBucket();
            _innerList = new List<SubscribeTag>();
        }
        #endregion

        #region ICollection
        public IEnumerator<SubscribeTag> GetEnumerator()
        {
            return _innerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _innerList).GetEnumerator();
        }

        public void Add(SubscribeTag item)
        {
            _innerList.Add(item);
            AddToTable(item,0,_table);
        }

        private static void AddToTable(SubscribeTag item, int i, TagsCollectionBucket addingTable)
        {
            if (i == item.TokensCount)
            {
                
                addingTable.AddTagToList(item);
                return;
            }


            var value = item[i];

            TagsCollectionBucket newBucket;

            if (!addingTable.NestedTable.ContainsKey(value))
            {
                newBucket = new TagsCollectionBucket();
                addingTable.NestedTable.Add(value, newBucket);
            }
            else newBucket = addingTable.NestedTable[value];

            if(value!=Tag.WILDCARD_ENDWITH.ToString())
                AddToTable(item, ++i, newBucket);
            else 
                newBucket.AddTagToList(item);

        }

        private static void RemoveFromTable(SubscribeTag item, int i, TagsCollectionBucket table)
        {
            if (item.IsAnEndWithSubject)
            {
                if (table.NestedTable.ContainsKey(Tag.WILDCARD_ENDWITH.ToString()))
                {
                    var subTable = table.NestedTable[Tag.WILDCARD_ENDWITH.ToString()];
                    subTable.Tags.Remove(item);
                    if (subTable.Tags.Count == 0)
                        table.NestedTable.Remove(Tag.WILDCARD_ENDWITH.ToString());
                }

                return;
            }


            if (i == item.TokensCount)
            {

                table.Tags.Remove(item);

                

                return;
            }

            var value = item[i];

            if (table.NestedTable.ContainsKey(value))
            {
                var nestedTable = table.NestedTable[value];
                RemoveFromTable(item,++i,nestedTable);
                if (nestedTable.Tags.Count == 0)
                    table.NestedTable.Remove(value);
            }

        }

        public void Clear()
        {
            _innerList.Clear();
            _table.NestedTable.Clear();
            _table.Tags.Clear();
        }

        public bool Contains(SubscribeTag item)
        {
            return _innerList.Contains(item);
        }

        public bool Match(MessageTag item)
        {
            return Match(item, 0, _table, false);
        }


        private static bool Match(MessageTag item, int i, TagsCollectionBucket bucket, bool searchtagsOnPath)
        {
            
            if(i==item.TokensCount || searchtagsOnPath)
                if (MatchInList(item, bucket.Tags)) return true;
            
            if(i==0 && bucket.NestedTable.ContainsKey(Tag.WILDCARD_ENDWITH.ToString()))
            {
                if (MatchInList(item, bucket.NestedTable[Tag.WILDCARD_ENDWITH.ToString()].Tags)) return true;
            }

            if (i<item.TokensCount)
            {
                var value = item[i];
                

                if(bucket.NestedTable.ContainsKey(value))
                    if (Match(item, ++i, bucket.NestedTable[value], false)) return true;

                if (bucket.NestedTable.ContainsKey(Tag.WILDCARD_JOLLY.ToString()))
                    if (Match(item, ++i, bucket.NestedTable[Tag.WILDCARD_JOLLY.ToString()], true)) return true;

                if (bucket.NestedTable.ContainsKey(Tag.WILDCARD_STARTWITH.ToString()))
                    if (Match(item, ++i, bucket.NestedTable[Tag.WILDCARD_STARTWITH.ToString()], true)) return true;
            }


            return false;


        }

        private static bool MatchInList(MessageTag item, HashSet<SubscribeTag> tags)
        {
            foreach (var subscribeTag in tags)
            {
                if (subscribeTag.MatchWith(item))
                    return true;
            }

            return false;
        }

        public void CopyTo(SubscribeTag[] array, int arrayIndex)
        {
            _innerList.CopyTo(array, arrayIndex);
        }

        public bool Remove(SubscribeTag item)
        {
            RemoveFromTable(item,0,_table);
            return _innerList.Remove(item);
        }

        public int Count => _innerList.Count;

        public bool IsReadOnly => ((ICollection<SubscribeTag>) _innerList).IsReadOnly;

        #endregion
    }
}
