using System;
using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Roanoke.Tests
{
    [TestClass]
    public class FolderSizeCalculatorTests
    {
        [TestMethod]
        public void FolderSizeCalculator_should_return_zero_file_count_and_size_for_a_missing_folder()
        {
            var calc = new FolderSizeCalculator();
            var testPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (Directory.Exists(testPath)) Assert.Inconclusive("Path that shouldn't exist, does exist.");

            var result = calc.Calculate(testPath);
            Assert.AreEqual(0, result.FileCount, "FileCount");
            Assert.AreEqual(0, result.FileSize, "FileSize");
        }

        [TestMethod]
        public void FolderSizeCalculator_should_return_the_correct_file_count_and_size_for_nested_folders()
        {
            var calc = new FolderSizeCalculator();
            var testPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            if (Directory.Exists(testPath)) Assert.Inconclusive("Path that shouldn't exist, does exist.");
            Directory.CreateDirectory(testPath);
            File.WriteAllText(Path.Combine(testPath, "rootfile"), "1234567890", Encoding.ASCII); // 10 bytes
            Directory.CreateDirectory(Path.Combine(testPath, "subdir"));
            File.WriteAllText(Path.Combine(testPath, @"subdir\subdirfile"), "12345", Encoding.ASCII); // 5 bytes
            File.WriteAllText(Path.Combine(testPath, @"subdir\subdirfile.ext"), "123", Encoding.ASCII); // 3 bytes

            FolderSizeResult result;
            try
            {
                result = calc.Calculate(testPath);
            }
            finally
            {
                Directory.Delete(testPath, true);
            }

            Assert.AreEqual(3, result.FileCount, "FileCount");
            Assert.AreEqual(10 + 5 + 3, result.FileSize, "FileSize");
        }


    }
}
