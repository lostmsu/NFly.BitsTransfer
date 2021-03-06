// MIT License 2016 joylei(leingliu@126.com)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace NFly.BitsTransfer
{
    public static class BitsJobExtensions
    {
        /// <summary>
        /// add files in folder to bitstransfer, including files in sub folders
        /// </summary>
        /// <param name="job">bitsjob</param>
        /// <param name="remoteShare">remoete location</param>
        /// <param name="localDir">local directory</param>
        /// <param name="fileFilter">only files match the filter will be transferred</param>
        public static void AddFolder(this BitsTransfer.BitsJob job, 
            string remoteShare, 
            string localDir,
            Func<string, bool> fileFilter = null)
        {
            if (!Directory.Exists(remoteShare))
            {
                throw new InvalidOperationException("remoteShare must be a directory");
            }
            if (!Directory.Exists(localDir))
            {
                Directory.CreateDirectory(localDir);
            }

            IEnumerable<string> files = Directory.GetFiles(remoteShare, "*.*", SearchOption.AllDirectories);
            if (fileFilter != null)
            {
                files = files.Where(fileFilter);
            }
            foreach (string file in files)
            {
                string path = GetRelativePath(file, remoteShare);
                string localPath = Path.Combine(localDir, path);
                Debug.WriteLine("- remote file:" + file);
                Debug.WriteLine(" local file:" + localPath);
                string fileDir = Path.GetDirectoryName(localPath);
                if (!Directory.Exists(fileDir))
                {
                    Directory.CreateDirectory(fileDir);
                }
                job.AddFile(file, localPath);
            }
        }

        /// <summary>
        /// relative path;
        /// all paths will be treated as directory
        /// <code>
        /// string  fromPath= @"C:\\test";
        /// string toPath = @"C:\\test\a\b\c\d";
        /// string expectedResult = @"a\b\c\d";
        /// string path = Utility.GetRelativePath(toPath, fromPath);
        /// Assert.Equal(expectedResult, path, StringComparer.OrdinalIgnoreCase);
        /// </code>
        /// </summary>
        /// <param name="toPath">the target path that has the relative path</param>
        /// <param name="fromPath">the path to be based on</param>
        /// <returns></returns>
        static string GetRelativePath(string toPath, string fromPath)
        {
            var fromFile = new DirectoryInfo(fromPath);
            var toFile = new DirectoryInfo(toPath);

            string result = fromFile.GetRelativePathTo(toFile);
            if (!string.IsNullOrEmpty(result))
            {
                return result.TrimEnd('\\');
            }
            return result;
        }

        static string GetRelativePathTo(this FileSystemInfo from, FileSystemInfo to)
        {
            Func<FileSystemInfo, string> getPath = fsi =>
            {
                var d = fsi as DirectoryInfo;
                return d == null ? fsi.FullName : d.FullName.TrimEnd('\\') + "\\";
            };

            var fromPath = getPath(from);
            var toPath = getPath(to);

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}