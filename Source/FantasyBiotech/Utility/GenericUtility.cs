using System;

namespace FantasyBiotech
{
    public static class GenericUtility
    {
        public static string GetClassName(Type declaringType)
        {
            while (declaringType?.DeclaringType != null)
            {
                declaringType = declaringType.DeclaringType;
            }

            return declaringType?.Name ?? "UnknownClass";
        }
    }
}
