using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Drt.Csv;
using System.IO;

namespace Drt.Business.Test
{
    [TestClass]
    public class CsvDataReaderTests
    {
        [TestMethod]
        public void SimpleOpen()
        {
            var reader = new CsvDataReader(new StreamReader(@"Simple.csv"));
            reader.Dispose();
        }

        [TestMethod]
        public void OpenAndClose()
        {
            var reader = new CsvDataReader(new StreamReader((@"Simple.csv")));
            Assert.AreEqual(reader.IsClosed, false);
            reader.Close();
            Assert.AreEqual(reader.IsClosed, true);
            reader.Dispose();
        }

        [TestMethod]
        public void HeadersParse()
        {
            var reader = new CsvDataReader(new StreamReader((@"Simple.csv")));
            Assert.AreEqual(0, reader.GetOrdinal("Header1"));
            Assert.AreEqual(1, reader.GetOrdinal("Header2"));
            Assert.AreEqual(2, reader.GetOrdinal("Header3"));
            reader.Dispose();
        }

        [TestMethod]
        public void CaseInsensitiveColumnNames()
        {
            var sr = new StreamReader((@"Simple.csv"));
            var reader = new CsvDataReader(sr);
            Assert.AreEqual(0, reader.GetOrdinal("header1"));
            Assert.AreEqual(1, reader.GetOrdinal("HEADER2"));
            Assert.AreEqual(2, reader.GetOrdinal("HeaDER3"));
            reader.Close();
            reader.Dispose();
        }

        [TestMethod]
        public void ReadAllRows()
        {
            var sr = new StreamReader((@"Simple.csv"));
            var reader = new CsvDataReader(sr);
            while (reader.Read())
            {
            }
            reader.Close();
            reader.Dispose();
        }

        [TestMethod]
        public void GetValue()
        {
            var sr = new StreamReader(@"Simple.csv");
            var reader = new CsvDataReader(sr);
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
        public void CommaSeperated()
        {
            var sr = new StreamReader(@"Comma.csv");
            var reader = new CsvDataReader(sr, fieldDelimiter: ',');
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
        public void TabSeperated()
        {
            var sr = new StreamReader(@"Tab.csv");
            var reader = new CsvDataReader(sr, fieldDelimiter: '\t');
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
        public void FieldCount()
        {
            var sr = new StreamReader(@"Simple.csv");
            var reader = new CsvDataReader(sr);
            Assert.AreEqual(3, reader.FieldCount);
            reader.Dispose();
        }

        [TestMethod]
        public void EmbeddedComma()
        {
            var sr = new StreamReader(@"Simple.csv");
            var reader = new CsvDataReader(sr);
            reader.Read();
            reader.Read();
            Assert.AreEqual(reader.GetValue(0).ToString(), "Quotes");
            string v1 = reader.GetValue(2).ToString();
            string expected = "Q;A";
            Assert.AreEqual(expected, v1);
            reader.Dispose();

        }
        [TestMethod]
        public void GetName()
        {
            var sr = new StreamReader(@"Simple.csv");
            var reader = new CsvDataReader(sr);
            Assert.AreEqual("Header1", reader.GetName(0));
            reader.Dispose();
        }

        [TestMethod]
        public void GetOrdinal()
        {
            var sr = new StreamReader(@"Simple.csv");
            var reader = new CsvDataReader(sr);
            Assert.AreEqual(0, reader.GetOrdinal("Header1"));
            reader.Dispose();
        }

        [TestMethod()]
        [ExpectedException(typeof(IndexOutOfRangeException), "The column ZZZZ could not be found in the results")]
        public void GetOrdinalFailure()
        {
            using (var sr = new StreamReader(@"Simple.csv"))
            using (var reader = new CsvDataReader(sr))
            {
                int i = reader.GetOrdinal("ZZZZ");
            }
        }

        [TestMethod]
        public void AddStaticValue()
        {
            var sr = new StreamReader(@"Simple.csv");
            var reader = new CsvDataReader(sr);
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
        public void TwoStaticColumns()
        {
            var sr = new StreamReader(@"Simple.csv");
            var reader = new CsvDataReader(sr);
            reader.AddConstantColumn("Column1", "Value");
            reader.AddConstantColumn("ColumnZ", "FileName");

            Assert.AreEqual(4, reader.GetOrdinal("ColumnZ"));
            while (reader.Read())
            {
                Assert.AreEqual("FileName", reader.GetValue(reader.GetOrdinal("ColumnZ")));
            }
            reader.Close();
            reader.Dispose();
        }

        [TestMethod]
        public void DisposeReader()
        {
            var sr = new StreamReader(@"Simple.csv");
            var reader = new CsvDataReader(sr);
            Assert.AreEqual(reader.IsClosed, false);
            reader.Close();
            reader.Dispose();
            reader.Dispose();
        }

        [TestMethod]
        public void MoveFile()
        {
            var sr = new StreamReader(@"Move.csv");
            var reader = new CsvDataReader(sr);
            Assert.AreEqual(reader.IsClosed, false);
            reader.Close();
            Assert.AreEqual(reader.IsClosed, true);
            reader.Dispose();

            File.Move(@"Move.csv", @"MoveTemp.csv");
            File.Move(@"MoveTemp.csv", @"Move.csv");
        }

        [TestMethod]
        public void MoveFileUsing()
        {
            using (var sr = new StreamReader(@"Move.csv"))
            using (var reader = new CsvDataReader(sr))
            {
                Assert.AreEqual(reader.IsClosed, false);
                reader.Close();
                Assert.AreEqual(reader.IsClosed, true);
            }

            File.Move(@"Move.csv", @"MoveTemp.csv");
            File.Move(@"MoveTemp.csv", @"Move.csv");
        }

        [TestMethod]
        public void OnlyHeadersParse()
        {
            using (var sr = new StreamReader(@"OnlyHeader.csv"))
            using (var reader = new CsvDataReader(sr))
            {
                Assert.AreEqual(0, reader.GetOrdinal("Header1"));
                Assert.AreEqual(1, reader.GetOrdinal("Header2"));
                Assert.AreEqual(2, reader.GetOrdinal("Header3"));

                Assert.IsFalse(reader.Read(), "OnlyHeader.csv bevat alleen een header regel, geen records");
            }
        }

        [TestMethod]
        public void EmptyValuesAsNull()
        {
            using (var sr = new StreamReader(@"SomeEmptyValues.csv"))
            using (var reader = new CsvDataReader(sr))
            {
                reader.Read();
                reader.Read();
                Assert.AreEqual("Quotes", reader.GetValue(0));
                Assert.AreEqual(null, reader.GetValue(1));
                Assert.AreEqual("Q;A", reader.GetValue(2));
            }
        }

        [TestMethod]
        public void EmptyValuesAsString()
        {
            using (var sr = new StreamReader(@"SomeEmptyValues.csv"))
            using (var reader = new CsvDataReader(sr, emptyValue: ""))
            {
                reader.Read();
                reader.Read();
                Assert.AreEqual("Quotes", reader.GetValue(0));
                Assert.AreEqual("", reader.GetValue(1));
                Assert.AreEqual("Q;A", reader.GetValue(2));
            }
        }

        [TestMethod]
        public void LongLines()
        {
            using (var sr = new StreamReader(@"LinetooLong.csv"))
            using (var reader = new CsvDataReader(sr))
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
        [ExpectedException(typeof(IndexOutOfRangeException), "Regl 1 is te kort en heeft geen waarde voor de derde kolom")]
        public void LinesTooShort()
        {
            using (var sr = new StreamReader(@"LinetooShort.csv"))
            using (var reader = new CsvDataReader(sr))
            {
                reader.Read();
                Assert.AreEqual("Row1A", reader[0]);
                Assert.AreEqual("Row1B", reader[1]);
                Assert.AreEqual("", reader[2]);
                Assert.Fail("mag hier niet komen");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException), "Regl 1 is te kort en heeft geen waarde voor de derde kolom")]
        public void LinesTooShortByName()
        {
            using (var sr = new StreamReader(@"LinetooShort.csv"))
            using (var reader = new CsvDataReader(sr))
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
        [ExpectedException(typeof(IndexOutOfRangeException), "Regl 1 is te kort en heeft geen waarde voor de derde kolom")]
        public void LinesTooShortWithAddedConstant()
        {
            using (var sr = new StreamReader(@"LinetooShort.csv"))
            using (var reader = new CsvDataReader(sr))
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

    }
}
