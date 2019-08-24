CsvDataReader
=============

Dit project heeft als startpunt de code van Bill Graziano.

Een C# class voor het lezen van CSV files die geschikt is om gebruikt te worden door de BulkCopy class (om een csv file efficient op te slaan naar een SQL Server database).

Voorbeeld Powershell code:

	[System.Reflection.Assembly]::LoadFrom("CsvDataReader.dll")
	$reader = New-Object SqlUtilities.CsvDataReader("SimpleCsv.txt")
		
	$bulkCopy = new-object ("Data.SqlClient.SqlBulkCopy") $ConnectionString
	$bulkCopy.DestinationTableName = "CsvDataReader"
		
	$bulkCopy.WriteToServer($reader);

