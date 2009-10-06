using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Simplify
{
    public class TaggedLineString<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        private readonly ILineString<TCoordinate> _parentLine;
        private readonly List<TaggedLineSegment<TCoordinate>> _segs = new List<TaggedLineSegment<TCoordinate>>();
        private readonly List<TaggedLineSegment<TCoordinate>> _resultSegs = new List<TaggedLineSegment<TCoordinate>>();
        private readonly Int32 _minimumSize;

        public TaggedLineString(ILineString<TCoordinate> parentLine) : this(parentLine, 2) {}

        public TaggedLineString(ILineString<TCoordinate> parentLine, Int32 minimumSize)
        {
            _parentLine = parentLine;
            _minimumSize = minimumSize;

            Int32 index = 0;

            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(_parentLine.Coordinates))
            {
                TaggedLineSegment<TCoordinate> seg = new TaggedLineSegment<TCoordinate>(
                    pair.First, pair.Second, _parentLine, index);

                _segs.Add(seg);

                index += 1;
            }
        }

        public Int32 MinimumSize
        {
            get { return _minimumSize; }
        }

        public ILineString<TCoordinate> Parent
        {
            get { return _parentLine; }
        }

        public IList<TCoordinate> ParentCoordinates
        {
            get { return _parentLine.Coordinates; }
        }

        public IEnumerable<TCoordinate> ResultCoordinates
        {
            get { return extractCoordinates(_resultSegs); }
        }

        public Int32 ResultSize
        {
            get
            {
                Int32 resultSegsSize = _resultSegs.Count;
                return resultSegsSize == 0 ? 0 : resultSegsSize + 1;
            }
        }

        public IList<TaggedLineSegment<TCoordinate>> Segments
        {
            get { return _segs.AsReadOnly(); }
        }

        public void AddToResult(TaggedLineSegment<TCoordinate> seg)
        {
            _resultSegs.Add(seg);
        }

        public ILineString<TCoordinate> AsLineString()
        {
            return _parentLine.Factory.CreateLineString(ResultCoordinates);
        }

        public ILinearRing<TCoordinate> AsLinearRing()
        {
            return _parentLine.Factory.CreateLinearRing(ResultCoordinates);
        }

        private static IEnumerable<TCoordinate> extractCoordinates(IEnumerable<TaggedLineSegment<TCoordinate>> segs)
        {
            TaggedLineSegment<TCoordinate> lastSegment = null;

            foreach (TaggedLineSegment<TCoordinate> segment in segs)
            {
                yield return segment.LineSegment.P0;
                lastSegment = segment;
            }

            if (lastSegment != null)
            {
                yield return lastSegment.LineSegment.P1;
            }
        }
    }
}