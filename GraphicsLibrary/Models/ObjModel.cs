using GraphicsLibrary.Space;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using GraphicsLibrary.Rasterization;

namespace GraphicsLibrary.Models {
    public class ObjModel
    {
        //public readonly Dictionary<uint, Vector3> SVertices; // space
        public readonly Dictionary<uint, Vector3> TextureCoordinates;
        public readonly Dictionary<uint, Vector3> VertexNormals;
        public readonly Dictionary<uint, Vector4> GVertices; // geometric
        public readonly List<PolygonalElement> Polygons;

        public List<List<Vector2>> PointsPerPolygon;
        public List<Vector2> Points;

        public readonly Dictionary<uint, Vector4> TransformGeometricVertices;
        public Matrix4x4 TransformMatrix;

        public Matrix4x4 ProjectionSpace { get; set; }
        public Matrix4x4 ViewSpace { get; set; }
        public ViewportSpace ViewportSpace { get; set; }

        public Matrix4x4 ScaleM { get; set; }
        public Matrix4x4 RotationMX { get; set; }
        public Matrix4x4 RotationMY { get; set; }
        public Matrix4x4 RotationMZ { get; set; }
        public Matrix4x4 TranslationM { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        // Ориентации осей в новом пространстве
        public float TranslationX { get; set; }
        public float TranslationY { get; set; }
        public float TranslationZ { get; set; }

        public float Scale { get; set; }
        public float AngleX { get; set; }
        public float AngleZ { get; set; }
        public float AngleY { get; set; }

        public ObjModel()
        {
            //SVertices = new Dictionary<uint, Vector3>();
            TextureCoordinates = new Dictionary<uint, Vector3>();
            VertexNormals = new Dictionary<uint, Vector3>();
            GVertices = new Dictionary<uint, Vector4>();
            Polygons = new List<PolygonalElement>();
            TransformGeometricVertices = new Dictionary<uint, Vector4>();
            Points = new List<Vector2>();
            Scale = 1;
        }

        public void TransformVertices()
        {
            ScaleM = Matrix4x4.CreateScale(Scale);

            // Translation - координаты, в котором находится новое пространство
            TranslationM = Matrix4x4.CreateTranslation(TranslationX, TranslationY, TranslationZ);

            RotationMX = Matrix4x4.CreateRotationX(AngleX);
            RotationMY = Matrix4x4.CreateRotationY(AngleY);
            RotationMZ = Matrix4x4.CreateRotationZ(AngleZ);

            TransformMatrix = RotationMX * RotationMY * RotationMZ * ScaleM * TranslationM *
                              ViewSpace * ProjectionSpace;
            
            Parallel.ForEach(GVertices.Keys, key =>
            {
                var vector = Vector4.Transform(GVertices[key], TransformMatrix);
                vector = Vector4.Divide(vector, vector.W);
                TransformGeometricVertices[key] = Vector4.Transform(vector, ViewportSpace.TransposeMatrix);
            });
        }

        public List<Vector2> FindPoints()
        {
            for (var i = 0; i < Polygons.Count; i++)
            {
                PointsPerPolygon[i].Clear();

                for (var j = 0; j < Polygons[i].GeometricVertices.Count - 1; j++)
                {
                    PointsPerPolygon[i].AddRange(LineRasterization.DdaLines(
                        TransformGeometricVertices[Polygons[i].GeometricVertices[j]],
                        TransformGeometricVertices[Polygons[i].GeometricVertices[j + 1]], Width, Height));
                }

                PointsPerPolygon[i].AddRange(LineRasterization.DdaLines(
                    TransformGeometricVertices[Polygons[i].GeometricVertices[^1]],
                    TransformGeometricVertices[Polygons[i].GeometricVertices[0]], Width, Height));
            }

            Points.Clear();
            Points.AddRange(PointsPerPolygon.SelectMany(x => x));

            return Points;
        }
    }
}