using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using Microsoft.Data.SqlClient;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MQ_Load
{
    public class CSVProperties
    {
        public string Queue { get; set; }
        public DateTime DateTime { get; set; }
        public long Messages {  get; set; }
    }
    public sealed class CSVPropertiesMap : ClassMap<CSVProperties>
    {
        public CSVPropertiesMap() 
        {
            Map(m => m.Queue).TypeConverter<StringConverter>();
            Map(m => m.DateTime).TypeConverterOption.Format("dd/MM/yyyy H:mm");
            Map(m => m.Messages).TypeConverter<Int64Converter>();
        }
    }
}
