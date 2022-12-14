using System;
using System.Globalization;
using System.Numerics;

namespace GraphicsLibrary.Models
{
    public static class TextureCoordinate
    {
        public const string Name = "vt";
        public const int MinArrayLength = 2;

        public static Vector3 FieldFromStringArray(string[] elements)
        {
            if (elements.Length < MinArrayLength)
            {
                throw new ArgumentException($"Elements array must be more or equal {MinArrayLength}");
            }

            if (!float.TryParse(elements[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var u))
            {
                throw new ArgumentException($"Couldn't convert u element to float");
            }


            if (elements.Length < MinArrayLength + 1)
            {
                return new Vector3(u, 0, 0);
            }

            if (!float.TryParse(elements[2], NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
            {
                throw new ArgumentException($"Couldn't convert v element to float");
            }

            if (elements.Length < MinArrayLength + 2)
            {
                return new Vector3(u, v, 0);
            }

            return !float.TryParse(elements[3], NumberStyles.Any, CultureInfo.InvariantCulture, out var w)
                ? throw new ArgumentException($"Couldn't convert w element to float")
                : new Vector3(u, v, w);
        }
    }
}