using CsvHelper;
using CsvHelper.Configuration;
using Dapper;
using System.Data.SqlClient;
using CsvHelper.TypeConversion;
using CsvHelper.Configuration.Attributes;
using Microsoft.SqlServer.Server;
using MQ_Load;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Formats.Asn1;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using MsSqlHelpers;
using System.Data;
using System.Data.SqlTypes;
using System.Collections;

namespace MQLoad
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            //Initialize config of the CSVHelper Library.
            var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HeaderValidated = null,
                HasHeaderRecord = true,
            };

            try
            {
                //Initilize variables for connecting to database and queries
                string connetionString = "data source=APTC7CNHW\\AOM;initial catalog=MQMonitor;persist security info=True;Integrated Security=True;multipleactiveresultsets=True;application name=EntityFramework";
                string sqlInsert = "INSERT INTO dom.QueueDepthLog (QueueID, DateTime, Depth) VALUES (@QueueID, @DateTime, @Depth);";
                string sqlRead = "SELECT ID, Name FROM dom.Queue";
                
                List<QueueProperties> queues = new List<QueueProperties>();
                List<FinalDataProperties> FinalData = new List<FinalDataProperties>();


                //Create dictionary from the list of queues stores in the SQL Server
                using (IDbConnection db = new SqlConnection(connetionString))
                { 
                    queues = db.Query<QueueProperties>(sqlRead).ToList();
                }

                //Open new streamreader to read from file and
                //read it into a list after registering the mapping.
                using (var reader = new StreamReader("C:\\Users\\c22523b\\Desktop\\QueueData.csv"))
                using (var csv = new CsvReader(reader, cfg))
                {
                    csv.Context.RegisterClassMap<CSVPropertiesMap>();
                    List<CSVProperties> records = csv.GetRecords<CSVProperties>().ToList();

                    //Used for testing to see if the output is the CSV file data.
                    foreach (CSVProperties CSVProperties in records)
                    {
                        Console.WriteLine($"Queue: {CSVProperties.Queue}, DateTime: {CSVProperties.DateTime}, Messages: {CSVProperties.Messages}");

                        FinalDataProperties finalData = new FinalDataProperties();

                        /*foreach (var queue in queues)
                        {
                            if (CSVProperties.Queue == queue.Name) 
                            {
                                finalData.QueueID = queue.ID;
                            }
                        }*/
                                              
                        long QueueID = queues.First(queue => CSVProperties.Queue == queue.Name).ID;

                        finalData.QueueID = QueueID;
                        finalData.DateTime = CSVProperties.DateTime;
                        finalData.Depth = CSVProperties.Messages;

                        FinalData.Add(finalData);
                    }

                    //Establish a SQL Server connection and insert records into database
                    using (IDbConnection db = new SqlConnection(connetionString))
                    {
                        db.Execute(sqlInsert, FinalData);
                    }
                }
            }
            catch (Exception ex)
            {
                //Throw messages into console instead of visual studio itself.
                Console.WriteLine(ex.Message);
            }
        }
    }
}