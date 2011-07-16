using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
#if !DOTNET40
using GeoAPI.DataStructures.Collections.Generic;
#endif
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// Contains information about the nature and location of 
    /// a <see cref="Geometry{TCoordinate}" /> validation error.
    /// </summary>
    public enum TopologyValidationErrors
    {
        /// <summary>
        /// Unknown error; default value.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Indicates a generic topology validation error which doesn't 
        /// fit into any of the other <see cref="TopologyValidationErrors"/> values.
        /// </summary>
        GenericTopologyValidationError = 1,

        /// <summary>
        /// Indicates that a hole of a polygon lies partially 
        /// or completely in the exterior of the shell.
        /// </summary>
        HoleOutsideShell = 2,

        /// <summary>
        /// Indicates that a hole lies 
        /// in the interior of another hole in the same polygon.
        /// </summary>
        NestedHoles = 3,

        /// <summary>
        /// Indicates that the interior of a polygon is disjoint
        /// (often caused by set of contiguous holes splitting 
        /// the polygon into two parts).
        /// </summary>
        DisconnectedInteriors = 4,

        /// <summary>
        /// Indicates that two rings of a polygonal geometry intersect.
        /// </summary>
        SelfIntersection = 5,

        /// <summary>
        /// Indicates that a ring self-intersects.
        /// </summary>
        RingSelfIntersection = 6,

        /// <summary>
        /// Indicates that a polygon component of a 
        /// <see cref="IMultiPolygon" /> lies inside another polygonal component.
        /// </summary>
        NestedShells = 7,

        /// <summary>
        /// Indicates that a polygonal geometry 
        /// contains two rings which are identical.
        /// </summary>
        DuplicateRings = 8,

        /// <summary>
        /// Indicates that either:
        /// - An <see cref="ILineString" /> contains a single point.
        /// - An <see cref="ILinearRing" /> contains 2 or 3 points.
        /// </summary>
        TooFewPoints = 9,

        /// <summary>
        /// Indicates that the <see cref="Ordinates.X">X</see>
        /// or <see cref="Ordinates.Y">Y</see> ordinate of
        /// an <see cref="ICoordinate" /> is not a valid 
        /// numeric value (e.g. <see cref="double.NaN" />).
        /// </summary>
        InvalidCoordinate = 10,

        /// <summary>
        /// Indicates that a ring is not correctly closed
        /// (the first and the last coordinate are different).
        /// </summary>
        RingNotClosed = 11,
    }

    /// <summary>
    /// Contains information about the nature and location of an <see cref="IGeometry{TCoordinate}"/>
    /// validation error.
    /// </summary>
    public class TopologyValidationError
    {
        // NOTE: modified for "safe" assembly in Sql 2005:  Added readonly
        /// <summary>
        /// These messages must match one-to-one up with the indexes above
        /// </summary>
        private static readonly IDictionary<TopologyValidationErrors, String> _errMsg;

        private readonly ICoordinate _coordinate;
        private readonly TopologyValidationErrors _errorType;

        static TopologyValidationError()
        {
            // I18N_UNSAFE
            Dictionary<TopologyValidationErrors, String> errors
                = new Dictionary<TopologyValidationErrors, String>();
            errors[TopologyValidationErrors.Unknown] = "Unknown error";
            errors[TopologyValidationErrors.GenericTopologyValidationError] = "Topology validation error";
            errors[TopologyValidationErrors.HoleOutsideShell] = "Hole lies outside shell";
            errors[TopologyValidationErrors.NestedHoles] = "Holes are nested";
            errors[TopologyValidationErrors.DisconnectedInteriors] = "Interior is disconnected";
            errors[TopologyValidationErrors.SelfIntersection] = "Self-intersection";
            errors[TopologyValidationErrors.RingSelfIntersection] = "Ring self-intersection";
            errors[TopologyValidationErrors.NestedShells] = "Nested shells";
            errors[TopologyValidationErrors.DuplicateRings] = "Duplicate Rings";
            errors[TopologyValidationErrors.TooFewPoints] = "Too few points in geometry component";
            errors[TopologyValidationErrors.InvalidCoordinate] = "Invalid Coordinate";
            errors[TopologyValidationErrors.RingNotClosed] = "Ring not closed: first and last points are different";

#if DOTNET40
            _errMsg = new Dictionary<TopologyValidationErrors, String>(errors);
#else
            _errMsg = new ReadOnlyDictionary<TopologyValidationErrors, String>(errors);
#endif
        }

        public TopologyValidationError(TopologyValidationErrors errorType, ICoordinate coordinate)
        {
            _errorType = errorType;

            if (coordinate != null)
            {
                _coordinate = (ICoordinate) coordinate.Clone();
            }
        }

        public TopologyValidationError(TopologyValidationErrors errorType)
            : this(errorType, null)
        {
        }

        public ICoordinate Coordinate
        {
            get { return _coordinate; }
        }

        public TopologyValidationErrors ErrorType
        {
            get { return _errorType; }
        }

        public String Message
        {
            get { return _errMsg[_errorType]; }
        }

        public override String ToString()
        {
            return Message + " at or near point " + _coordinate;
        }
    }
}