using CodeWalker.GameFiles;
using CodeWalker.Rendering;
using SharpDX;
using System.Collections.Generic;

namespace CodeWalker.World
{
    public class MeshTransformUndoStep : Project.UndoStep
    {
        private MeshEditor editor;
        private List<VertexElement> vertices;
        private Vector3 delta;

        public MeshTransformUndoStep(MeshEditor editor, IEnumerable<MeshElement> elements, Vector3 delta)
        {
            this.editor = editor;
            this.delta = delta;
            this.vertices = GetUniqueVertices(elements);
        }

        private List<VertexElement> GetUniqueVertices(IEnumerable<MeshElement> elements)
        {
            var unique = new HashSet<VertexElement>();
            foreach (var element in elements)
            {
                if (element is VertexElement v) unique.Add(v);
                else if (element is EdgeElement e)
                {
                    unique.Add(e.Vertex1);
                    unique.Add(e.Vertex2);
                }
                else if (element is FaceElement f)
                {
                    unique.Add(f.Vertex1);
                    unique.Add(f.Vertex2);
                    unique.Add(f.Vertex3);
                }
            }
            return new List<VertexElement>(unique);
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            if (editor == null) return;

            foreach (var v in vertices)
            {
                v.Move(-delta);
                v.CommitMove();
            }

            UpdateState(wf);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            if (editor == null) return;

            foreach (var v in vertices)
            {
                v.Move(delta);
                v.CommitMove();
            }

            UpdateState(wf);
        }

        private void UpdateState(WorldForm wf)
        {
            editor.UpdateVertexBuffer();

            if (editor.CurrentYdr?.Drawable != null)
            {
                var renderable = wf.Renderer.RenderableCache.GetRenderable(editor.CurrentYdr.Drawable);
                if (renderable != null && renderable.IsLoaded)
                {
                    renderable.UpdateVertexData(wf.Renderer.Device);
                }
            }
            
            // Should probably restore selection/gizmo position too?
            // wf.SetWidgetPosition(editor.GetSelectionCenter());
        }
        
        public override string ToString()
        {
            return $"Mesh Transform ({vertices.Count} vertices)";
        }
    }

    public class MeshFaceDeleteUndoStep : Project.UndoStep
    {
        private MeshEditor editor;
        private List<FaceElement> deletedFaces;

        public MeshFaceDeleteUndoStep(MeshEditor editor, List<FaceElement> deletedFaces)
        {
            this.editor = editor;
            this.deletedFaces = deletedFaces;
        }

        public override void Undo(WorldForm wf, ref MapSelection sel)
        {
            if (editor == null) return;

            editor.RestoreFaces(deletedFaces);
            // editor.UpdateIndexBuffers(deletedFaces.Select(f => f.Vertex1.Geometry).Distinct()); // Implemented in MeshEditor
            
            UpdateState(wf);
        }

        public override void Redo(WorldForm wf, ref MapSelection sel)
        {
            if (editor == null) return;

            editor.DeleteFaces(deletedFaces);
            
            UpdateState(wf);
        }

        private void UpdateState(WorldForm wf)
        {
            // Trigger redraw?
            if (editor.CurrentYdr?.Drawable != null)
            {
                var renderable = wf.Renderer.RenderableCache.GetRenderable(editor.CurrentYdr.Drawable);
                if (renderable != null && renderable.IsLoaded)
                {
                    // Update index buffer on GPU
                    renderable.UpdateIndexData(wf.Renderer.Device); 
                }
            }
        }

        public override string ToString()
        {
            return $"Delete {deletedFaces.Count} Faces";
        }
    }
}
