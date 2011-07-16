using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Nodes a set of <see cref="NodedSegmentString{TCoordinate}" />s by
    /// performing a brute-force comparison of every segment to every other one.
    /// This has n^2 performance, so is too slow for use on large numbers of segments.
    /// </summary>
    public class SimpleNoder<TCoordinate> : SinglePassNoder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleNoder{TCoordinate}"/> class.
        /// </summary>
        public SimpleNoder(ISegmentIntersector<TCoordinate> segInt)
            : base(segInt)
        {
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="NodedSegmentString{TCoordinate}" />s
        /// and returns an <see cref="IEnumerable{T}"/> of fully noded 
        /// <see cref="NodedSegmentString{TCoordinate}" />s.
        /// The <see cref="NodedSegmentString{TCoordinate}" />s have the same context as their parent.
        /// Some noders may add all these nodes to the input <see cref="NodedSegmentString{TCoordinate}" />s;
        /// others may only add some or none at all.
        /// </summary>
        public override IEnumerable<ISegmentString<TCoordinate>> Node(
            IEnumerable<ISegmentString<TCoordinate>> inputSegStrings)
        {
            foreach (ISegmentString<TCoordinate> edge0 in inputSegStrings)
            {
                foreach (ISegmentString<TCoordinate> edge1 in inputSegStrings)
                {
                    computeIntersects(edge0, edge1);
                }
            }

            return NodedSegmentString<TCoordinate>.GetNodedSubstrings(inputSegStrings);
        }

        public override void ComputeNodes(IEnumerable<ISegmentString<TCoordinate>> inputSegStrings)
        {
            foreach (ISegmentString<TCoordinate> edge0 in inputSegStrings)
            {
                foreach (ISegmentString<TCoordinate> edge1 in inputSegStrings)
                {
                    computeIntersects(edge0, edge1);
                }
            }
        }

        public override IEnumerable<TNodingResult> Node<TNodingResult>(
            IEnumerable<ISegmentString<TCoordinate>> segmentStrings,
            Func<ISegmentString<TCoordinate>, TNodingResult> generator)
        {
            foreach (ISegmentString<TCoordinate> segmentString in Node(segmentStrings))
            {
                yield return generator(segmentString);
            }
        }

        private void computeIntersects(ISegmentString<TCoordinate> e0, ISegmentString<TCoordinate> e1)
        {
            IEnumerator<TCoordinate> pts0 = e0.Coordinates.GetEnumerator();
            IEnumerator<TCoordinate> pts1 = e1.Coordinates.GetEnumerator();

            Int32 i0 = 0, i1 = 0;

            while (pts0.MoveNext())
            {
                while (pts1.MoveNext())
                {
                    SegmentIntersector.ProcessIntersections(e0, i0, e1, i1);

                    i1 += 1;
                }

                i0 += 1;
            }
        }
    }
}