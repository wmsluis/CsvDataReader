using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Drt.Csv;
using System.IO;

namespace Drt.Business.Test
{

    [TestClass]
    public class CsvFileReaderTests
    {
        [TestMethod]
        public void CsvSimpleOpen()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvOpenAndClose()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            Assert.AreEqual(sr.EndOfStream, false);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvReadAllRows()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            List<string> values;
            while ((values = csvr.ReadRow()) != null)
            {
            }
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvGetValue()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            var values = csvr.ReadRow();
            values = csvr.ReadRow();
            Assert.IsNotNull(values);
            CollectionAssert.AreEqual(new List<string> { "Row1A", "Row1B", "Row1C" }, values);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvCommaSeperated()
        {
            var sr = new StreamReader(@"Comma.csv");
            var csvr = new CsvFileReader(sr, fieldDelimiter: ',');

            var values = csvr.ReadRow();
            values = csvr.ReadRow();
            CollectionAssert.AreEqual(new List<string> { "Row1A", "Row1B", "Row1C" }, values);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvTabSeperated()
        {
            var sr = new StreamReader(@"Tab.csv");
            var csvr = new CsvFileReader(sr, fieldDelimiter: '\t');
            var values = csvr.ReadRow();
            values = csvr.ReadRow();
            CollectionAssert.AreEqual(new List<string> { "Row1A", "Row1B", "Row1C" }, values);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvFieldCount()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            var values = csvr.ReadRow();
            Assert.AreEqual(3, values.Count);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvEmbeddedComma()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            var values = csvr.ReadRow();
            values = csvr.ReadRow();
            values = csvr.ReadRow();
            Assert.AreEqual(values[2], "Q;A");
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvEmbeddedQuote()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            var values = csvr.ReadRow();
            values = csvr.ReadRow();
            values = csvr.ReadRow();
            Assert.AreEqual("Embedded \" Quote", values[1]);
            csvr.Dispose();
        }


        [TestMethod]
        public void CsvDisposeReader()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            csvr.Dispose();
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvMoveFile()
        {
            var sr = new StreamReader(@"Move.csv");
            var csvr = new CsvFileReader(sr);
            var values = new List<string>();

            csvr.Dispose();

            File.Move(@"Move.csv", @"MoveTemp.csv");
            File.Move(@"MoveTemp.csv", @"Move.csv");
        }

        [TestMethod]
        public void CsvMoveFileUsing()
        {
            using (var sr = new StreamReader(@"Move.csv"))
            using (var csvr = new CsvFileReader(sr))
            {
                ;
            }

            File.Move(@"Move.csv", @"MoveTemp.csv");
            File.Move(@"MoveTemp.csv", @"Move.csv");
        }

        [TestMethod]
        public void CsvEmptyValuesAsString()
        {
            using (var sr = new StreamReader(@"SomeEmptyValues.csv"))
            using (var csvr = new CsvFileReader(sr))
            {
                var values = csvr.ReadRow();  // header

                values = csvr.ReadRow();      // rij 1
                Assert.AreEqual(string.Empty, values[0]);
                CollectionAssert.AreEqual(new List<string> {string.Empty, "Row1B", "Row1C" }, values);

                values = csvr.ReadRow();      // rij 2
                CollectionAssert.AreEqual(new List<string> { "Quotes", string.Empty, "Q;A" }, values);
            }
        }

        [TestMethod]
        public void CsvLongLines()
        {
            using (var sr = new StreamReader(@"LineTooLong.csv"))
            using (var csvr = new CsvFileReader(sr))
            {
                var values = csvr.ReadRow();
                values = csvr.ReadRow();
                CollectionAssert.AreEqual(new List<string> { "Row1A", "Row1B", "Row1C", "Row1D" }, values);
                values = csvr.ReadRow();
                CollectionAssert.AreEqual(new List<string> { "Row2A", "Row2B", "Row2C" }, values);
            }
        }

        [TestMethod]
        public void CsvLinesTooShort()
        {
            using (var sr = new StreamReader(@"LineTooShort.csv"))
            using (var csvr = new CsvFileReader(sr))
            {
                var values = csvr.ReadRow();
                values = csvr.ReadRow();
                CollectionAssert.AreEqual(new List<string> { "Row1A", "Row1B" }, values);
            }
        }

        [TestMethod]
        public void CsvEmptyFile()
        {
            using (var sr = new StreamReader(@"Empty.csv"))
            using (var csvr = new CsvFileReader(sr))
            {
                var values = csvr.ReadRow();
                Assert.IsNull(values);
            }
        }

        [TestMethod]
        public void CsvMultiLine()
        {
            using (var sr = new StreamReader(@"Multiline.csv"))
            using (var csvr = new CsvFileReader(sr))
            {
                var values = csvr.ReadRow();  // header
                values = csvr.ReadRow();  // rij 1

                values = csvr.ReadRow(); // Rij 2
                Assert.IsNotNull(values);
                CollectionAssert.AreEqual(new List<string> { "Row2A", "Eerste regel\r\nTweede regel\r\nDerde regel", "Q;A" }, values);

                values = csvr.ReadRow(); // rij3
                Assert.IsNotNull(values);
                CollectionAssert.AreEqual(new List<string> { "Row3A", "Row3B", "Row3C" }, values);
            }
        }

        [TestMethod]
        public void CsvEmptyLineIgnore()
        {
            using (var sr = new StreamReader(@"Emptyline.csv"))
            using (var csvr = new CsvFileReader(sr, emptyLineBehavior: EmptyLineBehavior.Ignore))
            {
                var values = csvr.ReadRow();  // header
                values = csvr.ReadRow();  // rij 1

                // Rij 2 is leeg, wordt overgeslagen                

                values = csvr.ReadRow(); // rij3
                Assert.IsNotNull(values);
                CollectionAssert.AreEqual(new List<string> { "Row3A", "Row3B", "Row3C" }, values);
            }
        }

        [TestMethod]
        public void CsvEmptyLineReturnNoCells()
        {
            using (var sr = new StreamReader(@"Emptyline.csv"))
            using (var csvr = new CsvFileReader(sr, emptyLineBehavior: EmptyLineBehavior.NoCells))
            {
                var values = csvr.ReadRow();  // header
                values = csvr.ReadRow();  // rij 1

                // Rij 2 is leeg, geef een lege regel terug              
                values = csvr.ReadRow();
                CollectionAssert.AreEqual(new List<string> { }, values);

                values = csvr.ReadRow(); // rij3
                Assert.IsNotNull(values);
                CollectionAssert.AreEqual(new List<string> { "Row3A", "Row3B", "Row3C" }, values);
            }
        }

        [TestMethod]
        public void CsvEmptyLineReturnEmptyCell()
        {
            using (var sr = new StreamReader(@"Emptyline.csv"))
            using (var csvr = new CsvFileReader(sr, emptyLineBehavior: EmptyLineBehavior.EmptyCell))
            {
                var values = csvr.ReadRow();  // header
                values = csvr.ReadRow();  // rij 1

                // Rij 2 is leeg, geef een enkele cell terug met lege string             
                values = csvr.ReadRow();
                CollectionAssert.AreEqual(new List<string> { string.Empty }, values);

                values = csvr.ReadRow(); // rij3
                Assert.IsNotNull(values);
                CollectionAssert.AreEqual(new List<string> { "Row3A", "Row3B", "Row3C" }, values);
            }
        }


        [TestMethod]
        public void CsvEmptyLineIsEndOfFile()
        {
            using (var sr = new StreamReader(@"Emptyline.csv"))
            using (var csvr = new CsvFileReader(sr, emptyLineBehavior: EmptyLineBehavior.EndOfFile))
            {
                var values = csvr.ReadRow();  // header
                values = csvr.ReadRow();  // rij 1

                // Rij 2 is leeg, lees niet verder               

                values = csvr.ReadRow(); // rij3
                Assert.IsNull(values);
            }
        }

        [TestMethod]
        public void CsvBadSingleQuote()
        {
            using (var sr = new StreamReader(@"BadSingleQuote.csv"))
            using (var csvr = new CsvFileReader(sr, emptyLineBehavior: EmptyLineBehavior.EmptyCell))
            {
                var values = csvr.ReadRow();  // header

                values = csvr.ReadRow();      // rij 1
                Assert.IsNotNull(values);
                Assert.AreEqual(3, values.Count);
                Assert.AreEqual("Row1A", values[0]);
                Assert.AreEqual("Row\"1B", values[1]);
                Assert.AreEqual("Row1C", values[2]);

                values = csvr.ReadRow();      // rij 2
                Assert.IsNotNull(values);
                Assert.AreEqual(3, values.Count);
                Assert.AreEqual("Row2A", values[0]);
                Assert.AreEqual("Row\"\r\n2B", values[1]);
                Assert.AreEqual("Row2C", values[2]);
            }
        }
    }
}
