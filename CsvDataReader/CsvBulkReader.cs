using System;
using System.Collections.Generic;
using System.Data;

namespace Drt.Csv
{
    /// <summary>
    /// Lees csv files, zonder afhankelijkheden van b.v. office software.
    /// De class implementeert IDataReader en is dus geschikt om in combinatie met BulkImport te werken.
    /// De class gebruikt CsvFileReader voor het lezen van de csv file.
    /// N.B. Dit zou een generiek component kunnen zijn.
    /// </summary>
    public class CsvBulkReader : IDataReader, IDisposable
    {
        private bool _isClosed = false;
        private bool _disposed = false;

        private List<string> _headers;
        private List<string> _currentRow;
        private List<string> _constantValues;
        private readonly string _empytValue;
        private CsvFileReader _csvFileReader;

        /// <summary>
        /// Een lezer van csv files, geschikt voor bulkimport.
        /// </summary>
        /// <param name="emptyValue">Indien een veldwaarde in de csv file een lege string is, rapporteer dan in de plaats hiervan deze waarde. 
        /// Voor bulk import is dit typisch gesproken null (default).</param>
        public CsvBulkReader(CsvFileReader csvFileReader, string emptyValue = null)
        {
            _constantValues = new List<string>();
            _empytValue = emptyValue;

            _csvFileReader = csvFileReader;
            _headers = new List<string>();
            _csvFileReader.ReadRow(_headers);
        }

        /// <summary>
        /// Voeg een nieuwe kolom achteraan toe met voor alle regels steeds een vaste waarde
        /// </summary>
        /// <param name="columnHeader"></param>
        /// <param name="constantValue"></param>
        public void AddConstantColumn(string columnHeader, string constantValue)
        {
            _headers.Add(columnHeader);
            _constantValues.Add(constantValue);
        }

        public bool Read()
        {
            _currentRow = new List<string>();
            bool result = _csvFileReader.ReadRow(_currentRow);

            if (_constantValues.Count > 0)
                _currentRow.AddRange(_constantValues);

            return result;
        }

        public string EmtpyValue
        {
            get { return _empytValue; }
        }

        public object GetValue(int i)
        {
            return _currentRow[i].Length > 0 ? _currentRow[i] : _empytValue;
        }

        public string GetName(int i)
        {
            return _headers[i];
        }

        public int FieldCount
        {
            get { return _headers.Count; }
        }

        #region IDataReader
        public int Depth
        {
            get { return 0; }
        }

        public bool IsClosed
        {
            get { return _isClosed; }
        }

        public int RecordsAffected
        {
            get { return -1; }
        }

        public void Close()
        {
            _csvFileReader.Dispose();
            _isClosed = true;
        }

        public bool NextResult()
        {
            return false;
        }

        public DataTable GetSchemaTable()
        {
            throw new NotSupportedException();
        }
        #endregion


        #region IDataRecord
        public string GetDataTypeName(int i)
        {
            // niet echt geimplementeerd voor andere types dan string ... 
            return "string";
        }

        public Type GetFieldType(int i)
        {
            return typeof(string);
        }

        public int GetValues(object[] values)
        {
            for (int i = 0; i < _headers.Count; i++)
            {
                values[i] = GetValue(i);
            }
            return _headers.Count;
        }

        public int GetOrdinal(string name)
        {
            for (int i = 0; i < _headers.Count; i++)
            {
                if (string.Equals(_headers[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            throw new IndexOutOfRangeException($"De kolom '{name}' kon niet worden gevonden");
        }

        public object this[int i]
        {
            get { return GetValue(i); }
        }

        public object this[string name]
        {
            get { return this[GetOrdinal(name)]; }
        }

        #endregion


        public void Dispose()
        {
            this.Dispose(true);
            System.GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _csvFileReader.Dispose();

            _disposed = true;
        }

        #region Not Implemented interface methods

        public Int32 GetInt32(int i)
        {
            throw new NotSupportedException();
        }

        public bool GetBoolean(int i)
        {
            throw new NotSupportedException();
        }
        public byte GetByte(int i)
        {
            throw new NotSupportedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public char GetChar(int i)
        {
            throw new NotSupportedException();
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new NotSupportedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotSupportedException();
        }

        public Int16 GetInt16(int i)
        {
            throw new NotSupportedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotSupportedException();
        }

        public Int64 GetInt64(int i)
        {
            throw new NotSupportedException();
        }

        public float GetFloat(int i)
        {
            throw new NotSupportedException();
        }

        public double GetDouble(int i)
        {
            throw new NotSupportedException();
        }

        public string GetString(int i)
        {
            throw new NotSupportedException();
        }

        public Decimal GetDecimal(int i)
        {
            throw new NotSupportedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotSupportedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotSupportedException();
        }

        #endregion


    }
}
