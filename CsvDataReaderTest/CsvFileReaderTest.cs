using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Drt.Csv;
using System.IO;

namespace Drt.Business.Test
{
    [TestClass]
    public class CsvFileReaderTest
    {
        [TestMethod]
        public void CsvSimpleOpen()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            fr.Dispose();
        }

        [TestMethod]
        public void CsvOpenAndClose()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            Assert.AreEqual(sr.EndOfStream, false);
            fr.Dispose();
        }

        [TestMethod]
        public void CsvReadAllRows()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            while (fr.ReadRow())
            {
            }
            fr.Dispose();
        }

        [TestMethod]
        public void CsvGetValue()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);

            bool ok1 = fr.ReadRow();
            Assert.IsTrue(ok1, "rij 1");
            var values1 = fr.Values();
            Assert.IsNotNull(values1);
            CollectionAssert.AreEqual(new List<string> { "Header1", "Header2", "Header3" }, values1);

            bool ok2 = fr.ReadRow();
            Assert.IsTrue(ok2, "rij 2");
            var values2 = fr.Values();
            Assert.IsNotNull(values2);
            CollectionAssert.AreEqual(new List<string> { "Row1A", "Row1B", "Row1C" }, values2);

            bool ok3 = fr.ReadRow();
            Assert.IsTrue(ok3, "rij 3");
            var values3 = fr.Values();
            Assert.IsNotNull(values3);
            CollectionAssert.AreEqual(new List<string> { "Quotes", "Embedded \" Quote", "Q;A" }, values3);

            bool ok4 = fr.ReadRow();
            Assert.IsFalse(ok4, "er is geen rij 4");
            Assert.IsNull(fr.Values());

            fr.Dispose();
        }

        [TestMethod]
        public void CsvCommaSeperated()
        {
            var sr = new StreamReader(@"CsvReader\Comma.csv");
            var fr = new CsvFileReader(sr, fieldDelimiter: ',');

            fr.ReadRow();
            fr.ReadRow();
            var values = fr.Values();
            CollectionAssert.AreEqual(new List<string> { "Row1A", "Row1B", "Row1C" }, values);
            fr.Dispose();
        }

        [TestMethod]
        public void CsvTabSeperated()
        {
            var sr = new StreamReader(@"CsvReader\Tab.csv");
            var fr = new CsvFileReader(sr, fieldDelimiter: '\t');
            fr.ReadRow();
            fr.ReadRow();
            var values = fr.Values();
            CollectionAssert.AreEqual(new List<string> { "Row1A", "Row1B", "Row1C" }, values);
            fr.Dispose();
        }

        [TestMethod]
        public void CsvFieldCount()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            fr.ReadRow();
            var values = fr.Values();
            Assert.AreEqual(3, values.Count);
            fr.Dispose();
        }

        [TestMethod]
        public void CsvEmbeddedComma()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            fr.ReadRow();
            fr.ReadRow();
            fr.ReadRow();
            var values = fr.Values();
            Assert.AreEqual(values[2], "Q;A");
            fr.Dispose();
        }

        [TestMethod]
        public void CsvEmbeddedQuote()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            fr.ReadRow();
            fr.ReadRow();
            fr.ReadRow();
            var values = fr.Values();
            Assert.AreEqual("Embedded \" Quote", values[1]);
            fr.Dispose();
        }


        [TestMethod]
        public void CsvDisposeReader()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            fr.Dispose();
            fr.Dispose();
        }

        [TestMethod]
        public void CsvMoveFile()
        {
            var sr = new StreamReader(@"CsvReader\Move.csv");
            var fr = new CsvFileReader(sr);
            var values = new List<string>();

            fr.Dispose();

            File.Move(@"CsvReader\Move.csv", @"CsvReader\MoveTemp.csv");
            File.Move(@"CsvReader\MoveTemp.csv", @"CsvReader\Move.csv");
        }

        [TestMethod]
        public void CsvMoveFileUsing()
        {
            using (var sr = new StreamReader(@"CsvReader\Move.csv"))
            using (var fr = new CsvFileReader(sr))
            {
                ;
            }

            File.Move(@"CsvReader\Move.csv", @"CsvReader\MoveTemp.csv");
            File.Move(@"CsvReader\MoveTemp.csv", @"CsvReader\Move.csv");
        }

        [TestMethod]
        public void CsvEmptycellsAsString()
        {
            using (var sr = new StreamReader(@"CsvReader\SomeEmptyCells.csv"))
            using (var fr = new CsvFileReader(sr))
            {
                fr.ReadRow();  // header
                fr.ReadRow();      // rij 1
                var values = fr.Values();
                Assert.AreEqual(string.Empty, values[0]);
                CollectionAssert.AreEqual(new List<string> { string.Empty, "Row1B", "Row1C" }, values);

                fr.ReadRow();      // rij 2
                values = fr.Values();
                CollectionAssert.AreEqual(new List<string> { "Quotes", string.Empty, "Q;A" }, values);
            }
        }

        [TestMethod]
        public void CsvLongLines()
        {
            using (var sr = new StreamReader(@"CsvReader\LineTooLong.csv"))
            using (var fr = new CsvFileReader(sr))
            {
                fr.ReadRow();
                fr.ReadRow();
                var values = fr.Values();
                CollectionAssert.AreEqual(new List<string> { "Row1A", "Row1B", "Row1C", "Row1D" }, values);

                fr.ReadRow();
                values = fr.Values();
                CollectionAssert.AreEqual(new List<string> { "Row2A", "Row2B", "Row2C" }, values);
            }
        }

        [TestMethod]
        public void CsvLinesTooShort()
        {
            using (var sr = new StreamReader(@"CsvReader\LineTooShort.csv"))
            using (var fr = new CsvFileReader(sr))
            {
                fr.ReadRow();
                fr.ReadRow();
                var values = fr.Values();
                CollectionAssert.AreEqual(new List<string> { "Row1A", "Row1B" }, values);
            }
        }

        [TestMethod]
        public void CsvEmptyFile()
        {
            using (var sr = new StreamReader(@"CsvReader\Empty.csv"))
            using (var fr = new CsvFileReader(sr))
            {
                fr.ReadRow();
                var values = fr.Values();
                Assert.IsNull(values);
            }
        }

        [TestMethod]
        public void CsvMultiLine()
        {
            using (var sr = new StreamReader(@"CsvReader\Multiline.csv"))
            using (var fr = new CsvFileReader(sr))
            {
                fr.ReadRow();  // header
                fr.ReadRow();  // rij 1

                fr.ReadRow();  // Rij 2
                var values = fr.Values();
                Assert.IsNotNull(values);
                CollectionAssert.AreEqual(new List<string> { "Row2A", "Eerste regel\r\nTweede regel\r\nDerde regel", "Q;A" }, values);

                fr.ReadRow();  // rij3
                values = fr.Values();
                Assert.IsNotNull(values);
                CollectionAssert.AreEqual(new List<string> { "Row3A", "Row3B", "Row3C" }, values);
            }
        }

        [TestMethod]
        public void CsvEmptyLineIgnore()
        {
            using (var sr = new StreamReader(@"CsvReader\Emptyline.csv"))
            using (var fr = new CsvFileReader(sr, emptyLineBehavior: EmptyLineBehavior.Ignore))
            {
                fr.ReadRow();  // header
                fr.ReadRow();  // rij 1

                // Rij 2 is leeg, wordt overgeslagen

                bool ok = fr.ReadRow(); // rij3
                Assert.IsTrue(ok);
                var values = fr.Values();
                Assert.IsNotNull(values);
                CollectionAssert.AreEqual(new List<string> { "Row3A", "Row3B", "Row3C" }, values);
            }
        }

        [TestMethod]
        public void CsvEmptyLineReturnNoCells()
        {
            using (var sr = new StreamReader(@"CsvReader\Emptyline.csv"))
            using (var fr = new CsvFileReader(sr, emptyLineBehavior: EmptyLineBehavior.NoCells))
            {
                fr.ReadRow();  // header
                fr.ReadRow();  // rij 1

                // Rij 2 is leeg, geef een lege collectie cellen terug
                bool ok = fr.ReadRow();
                Assert.IsTrue(ok);
                var values = fr.Values();
                CollectionAssert.AreEqual(new List<string> { }, values);

                fr.ReadRow(); // rij3
                values = fr.Values();
                Assert.IsNotNull(values);
                CollectionAssert.AreEqual(new List<string> { "Row3A", "Row3B", "Row3C" }, values);
            }
        }

        [TestMethod]
        public void CsvEmptyLineReturnEmptyCell()
        {
            using (var sr = new StreamReader(@"CsvReader\Emptyline.csv"))
            using (var fr = new CsvFileReader(sr, emptyLineBehavior: EmptyLineBehavior.EmptyCell))
            {
                fr.ReadRow();  // header
                fr.ReadRow();  // rij 1

                // Rij 2 is leeg, geef een enkele cell terug met lege string
                bool ok = fr.ReadRow();
                Assert.IsTrue(ok);
                var values = fr.Values();
                CollectionAssert.AreEqual(new List<string> { string.Empty }, values);

                fr.ReadRow(); // rij3
                values = fr.Values();
                Assert.IsNotNull(values);
                CollectionAssert.AreEqual(new List<string> { "Row3A", "Row3B", "Row3C" }, values);
            }
        }


        [TestMethod]
        public void CsvEmptyLineIsEndOfFile()
        {
            using (var sr = new StreamReader(@"CsvReader\Emptyline.csv"))
            using (var fr = new CsvFileReader(sr, emptyLineBehavior: EmptyLineBehavior.EndOfFile))
            {
                fr.ReadRow();  // header
                fr.ReadRow();  // rij 1

                // Rij 2 is leeg, lees niet verder

                bool ok = fr.ReadRow(); // rij 3
                Assert.IsFalse(ok);

                var values = fr.Values();
                Assert.IsNull(values);
            }
        }

        [TestMethod]
        public void CsvBadSingleQuote()
        {
            using (var sr = new StreamReader(@"CsvReader\BadSingleQuote.csv"))
            using (var fr = new CsvFileReader(sr, emptyLineBehavior: EmptyLineBehavior.EmptyCell))
            {
                fr.ReadRow();  // header

                fr.ReadRow();      // rij 1
                var values = fr.Values();
                Assert.IsNotNull(values);
                Assert.AreEqual(3, values.Count);
                Assert.AreEqual("Row1A", values[0]);
                Assert.AreEqual("Row\"1B", values[1]);
                Assert.AreEqual("Row1C", values[2]);

                fr.ReadRow();      // rij 2
                values = fr.Values();
                Assert.IsNotNull(values);
                Assert.AreEqual(3, values.Count);
                Assert.AreEqual("Row2A", values[0]);
                Assert.AreEqual("Row\"\r\n2B", values[1]);
                Assert.AreEqual("Row2C", values[2]);
            }
        }
    }
}

