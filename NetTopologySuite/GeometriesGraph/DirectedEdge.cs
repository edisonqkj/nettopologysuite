using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    public class DirectedEdge<TCoordinate> : EdgeEnd<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        /// <summary>
        /// Computes the factor for the change in depth 
        /// when moving from one location to another.
        /// E.g. if crossing from the <see cref="Locations.Interior"/> 
        /// to the <see cref="Locations.Exterior"/>
        /// the depth decreases, so the factor is -1.
        /// </summary>
        public static Int32 DepthFactor(Locations from, Locations to)
        {
            if (from == Locations.Exterior && to == Locations.Interior)
            {
                return 1;
            }

            if (from == Locations.Interior && to == Locations.Exterior)
            {
                return -1;
            }

            return 0;
        }

        private Boolean _isForward;
        private Boolean _isInResult;
        private Boolean _isVisited;

        private DirectedEdge<TCoordinate> _sym; // the symmetric edge
        private DirectedEdge<TCoordinate> _next; // the next edge in the edge ring for the polygon containing this edge
        private DirectedEdge<TCoordinate> _nextMin; // the next edge in the MinimalEdgeRing that contains this edge
        private EdgeRing<TCoordinate> _edgeRing; // the EdgeRing that this edge is part of
        private EdgeRing<TCoordinate> _minEdgeRing; // the MinimalEdgeRing that this edge is part of

        /// <summary> 
        /// The depth of each side (position) of this edge.
        /// The 0 element of the array is never used.
        /// </summary>
        private readonly Int32[] _depth = { 0, -999, -999 };

        public DirectedEdge(Edge<TCoordinate> edge, Boolean isForward)
            : base(edge)
        {
            _isForward = isForward;

            if (isForward)
            {
                Init(edge.Coordinates[0], edge.Coordinates[1]);
            }
            else
            {
                Int32 n = edge.PointCount - 1;
                Init(edge.Coordinates[n], edge.Coordinates[n - 1]);
            }

            computeDirectedLabel();
        }

        public Boolean IsInResult
        {
            get { return _isInResult; }
            set { _isInResult = value; }
        }

        //public Boolean IsInResult
        //{
        //    get { return _isInResult; }
        //}

        public Boolean IsVisited
        {
            get { return _isVisited; }
            set { _isVisited = value; }
        }

        //public Boolean IsVisited
        //{
        //    get { return _isVisited; }
        //}

        public EdgeRing<TCoordinate> EdgeRing
        {
            get { return _edgeRing; }
            set { _edgeRing = value; }
        }

        public EdgeRing<TCoordinate> MinEdgeRing
        {
            get { return _minEdgeRing; }
            set { _minEdgeRing = value; }
        }

        public Int32 GetDepth(Positions position)
        {
            return _depth[(Int32)position];
        }

        public void SetDepth(Positions position, Int32 depthVal)
        {
            if (_depth[(Int32)position] != -999)
            {
                if (_depth[(Int32)position] != depthVal)
                {
                    throw new TopologyException("Assigned depths do not match", Coordinate);
                }
            }

            _depth[(Int32)position] = depthVal;
        }

        public Int32 DepthDelta
        {
            get
            {
                Int32 depthDelta = Edge.DepthDelta;

                if (!IsForward)
                {
                    depthDelta = -depthDelta;
                }

                return depthDelta;
            }
        }

        /// <summary>
        /// Gets or sets whether the entire edge is visited. The edge is visted when 
        /// both the <see cref="DirectedEdge{TCoordinate}"/> and the corresponding 
        /// <see cref="DirectedEdge{TCoordinate}.Sym"/> have an <see cref="IsVisited"/>
        /// property with the value of <see langword="true"/>.
        /// </summary>
        public Boolean IsEdgeVisited
        {
            get { return IsVisited && _sym.IsVisited; }
            set
            {
                IsVisited = value;
                _sym.IsVisited = value;
            }
        }

        public Boolean IsForward
        {
            get { return _isForward; }
            protected set { _isForward = value; }
        }

        public DirectedEdge<TCoordinate> Sym
        {
            get { return _sym; }
            set { _sym = value; }
        }

        public DirectedEdge<TCoordinate> Next
        {
            get { return _next; }
            set { _next = value; }
        }

        public DirectedEdge<TCoordinate> NextMin
        {
            get { return _nextMin; }
            set { _nextMin = value; }
        }

        /// <summary>
        /// Gets <see langword="true"/> if at least one of the edge's labels is a line label
        /// any labels which are not line labels have all <see cref="Positions"/> 
        /// equal to <see cref="Locations.Exterior"/>.
        /// </summary>
        public Boolean IsLineEdge
        {
            get
            {
                Debug.Assert(Label.HasValue);

                Label label = Label.Value;

                Boolean isLine = label.IsLine(0) || label.IsLine(1);

                Boolean isExteriorIfArea0 =
                    !label.IsArea(0) || label.AllPositionsEqual(0, Locations.Exterior);

                Boolean isExteriorIfArea1 =
                    !label.IsArea(1) || label.AllPositionsEqual(1, Locations.Exterior);

                return isLine && isExteriorIfArea0 && isExteriorIfArea1;
            }
        }

        /// <summary> 
        /// Gets <see langword="true"/> if
        /// the edge's label is an area label for both geometries
        /// and for each geometry both sides are in the interior.
        /// </summary>
        /// <returns><see langword="true"/> if this is an interior Area edge.</returns>
        public Boolean IsInteriorAreaEdge
        {
            get
            {
                Boolean isInteriorAreaEdge = true;

                Debug.Assert(Label.HasValue);

                Label label = Label.Value;

                for (Int32 i = 0; i < 2; i++)
                {
                    if (!(label.IsArea(i)
                          && label[i, Positions.Left] == Locations.Interior
                          && label[i, Positions.Right] == Locations.Interior))
                    {
                        isInteriorAreaEdge = false;
                    }
                }

                return isInteriorAreaEdge;
            }
        }

        /// <summary> 
        /// Set both edge depths.  
        /// One depth for a given side is provided.  
        /// The other is computed depending on the Location 
        /// transition and the depthDelta of the edge.
        /// </summary>
        public void SetEdgeDepths(Positions position, Int32 depth)
        {
            // get the depth transition delta from R to Curve for this directed Edge
            Int32 depthDelta = Edge.DepthDelta;

            if (!_isForward)
            {
                depthDelta = -depthDelta;
            }

            // if moving from Curve to R instead of R to Curve must change sign of delta
            Int32 directionFactor = 1;

            if (position == Positions.Left)
            {
                directionFactor = -1;
            }

            Positions oppositePos = Position.Opposite(position);
            Int32 delta = depthDelta * directionFactor;
            Int32 oppositeDepth = depth + delta;
            SetDepth(position, depth);
            SetDepth(oppositePos, oppositeDepth);
        }

        //public override String ToString()
        //{
        //    StringBuilder sb = new StringBuilder();

        //    Int32 leftDepth = _depth[(Int32)Positions.Left];
        //    Int32 rightDepth = _depth[(Int32)Positions.Right];
        //    sb.AppendFormat("{0} {1}/{2} ({3}) ", IsForward ? "Forward" : "Reverse", 
        //                                         leftDepth, rightDepth, DepthDelta);

        //    if (_isInResult)
        //    {
        //        sb.Append(" in result ");
        //    }

        //    sb.Append(base.ToString());

        //    return sb.ToString();
        //}

        // Compute the label in the appropriate orientation for this directed edge.
        private void computeDirectedLabel()
        {
            Debug.Assert(Edge.Label.HasValue);

            Label = Edge.Label.Value;

            if (!_isForward)
            {
                Label = Label.Value.Flip();
            }
        }

        //public void WriteEdge(StreamWriter outstream)
        //{
        //    outstream.Write(ToString());
        //    outstream.Write(" ");

        //    if (_isForward)
        //    {
        //        Edge.Write(outstream);
        //    }
        //    else
        //    {
        //        Edge.WriteReverse(outstream);
        //    }
        //}


        public override string ToString()
        {
            return
                Coordinate +
                (" -> ") +
                DirectedCoordinate +
                (_isForward ? "F " : "R ") +
                (_isVisited ? "* " : "_ ") +
                (_isInResult ? "in " : "no ") +
                (Next == null
                    ? "__________ "
                    : Next.Coordinate +
                      (" -> ") +
                      Next.DirectedCoordinate +
                      " ") +
                (printCoords(Edge.Coordinates));
        }

        private static string printCoords(ICoordinateSequence coordinates)
        {
            StringBuilder builder = new StringBuilder();

            foreach (ICoordinate coordinate in coordinates)
            {
                builder.Append(coordinate);
                builder.Append(" ");
            }

            --builder.Length;

            return builder.ToString();
        }
    }
}