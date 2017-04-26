using Npgsql;
using StackExchange.Redis;
using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Dapper;
using System.IO;
using eric.coreminimal.data;
using System.IO.Compression;
using Microsoft.Extensions.DependencyModel;
using Microsoft.DotNet.InternalAbstractions;
using System.Reflection;
using System.Net.Sockets;

namespace eric.coreminimal
{
    public class Program
    {
        public static string DllPath = @"..\..\..\..\core-dynamic\bin\Debug\netstandard1.4\core-dynamic.dll";
        public static bool TestDBs = false;

        public static void Main(string[] args)
        {
            //List loaded assemblies 
            var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
            var assemblies = DependencyContext.Default.GetRuntimeAssemblyNames(runtimeId);

            /*foreach (var assembly in assemblies)
            {
                Console.WriteLine(assembly.FullName);
            }*/

            Console.WriteLine("Hello World from .net core!");
            Console.WriteLine(RuntimeInformation.FrameworkDescription + " " + RuntimeInformation.OSArchitecture + " " + RuntimeInformation.OSDescription + " " + RuntimeInformation.ProcessArchitecture);

            //DB Code assumes a table PROJECTS with 
            //ID - Integer
            //Name - Varchar or Text
            //Data - Image (VarBinary) / BLOB / Bytea
            //Filled at least 1 entry with an ID 1 to test Dapper

            
            //Dapper (Dapper 1.50.0-rc3 pre-release) 
            //SQL Server (System.data.SqlClient)
            if (TestDBs)
            {
                using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\ProjectsV13;Initial Catalog=test;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
                {
                    /* Create script for SQL Server
                    CREATE TABLE [dbo].[Projects] (
                    [Id]   INT             NOT NULL,
                    [Name] VARCHAR (50)    NOT NULL,
                    [Data] VARBINARY (MAX) NULL,
                    PRIMARY KEY CLUSTERED ([Id] ASC)
                    );
                    */

                    conn.Open();
                    RunSimpleSelectQuery(conn);

                    //Using SQL Server With Dapper - @ for binding 
                    string DapperQuery = "SELECT ID, NAME FROM PROJECTS WHERE ID = @Pk";
                    RunDapper(conn, DapperQuery);

                    byte[] data = new byte[100000];
                    data[0] = 0xff;
                    data[1] = 0x10;

                    //BLOB Testing
                    string blobinsert = "INSERT INTO PROJECTS (Id, Name, Data) VALUES (@Id, @Name, @Data)";
                    using (var binset = conn.CreateCommand())
                    {
                        binset.CommandText = blobinsert;
                        binset.Parameters.Add(new SqlParameter("@Id", 3));
                        binset.Parameters.Add(new SqlParameter("@Name", "Inserted"));
                        var p = binset.Parameters.Add(new SqlParameter("@Data", System.Data.SqlDbType.Image, data.Length));
                        p.Value = data;
                        binset.ExecuteNonQuery();
                    }
                    Console.WriteLine("Inserted");

                    string blobSelect = "SELECT Id, Name, Data FROM PROJECTS WHERE ID = 3";
                    ReadAndCheckBlob(conn, data, blobSelect);

                    string deletefrom = "DELETE FROM PROJECTS WHERE Id = 3";
                    ExecuteDirect(conn, deletefrom);
                    Console.WriteLine("-- Deleted BLOB");
                }
                //Postgres SQL (npgsql)
                using (NpgsqlConnection conn = new NpgsqlConnection("Server=localhost;Port=5432;Database=Test;User Id=postgres;Password = tspostgres; "))
                {
                    /* Create script for Postgresql
                    CREATE TABLE public."Projects"
                    (
                        "Id" integer NOT NULL,
                        "Name" text,
                        "Data" bytea,
                        CONSTRAINT "ProjectsPK" PRIMARY KEY ("Id")
                    )
                    */

                    conn.Open();
                    RunSimpleSelectQuery(conn);

                    //Using Postgres With Dapper - Quoted fields and : for binding
                    string DapperQuery = "SELECT \"Id\", \"Name\" FROM \"Projects\" WHERE \"Id\" = :Pk";
                    RunDapper(conn, DapperQuery);

                    //BLOB testing
                    byte[] data = new byte[100000];
                    data[0] = 0xff;
                    data[1] = 0x10;

                    //BLOB Testing
                    string blobinsert = "INSERT INTO \"Projects\" (\"Id\", \"Name\", \"Data\") VALUES (:Id, :Name, :Data)";
                    using (var binset = conn.CreateCommand())
                    {
                        binset.CommandText = blobinsert;
                        binset.Parameters.Add(new NpgsqlParameter(":Id", 3));
                        binset.Parameters.Add(new NpgsqlParameter(":Name", "Inserted"));
                        var p = binset.Parameters.Add(new NpgsqlParameter(":Data", NpgsqlTypes.NpgsqlDbType.Bytea, data.Length));
                        p.Value = data;
                        binset.ExecuteNonQuery();
                    }
                    Console.WriteLine("Inserted");

                    string blobSelect = "SELECT \"Id\", \"Name\", \"Data\" FROM \"Projects\" WHERE \"Id\" = 3";
                    ReadAndCheckBlob(conn, data, blobSelect);

                    string deletefrom = "DELETE FROM \"Projects\" WHERE \"Id\" = 3";
                    ExecuteDirect(conn, deletefrom);
                    Console.WriteLine("-- Deleted BLOB");
                }
                //SQLite

                //Oracle (When .net core support is there)

                //Redis (Ngonzalez.StackExchange.Redis)
                Console.WriteLine("Redis - Connecting ...");
                var cm = ConnectionMultiplexer.Connect("localhost");
                IDatabase db = cm.GetDatabase();
                db.StringSet("NetCoreString", "Basic String from .net core");
                db.StringIncrement("NetCoreCount");
                Console.WriteLine("Redis - Setters done");
                Console.WriteLine("NetCoreString -> " + db.StringGet("NetCoreString"));
                Console.WriteLine("NetCoreCount -> " + db.StringGet("NetCoreCount"));
                Console.WriteLine("Redis - Getters done");
                Console.WriteLine("--");
            }
            //JSON.NET newtonsoft (Newtonsoft.Json)
            Console.WriteLine("JSON Newtonsoft");
            List<TimeWithData> list = new List<TimeWithData>();
            for (int i = 0; i < 10; i++)
            {
                list.Add(new TimeWithData() { t = DateTime.Now.AddDays(i).Date, v = Math.PI * i });
            }
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
            Console.WriteLine("Serialized: " + json);

            List<TimeWithData> list2 = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TimeWithData>>(json);
            for (int i = 0; i < list2.Count; i++)
            {
                Console.WriteLine(i.ToString() + " : " + list[i] + " == " + list2[i] + " is equals " + list[i].Equals(list2[i]).ToString());
            }
            Console.WriteLine("--");

            //WindowsService - Not there yet
            //Environment.UserInteractive 

            //WCF Client

            //WCF Server


            //Where am I executing
            Uri x = new Uri(System.Reflection.Assembly.GetEntryAssembly().CodeBase);
            string s = x.LocalPath;
            DirectoryInfo di = (new FileInfo(s)).Directory;
            Console.WriteLine("URI: " + x + Environment.NewLine + "LocalPath: " + s + Environment.NewLine + "DirectoryFullName: " + di.FullName);

            Program.DllPath = Path.Combine(di.FullName, Program.DllPath);

            //Struct Binary I/O 
            //Unsafe
            string FileName = "demoBin.bin";
            File.Delete(Path.Combine(di.FullName, FileName));
            BinaryFortranInterchange.ToBinary(di.FullName, FileName, 5);
            BinaryFortranInterchange.FromBinary(di.FullName, FileName, 5);
            Console.WriteLine("-- Binary IO");

            //Compression
            FileInfo fi = new FileInfo(Path.Combine(di.FullName, FileName));
            using (MemoryStream Compressed = new MemoryStream())
            {
                using (BinaryReader bw = new BinaryReader(fi.OpenRead()))
                {
                    long TotalLength = bw.BaseStream.Length;
                    using (GZipStream Gz = new GZipStream(Compressed, CompressionMode.Compress, true))
                    {
                        Gz.Write(bw.ReadBytes((int)bw.BaseStream.Length), 0, (int)bw.BaseStream.Length);
                    }
                    Console.WriteLine("Length: T: " + TotalLength + " / C: " + Compressed.Length);
                    //To use the compressed data (eg. Blob upload - Compressed.ToArray())
                }
            }
            Console.WriteLine("-- Binary Compression");

            //Dynamic DLL loading on Miss (opt from DB) - Currently doesn't work
            //System.Runtime.Loader.AssemblyLoadContext.Default.Resolving += Assembly_Resolve;
            try
            {
                //Works
                /*
                 //compiled into core-minimal-dynamic.dll
                 
                namespace eric.coreminimal.dynamic
                {
                    public class DemoDynamicClass
                    {
                        public int x { get; set; }

                        public DemoDynamicClass()
                        {
                            x = 5; 
                        }
                    }
                }

                */
                FileStream fs = File.OpenRead(Program.DllPath);
                MemoryStream ms = new MemoryStream();
                fs.CopyTo(ms);
                ms.Position = 0;
                Assembly asm = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(ms);
                var c = Type.GetType("eric.coreminimal.dynamic.DemoDynamicClass,core-dynamic");
                dynamic d = Activator.CreateInstance(c);
                Console.WriteLine(d.x);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            //Windows Workflow Foundation 


            //Simple network communication 
            //var wl = new TcpListener(); 
        }

        /*private static System.Reflection.Assembly Assembly_Resolve(System.Runtime.Loader.AssemblyLoadContext loadContext, System.Reflection.AssemblyName TargetAssembly)
        {
            if (TargetAssembly.Name == "core-minimal-dynamic")
            {
                FileStream fs = File.OpenRead(Program.DllPath);
                MemoryStream ms = new MemoryStream();
                fs.CopyTo(ms);
                ms.Position = 0;
                Assembly asm = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromStream(ms);
                return asm; 
            }
            /////////
            //var runtimeId = RuntimeEnvironment.GetRuntimeIdentifier();
            //var assemblies = DependencyContext.Default.GetRuntimeAssemblyNames(runtimeId);

            //foreach (var assembly in assemblies)
            //{
            //    Console.WriteLine(assembly.FullName);
            //    if (assembly.Name.Equals(TargetAssembly.Name))
            //        return loadContext.LoadFromAssemblyName(assembly); //Maybe we don't need this anymore, we don't hit assembly_resolve for what we find?
            //}
            //using (FileStream fs = File.OpenRead(Program.DllPath)
            //{
            //    Assembly asm = loadContext.LoadFromStream(fs);
            //    return asm; 
            //}
            return null; 
        }*/

        private static void ReadAndCheckBlob(DbConnection conn, byte[] data, string blobSelect)
        {
            using (var bSelect = conn.CreateCommand())
            {
                bSelect.CommandText = blobSelect;
                using (var result = bSelect.ExecuteReader())
                {
                    while (result.Read())
                    {
                        if (result.HasRows)
                        {
                            var r = result.GetFieldValue<byte[]>(2);
                            bool isIdentical = true;
                            for (int i = 0; i < r.Length; i++)
                                if (r[i] != data[i]) { isIdentical = false; break; }

                            Console.WriteLine("Selected : Id " + result.GetInt32(0) + " Name" + result.GetString(1) + " Binary data identical : " + isIdentical.ToString());
                        }
                    }
                }
            }
        }

        private static void ExecuteDirect(DbConnection conn, string sqlNonQuery)
        {
            using (var bdelete = conn.CreateCommand())
            {
                bdelete.CommandText = sqlNonQuery;
                bdelete.ExecuteNonQuery();
            }
        }

        private static void RunDapper(DbConnection conn, string DapperQuery)
        {
            Project p = conn.QuerySingle<Project>(DapperQuery, new { Pk = 1 });
            if (p != null) Console.WriteLine(p.Id + " :: " + p.Name);
            Console.WriteLine("-- Dapper done with " + conn.ToString());
        }

        private static void RunSimpleSelectQuery(DbConnection conn)
        {
            Console.WriteLine(conn.ToString() + " " + conn.DataSource + " " + conn.Database + " " + conn.ServerVersion);
            using (DbCommand c = conn.CreateCommand())
            {
                //Postgres likes quoted table names 
                c.CommandText = "Select * from \"Projects\"";
                using (var dr = c.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        Console.WriteLine(dr[1]);
                    }
                }
            }
            Console.WriteLine("--");
        }
    }
}
