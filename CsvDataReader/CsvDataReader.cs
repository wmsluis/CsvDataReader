using System;
using System.Collections.Generic;
using System.Text;

using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace Drt.Csv
{
    /// <summary>
    /// Lees csv files, zonder afhankelijkheden van b.v. office software
    /// Instelbaar: scheidingskarakter (standaard is de puntkomma) en hoe een leeg veld geinterpreteerd moet worden.
    /// De class implementeert IDataReader en is dus geschikt om in combinatie met BulkImport te werken.
    /// N.B. Dit zou een generiek component kunnen zijn.
    /// </summary>
    public class CsvDataReader : IDataReader, IDisposable
    {
        private bool _isClosed = false;
        private bool _disposed = false;

        private StreamReader _stream;
        private string _headerRow;
        private string[] _headers;
        private string[] _currentRow;
        private string _constantValues;
        private readonly char _delim;
        private readonly string _empytValue;

        private Regex _delimRegex;

        /// <summary>
        /// Een lezer van csv files, geschikt voor bulkimport.
        /// </summary>
        /// <param name="stream">StreamReader naar csv file, met juiste Encoding, buffering etc... 
        /// Is verantwoordelijkheid van de client</param>
        /// <param name="fieldDelimiter">b.v. een puntkomma (default), een komma of een tab</param>
        /// <param name="emptyValue">Indien een veldwaarde in de csv file een lege string is, rapporteer dan in de plaats hiervoor deze waarde. 
        /// Voor bulk import is dit typisch gesproken null (default).</param>
        public CsvDataReader(StreamReader stream, char fieldDelimiter = ';', string emptyValue = null)
        {
            _delim = fieldDelimiter;
            _constantValues = string.Empty;
            _empytValue = emptyValue;

            // Match de field delimiers (b.v. puntkomma), maar alleen daar waar een regel gesplitst moet worden.
            // Maatregelen worden genomen om regels niet te splitsen op delimiters die binnen dubbele quotes vallen.
            // (?=...) is een beetje technisch, zoek de preciese betekenis op in de documentatie. 
            //     komt erop neer dat de regex na de delimiter nog een stukje doorzoekt (b.v. een stuk tussen dubbele quotes)
            //     maar dat we dat niet rapporteren als onderdeel van de match result (dat moet nl. alleen de delimiter zijn).
            _delimRegex = new Regex(_delim + "(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))", RegexOptions.Compiled);

            _stream = stream;
            _headerRow = _stream.ReadLine();
            _headers = Split(_headerRow);
        }

        public void AddConstantColumn(string columnHeader, string constantValue)
        {
            // voeg de nieuwe kolom achteraan toe
            _headerRow += $"{_delim}{columnHeader}";
            _constantValues += $"{_delim}{constantValue}";
            _headers = Split(_headerRow);
        }

        public bool Read()
        {
            if (_stream == null) return false;
            if (_stream.EndOfStream) return false;

            string rawRow = _stream.ReadLine();

            // voeg constante velden toe
            if (_constantValues.Length > 0)
                rawRow += _constantValues;

            _currentRow = Split(rawRow);

            return true;
        }

        private string[] Split(string input)
        {
            string[] parts = _delimRegex.Split(input);

            // verwijder mogelijke dubbele quotes aan de uiteinden
            for (int i = 0; i < parts.Length; i++)
            {
                parts[i] = parts[i].Trim('"');
            }
            return parts;
        }

        public char FieldDelimeter
        {
            get { return _delim; }
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
            get { return _headers.Length; }
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
            _stream.Close();
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
            for (int i = 0; i < _headers.Length; i++)
            {
                values[i] = GetValue(i);
            }
            return _headers.Length;
        }

        public int GetOrdinal(string name)
        {
            for (int i = 0; i < _headers.Length; i++)
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
                _stream.Dispose();

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
