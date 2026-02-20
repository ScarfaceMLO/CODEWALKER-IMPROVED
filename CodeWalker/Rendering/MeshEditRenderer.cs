using System;
using System.Collections.Generic;
using System.Linq;
using CodeWalker.World;
using SharpDX;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;

namespace CodeWalker.Rendering
{
    /// <summary>
    /// Rendu des éléments de mesh en mode édition
    /// </summary>
    public class MeshEditRenderer
    {
        private Renderer renderer;
        
        // Couleurs pour le rendu

        private static readonly Color4 VertexHoverColor = new Color4(1.0f, 1.0f, 0.5f, 1.0f); // Light Yellow
        private static readonly Color4 EdgeHoverColor = new Color4(1.0f, 1.0f, 0.5f, 1.0f); // Light Yellow
        private static readonly Color4 FaceHoverColor = new Color4(1.0f, 0.5f, 0.0f, 0.4f); // Orange transparent

        public Color4 VertexColor = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
        public Color4 VertexSelectedColor = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
        public Color4 EdgeColor = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
        public Color4 EdgeSelectedColor = new Color4(1.0f, 0.0f, 0.0f, 1.0f);
        public Color4 FaceColor = new Color4(0.0f, 0.5f, 1.0f, 0.3f);
        public Color4 FaceSelectedColor = new Color4(0.0f, 1.0f, 0.0f, 0.7f);
        public float VertexSize = 0.05f;
        private const float EdgeWidth = 2.0f; // Épaisseur des lignes d'edge

        public MeshEditRenderer(Renderer renderer)
        {
            this.renderer = renderer;
        }

        /// <summary>
        /// Rendu de tous les vertices
        /// </summary>
        public void RenderVertices(IEnumerable<VertexElement> vertices, Matrix transform)
        {
            foreach (var vertex in vertices)
            {
                var color = vertex.IsSelected ? VertexSelectedColor : VertexColor;
                var worldPos = Vector3.TransformCoordinate(vertex.Position, transform);
                RenderSphere(worldPos, VertexSize, color);
            }
        }

        /// <summary>
        /// Rendu de toutes les edges
        /// </summary>
        public void RenderEdges(IEnumerable<EdgeElement> edges, Matrix transform)
        {
            foreach (var edge in edges)
            {
                var color = edge.IsSelected ? EdgeSelectedColor : EdgeColor;
                var pos1 = Vector3.TransformCoordinate(edge.Vertex1.Position, transform);
                var pos2 = Vector3.TransformCoordinate(edge.Vertex2.Position, transform);
                RenderLine(pos1, pos2, color);
            }
        }

        /// <summary>
        /// Rendu de toutes les faces
        /// </summary>
        public void RenderFaces(IEnumerable<FaceElement> faces, Matrix transform)
        {
            foreach (var face in faces)
            {
                var color = face.IsSelected ? FaceSelectedColor : FaceColor;
                var pos1 = Vector3.TransformCoordinate(face.Vertex1.Position, transform);
                var pos2 = Vector3.TransformCoordinate(face.Vertex2.Position, transform);
                var pos3 = Vector3.TransformCoordinate(face.Vertex3.Position, transform);
                RenderTriangle(pos1, pos2, pos3, color);
            }
        }

        /// <summary>
        /// Rendu d'une sphère pour représenter un vertex
        /// </summary>
        private void RenderSphere(Vector3 position, float radius, Color4 color)
        {
            var sphere = new BoundingSphere(position, radius);
            renderer.RenderSelectionSphere(sphere, color);
        }

        /// <summary>
        /// Rendu d'une ligne pour représenter une edge
        /// </summary>
        private void RenderLine(Vector3 start, Vector3 end, Color4 color)
        {
            var verts = new[]
            {
                new VertexTypePC { Position = start, Colour = (uint)color.ToRgba() },
                new VertexTypePC { Position = end, Colour = (uint)color.ToRgba() }
            };
            
            renderer.RenderLines(verts, Matrix.Identity);
        }

        /// <summary>
        /// Rendu d'un triangle pour représenter une face
        /// </summary>
        private void RenderTriangle(Vector3 v1, Vector3 v2, Vector3 v3, Color4 color)
        {
            var verts = new[]
            {
                new VertexTypePC { Position = v1, Colour = (uint)color.ToRgba() },
                new VertexTypePC { Position = v2, Colour = (uint)color.ToRgba() },
                new VertexTypePC { Position = v3, Colour = (uint)color.ToRgba() }
            };
            
            renderer.RenderFilledTriangles(verts, Matrix.Identity);
        }

        /// <summary>
        /// Rendu de la grille de sélection pour les éléments sélectionnés
        /// </summary>
        public void RenderSelectionHighlight(IEnumerable<MeshElement> selectedElements, Matrix transform)
        {
            foreach (var element in selectedElements)
            {
                if (element is VertexElement vertex)
                {
                    var worldPos = Vector3.TransformCoordinate(vertex.Position, transform);
                    RenderSphere(worldPos, VertexSize * 1.5f, VertexSelectedColor);
                }
                else if (element is EdgeElement edge)
                {
                    var pos1 = Vector3.TransformCoordinate(edge.Vertex1.Position, transform);
                    var pos2 = Vector3.TransformCoordinate(edge.Vertex2.Position, transform);
                    RenderLine(pos1, pos2, EdgeSelectedColor);
                }
                else if (element is FaceElement face)
                {
                    var pos1 = Vector3.TransformCoordinate(face.Vertex1.Position, transform);
                    var pos2 = Vector3.TransformCoordinate(face.Vertex2.Position, transform);
                    var pos3 = Vector3.TransformCoordinate(face.Vertex3.Position, transform);
                    RenderTriangle(pos1, pos2, pos3, FaceSelectedColor);
                }
            }
        }
        /// <summary>
        /// Méthode principale de rendu appelée depuis WorldForm
        /// </summary>
        public void Render(DeviceContext context, Camera camera, MeshEditor editor)
        {
            if (editor == null || !editor.IsActive) return;

            // Set render states for solid drawing with depth testing and backface culling
            renderer.shaders.SetRasterizerMode(context, RasterizerMode.Solid);
            renderer.shaders.SetDepthStencilMode(context, DepthStencilMode.Enabled);

            lock (editor.SyncRoot)
            {
                var transform = editor.Transform;
                var elements = editor.GetAllElements();

                // Rendu des éléments selon le mode
                switch (editor.CurrentMode)
                {
                    case MeshEditMode.Vertex:
                        RenderVertices(elements.Cast<VertexElement>(), transform);
                        break;
                    case MeshEditMode.Edge:
                        RenderEdges(elements.Cast<EdgeElement>(), transform);
                        break;
                    case MeshEditMode.Face:
                        RenderFaces(elements.Cast<FaceElement>(), transform);
                        break;
                }

                // Rendu de la sélection
                if (editor.SelectedElements.Count > 0)
                {
                    RenderSelectionHighlight(editor.SelectedElements, transform);
                }

                // Rendu du survol
                if (editor.HoveredElement != null && !editor.HoveredElement.IsSelected)
                {
                    if (editor.HoveredElement is VertexElement v)
                    {
                        var worldPos = Vector3.TransformCoordinate(v.Position, transform);
                        RenderSphere(worldPos, VertexSize * 1.5f, VertexHoverColor);
                    }
                    else if (editor.HoveredElement is EdgeElement e)
                    {
                        var pos1 = Vector3.TransformCoordinate(e.Vertex1.Position, transform);
                        var pos2 = Vector3.TransformCoordinate(e.Vertex2.Position, transform);
                        RenderLine(pos1, pos2, EdgeHoverColor);
                    }
                    else if (editor.HoveredElement is FaceElement f)
                    {
                        var pos1 = Vector3.TransformCoordinate(f.Vertex1.Position, transform);
                        var pos2 = Vector3.TransformCoordinate(f.Vertex2.Position, transform);
                        var pos3 = Vector3.TransformCoordinate(f.Vertex3.Position, transform);
                        RenderTriangle(pos1, pos2, pos3, FaceHoverColor);
                    }
                }
            }
        }
    }
}
