using System;
using System.Numerics;

namespace Jarvis.API.Simulation
{
    public struct Bounds : IEquatable<Bounds>
    {
        public Vector3 origin, size;

        public static Bounds Infinite => new Bounds(Vector3.Zero, new Vector3(float.MaxValue, float.MaxValue, float.MaxValue));
        public static Bounds One => new Bounds(Vector3.Zero, Vector3.One);

        public Bounds(Vector3 origin, Vector3 size)
        {
            this.origin = origin;
            this.size = size;
        }

        public bool IsInside(Vector3 pt)
        {
            Vector3 rel = pt - origin;
            return Math.Abs(rel.X) <= size.X && Math.Abs(rel.Y) <= size.Y && Math.Abs(rel.Z) <= size.Z;
        }

        public override bool Equals(object obj) => obj is Bounds bounds && Equals(bounds);
        public bool Equals(Bounds other) => origin.Equals(other.origin) && size.Equals(other.size);

        public override int GetHashCode()
        {
            int hashCode = 2122828547;
            hashCode = hashCode * -1521134295 + origin.GetHashCode();
            hashCode = hashCode * -1521134295 + size.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Bounds left, Bounds right) => left.Equals(right);
        public static bool operator !=(Bounds left, Bounds right) => !left.Equals(right);
    }
}