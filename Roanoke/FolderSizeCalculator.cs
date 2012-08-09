using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Roanoke
{
    public class FolderSizeCalculator : IFolderSizeCalculator
    {
        public FolderSizeResult Calculate(string path)
        {
            var count = 0;
            long bytes = 0;
            
            IEnumerable<string> files = new string[] {};
            try
            {
                files = Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex); //log failure to get file size
            }

            foreach (var filePath in files)
            {
                try
                {
                    bytes += new FileInfo(filePath).Length;
                    count++;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex); //log failure to get file size
                }
            }
            return new FolderSizeResult {FileCount = count, FileSize = bytes};
        }
    }
}