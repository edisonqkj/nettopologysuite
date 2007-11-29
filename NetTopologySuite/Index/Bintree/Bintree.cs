using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.DataStructures;

namespace GisSharpBlog.NetTopologySuite.Index.Bintree
{
    /// <summary>
    /// A <see cref="BinTree{TItem}"/> indexes values along a field of values
    /// (such as ℝ - the set of real numbers), to decompose the field into 
    /// nested intervals. This nesting allows searches on a range of values.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="BinTree{TItem}"/> (or "Binary Interval Tree")
    /// is a 1-dimensional version of a quadtree. It indexes 1-dimensional 
    /// intervals (which of course may be the projection of 2-D objects on an axis).
    /// It supports range searching (where the range may be a single point).
    /// This implementation does not require specifying the extent of the inserted
    /// items beforehand.  It will automatically expand to accomodate any extent
    /// of dataset.
    /// </para>
    /// <para>
    /// This index is different to the Interval Tree of Edelsbrunner
    /// or the Segment Tree of Bentley.
    /// </para>
    /// </remarks>
    public class BinTree<TItem>
    {
        /// <summary>
        /// Ensure that the Interval for the inserted item has non-zero extents.
        /// Use the current minExtent to pad it, if necessary.
        /// </summary>
        public static Interval EnsureExtent(Interval itemInterval, Double minExtent)
        {
            Double min = itemInterval.Min;
            Double max = itemInterval.Max;

            // has a non-zero extent
            if (min != max)
            {
                return itemInterval;
            }

            // pad extent
            if (min == max)
            {
                min = min - minExtent / 2.0;
                max = min + minExtent / 2.0;
            }

            return new Interval(min, max);
        }

        private readonly Root<TItem> _root = new Root<TItem>();

        /*
        * Statistics:
        * _minExtent is the minimum extent of all items
        * inserted into the tree so far. It is used as a heuristic value
        * to construct non-zero extents for features with zero extent.
        * Start with a non-zero extent, in case the first feature inserted has
        * a zero extent in both directions.  This value may be non-optimal, but
        * only one feature will be inserted with this value.
        **/
        private Double _minExtent = 1.0;

        public Int32 Depth
        {
            get
            {
                Debug.Assert(_root != null);
                return _root.Depth;
            }
        }

        public Int32 Count
        {
            get
            {
                Debug.Assert(_root != null);
                return _root.Count;
            }
        }

        /// <summary>
        /// Compute the total number of nodes in the tree.
        /// </summary>
        /// <returns>The number of nodes in the tree.</returns>
        public Int32 NodeSize
        {
            get
            {
                Debug.Assert(_root != null);
                return _root.NodeCount;
            }
        }

        public void Insert(Interval itemInterval, TItem item)
        {
            collectStats(itemInterval);
            Interval insertInterval = EnsureExtent(itemInterval, _minExtent);
            _root.Insert(insertInterval, item);
        }

        public IEnumerable<TItem> Query(Double x)
        {
            return Query(new Interval(x, x));
        }

        /// <remarks>
        /// <paramref name="interval"/> might have a 0 width, representing a point.
        /// </remarks>
        public IEnumerable<TItem> Query(Interval interval)
        {
            // The items that are matched are all items in intervals
            // which overlap the query interval
            return _root.Query(interval);
        }

        //public void Query(Interval interval, IList foundItems)
        //{
        //    _root.AddAllItemsFromOverlapping(interval, foundItems);
        //}

        private void collectStats(Interval interval)
        {
            Double del = interval.Width;

            if (del < _minExtent && del > 0.0)
            {
                _minExtent = del;
            }
        }
    }
}