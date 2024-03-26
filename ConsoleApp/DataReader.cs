namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class DataReader
    {
        List<ImportedObject> ImportedObjects = new List<ImportedObject>();

        public void ImportAndPrintData(string fileToImportPath)
        {
            ImportNewObjects(fileToImportPath);
            PrintImportedObjects();

            Console.ReadLine();
        }

        private void ImportNewObjects(string fileToImportPath)
        {
            ImportedObjects.Clear();

            ImportObjectsFromLines(GetImportedLinesFromPath(fileToImportPath), ImportedObjects);
            ClearAllImportedObjects();

            AssignNumberOfChildren(ImportedObjects);
        }

        private void ImportObjectsFromLines(List<string> importedLines, List<ImportedObject> listToImportInto)
        {
            for (int i = 0; i < importedLines.Count; i++)
            {
                var importedLine = importedLines[i];
                var values = importedLine.Split(';');
                var importedObject = new ImportedObject();

                if(values.Length < 7)
                {
                    continue;
                }

                importedObject.Type = values[0];
                importedObject.Name = values[1];
                importedObject.Schema = values[2];
                importedObject.ParentName = values[3];
                importedObject.ParentType = values[4];
                importedObject.DataType = values[5];
                importedObject.IsNullable = values[6];
                listToImportInto.Add(importedObject);
            }
        }

        private List<string> GetImportedLinesFromPath(string filePath)
        {
            var streamReader = new StreamReader(filePath);
            var importedLines = new List<string>();

            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                importedLines.Add(line);
            }

            return importedLines;
        }

        private void AssignNumberOfChildren(List<ImportedObject> targetList)
        {
            foreach(var parentObject in targetList)
            {
                foreach(var childObject in targetList)
                {
                    if (childObject.ParentType == parentObject.Type && childObject.ParentName == parentObject.Name)
                    {
                        parentObject.NumberOfChildren = 1 + parentObject.NumberOfChildren;
                    }
                }
            }
        }

        private void ClearAllImportedObjects()
        {
            foreach (var importedObject in ImportedObjects)
            {
                ClearImportedObject(importedObject);
            }
        }

        private void ClearImportedObject(ImportedObject importedObject)
        {
            importedObject.Type = ClearLine(importedObject.Type).ToUpper();
            importedObject.Name = ClearLine(importedObject.Name);
            importedObject.Schema = ClearLine(importedObject.Schema);
            importedObject.ParentName = ClearLine(importedObject.ParentName);
            importedObject.ParentType = ClearLine(importedObject.ParentType);
        }

        private string ClearLine(string lineToClear)
        {
            return lineToClear.Trim().Replace(" ", "").Replace(Environment.NewLine, "");
        }

        private void PrintImportedObjects()
        {
            foreach (var database in ImportedObjects)
            {
                if (database.Type == "DATABASE")
                {
                    Console.WriteLine($"Database '{database.Name}' ({database.NumberOfChildren} tables)");
                    PrintTables(database);
                }
            }
        }

        private void PrintTables(ImportedObject database)
        {
            foreach (var table in ImportedObjects)
            {
                if (table.ParentType.ToUpper() == database.Type)
                {
                    if (table.ParentName == database.Name)
                    {
                        Console.WriteLine($"\tTable '{table.Schema}.{table.Name}' ({table.NumberOfChildren} columns)");
                        PrintColumns(table);
                    }
                }
            }
        }

        private void PrintColumns(ImportedObject table)
        {
            foreach (var column in ImportedObjects)
            {
                if (column.ParentType.ToUpper() == table.Type)
                {
                    if (column.ParentName == table.Name)
                    {
                        Console.WriteLine($"\t\tColumn '{column.Name}' with {column.DataType} data type {(column.IsNullable == "1" ? "accepts nulls" : "with no nulls")}");
                    }
                }
            }
        }
    }

    class ImportedObject
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Schema { get; set; }

        public string ParentName { get; set; }
        public string ParentType{ get; set; }

        public string DataType { get; set; }
        public string IsNullable { get; set; }

        public double NumberOfChildren { get; set; }
    }
}
