using System;
using System.Collections;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Geometries.Utilities
{
    /// <summary> 
    /// Extracts all the 2-dimensional (<see cref="Polygon{TCoordinate}" />) components from 
    /// a <see cref="Geometry{TCoordinate}"/>.
    /// </summary>
    public class PolygonExtracter<TCoordinate> : IGeometryFilter<TCoordinate>
         where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>, 
                             IComputable<TCoordinate>, IConvertible
    {
        /// <summary> 
        /// Returns the Polygon components from a single point.
        /// If more than one point is to be processed, it is more
        /// efficient to create a single <c>PolygonExtracterFilter</c> instance
        /// and pass it to multiple geometries.
        /// </summary>
        public static IList GetPolygons(IGeometry<TCoordinate> geom)
        {
            IList comps = new ArrayList();
            geom.Apply(new PolygonExtracter<TCoordinate>(comps));
            return comps;
        }

        private IList comps;

        /// <summary> 
        /// Constructs a PolygonExtracterFilter with a list in which to store Polygons found.
        /// </summary>
        public PolygonExtracter(IList comps)
        {
            this.comps = comps;
        }

        public void Filter(IGeometry<TCoordinate> geom)
        {
            if (geom is IPolygon)
                comps.Add(geom);
        }
    }
}