using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using GraphicsLibrary.Models;

namespace GraphicsLibrary.ObjParser
{
    public class ParserObj
    {

        private static string _filePath;
        public ParserObj(string filePath) {
            _filePath = filePath;
        }

        public ObjModel GetObjModel()
        {
            string[] lines = File.ReadAllLines(_filePath);
            if (string.IsNullOrEmpty(_filePath)) {
                throw new ArgumentException("File path can not be null or empty");
            }

            if (!File.Exists(_filePath)) {
                throw new ArgumentException("File does not exist");
            }

            if (lines == null)
            {
                throw new ArgumentNullException();
            }

            var objModel = new ObjModel();
            foreach (var line in lines)
            {
                var lineElements = line.Replace("  ", " ").Split(' ');
                if (lineElements.Length == 0)
                {
                    continue;
                }

                switch (lineElements[0])
                {
                    case GeometricVertex.Name:
                        objModel.GVertices.Add((uint)objModel.GVertices.Count + 1, GeometricVertex.FieldFromStringArray(lineElements));
                        objModel.TransformGeometricVertices.Add((uint)objModel.GVertices.Count, Vector4.One);
                        break;
                    case TextureCoordinate.Name:
                        objModel.TextureCoordinates.Add((uint)objModel.TextureCoordinates.Count + 1, TextureCoordinate.FieldFromStringArray(lineElements));
                        break;
                    case VertexNormal.Name:
                        objModel.VertexNormals.Add((uint)objModel.VertexNormals.Count + 1, VertexNormal.FieldFromStringArray(lineElements));
                        break;
                    case PolygonalElement.Name:
                        objModel.Polygons.Add(PolygonalElement.FieldFromStringArray(lineElements));
                        break;
                }

            }

            objModel.PointsPerPolygon = new List<List<Vector2>>();
            for (var i = 0; i < objModel.Polygons.Count; i++)
            {
                objModel.PointsPerPolygon.Add(new List<Vector2>());
            }

            return objModel;
        }
    }
}