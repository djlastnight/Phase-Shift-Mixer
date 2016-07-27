namespace VuShaderEffect
{
    using System;
    using System.Reflection;

    internal static class Global
    {
        private static string assemblyShortName;

        private static string AssemblyShortName
        {
            get
            {
                if (assemblyShortName == null)
                {
                    Assembly a = typeof(Global).Assembly;

                    // Pull out the short name.
                    assemblyShortName = a.ToString().Split(',')[0];
                }

                return assemblyShortName;
            }
        }

        public static Uri MakePackUri(string relativeFile)
        {
            string uriString = "pack://application:,,,/" + AssemblyShortName + ";component/" + relativeFile;
            return new Uri(uriString);
        }
    }
}