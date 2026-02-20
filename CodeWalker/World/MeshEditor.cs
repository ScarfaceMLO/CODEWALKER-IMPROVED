using System;
using System.Collections.Generic;
using System.Linq;
using CodeWalker.GameFiles;
using SharpDX;

namespace CodeWalker.World
{
    /// <summary>
    /// Gestionnaire principal de l'édition de mesh pour les fichiers YDR
    /// </summary>
    public class MeshEditor
    {
        public YdrFile CurrentYdr { get; private set; }
        public MeshEditMode CurrentMode { get; set; }
        public List<MeshElement> SelectedElements { get; private set; }
        
        public object SyncRoot { get; } = new object();
        
        private List<VertexElement> allVertices;
        private List<EdgeElement> allEdges;
        private List<FaceElement> allFaces;
        private Dictionary<int, VertexElement> vertexLookup; // Needs to be keyed by geometry too? No, just clear per geometry build.

        // Global lists for raycasting/rendering
        public List<VertexElement> AllVertices => allVertices;
        public List<EdgeElement> AllEdges => allEdges;
        public List<FaceElement> AllFaces => allFaces;

        public bool IsActive { get; private set; }
        public Matrix Transform { get; private set; } = Matrix.Identity;
        public Matrix InverseTransform { get; private set; } = Matrix.Identity;

        public MeshEditor()
        {
            SelectedElements = new List<MeshElement>();
            allVertices = new List<VertexElement>();
            allEdges = new List<EdgeElement>();
            allFaces = new List<FaceElement>();
            vertexLookup = new Dictionary<int, VertexElement>();
            CurrentMode = MeshEditMode.None;
            IsActive = false;
        }

        /// <summary>
        /// Active le mode d'édition pour un YDR spécifique
        /// </summary>
        public bool StartEditing(YdrFile ydr, Matrix transform)
        {
            if (ydr?.Drawable == null)
                return false;

            var models = ydr.Drawable.AllModels;
            if (models == null || models.Length == 0)
                return false;

            CurrentYdr = ydr;
            Transform = transform;
            InverseTransform = Matrix.Invert(transform);
            
            BuildMeshElements();
            IsActive = true;
            CurrentMode = MeshEditMode.Vertex; // Mode par défaut
            
            return true;
        }

        /// <summary>
        /// Costruit les éléments de mesh pour tous les modèles et géométries
        /// </summary>
        private void BuildMeshElements()
        {
            lock (SyncRoot)
            {
                allVertices.Clear();
                allEdges.Clear();
                allFaces.Clear();
                SelectedElements.Clear();

                var models = CurrentYdr?.Drawable?.AllModels;
                if (models == null) return;

                foreach (var model in models)
                {
                    if (model.Geometries == null) continue;
                    foreach (var geometry in model.Geometries)
                    {
                        BuildGeometryElements(geometry);
                    }
                }
            }
        }

        private void BuildGeometryElements(DrawableGeometry geometry)
        {
            if (geometry?.VertexData == null || geometry?.IndexBuffer == null)
                return;

            vertexLookup.Clear(); // Local lookup for this geometry only

            // Créer les vertices
            var vertexData = geometry.VertexData;
            int vertexCount = vertexData.VertexCount;
            
            for (int i = 0; i < vertexCount; i++)
            {
                var position = GetVertexPosition(geometry, i);
                var vertex = new VertexElement(i, position, geometry);
                allVertices.Add(vertex);
                vertexLookup[i] = vertex;
            }

            // Créer les faces et edges à partir des indices
            var indices = geometry.IndexBuffer.Indices;
            if (indices != null)
            {
                var edgeSet = new HashSet<(int, int)>();
                
                for (int i = 0; i < indices.Length; i += 3)
                {
                    if (i + 2 >= indices.Length) break;

                    int idx1 = (int)indices[i];
                    int idx2 = (int)indices[i + 1];
                    int idx3 = (int)indices[i + 2];

                    if (!vertexLookup.ContainsKey(idx1) || 
                        !vertexLookup.ContainsKey(idx2) || 
                        !vertexLookup.ContainsKey(idx3))
                        continue;

                    var v1 = vertexLookup[idx1];
                    var v2 = vertexLookup[idx2];
                    var v3 = vertexLookup[idx3];

                    // Créer la face
                    // FaceIndex is just sequential within the list, doesn't matter much for now
                    var face = new FaceElement(allFaces.Count, v1, v2, v3);
                    allFaces.Add(face);

                    // Créer les edges (éviter les doublons)
                    AddEdgeIfNew(edgeSet, idx1, idx2, v1, v2);
                    AddEdgeIfNew(edgeSet, idx2, idx3, v2, v3);
                    AddEdgeIfNew(edgeSet, idx3, idx1, v3, v1);
                }
            }
        }

        private void AddEdgeIfNew(HashSet<(int, int)> edgeSet, int idx1, int idx2, VertexElement v1, VertexElement v2)
        {
            var edgeKey = idx1 < idx2 ? (idx1, idx2) : (idx2, idx1);
            if (!edgeSet.Contains(edgeKey))
            {
                edgeSet.Add(edgeKey);
                allEdges.Add(new EdgeElement(v1, v2));
            }
        }

        /// <summary>
        /// Obtient la position d'un vertex à partir du VertexData
        /// </summary>
        private Vector3 GetVertexPosition(DrawableGeometry geometry, int index)
        {
            var vertexData = geometry.VertexData;
            if (vertexData?.VertexBytes == null)
                return Vector3.Zero;

            int stride = vertexData.VertexStride;
            int offset = index * stride;

            if (offset + 12 > vertexData.VertexBytes.Length)
                return Vector3.Zero;

            // Les positions sont généralement les 12 premiers bytes (3 floats)
            float x = BitConverter.ToSingle(vertexData.VertexBytes, offset);
            float y = BitConverter.ToSingle(vertexData.VertexBytes, offset + 4);
            float z = BitConverter.ToSingle(vertexData.VertexBytes, offset + 8);

            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Sélectionne un élément en utilisant un rayon de picking
        /// </summary>
        public bool SelectElement(Ray pickRay, bool addToSelection, float maxDistance = 0.5f)
        {
            lock (SyncRoot)
            {
                MeshElement closestElement = null;
                float closestDistance = float.MaxValue;

                switch (CurrentMode)
                {
                    case MeshEditMode.Vertex:
                        closestElement = FindClosestVertex(pickRay, maxDistance, ref closestDistance);
                        break;
                    case MeshEditMode.Edge:
                        closestElement = FindClosestEdge(pickRay, maxDistance, ref closestDistance);
                        break;
                    case MeshEditMode.Face:
                        closestElement = FindClosestFace(pickRay, ref closestDistance);
                        break;
                }

                if (closestElement != null)
                {
                    if (!addToSelection)
                    {
                        // Deselect others if not adding
                        foreach (var elem in SelectedElements)
                            elem.IsSelected = false;
                        SelectedElements.Clear();

                        SelectedElements.Add(closestElement);
                        closestElement.IsSelected = true;
                    }
                    else
                    {
                        // Toggle selection
                        if (SelectedElements.Contains(closestElement))
                        {
                            SelectedElements.Remove(closestElement);
                            closestElement.IsSelected = false;
                        }
                        else
                        {
                            SelectedElements.Add(closestElement);
                            closestElement.IsSelected = true;
                        }
                    }
                    return true;
                }
                else if (!addToSelection)
                {
                    // Clicked nothing without modifier -> clear selection
                    foreach (var elem in SelectedElements)
                        elem.IsSelected = false;
                    SelectedElements.Clear();
                }

                return false;
            }
        }

        public MeshElement HoveredElement { get; private set; }

        public void UpdateHover(Ray pickRay, float maxDistance = 0.5f)
        {
            lock (SyncRoot)
            {
                MeshElement closestElement = null;
                float closestDistance = float.MaxValue;

                switch (CurrentMode)
                {
                    case MeshEditMode.Vertex:
                        closestElement = FindClosestVertex(pickRay, maxDistance, ref closestDistance);
                        break;
                    case MeshEditMode.Edge:
                        closestElement = FindClosestEdge(pickRay, maxDistance, ref closestDistance);
                        break;
                    case MeshEditMode.Face:
                        closestElement = FindClosestFace(pickRay, ref closestDistance);
                        break;
                }

                HoveredElement = closestElement;
            }
        }

        private VertexElement FindClosestVertex(Ray ray, float maxDistance, ref float closestDistance)
        {
            VertexElement closest = null;
            
            foreach (var vertex in allVertices)
            {
                float distance = RayToPointDistance(ray, vertex.Position);
                if (distance < maxDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = vertex;
                }
            }
            
            return closest;
        }

        private EdgeElement FindClosestEdge(Ray ray, float maxDistance, ref float closestDistance)
        {
            EdgeElement closest = null;
            
            foreach (var edge in allEdges)
            {
                float distance = RayToLineSegmentDistance(ray, edge.Vertex1.Position, edge.Vertex2.Position);
                if (distance < maxDistance && distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = edge;
                }
            }
            
            return closest;
        }

        private FaceElement FindClosestFace(Ray ray, ref float closestDistance)
        {
            FaceElement closest = null;
            
            foreach (var face in allFaces)
            {
                float? distance = RayIntersectsTriangle(ray, face.Vertex1.Position, face.Vertex2.Position, face.Vertex3.Position);
                if (distance.HasValue && distance.Value < closestDistance)
                {
                    closestDistance = distance.Value;
                    closest = face;
                }
            }
            
            return closest;
        }

        /// <summary>
        /// Calcule la distance entre un rayon et un point
        /// </summary>
        private float RayToPointDistance(Ray ray, Vector3 point)
        {
            var toPoint = point - ray.Position;
            var projection = Vector3.Dot(toPoint, ray.Direction);
            
            if (projection < 0)
                return Vector3.Distance(ray.Position, point);
            
            var closestPoint = ray.Position + ray.Direction * projection;
            return Vector3.Distance(closestPoint, point);
        }

        /// <summary>
        /// Calcule la distance entre un rayon et un segment de ligne
        /// </summary>
        private float RayToLineSegmentDistance(Ray ray, Vector3 p1, Vector3 p2)
        {
            var midPoint = (p1 + p2) * 0.5f;
            return RayToPointDistance(ray, midPoint);
        }

        /// <summary>
        /// Test d'intersection rayon-triangle (algorithme de Möller-Trumbore)
        /// </summary>
        private float? RayIntersectsTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            const float EPSILON = 0.0000001f;
            
            var edge1 = v1 - v0;
            var edge2 = v2 - v0;
            var h = Vector3.Cross(ray.Direction, edge2);
            var a = Vector3.Dot(edge1, h);
            
            if (a > -EPSILON && a < EPSILON)
                return null;
            
            var f = 1.0f / a;
            var s = ray.Position - v0;
            var u = f * Vector3.Dot(s, h);
            
            if (u < 0.0f || u > 1.0f)
                return null;
            
            var q = Vector3.Cross(s, edge1);
            var v = f * Vector3.Dot(ray.Direction, q);
            
            if (v < 0.0f || u + v > 1.0f)
                return null;
            
            var t = f * Vector3.Dot(edge2, q);
            
            if (t > EPSILON)
                return t;
            
            return null;
        }

        /// <summary>
        /// Déplace les éléments sélectionnés
        /// </summary>
        public void MoveSelectedElements(Vector3 delta)
        {
            lock (SyncRoot)
            {
                var uniqueVertices = new HashSet<VertexElement>();

                foreach (var element in SelectedElements)
                {
                    if (element is VertexElement v)
                    {
                        uniqueVertices.Add(v);
                    }
                    else if (element is EdgeElement e)
                    {
                        uniqueVertices.Add(e.Vertex1);
                        uniqueVertices.Add(e.Vertex2);
                    }
                    else if (element is FaceElement f)
                    {
                        uniqueVertices.Add(f.Vertex1);
                        uniqueVertices.Add(f.Vertex2);
                        uniqueVertices.Add(f.Vertex3);
                    }
                }

                foreach (var v in uniqueVertices)
                {
                    v.Move(delta);
                }
            }
        }

        /// <summary>
        /// Met à jour le buffer de vertices avec les positions modifiées
        /// </summary>
        public void UpdateVertexBuffer()
        {
            // Group vertices by geometry since efficient multiple modification support is needed
            var groups = allVertices.GroupBy(v => v.Geometry);

            foreach (var group in groups)
            {
                var geometry = group.Key;
                if (geometry == null) continue;

                var vertexData = geometry.VertexData;
                if (vertexData?.VertexBytes == null) continue;

                int stride = vertexData.VertexStride;

                foreach (var vertex in group)
                {
                    int offset = vertex.VertexIndex * stride;
                    if (offset + 12 > vertexData.VertexBytes.Length)
                        continue;

                    var pos = vertex.Position; // Position is already transformed relative to model/origin? 
                    // Wait, VertexElement stores local position or world position?
                    // In BuildMeshElements, we called GetVertexPosition which returns local position from raw bytes.
                    // Wait, in StartEditing we stored Transform.
                    // The renderer transforms vertex.Position by Transform.
                    // So vertex.Position is LOCAL.
                    // So we can write it directly back to the buffer.
                    
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.X), 0, vertexData.VertexBytes, offset, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.Y), 0, vertexData.VertexBytes, offset + 4, 4);
                    Buffer.BlockCopy(BitConverter.GetBytes(pos.Z), 0, vertexData.VertexBytes, offset + 8, 4);
                }
            }
        }

        /// <summary>
        /// Sauvegarde les modifications dans le fichier YDR
        /// </summary>
        public byte[] SaveModifications()
        {
            UpdateVertexBuffer();
            return CurrentYdr?.Save();
        }

        /// <summary>
        /// Annule toutes les modifications et quitte le mode d'édition
        /// </summary>
        public void Cancel()
        {
            foreach (var vertex in allVertices)
            {
                vertex.ResetPosition();
            }
            
            StopEditing();
        }

        /// <summary>
        /// Arrête le mode d'édition
        /// </summary>
        public void StopEditing()
        {
            lock (SyncRoot)
            {
                SelectedElements.Clear();
                allVertices.Clear();
                allEdges.Clear();
                allFaces.Clear();
                vertexLookup.Clear();
                CurrentYdr = null;
                CurrentMode = MeshEditMode.None;
                IsActive = false;
            }
        }

        /// <summary>
        /// Obtient tous les éléments selon le mode actuel
        /// </summary>
        public IEnumerable<MeshElement> GetAllElements()
        {
            switch (CurrentMode)
            {
                case MeshEditMode.Vertex:
                    return allVertices;
                case MeshEditMode.Edge:
                    return allEdges;
                case MeshEditMode.Face:
                    return allFaces;
                default:
                    return Enumerable.Empty<MeshElement>();
            }
        }

        /// <summary>
        /// Obtient la position centrale des éléments sélectionnés (pour le gizmo)
        /// </summary>
        public Vector3 GetSelectionCenter()
        {
            if (SelectedElements.Count == 0)
                return Vector3.Zero;

            var sum = Vector3.Zero;
            foreach (var element in SelectedElements)
            {
                sum += element.Position;
            }
            return sum / SelectedElements.Count;
        }
        public void CommitChanges()
        {
            foreach (var element in SelectedElements)
            {
                element.CommitMove();
            }
        }

        /// <summary>
        /// Supprime les faces sélectionnées et retourne un UndoStep
        /// </summary>
        public MeshFaceDeleteUndoStep DeleteSelectedFaces()
        {
            if (CurrentMode != MeshEditMode.Face) return null;

            var facesToDelete = SelectedElements.OfType<FaceElement>().ToList();
            if (facesToDelete.Count == 0) return null;

            // Deselect faces so that if undone, they return deselected
            foreach (var face in facesToDelete)
            {
                face.IsSelected = false;
            }

            DeleteFaces(facesToDelete);

            SelectedElements.Clear();

            return new MeshFaceDeleteUndoStep(this, facesToDelete);
        }

        public void DeleteFaces(List<FaceElement> faces)
        {
            lock (SyncRoot)
            {
                var geometries = new HashSet<DrawableGeometry>();

                foreach (var face in faces)
                {
                    allFaces.Remove(face);
                    geometries.Add(face.Vertex1.Geometry);
                }

                UpdateIndexBuffers(geometries);
            }
        }


        public void RestoreFaces(List<FaceElement> faces)
        {
            lock (SyncRoot)
            {
                var geometries = new HashSet<DrawableGeometry>();

                foreach (var face in faces)
                {
                    allFaces.Add(face);
                    geometries.Add(face.Vertex1.Geometry);
                }

                UpdateIndexBuffers(geometries);
            }
        }

        private void UpdateIndexBuffers(IEnumerable<DrawableGeometry> geometries)
        {
            foreach (var geometry in geometries)
            {
                if (geometry?.IndexBuffer == null) continue;

                // Get all faces for this geometry
                var geomFaces = allFaces.Where(f => f.Vertex1.Geometry == geometry).ToList();

                var newIndices = new List<ushort>(); // Assuming ushort for now
                // Check if we need uint
                bool useUint = (geometry.VertexData.VertexCount > 65535); 
                // Wait, IndexBuffer handles the type internally?
                // The IndexBuffer class usually has a data array.
                // Let's assume we can set Indices to a ushort[] array.
                
                foreach (var face in geomFaces)
                {
                    if (face.Vertex1.VertexIndex < 0 || face.Vertex1.VertexIndex >= geometry.VertexData.VertexCount)
                    {
                        string msg = $"[MeshEditor] Invalid Vertex1 Index: {face.Vertex1.VertexIndex} (Max: {geometry.VertexData.VertexCount})\n";
                        try { System.IO.File.AppendAllText("crash.log", DateTime.Now + " " + msg); } catch { }
                        Console.WriteLine(msg);
                    }
                    if (face.Vertex2.VertexIndex < 0 || face.Vertex2.VertexIndex >= geometry.VertexData.VertexCount)
                    {
                        string msg = $"[MeshEditor] Invalid Vertex2 Index: {face.Vertex2.VertexIndex} (Max: {geometry.VertexData.VertexCount})\n";
                        try { System.IO.File.AppendAllText("crash.log", DateTime.Now + " " + msg); } catch { }
                        Console.WriteLine(msg);
                    }
                    if (face.Vertex3.VertexIndex < 0 || face.Vertex3.VertexIndex >= geometry.VertexData.VertexCount)
                    {
                        string msg = $"[MeshEditor] Invalid Vertex3 Index: {face.Vertex3.VertexIndex} (Max: {geometry.VertexData.VertexCount})\n";
                        try { System.IO.File.AppendAllText("crash.log", DateTime.Now + " " + msg); } catch { }
                        Console.WriteLine(msg);
                    }

                    newIndices.Add((ushort)face.Vertex1.VertexIndex);
                    newIndices.Add((ushort)face.Vertex2.VertexIndex);
                    newIndices.Add((ushort)face.Vertex3.VertexIndex);
                }

                geometry.IndexBuffer.Indices = newIndices.ToArray();
                geometry.IndexBuffer.IndicesCount = (uint)newIndices.Count;
                geometry.IndicesCount = (uint)newIndices.Count;
                geometry.TrianglesCount = (uint)(newIndices.Count / 3);
            }
        }
    }
}
