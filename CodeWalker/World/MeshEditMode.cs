using System;
using System.Collections.Generic;
using SharpDX;

using CodeWalker.GameFiles;

namespace CodeWalker.World
{
    /// <summary>
    /// Modes d'édition de mesh disponibles
    /// </summary>
    public enum MeshEditMode
    {
        None = 0,
        Vertex = 1,
        Edge = 2,
        Face = 3
    }

    /// <summary>
    /// Classe de base pour les éléments de mesh sélectionnables
    /// </summary>
    public abstract class MeshElement
    {
        public abstract Vector3 Position { get; }
        public abstract void Move(Vector3 delta);
        public abstract void CommitMove();
        public bool IsSelected { get; set; }

    }

    /// <summary>
    /// Représente un vertex sélectionnable dans le mesh
    /// </summary>
    public class VertexElement : MeshElement
    {
        public int VertexIndex { get; set; }
        public Vector3 OriginalPosition { get; set; }
        public DrawableGeometry Geometry { get; set; }
        private Vector3 currentPosition;

        public override Vector3 Position => currentPosition;

        public VertexElement(int index, Vector3 position, DrawableGeometry geometry)
        {
            VertexIndex = index;
            OriginalPosition = position;
            currentPosition = position;
            Geometry = geometry;
        }

        public override void Move(Vector3 delta)
        {
            currentPosition += delta;
        }

        public void ResetPosition()
        {
            currentPosition = OriginalPosition;
        }


        public override void CommitMove()
        {
            OriginalPosition = currentPosition;
        }
    }

    /// <summary>
    /// Représente une edge (arête) sélectionnable dans le mesh
    /// </summary>
    public class EdgeElement : MeshElement
    {
        public VertexElement Vertex1 { get; set; }
        public VertexElement Vertex2 { get; set; }

        public override Vector3 Position => (Vertex1.Position + Vertex2.Position) * 0.5f;

        public EdgeElement(VertexElement v1, VertexElement v2)
        {
            Vertex1 = v1;
            Vertex2 = v2;
        }

        public override void Move(Vector3 delta)
        {
            Vertex1.Move(delta);
            Vertex2.Move(delta);

        }

        public override void CommitMove()
        {
            Vertex1.CommitMove();
            Vertex2.CommitMove();
        }
    }

    /// <summary>
    /// Représente une face (triangle) sélectionnable dans le mesh
    /// </summary>
    public class FaceElement : MeshElement
    {
        public VertexElement Vertex1 { get; set; }
        public VertexElement Vertex2 { get; set; }
        public VertexElement Vertex3 { get; set; }
        public int FaceIndex { get; set; }

        public override Vector3 Position => (Vertex1.Position + Vertex2.Position + Vertex3.Position) / 3.0f;

        public FaceElement(int faceIndex, VertexElement v1, VertexElement v2, VertexElement v3)
        {
            FaceIndex = faceIndex;
            Vertex1 = v1;
            Vertex2 = v2;
            Vertex3 = v3;
        }

        public override void Move(Vector3 delta)
        {
            Vertex1.Move(delta);
            Vertex2.Move(delta);
            Vertex3.Move(delta);

        }

        public override void CommitMove()
        {
            Vertex1.CommitMove();
            Vertex2.CommitMove();
            Vertex3.CommitMove();
        }

        public Vector3 GetNormal()
        {
            var edge1 = Vertex2.Position - Vertex1.Position;
            var edge2 = Vertex3.Position - Vertex1.Position;
            var normal = Vector3.Cross(edge1, edge2);
            normal.Normalize();
            return normal;
        }
    }
}
