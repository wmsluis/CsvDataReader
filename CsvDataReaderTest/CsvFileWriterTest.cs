using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Drt.Csv;
using System.IO;

namespace Drt.Business.Test
{
    [TestClass]
    public class CsvFileWriterTest
    {
        private List<List<string>> ReadAllRows(string path)
        {
            using (var reader = new CsvFileReader(new StreamReader(path)))
            {
                var rows = new List<List<string>>();
                while (reader.ReadRow())
                {
                    var row = reader.Values();
                    rows.Add(row);
                }
                return rows;
            }
        }

        private void WriteAllRows(string path, List<List<string>> rows)
        {
            using (var writer = new CsvFileWriter(new StreamWriter(path)))
            {
                foreach (var row in rows)
                {
                    writer.WriteRow(row);
                }
            }
        }

        [TestMethod]
        public void CsvSimpleOpen()
        {
            string path = Path.GetTempFileName();
            var writer = new CsvFileWriter(new StreamWriter(path));
            writer.Dispose();
        }

        [TestMethod]
        public void CsvReadWriteSimple()
        {
            string path = Path.GetTempFileName();
            Console.WriteLine(path);

            var rows = ReadAllRows(@"CsvReader\Simple.csv");
            WriteAllRows(path, rows);
            var copies = ReadAllRows(path);

            Assert.AreEqual(rows.Count, copies.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                CollectionAssert.AreEqual(rows[i], copies[i]);
            }
        }

        [TestMethod]
        public void CsvReadWriteSomeEmptyValues()
        {
            string path = Path.GetTempFileName();
            Console.WriteLine(path);

            var rows = ReadAllRows(@"CsvReader\SomeEmptyCells.csv");
            WriteAllRows(path, rows);
            var copies = ReadAllRows(path);

            Assert.AreEqual(rows.Count, copies.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                CollectionAssert.AreEqual(rows[i], copies[i]);
            }
        }

        [TestMethod]
        public void CsvReadWriteMultiLine()
        {
            string path = Path.GetTempFileName();
            Console.WriteLine(path);

            var rows = ReadAllRows(@"CsvReader\MultiLine.csv");
            WriteAllRows(path, rows);
            var copies = ReadAllRows(path);

            Assert.AreEqual(rows.Count, copies.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                CollectionAssert.AreEqual(rows[i], copies[i]);
            }
        }

        [TestMethod]
        public void CsvReadWriteEmptyLine()
        {
            string path = Path.GetTempFileName();
            Console.WriteLine(path);

            var rows = ReadAllRows(@"CsvReader\EmptyLine.csv");
            WriteAllRows(path, rows);
            var copies = ReadAllRows(path);

            Assert.AreEqual(rows.Count, copies.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                CollectionAssert.AreEqual(rows[i], copies[i]);
            }
        }
    }
}
