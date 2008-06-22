using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Noding;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// Validates that a collection of SegmentStrings is correctly noded.
    /// Throws an appropriate exception if an noding error is found.
    /// </summary>
    public class EdgeNodingValidator<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        private static IEnumerable<NodedSegmentString<TCoordinate>> toSegmentStrings(IEnumerable<Edge<TCoordinate>> edges)
        {
            foreach (Edge<TCoordinate> e in edges)
            {
                yield return new NodedSegmentString<TCoordinate>(e.Coordinates, e);
            }
        }

        private readonly NodingValidator<TCoordinate> _nodingValidator;

        public EdgeNodingValidator(IGeometryFactory<TCoordinate> geoFactory, IEnumerable<Edge<TCoordinate>> edges)
        {
            _nodingValidator = new NodingValidator<TCoordinate>(geoFactory, toSegmentStrings(edges));
        }

        public void CheckValid()
        {
            _nodingValidator.CheckValid();
        }
    }
}