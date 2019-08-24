using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Drt.Csv;
using System.IO;
 
namespace Drt.Business.Test
{
    [TestClass]
    [DeploymentItem(@"..\..\Testdata")]
    public class CsvBulkReaderTest
    {
        [TestMethod]
        public void BulkBulkSimpleOpen()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkOpenAndClose()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            Assert.AreEqual(reader.IsClosed, false);
            reader.Close();
            Assert.AreEqual(reader.IsClosed, true);
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkHeadersParse()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            Assert.AreEqual(0, reader.GetOrdinal("Header1"));
            Assert.AreEqual(1, reader.GetOrdinal("Header2"));
            Assert.AreEqual(2, reader.GetOrdinal("Header3"));
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkCaseInsensitiveColumnNames()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            Assert.AreEqual(0, reader.GetOrdinal("header1"));
            Assert.AreEqual(1, reader.GetOrdinal("HEADER2"));
            Assert.AreEqual(2, reader.GetOrdinal("HeaDER3"));
            reader.Close();
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkReadAllRows()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            while (reader.Read())
            {
            }
            reader.Close();
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkGetValue()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            bool result1 = reader.Read();
            Assert.IsTrue(result1);
            Assert.AreEqual("Row1A", reader.GetValue(0));
            Assert.AreEqual("Row1B", reader.GetValue(1));
            Assert.AreEqual("Row1A", reader[0]);
            Assert.AreEqual("Row1B", reader[1]);
            Assert.AreEqual("Row1A", reader["Header1"]);
            Assert.AreEqual("Row1B", reader["header2"]);
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkCommaSeperated()
        {
            var sr = new StreamReader(@"CsvReader\Comma.csv");
            var fr = new CsvFileReader(sr, fieldDelimiter: ',');
            var reader = new CsvBulkReader(fr);

            reader.Read();
            Assert.AreEqual("Row1A", reader.GetValue(0));
            Assert.AreEqual("Row1B", reader.GetValue(1));
            Assert.AreEqual("Row1A", reader[0]);
            Assert.AreEqual("Row1B", reader[1]);
            Assert.AreEqual("Row1A", reader["Header1"]);
            Assert.AreEqual("Row1B", reader["header2"]);
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkTabSeperated()
        {
            var sr = new StreamReader(@"CsvReader\Tab.csv");
            var fr = new CsvFileReader(sr, fieldDelimiter: '\t');
            var reader = new CsvBulkReader(fr);
            reader.Read();
            Assert.AreEqual("Row1A", reader.GetValue(0));
            Assert.AreEqual("Row1B", reader.GetValue(1));
            Assert.AreEqual("Row1A", reader[0]);
            Assert.AreEqual("Row1B", reader[1]);
            Assert.AreEqual("Row1A", reader["Header1"]);
            Assert.AreEqual("Row1B", reader["header2"]);
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkFieldCount()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            Assert.AreEqual(3, reader.FieldCount);
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkEmbeddedComma()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            reader.Read();
            reader.Read();
            Assert.AreEqual("Q;A", reader.GetValue(2));
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkEmbeddedQuote()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            reader.Read();
            reader.Read();
            Assert.AreEqual(reader.GetValue(1), "Embedded \" Quote");
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkGetName()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            Assert.AreEqual("Header1", reader.GetName(0));
            reader.Dispose();
        }

        [TestMethod]
        public void BulkBulkGetOrdinal()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            Assert.AreEqual(0, reader.GetOrdinal("Header1"));
            reader.Dispose();
        }

        [TestMethod()]
        [ExpectedException(typeof(IndexOutOfRangeException), "The column ZZZZ could not be found in the results")]
        public void BulkBulkGetOrdinalFailure()
        {
            using (var sr = new StreamReader(@"CsvReader\Simple.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr))
            {
                int i = reader.GetOrdinal("ZZZZ");
            }
        }

        [TestMethod]
        public void BulkBulkAddStaticValue()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            reader.AddConstantColumn("Column1", "Value");

            Assert.AreEqual(3, reader.GetOrdinal("Column1"));
            while (reader.Read())
            {
                Assert.AreEqual("Value", reader.GetValue(reader.GetOrdinal("Column1")));
            }
            reader.Close();
            reader.Dispose();
        }

        [TestMethod]
        public void BulkTwoStaticColumns()
        {
            using (var sr = new StreamReader(@"CsvReader\Simple.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr))
            {
                reader.AddConstantColumn("Column1", "1234.5");
                reader.AddConstantColumn("Column2", "Boohoo");

                reader.Read();
                Assert.AreEqual("1234.5", reader[3]);
                Assert.AreEqual("Boohoo", reader[4]);

                reader.Read();
                Assert.AreEqual("1234.5", reader["Column1"]);
                Assert.AreEqual("Boohoo", reader["Column2"]);
            }
        }

        [TestMethod]
        public void BulkDisposeReader()
        {
            var sr = new StreamReader(@"CsvReader\Simple.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            Assert.AreEqual(reader.IsClosed, false);
            reader.Close();
            reader.Dispose();
            reader.Dispose();
        }

        [TestMethod]
        public void BulkMoveFile()
        {
            var sr = new StreamReader(@"CsvReader\Move.csv");
            var fr = new CsvFileReader(sr);
            var reader = new CsvBulkReader(fr);
            Assert.AreEqual(reader.IsClosed, false);
            reader.Close();
            Assert.AreEqual(reader.IsClosed, true);
            reader.Dispose();

            File.Move(@"CsvReader\Move.csv", @"CsvReader\MoveTemp.csv");
            File.Move(@"CsvReader\MoveTemp.csv", @"CsvReader\Move.csv");
        }

        [TestMethod]
        public void BulkMoveFileUsing()
        {
            using (var sr = new StreamReader(@"CsvReader\Move.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr))
            {
                Assert.AreEqual(reader.IsClosed, false);
                reader.Close();
                Assert.AreEqual(reader.IsClosed, true);
            }

            File.Move(@"CsvReader\Move.csv", @"CsvReader\MoveTemp.csv");
            File.Move(@"CsvReader\MoveTemp.csv", @"CsvReader\Move.csv");
        }

        [TestMethod]
        public void BulkOnlyHeadersParse()
        {
            using (var sr = new StreamReader(@"CsvReader\OnlyHeader.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr))
            {
                Assert.AreEqual(0, reader.GetOrdinal("Header1"));
                Assert.AreEqual(1, reader.GetOrdinal("Header2"));
                Assert.AreEqual(2, reader.GetOrdinal("Header3"));

                Assert.IsFalse(reader.Read(), "OnlyHeader.csv bevat alleen een header regel, geen records");
            }
        }

        [TestMethod]
        public void BulkEmptyValuesAsNull()
        {
            using (var sr = new StreamReader(@"CsvReader\SomeEmptyCells.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr))
            {
                bool result1 = reader.Read();
                Assert.IsTrue(result1);

                bool result2 = reader.Read();
                Assert.IsTrue(result2);
                Assert.AreEqual("Quotes", reader.GetValue(0));
                Assert.AreEqual(null, reader.GetValue(1));
                Assert.AreEqual("Q;A", reader.GetValue(2));

                bool result3 = reader.Read();
                Assert.IsFalse(result3);
            }
        }

        [TestMethod]
        public void BulkEmptyValuesAsString()
        {
            using (var sr = new StreamReader(@"CsvReader\SomeEmptyCells.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr, emptyValue: ""))
            {
                reader.Read();
                reader.Read();
                Assert.AreEqual("Quotes", reader.GetValue(0));
                Assert.AreEqual("", reader.GetValue(1));
                Assert.AreEqual("Q;A", reader.GetValue(2));
            }
        }

        [TestMethod]
        public void BulkLongLines()
        {
            using (var sr = new StreamReader(@"CsvReader\LinetooLong.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr))
            {
                reader.Read();
                Assert.AreEqual("Row1A", reader.GetValue(0));
                Assert.AreEqual("Row1B", reader.GetValue(1));
                Assert.AreEqual("Row1C", reader.GetValue(2));

                reader.Read();
                Assert.AreEqual("Row2A", reader.GetValue(0));
                Assert.AreEqual("Row2B", reader.GetValue(1));
                Assert.AreEqual("Row2C", reader.GetValue(2));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Regl 1 is te kort en heeft geen waarde voor de derde kolom")]
        public void BulkLinesTooShort()
        {
            using (var sr = new StreamReader(@"CsvReader\LinetooShort.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr))
            {
                reader.Read();
                Assert.AreEqual("Row1A", reader[0]);
                Assert.AreEqual("Row1B", reader[1]);
                Assert.AreEqual("", reader[2]);
                Assert.Fail("mag hier niet komen");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Regl 1 is te kort en heeft geen waarde voor de derde kolom")]
        public void BulkLinesTooShortByName()
        {
            using (var sr = new StreamReader(@"CsvReader\LinetooShort.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr))
            {
                reader.Read();
                Assert.AreEqual("Row1A", reader["Header1"]);
                Assert.AreEqual("Row1B", reader["Header2"]);
                // Header3 ontbreekt
                Assert.AreEqual("", reader["Header3"]);
                Assert.Fail("mag hier niet komen");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException), "Regl 1 is te kort en heeft geen waarde voor de derde kolom")]
        public void BulkLinesTooShortWithAddedConstant()
        {
            using (var sr = new StreamReader(@"CsvReader\LinetooShort.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr))
            {
                reader.AddConstantColumn("NewColumn", "1000");
                reader.Read();
                Assert.AreEqual("Row1A", reader["Header1"]);
                Assert.AreEqual("Row1B", reader["Header2"]);
                //Header3 ontbreekt !!!
                Assert.AreEqual("1000", reader["NewColumn"]);
                Assert.Fail("mag hier niet komen");
            }
        }

        [TestMethod]
        public void BulkEmptyFile()
        {
            using (var sr = new StreamReader(@"CsvReader\Empty.csv"))
            using (var fr = new CsvFileReader(sr))
            using (var reader = new CsvBulkReader(fr))
            {
                bool result = reader.Read();
                Assert.IsFalse(result);
            }
        }


    }
}
