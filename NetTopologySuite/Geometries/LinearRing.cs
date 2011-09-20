using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// Models an OGC SFS <c>LinearRing</c>.
    /// </summary>
    /// <remarks>
    /// A LinearRing is a LineString which is both closed and simple.
    /// In other words,
    /// the first and last coordinate in the ring must be equal,
    /// and the interior of the ring must not self-intersect.
    /// Either orientation of the ring is allowed.
    /// <para>
    /// A ring must have either 0 or 4 or more points.  
    /// The first and last points must be equal (in 2D).
    /// If these conditions are not met, the constructors throw 
    /// an <see cref="ArgumentException"/></para>
    /// </remarks>
//#if !SILVERLIGHT
    [Serializable]
//#endif
    public class LinearRing : LineString, ILinearRing
    {
        /// <summary>
        /// Constructs a <c>LinearRing</c> with the given points.
        /// </summary>
        /// <param name="points">
        /// Points forming a closed and simple linestring, or
        /// <c>null</c> or an empty array to create the empty point.
        /// This array must not contain <c>null</c> elements.
        /// </param>
        /// <param name="factory"></param>
        /// <exception cref="ArgumentException">If the ring is not closed, or has too few points</exception>
        public LinearRing(ICoordinateSequence points, IGeometryFactory factory) : base(points, factory)
        {            
            ValidateConstruction();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ValidateConstruction() 
        {
	        if (!IsEmpty && !base.IsClosed) 
                throw new ArgumentException("points must form a closed linestring");            
            if (CoordinateSequence.Count >= 1 && CoordinateSequence.Count <= 3) 
                throw new ArgumentException("Number of points must be 0 or >3");            
        }

        /// <summary>
        /// Returns <c>Dimensions.False</c>, since by definition LinearRings do not have a boundary.
        /// </summary>
        public override Dimension BoundaryDimension
        {
	        get 
	        { 
		         return Dimension.False;
	        }
        }

  /*


        /// <summary>
        /// 
        /// </summary>
        public override bool IsSimple
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string GeometryType
        {
            get
            {
                return "LinearRing";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsClosed
        {
            get
            {
                return true;
            }
        }

        /* BEGIN ADDED BY MPAUL42: monoGIS team */

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearRing"/> class.
        /// </summary>
        /// <param name="points">The points used for create this instance.</param>
        /// <remarks>
        /// For create this <see cref="Geometry"/> is used a standard <see cref="GeometryFactory"/> 
        /// with <see cref="PrecisionModel" /> <c> == </c> <see cref="PrecisionModels.Floating"/>.
        /// </remarks>
        /// <exception cref="ArgumentException">If the ring is not closed, or has too few points</exception>
        //[Obsolete("Use GeometryFactory instead")]
        public LinearRing(Coordinate[] points) : 
            this(DefaultFactory.CoordinateSequenceFactory.Create(points), DefaultFactory) { }
        
        /* END ADDED BY MPAUL42: monoGIS team */
    }
}
