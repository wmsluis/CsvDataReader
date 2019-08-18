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
            var values = new List<string>();
            while (csvr.ReadRow(values))
            {
            }
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvGetValue()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            var values = new List<string>();
            bool result1 = csvr.ReadRow(values);
            bool result2 = csvr.ReadRow(values);
            Assert.IsTrue(result2);
            Assert.AreEqual("Row1A", values[0]);
            Assert.AreEqual("Row1B", values[1]);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvCommaSeperated()
        {
            var sr = new StreamReader(@"Comma.csv");
            var csvr = new CsvFileReader(sr, fieldDelimiter : ',');

            var values = new List<string>();
            csvr.ReadRow(values);
            csvr.ReadRow(values);
            Assert.AreEqual("Row1A", values[0]);
            Assert.AreEqual("Row1B", values[1]);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvTabSeperated()
        {
            var sr = new StreamReader(@"Tab.csv");
            var csvr = new CsvFileReader(sr, fieldDelimiter : '\t');
            var values = new List<string>();
            csvr.ReadRow(values);
            csvr.ReadRow(values);
            Assert.AreEqual("Row1A", values[0]);
            Assert.AreEqual("Row1B", values[1]);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvFieldCount()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            var values = new List<string>();
            csvr.ReadRow(values);
            Assert.AreEqual(3, values.Count);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvEmbeddedComma()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            var values = new List<string>();
            csvr.ReadRow(values);
            csvr.ReadRow(values);
            csvr.ReadRow(values);
            Assert.AreEqual("Q;A", values[2]);
            csvr.Dispose();
        }

        [TestMethod]
        public void CsvEmbeddedQuote()
        {
            var sr = new StreamReader(@"Simple.csv");
            var csvr = new CsvFileReader(sr);
            var values = new List<string>();
            csvr.ReadRow(values);
            csvr.ReadRow(values);
            csvr.ReadRow(values);
            Assert.AreEqual(values[1], "Embedded \" Quote");
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
                var values = new List<string>();
                csvr.ReadRow(values);
                csvr.ReadRow(values);
                csvr.ReadRow(values);
                Assert.AreEqual("Quotes", values[0]);
                Assert.AreEqual("", values[1]);
                Assert.AreEqual("Q;A", values[2]);
            }
        }

        [TestMethod]
        public void CsvLongLines()
        {
            using (var sr = new StreamReader(@"LinetooLong.csv"))
            using (var csvr = new CsvFileReader(sr))
            {
                var values = new List<string>();
                csvr.ReadRow(values);
                csvr.ReadRow(values);
                Assert.AreEqual("Row1A", values[0]);
                Assert.AreEqual("Row1B", values[1]);
                Assert.AreEqual("Row1C", values[2]);
                csvr.ReadRow(values);
                Assert.AreEqual("Row2A", values[0]);
                Assert.AreEqual("Row2B", values[1]);
                Assert.AreEqual("Row2C", values[2]);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Regl 1 is te kort en heeft geen waarde voor de derde kolom")]
        public void CsvLinesTooShort()
        {
            using (var sr = new StreamReader(@"LinetooShort.csv"))
            using (var csvr = new CsvFileReader(sr))
            {
                var values = new List<string>();
                csvr.ReadRow(values);
                csvr.ReadRow(values);
                Assert.AreEqual("Row1A", values[0]);
                Assert.AreEqual("Row1B", values[1]);
                Assert.AreEqual("", values[2]);
                Assert.Fail("mag hier niet komen");
            }
        }

        [TestMethod]
        public void CsvEmptyFile()
        {
            using (var sr = new StreamReader(@"Empty.csv"))
            using (var csvr = new CsvFileReader(sr))
            {
                var values = new List<string>();
                bool result = csvr.ReadRow(values);
                Assert.IsFalse(result);
            }
        }

        [TestMethod]
        public void CsvMultiLine()
        {
            using (var sr = new StreamReader(@"Multiline.csv"))
            using (var csvr = new CsvFileReader(sr))
            {
                var values = new List<string>();
                csvr.ReadRow(values);  // header
                csvr.ReadRow(values);  // rij 1

                bool result2 = csvr.ReadRow(values); // Rij 2
                Assert.IsTrue(result2);
                Assert.AreEqual("Row2A", values[0]);
                Assert.AreEqual(@"Eerste regel
Tweede regel",values[1]);
                Assert.AreEqual("Q;A", values[2]);

                bool result3 = csvr.ReadRow(values); // rij3
                Assert.IsTrue(result3);
                Assert.AreEqual("Row3A", values[0]);
                Assert.AreEqual("Row3B", values[1]);
                Assert.AreEqual("Row3C", values[2]);

            }
        }

    }
}
