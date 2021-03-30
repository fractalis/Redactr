using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.IO.Abstractions.TestingHelpers;
using Redactr;

namespace Redactr.Test
{

    public class RedactrTest
    {
        [Fact]
        public void RedactsTextInDirectory()
        {
            // Arrange
            var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
            {
                {  @"C:\Test\test.txt", new MockFileData("This is a test. This is just a test") },
                {  @"C:\Test\Test\test.txt", new MockFileData("This is another test. Just another test.") },
                {  @"C:\Test\Test\Test\test.txt", new MockFileData("This is the last test. One last test.") }
            });

            // Act
            Redactr r = new Redactr(fileSystem);
            r.TraverseDirectory(@"C:\Test", "test");

            while (Redactr.tasks.Any(t => !t.IsCompleted)) { }

            // Assert
            foreach(var file in fileSystem.AllFiles)
            {
                var txt = fileSystem.File.ReadAllText(file);
                Assert.DoesNotContain("test", txt);
            }
        }
    }
}
