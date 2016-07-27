namespace PsMixer.Helpers
{
    using System;
    using System.IO;
    using System.Text;

    public static class IniReader
    {
        public static string[] GetValues(string iniFilePath, Encoding encoding, params string[] tags)
        {
            if (iniFilePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            if (!File.Exists(iniFilePath))
            {
                throw new ArgumentException("filePath does not exists");
            }

            if (tags == null)
            {
                return null;
            }

            string[] results = new string[tags.Length];

            for (int i = 0; i < results.Length; i++)
            {
                results[i] = "n/a";
            }

            using (var reader = new StreamReader(iniFilePath, encoding))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line.StartsWith("["))
                    {
                        continue;
                    }

                    for (int i = 0; i < tags.Length; i++)
                    {
                        if (line.StartsWith(tags[i], StringComparison.OrdinalIgnoreCase))
                        {
                            int indexOfEqualSign = line.IndexOf("=");
                            if (indexOfEqualSign == -1)
                            {
                                break;
                            }

                            string value = line.Substring(indexOfEqualSign + 1);
                            if (value.StartsWith(" "))
                            {
                                value = value.Substring(1);
                            }

                            results[i] = value;
                            break;
                        }
                    }
                }
            }

            return results;
        }

        public static string GetFirstValue(string iniFilePath, Encoding encoding, string tag)
        {
            if (iniFilePath == null)
            {
                throw new ArgumentNullException("filePath");
            }

            if (!File.Exists(iniFilePath))
            {
                throw new ArgumentException("filePath does not exists");
            }

            if (tag == null)
            {
                throw new ArgumentNullException("tag");
            }

            string value = "n/a";

            using (var reader = new StreamReader(iniFilePath, encoding))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line.StartsWith("["))
                    {
                        continue;
                    }

                    if (line.StartsWith(tag, StringComparison.OrdinalIgnoreCase))
                    {
                        int indexOfEqualSign = line.IndexOf("=");
                        if (indexOfEqualSign == -1)
                        {
                            continue;
                        }

                        value = line.Substring(indexOfEqualSign + 1);
                        if (value.StartsWith(" "))
                        {
                            value = value.Substring(1);
                        }

                        break;
                    }
                }
            }

            return value;
        }
    }
}