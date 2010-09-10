using System;

namespace GisSharpBlog.NetTopologySuite.Algorithm
{
    /// <summary> 
    /// Non-robust versions of various fundamental Computational Geometric algorithms,
    /// FOR TESTING PURPOSES ONLY!.
    /// The non-robustness is due to rounding error in floating point computation.
    /// </summary>
    public static class NonRobustCGAlgorithms
    {
        public static Boolean IsPointInRing(ICoordinate p, ICoordinate[] ring)
        {
            Int32 i, i1; // point index; i1 = i-1 mod n
            Double xInt; // x intersection of e with ray
            Int32 crossings = 0; // number of edge/ray crossings
            Double x1, y1, x2, y2;
            Int32 nPts = ring.Length;

            /* For each line edge l = (i-1, i), see if it crosses ray from test point in positive x direction. */
            for (i = 1; i < nPts; i++)
            {
                i1 = i - 1;
                ICoordinate p1 = ring[i];
                ICoordinate p2 = ring[i1];
                x1 = p1.X - p.X;
                y1 = p1.Y - p.Y;
                x2 = p2.X - p.X;
                y2 = p2.Y - p.Y;

                if (((y1 > 0) && (y2 <= 0)) || ((y2 > 0) && (y1 <= 0)))
                {
                    /* e straddles x axis, so compute intersection. */
                    xInt = (x1 * y2 - x2 * y1) / (y2 - y1);

                    /* crosses ray if strictly positive intersection. */
                    if (0.0 < xInt)
                    {
                        crossings++;
                    }
                }
            }

            /* p is inside if an odd number of crossings. */
            if ((crossings % 2) == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Computes whether a ring defined by an array of <c>Coordinate</c> is
        /// oriented counter-clockwise.
        /// This will handle coordinate lists which contain repeated points.
        /// </summary>
        /// <param name="ring">an array of coordinates forming a ring.</param>
        /// <returns>
        /// <see langword="true"/> if the ring is oriented counter-clockwise.
        /// throws <c>ArgumentException</c> if the ring is degenerate (does not contain 3 different points)
        /// </returns>
        public static Boolean IsCCW(ICoordinate[] ring)
        {
            // # of points without closing endpoint
            Int32 nPts = ring.Length - 1;

            // check that this is a valid ring - if not, simply return a dummy value
            if (nPts < 4)
            {
                return false;
            }

            // algorithm to check if a Ring is stored in CCW order
            // find highest point
            ICoordinate hip = ring[0];
            Int32 hii = 0;

            for (Int32 i = 1; i <= nPts; i++)
            {
                ICoordinate p = ring[i];

                if (p.Y > hip.Y)
                {
                    hip = p;
                    hii = i;
                }
            }

            // find different point before highest point
            Int32 iPrev = hii;

            do
            {
                iPrev = (iPrev - 1) % nPts;
            } while (ring[iPrev].Equals(hip) && iPrev != hii);

            // find different point after highest point
            Int32 iNext = hii;

            do
            {
                iNext = (iNext + 1) % nPts;
            } while (ring[iNext].Equals(hip) && iNext != hii);

            ICoordinate prev = ring[iPrev];
            ICoordinate next = ring[iNext];
           
            if (prev.Equals(hip) || next.Equals(hip) || prev.Equals(next))
            {
                throw new ArgumentException("degenerate ring (does not contain 3 different points)");
            }

            // translate so that hip is at the origin.
            // This will not affect the area calculation, and will avoid
            // finite-accuracy errors (i.e very small vectors with very large coordinates)
            // This also simplifies the discriminant calculation.
            Double prev2x = prev.X - hip.X;
            Double prev2y = prev.Y - hip.Y;
            Double next2x = next.X - hip.X;
            Double next2y = next.Y - hip.Y;

            // compute cross-product of vectors hip->next and hip->prev
            // (e.g. area of parallelogram they enclose)
            Double disc = next2x * prev2y - next2y * prev2x;

            /* If disc is exactly 0, lines are collinear.  There are two possible cases:
                    (1) the lines lie along the x axis in opposite directions
                    (2) the line lie on top of one another
                    
                    (2) should never happen, so we're going to ignore it!
                        (Might want to assert this)
            
                    (1) is handled by checking if next is left of prev ==> CCW
            */
            if (disc == 0.0)
            {
                return (prev.X > next.X); // poly is CCW if prev x is right of next x
            }
            else
            {
                return (disc > 0.0); // if area is positive, points are ordered CCW                 
            }
        }

        public static Int32 ComputeOrientation(ICoordinate p1, ICoordinate p2, ICoordinate q)
        {
            Double dx1 = p2.X - p1.X;
            Double dy1 = p2.Y - p1.Y;
            Double dx2 = q.X - p2.X;
            Double dy2 = q.Y - p2.Y;
            Double det = dx1 * dy2 - dx2 * dy1;

            if (det > 0.0)
            {
                return 1;
            }

            if (det < 0.0)
            {
                return -1;
            }

            return 0;
        }
    }
}