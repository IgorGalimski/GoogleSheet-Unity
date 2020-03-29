using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Data
{
    public class GoogleSheet : IEnumerable<GoogleSheetRow>
    {
        public int ID { get; }
        public string Name { get; }
        
        public ICollection<GoogleSheetRow> GoogleSheetRows { get; private set; } = new List<GoogleSheetRow>();

        public GoogleSheet(int id, string name)
        {
            ID = id;
            Name = name;
        }

        public Cell this[string range] => GetCell(range);

        private Cell GetCell(string range)
        {
            if (TryParseRange(range, out var columnIndex, out var rowIndex))
            {
                var listRowIndex = rowIndex - 1;
                
                var row = GoogleSheetRows.ElementAtOrDefault(listRowIndex);
                if (row == null)
                {
                    for (var index = GoogleSheetRows.Count; index < rowIndex; index++)
                    {
                        var newRow = new GoogleSheetRow(index, new List<Cell>());
                        GoogleSheetRows.Add(newRow);

                        if (index == listRowIndex)
                        {
                            row = newRow;
                        }
                    }
                }

                var listCellIndex = columnIndex - 1;

                var cell = row[listCellIndex];
                if (cell == null)
                {
                    var cells = row.Values;
                    
                    for (var index = cells.Count; index < columnIndex; index++)
                    {
                        var newCell = new Cell();
                        
                        cells.Add(newCell);

                        if (index == listCellIndex)
                        {
                            cell = newCell;
                        }
                    }
                }

                return cell;
            }

            return null;
        }

        private bool TryParseRange(string range, out int columnIndex, out int rowIndex)
        {
            columnIndex = -1;
            rowIndex = -1;

            if (!string.IsNullOrEmpty(range) && range.Length == 2)
            {
                columnIndex = char.ToUpper(range[0]) - 64;
                if (int.TryParse(range[1].ToString(), out rowIndex))
                {
                    return true;
                }
            }

            return false;
        }

        public void Parse(IEnumerable<JToken> valuesToken)
        {
            GoogleSheetRows.Clear();

            for (int i = 0; i < valuesToken.Count(); i++)
            {
                var jToken = valuesToken.ElementAt(i);
                
                var row = new List<Cell>();
                foreach (var value in jToken)
                {
                    var cell = new Cell
                    {
                        Value = GetJValueByGoogleSheetType(value)
                    };

                    row.Add(cell);
                }
                GoogleSheetRows.Add(new GoogleSheetRow(i, row));
            }
        }
        
        public void Clear()
        {
            GoogleSheetRows.Clear();
        }

        private JValue GetJValueByGoogleSheetType(JToken obj)
        {
            string objString = obj.ToString();
            JTokenType type = obj.Type;
            
            switch (type)
            {
                case JTokenType.Boolean:
                {
                    if (!bool.TryParse(objString, out var result));
                    return new JValue(result);
                }
                
                case JTokenType.Integer:
                {
                    int.TryParse(objString, out var result);
                    return new JValue(result);
                }
                    
                case JTokenType.Float:
                {
                    float.TryParse(objString, out var result);
                    return new JValue(result);
                } 
                    
                case JTokenType.Date:
                {
                    DateTime.TryParse(objString, out var result);
                    return new JValue(result);
                }

                default:
                {
                    return new JValue(objString);
                }
            }
        }

        public IEnumerator<GoogleSheetRow> GetEnumerator()
        {
            return GoogleSheetRows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}