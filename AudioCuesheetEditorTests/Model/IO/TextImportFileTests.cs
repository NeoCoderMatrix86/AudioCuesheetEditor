//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AudioCuesheetEditor.Model.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using AudioCuesheetEditorTests.Utility;

namespace AudioCuesheetEditor.Model.IO.Tests
{
    [TestClass()]
    public class TextImportFileTests
    {
        [TestMethod()]
        public void TextImportFileTest()
        {
            //Prepare text input file
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Sample Artist 1 - Sample Title 1				00:05:00");
            builder.AppendLine("Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("Sample Artist 3 - Sample Title 3				00:15:54");
            builder.AppendLine("Sample Artist 4 - Sample Title 4				00:20:13");
            builder.AppendLine("Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("Sample Artist 6 - Sample Title 6				00:31:54");
            builder.AppendLine("Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("Sample Artist 8 - Sample Title 8				01:15:54");
            
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            //Test TextImportFile
            var textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)))
            {
                ImportScheme = "%Artist% - %Title%[\t]{1,}%End%"
            };
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Tracks.Count == 8);
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].End, new TimeSpan(0,5,0));

            File.Delete(tempFile);

            //Prepare next Test
            builder.Clear();

            builder.AppendLine("1|Sample Artist 1 - Sample Title 1				00:05:00");
            builder.AppendLine("2|Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("3|Sample Artist 3 - Sample Title 3				00:15:54");
            builder.AppendLine("4|Sample Artist 4 - Sample Title 4				00:20:13");
            builder.AppendLine("5|Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("6|Sample Artist 6 - Sample Title 6				00:31:54");
            builder.AppendLine("7|Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("8|Sample Artist 8 - Sample Title 8				01:15:54");

            File.WriteAllText(tempFile, builder.ToString());

            textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)))
            {
                ImportScheme = "%Position%|%Artist% - %Title%[\t]{1,}%End%"
            };

            Assert.IsNotNull(textImportFile.AnalyseException);

            textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)))
            {
                ImportScheme = "%Position%[|]%Artist% - %Title%[\t]{1,}%End%"
            };

            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.Tracks.ToArray()[5].Position == 6);
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].End, new TimeSpan(0, 5, 0));

            File.Delete(tempFile);

            //Prepare next Test
            builder.Clear();

            builder.AppendLine("1|Sample Artist 1 - Sample Title 1				00:05:00");
            builder.AppendLine("2|Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("3|Sample Artist 3 - Sample Title 3				00:15:54");
            builder.AppendLine("4|Sample Artist 4 - Sample Title 4				00:20:13");
            builder.AppendLine("5|Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("6|Sample Artist 6 - Sample Title 6				00:31:54");
            builder.AppendLine("7|Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("8|Sample Artist 8 - Sample Title 8				01:15:54");
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);
            builder.AppendLine(String.Empty);

            File.WriteAllText(tempFile, builder.ToString());

            textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)))
            {
                ImportScheme = "%Position%|%Artist% - %Title%[\t]{1,}%End%"
            };

            Assert.IsNotNull(textImportFile.AnalyseException);

            textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)))
            {
                ImportScheme = "%Position%[|]%Artist% - %Title%[\t]{1,}%End%"
            };

            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Tracks.Count == 8);
            Assert.IsTrue(textImportFile.Tracks.ToArray()[5].Position == 6);
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].End, new TimeSpan(0, 5, 0));

            File.Delete(tempFile);

            //Prepare next test

            //Prepare text input file
            builder = new StringBuilder();
            builder.AppendLine("Sample Artist 1 - Sample Title 1				00:05:00");
            builder.AppendLine("Sample Artist 2 - Sample Title 2				00:09:23");
            builder.AppendLine("Sample Artist 3 - Sample Title 3				00:15:54");
            builder.AppendLine("Sample Artist 4 - Sample Title 4				00:20:13");
            builder.AppendLine("Sample Artist 5 - Sample Title 5				00:24:54");
            builder.AppendLine("Sample Artist 6 - Sample Title 6				00:31:54");
            builder.AppendLine("Sample Artist 7 - Sample Title 7				00:45:54");
            builder.AppendLine("Sample Artist 8 - Sample Title 8				01:15:54");
            builder.AppendLine("Sample Artist 9 - Sample Title 9");

            tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, builder.ToString());

            //Test TextImportFile
            textImportFile = new TextImportFile(new MemoryStream(File.ReadAllBytes(tempFile)))
            {
                ImportScheme = "%Artist% - %Title%[\t]{1,}%End%"
            };
            Assert.IsNull(textImportFile.AnalyseException);
            Assert.IsTrue(textImportFile.Tracks.Count == 9);
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].Artist, "Sample Artist 1");
            Assert.AreEqual(textImportFile.Tracks.ToArray()[0].Title, "Sample Title 1");
            Assert.AreEqual(textImportFile.Tracks.ToArray()[7].End, new TimeSpan(1, 15, 54));

            File.Delete(tempFile);

        }
    }
}