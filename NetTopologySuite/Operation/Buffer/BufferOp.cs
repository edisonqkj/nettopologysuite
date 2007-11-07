using System;
using GeoAPI.Geometries;
using GeoAPI.Operations.Buffer;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Precision;

namespace GisSharpBlog.NetTopologySuite.Operation.Buffer
{
    /// <summary>
    /// Computes the buffer of a point, for both positive and negative buffer distances.    
    /// In GIS, the buffer of a point is defined as
    /// the Minkowski sum or difference of the point
    /// with a circle with radius equal to the absolute value of the buffer distance.
    /// In the CAD/CAM world buffers are known as offset curves.
    /// Since true buffer curves may contain circular arcs,
    /// computed buffer polygons can only be approximations to the true point.
    /// The user can control the accuracy of the curve approximation by specifying
    /// the number of linear segments with which to approximate a curve.
    /// The end cap endCapStyle of a linear buffer may be specified. The
    /// following end cap styles are supported:
    /// <para>
    /// {CAP_ROUND} - the usual round end caps
    /// {CAP_BUTT} - end caps are truncated flat at the line ends
    /// {CAP_SQUARE} - end caps are squared off at the buffer distance beyond the line ends
    /// </para>
    /// The computation uses an algorithm involving iterated noding and precision reduction
    /// to provide a high degree of robustness.
    /// </summary>
    public class BufferOp
    {
        // NOTE: modified for "safe" assembly in Sql 2005
        // Const added!
        private const Int32 MaxPrecisionDigits = 12;

        /// <summary>
        /// Compute a reasonable scale factor to limit the precision of
        /// a given combination of Geometry and buffer distance.
        /// The scale factor is based on a heuristic.
        /// </summary>
        /// <param name="g">The Geometry being buffered.</param>
        /// <param name="distance">The buffer distance.</param>
        /// <param name="maxPrecisionDigits">The mzx # of digits that should be allowed by
        /// the precision determined by the computed scale factor.</param>
        /// <returns>A scale factor that allows a reasonable amount of precision for the buffer computation.</returns>
        private static Double PrecisionScaleFactor(IGeometry g, Double distance, Int32 maxPrecisionDigits)
        {
            IExtents env = g.EnvelopeInternal;
            Double envSize = Math.Max(env.Height, env.Width);
            Double expandByDistance = distance > 0.0 ? distance : 0.0;
            Double bufEnvSize = envSize + 2*expandByDistance;

            // the smallest power of 10 greater than the buffer envelope
            Int32 bufEnvLog10 = (Int32) (Math.Log(bufEnvSize)/Math.Log(10) + 1.0);
            Int32 minUnitLog10 = bufEnvLog10 - maxPrecisionDigits;

            // scale factor is inverse of min Unit size, so flip sign of exponent
            Double scaleFactor = Math.Pow(10.0, -minUnitLog10);
            return scaleFactor;
        }

        /// <summary>
        /// Computes the buffer of a point for a given buffer distance.
        /// </summary>
        /// <param name="g">The point to buffer.</param>
        /// <param name="distance">The buffer distance.</param>
        /// <returns> The buffer of the input point.</returns>
        public static IGeometry Buffer(IGeometry g, Double distance)
        {
            BufferOp gBuf = new BufferOp(g);
            IGeometry geomBuf = gBuf.GetResultGeometry(distance);
            return geomBuf;
        }

        /// <summary>
        /// Computes the buffer of a point for a given buffer distance,
        /// using the given Cap Style for borders of the point.
        /// </summary>
        /// <param name="g">The point to buffer.</param>
        /// <param name="distance">The buffer distance.</param>        
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns> The buffer of the input point.</returns>
        public static IGeometry Buffer(IGeometry g, Double distance, BufferStyle endCapStyle)
        {
            BufferOp gBuf = new BufferOp(g);
            gBuf.EndCapStyle = endCapStyle;
            IGeometry geomBuf = gBuf.GetResultGeometry(distance);
            return geomBuf;
        }

        /// <summary>
        /// Computes the buffer for a point for a given buffer distance
        /// and accuracy of approximation.
        /// </summary>
        /// <param name="g">The point to buffer.</param>
        /// <param name="distance">The buffer distance.</param>
        /// <param name="quadrantSegments">The number of segments used to approximate a quarter circle.</param>
        /// <returns>The buffer of the input point.</returns>
        public static IGeometry Buffer(IGeometry g, Double distance, Int32 quadrantSegments)
        {
            BufferOp bufOp = new BufferOp(g);
            bufOp.QuadrantSegments = quadrantSegments;
            IGeometry geomBuf = bufOp.GetResultGeometry(distance);
            return geomBuf;
        }

        /// <summary>
        /// Computes the buffer for a point for a given buffer distance
        /// and accuracy of approximation.
        /// </summary>
        /// <param name="g">The point to buffer.</param>
        /// <param name="distance">The buffer distance.</param>
        /// <param name="quadrantSegments">The number of segments used to approximate a quarter circle.</param>
        /// <param name="endCapStyle">Cap Style to use for compute buffer.</param>
        /// <returns>The buffer of the input point.</returns>
        public static IGeometry Buffer(IGeometry g, Double distance, Int32 quadrantSegments, BufferStyle endCapStyle)
        {
            BufferOp bufOp = new BufferOp(g);
            bufOp.EndCapStyle = endCapStyle;
            bufOp.QuadrantSegments = quadrantSegments;
            IGeometry geomBuf = bufOp.GetResultGeometry(distance);
            return geomBuf;
        }

        private IGeometry argGeom;
        private Double distance;
        private Int32 quadrantSegments = OffsetCurveBuilder.DefaultQuadrantSegments;
        private BufferStyle endCapStyle = BufferStyle.CapRound;
        private IGeometry resultGeometry = null;
        private TopologyException saveException; // debugging only

        /// <summary>
        /// Initializes a buffer computation for the given point.
        /// </summary>
        /// <param name="g">The point to buffer.</param>
        public BufferOp(IGeometry g)
        {
            argGeom = g;
        }

        /// <summary> 
        /// Specifies the end cap endCapStyle of the generated buffer.
        /// The styles supported are CapRound, CapButt, and CapSquare.
        /// The default is CapRound.
        /// </summary>
        public BufferStyle EndCapStyle
        {
            get { return endCapStyle; }
            set { endCapStyle = value; }
        }

        public Int32 QuadrantSegments
        {
            get { return quadrantSegments; }
            set { quadrantSegments = value; }
        }

        public IGeometry GetResultGeometry(Double distance)
        {
            this.distance = distance;
            ComputeGeometry();
            return resultGeometry;
        }

        public IGeometry GetResultGeometry(Double distance, Int32 quadrantSegments)
        {
            this.distance = distance;
            QuadrantSegments = quadrantSegments;
            ComputeGeometry();
            return resultGeometry;
        }

        private void ComputeGeometry()
        {
            BufferOriginalPrecision();
            if (resultGeometry != null)
            {
                return;
            }

            // try and compute with decreasing precision
            for (Int32 precDigits = MaxPrecisionDigits; precDigits >= 0; precDigits--)
            {
                try
                {
                    BufferFixedPrecision(precDigits);
                }
                catch (TopologyException ex)
                {
                    saveException = ex;
                    // don't propagate the exception - it will be detected by fact that resultGeometry is null
                }
                if (resultGeometry != null)
                {
                    return;
                }
            }

            // tried everything - have to bail
            throw saveException;
        }

        private void BufferOriginalPrecision()
        {
            try
            {
                BufferBuilder bufBuilder = new BufferBuilder();
                bufBuilder.QuadrantSegments = quadrantSegments;
                bufBuilder.EndCapStyle = endCapStyle;
                resultGeometry = bufBuilder.Buffer(argGeom, distance);
            }
            catch (TopologyException ex)
            {
                saveException = ex;
                // don't propagate the exception - it will be detected by fact that resultGeometry is null
            }
        }

        private void BufferFixedPrecision(Int32 precisionDigits)
        {
            Double sizeBasedScaleFactor = PrecisionScaleFactor(argGeom, distance, precisionDigits);

            PrecisionModel fixedPM = new PrecisionModel(sizeBasedScaleFactor);

            // don't change the precision model of the Geometry, just reduce the precision
            SimpleGeometryPrecisionReducer reducer = new SimpleGeometryPrecisionReducer(fixedPM);
            IGeometry reducedGeom = reducer.Reduce(argGeom);

            BufferBuilder bufBuilder = new BufferBuilder();
            bufBuilder.WorkingPrecisionModel = fixedPM;
            bufBuilder.QuadrantSegments = quadrantSegments;

            // this may throw an exception, if robustness errors are encountered
            resultGeometry = bufBuilder.Buffer(reducedGeom, distance);
        }
    }
}