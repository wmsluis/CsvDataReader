CsvDataReader
=============

A simple C# IDataReader implementation to read CSV files.  This was built to improve CSV performance in PowerShell.  The goal is to enable code like the following.

Dit project heeft als startpunt de code van Bill Graziano genomen.

__If you download the DLL, you need to Unblock it before you can use it.  That's done in the File Properties dialog box.__


	[System.Reflection.Assembly]::LoadFrom("CsvDataReader.dll")
	$reader = New-Object SqlUtilities.CsvDataReader("SimpleCsv.txt")
		
	$bulkCopy = new-object ("Data.SqlClient.SqlBulkCopy") $ConnectionString
	$bulkCopy.DestinationTableName = "CsvDataReader"
		
	$bulkCopy.WriteToServer($reader);




