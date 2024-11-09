namespace Audionomy.BL.Extensions
{
    public static class StringExtentions
    {
        /// <summary>
        /// Retrieves the file name without its extension from a given file path.
        /// </summary>
        /// <param name="filename">The full file path or file name.</param>
        /// <returns>A string representing the file name with extension.</returns>
        public static string GetFilename(this string filename)
        {
            var fileInfo = new FileInfo(filename);
            return fileInfo.Name;
        }

        /// <summary>
        /// Retrieves the file name without its extension from a given file path.
        /// </summary>
        /// <param name="filename">The full file path or file name.</param>
        /// <returns>A string representing the file name without the extension.</returns>
        public static string GetFilenameWithoutExtenstion(this string filename)
        {
            var fileInfo = new FileInfo(filename);
            return fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
        }

        // <summary>
        /// Retrieves the full file path without its extension from a given file path.
        /// </summary>
        /// <param name="filename">The full file path.</param>
        /// <returns>A string representing the full file path without the extension.</returns>
        public static string GetFullFilenameWithoutExtenstion(this string filename)
        {
            var fileInfo = new FileInfo(filename);
            return fileInfo.FullName.Substring(0, fileInfo.FullName.Length - fileInfo.Extension.Length);
        }

        public static string? GetFolderName(this string filename)
        {
            var fileInfo = new FileInfo(filename);
            return fileInfo.DirectoryName;
        }
    }
}
