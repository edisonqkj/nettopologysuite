#if !DOTNET40
#define C5
#endif
using System;
using System.Collections.Generic;
#if C5
using C5;
#endif
using GeoAPI.Coordinates;
#if !DOTNET40
using GeoAPI.DataStructures.Collections.Generic;
#endif
using NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Planargraph
{
    /// <summary>
    /// A node in a <see cref="PlanarGraph{TCoordinate}"/> is a location 
    /// where 0 or more <see cref="Edge{TCoordinate}"/>s meet. 
    /// </summary>
    /// <remarks>
    /// A node is connected to each of its incident <see cref="Edge{TCoordinate}"/>s 
    /// via an outgoing <see cref="DirectedEdge{TCoordinate}"/>. 
    /// Some clients using a <see cref="PlanarGraph{TCoordinate}"/> may want to
    /// subclass <see cref="Node{TCoordinate}"/> to add their own application-specific
    /// data and methods.
    /// </remarks>
    public class Node<TCoordinate> : GraphComponent<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        /// <summary>
        /// The collection of DirectedEdges that leave this Node.
        /// </summary>
        private readonly DirectedEdgeStar<TCoordinate> _directedEdgeStar;

        /// <summary>
        /// The location of this Node.
        /// </summary>
        private TCoordinate _coordinate;

        /// <summary>
        /// Constructs a Node with the given location.
        /// </summary>
        public Node(TCoordinate coordinate)
            : this(coordinate, new DirectedEdgeStar<TCoordinate>())
        {
        }

        /// <summary>
        /// Constructs a Node with the given location and collection of outgoing DirectedEdges.
        /// </summary>
        public Node(TCoordinate coordinate, DirectedEdgeStar<TCoordinate> deStar)
        {
            _coordinate = coordinate;
            _directedEdgeStar = deStar;
        }

        /// <summary>
        /// Returns the location of this Node.
        /// </summary>
        public TCoordinate Coordinate
        {
            get { return _coordinate; }
        }

        /// <summary>
        /// Returns the collection of DirectedEdges that leave this Node.
        /// </summary>
        public DirectedEdgeStar<TCoordinate> OutEdges
        {
            get { return _directedEdgeStar; }
        }

        /// <summary>
        /// Returns the number of edges around this Node.
        /// </summary>
        public Int32 Degree
        {
            get { return _directedEdgeStar.Degree; }
        }

        /// <summary>
        /// Tests whether this component has been removed from its containing graph.
        /// </summary>
        public override Boolean IsRemoved
        {
            get { return Coordinates<TCoordinate>.IsEmpty(_coordinate); }
        }

        /// <summary>
        /// Returns all Edges that connect the two nodes (which are assumed to be different).
        /// </summary>
        public static IEnumerable<Edge<TCoordinate>> GetEdgesBetween(Node<TCoordinate> node0, Node<TCoordinate> node1)
        {
            IEnumerable<Edge<TCoordinate>> edges0 = DirectedEdge<TCoordinate>.ToEdges(node0.OutEdges.Edges);
#if C5
            HashSet<Edge<TCoordinate>> commonEdges = new HashSet<Edge<TCoordinate>>();
#else
#if DOTNET40
            ISet<Edge<TCoordinate>> commonEdges = new HashSet<Edge<TCoordinate>>(edges0);
#else
            ISet<Edge<TCoordinate>> commonEdges = new HashedSet<Edge<TCoordinate>>(edges0);
#endif
#endif
            IEnumerable<Edge<TCoordinate>> edges1 = DirectedEdge<TCoordinate>.ToEdges(node1.OutEdges.Edges);
#if DOTNET40
            commonEdges.IntersectWith(edges1);
#else
            commonEdges.RetainAll(edges1);
#endif
            return commonEdges;
        }

        /// <summary>
        /// Adds an outgoing DirectedEdge to this Node.
        /// </summary>
        public void AddOutEdge(DirectedEdge<TCoordinate> de)
        {
            _directedEdgeStar.Add(de);
        }

        /// <summary>
        /// Returns the zero-based index of the given Edge, after sorting in ascending order
        /// by angle with the positive x-axis.
        /// </summary>
        public Int32 GetIndex(Edge<TCoordinate> edge)
        {
            return _directedEdgeStar.GetIndex(edge);
        }

        /// <summary>
        /// Removes this node from its containing graph.
        /// </summary>
        internal void Remove()
        {
            _coordinate = default(TCoordinate);
        }

        public override string ToString()
        {
            return "NODE: " + _coordinate + ": " + Degree;
        }
    }
}