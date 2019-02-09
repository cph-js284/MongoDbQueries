using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using MongoDB.Bson;
using MongoDB.Driver;
using mongoTwitter1.Models;

namespace mongoTwitter1
{
    public class SettingUp
    {
        string Zipfile, CSVfile, DBname, CollectionName,host,port,ConnectionString;
        Stopwatch stopwatch;
        MongoClient client;
        IMongoDatabase mdb;

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
            stopwatch = new Stopwatch();
            client = new MongoClient(ConnectionString);
            mdb = client.GetDatabase(DBname);

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
            Questions();
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

        /*Question 1  */
        public void CalculateUniqueUN(){
            System.Console.WriteLine("Question 1) How many Twitter users are in the database?");
            System.Console.WriteLine("Calculating unique number of usernames in Db - standby");
            var collection = mdb.GetCollection<TwitterData>(CollectionName);

            var UserNameCounter = collection.Distinct(x => x.UserName, _=> true).ToList().Count();

            System.Console.WriteLine($"Number of unique username found in Db : {UserNameCounter}");
            System.Console.WriteLine();
        }

        /*Question 2  */
        public void CalculateUserLinkToOther(){
            System.Console.WriteLine("Question 2) Which Twitter users link the most to other Twitter users? (Provide the top ten.)");            
            System.Console.WriteLine("Calculating linking between users - standby");
            var collection = mdb.GetCollection<TwitterData>(CollectionName);

            var res = collection.AsQueryable()
                    .Where(x => x.Text.Contains("@"))
                    .GroupBy(u => u.UserName)
                    .Select(usr => new{UserName = usr.Key, TweetCount = usr.Count()})
                    .OrderByDescending(c => c.TweetCount)
                    .Take(10);

            foreach (var item in res)
            {
                System.Console.WriteLine($"User : {item.UserName} references other users {item.TweetCount} times");
            }    
            System.Console.WriteLine();
        }

        /*Question 3  */
        public void CalculateMostMentioned(){
            System.Console.WriteLine("Question 3) Who are the most mentioned Twitter users? (Provide the top five.)");
            System.Console.WriteLine("Calculating most mentioned users - standby");
            Dictionary<string, int> store = new Dictionary<string, int>();

            var collection = mdb.GetCollection<TwitterData>(CollectionName);
            var res = collection.AsQueryable().Where(x=>x.Text.Contains("@")).ToList();
            
            Regex regex = new Regex(@"@[\w]+");

            foreach (var item in res)
            {
                var gr = regex.Match(item.Text).Groups;
                    if(gr[0].Length>0){
                        var tmp = gr[0].Value;
                        if(store.TryGetValue(tmp, out int Counter)){
                            store[tmp] = Counter + 1;
                        }else{
                            store[tmp] = 1;
                        }
                    }
            }

            var orderBy = store.OrderByDescending(x=>x.Value).Take(5);
            foreach (var item in orderBy){
                System.Console.WriteLine(item.Key +" - "+ item.Value);
            }
            System.Console.WriteLine();
        }


        /*Question 4  */
        public void CalculateMostActive(){
            System.Console.WriteLine("Question 4) Who are the most active Twitter users (top ten)?");
            System.Console.WriteLine("Calculating most active twitter users - standby");
            var collection = mdb.GetCollection<TwitterData>(CollectionName);

            var res = collection.AsQueryable()
                    .GroupBy(u => u.UserName)
                    .Select(usr => new{UserName = usr.Key, TweetCount = usr.Count()})
                    .OrderByDescending(c => c.TweetCount)
                    .Take(10);

            foreach (var item in res){
                System.Console.WriteLine($"User : {item.UserName} has made {item.TweetCount} tweets");
            }
            System.Console.WriteLine();
        }

        /*Question 5  */
        public void CalculateMostGrumpy(){
            System.Console.WriteLine("Question 5) Who are the five most grumpy (most negative tweets) and the most happy (most positive tweets)?");
            System.Console.WriteLine("Calculating most grumpy twitter users - standby");

            var collection = mdb.GetCollection<TwitterData>(CollectionName);
            var args = new AggregateOptions(){AllowDiskUse=true};
            
            var Full = collection.Aggregate(args)
                    .Group(u=>u.UserName, grp=> new{
                        UserName = grp.Key,
                        PolScore = grp.Sum(y=>y.Polarity)
                    }).ToList().OrderBy(x=>x.PolScore);

            var Gres = Full.Take(5);
            var Hres = Full.TakeLast(5);

            System.Console.WriteLine("5 users with lowest polarity score (most grumpy)");
            foreach (var item in Gres){
                System.Console.WriteLine(item.UserName +" - - " + item.PolScore);
            }
            System.Console.WriteLine();
            System.Console.WriteLine("5 users with highest polarity score (least grumpy)");
            foreach (var item in Hres.Reverse()){
                System.Console.WriteLine(item.UserName +" - - " + item.PolScore);
            }
            System.Console.WriteLine();
        }

        /*method-wrapper; going through each query */
        public void Questions(){
            BuildIndex();
            System.Console.WriteLine("--------------------------------------------------------------------");
            System.Console.WriteLine("-------------- Setup done - running queries ------------------------");
            System.Console.WriteLine("--------------------------------------------------------------------");
            System.Console.WriteLine();

            stopwatch.Start();
            CalculateUniqueUN();
            stopwatch.Stop();
                System.Console.WriteLine($"Method completed in {stopwatch.ElapsedMilliseconds} ms\n\n");
            stopwatch.Reset();

            stopwatch.Start();
            CalculateUserLinkToOther();
            stopwatch.Stop();
                System.Console.WriteLine($"Method completed in {stopwatch.ElapsedMilliseconds} ms\n\n");
            stopwatch.Reset();

            stopwatch.Start();
            CalculateMostMentioned();
            stopwatch.Stop();
                System.Console.WriteLine($"Method completed in {stopwatch.ElapsedMilliseconds} ms\n\n");
            stopwatch.Reset();

            stopwatch.Start();
            CalculateMostActive();
            stopwatch.Stop();
                System.Console.WriteLine($"Method completed in {stopwatch.ElapsedMilliseconds} ms\n\n");
            stopwatch.Reset();

            stopwatch.Start();
            CalculateMostGrumpy();
            stopwatch.Stop();
                System.Console.WriteLine($"Method completed in {stopwatch.ElapsedMilliseconds} ms\n\n");
            System.Console.WriteLine("Done... Press any key to exit program - This will also stop and remove (--rm) the docker container running this program");
        }
    }
}