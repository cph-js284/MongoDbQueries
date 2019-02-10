using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbQueries;
using mongoTwitter1.Models;

namespace mongoTwitter1
{
    public class SettingUp
    {
        string Zipfile, CSVfile, DBname, CollectionName,host,port,ConnectionString;
        MongoClient client;
        IMongoDatabase mdb;
        Qs queries;

        public SettingUp()
        {
            Zipfile= "twitterdata.zip"; 
            //testdata file
            //CSVfile= "testdata.manual.2009.06.14.csv"; 
            
            //real data - large file
            CSVfile = "training.1600000.processed.noemoticon.csv";
            DBname = "TwitterTextDb";
            CollectionName ="TweetDocs";       
            host = DetectDockerBridgeGateWay();
            port = ":27017";
            ConnectionString = "mongodb://"+host+port;
            client = new MongoClient(ConnectionString);
            mdb = client.GetDatabase(DBname);
            queries = new Qs();
        }

        /*Downloading the zip from Stanford */
        public void DownLoadZipFile(){
            using (WebClient wc = new WebClient())
            {
                string Wadr = "http://cs.stanford.edu/people/alecmgo/";
                string file = "trainingandtestdata.zip";
                System.Console.WriteLine($"Connecting to {Wadr}");
                System.Console.WriteLine($"Retriving file : {file}");
                Uri uri = new Uri(Wadr + file);
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadProgress);
                wc.DownloadFileAsync(uri, Zipfile);
            }
        }

        /*Event handler - updating the console */
        private void downloadProgress(object sender, DownloadProgressChangedEventArgs e){
            if (e.ProgressPercentage == 100){
                System.Console.WriteLine("Download done ...");
                Unzip();
            }else{
                System.Console.Write($"\rDownload progress :     {e.ProgressPercentage}%");
            }
        }
        /*Decompress zipfile */
        public void Unzip(){
            System.Console.WriteLine("Unzipping file ...");
            System.IO.Compression.ZipFile.ExtractToDirectory(Zipfile, ".");
            Console.Clear();
            System.Console.WriteLine($"Done unzipping {Zipfile}");
            InsertDataIntoMongo();
        }

        /*prepares a string from the CSV(scrub and split), turns it into object for mongo */
        private TwitterData ConvertToTwitterData(string indata){
            TwitterData twitterData = new TwitterData();
            var lineFromFile = indata.Replace("\"","").Split(',');
            twitterData.Standford_id = long.Parse((lineFromFile[1]));
            twitterData.Polarity = int.Parse(lineFromFile[0]);
            twitterData.LongDate = lineFromFile[2];
            twitterData.Query = lineFromFile[3];
            twitterData.UserName = lineFromFile[4];
            for (int i = 5; i < lineFromFile.Length; i++)
            {
                twitterData.Text += lineFromFile[i];
            }
            return twitterData;
        }
        /*Read docker inet-adr from gateway.txt*/
        public string DetectDockerBridgeGateWay(){
            using (FileStream fs = new FileStream("gateway.txt", FileMode.Open, FileAccess.Read)){
                using (StreamReader sr = new StreamReader(fs)){
                    var res = sr.ReadLine().Split(':');
                    if (res[1]!=null){
                        var tmp = res[1].Replace("\"","").Replace(" ", "");
                        return tmp;
                    }else{
                        throw new Exception("Failed to detect docker network bridgeinet adr");
                    }
                }
            }
        }

        /*Inserts data into mongodb using bulk-insert (InsertMany) */
        public void InsertDataIntoMongo(){
            using(FileStream fs = new FileStream(CSVfile, FileMode.Open, FileAccess.Read)){
                using (StreamReader sr = new StreamReader(fs)){
                    
                    System.Console.WriteLine($"Connecting to Mongodb using connectionstring : {ConnectionString}");
                    System.Console.WriteLine($"Using database name : {DBname}");
                    System.Console.WriteLine($"Using collection name : {CollectionName}");

                    mdb.DropCollection(CollectionName);
                    var collection = mdb.GetCollection<TwitterData>(CollectionName);
                    int InsertCounter = 0;

                    System.Console.WriteLine("Building documents...");

                    List<TwitterData> ListOfData = new List<TwitterData>();

                    while (!sr.EndOfStream){
                        var tObj = ConvertToTwitterData(sr.ReadLine());
                        ListOfData.Add(tObj);
                        InsertCounter++;
                        if(InsertCounter % 2000 == 0){
                            System.Console.Write($"\rNumber of Docs created :          {InsertCounter} of 1600000 ");
                        }
                    }
                    System.Console.WriteLine("\nInserting documents into Db - standby");
                    System.Console.WriteLine("This could take several seconds");
                    collection.InsertMany(ListOfData);
                    System.Console.WriteLine("All documents inserted");
                }
            }
            BuildIndex();
            queries.Questions();
        }
        /*Build indexes on UserName field */
        public void BuildIndex(){
            System.Console.WriteLine("Building indexes - standby");
            var collection = mdb.GetCollection<TwitterData>(CollectionName);
            var indexBuilder = Builders<TwitterData>.IndexKeys;
            var indexmodel = new CreateIndexModel<TwitterData>(indexBuilder.Ascending(x=>x.UserName));
            collection.Indexes.CreateOne(indexmodel);
            System.Console.WriteLine("Done building indexes");
        }

    }
}