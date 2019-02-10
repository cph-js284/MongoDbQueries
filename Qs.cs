using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using mongoTwitter1.Models;

namespace MongoDbQueries
{
    public class Qs
    {
        Stopwatch stopwatch;
        IMongoCollection<TwitterData> _collection;

        public Qs(IMongoCollection<TwitterData> coll)
        {
            stopwatch = new Stopwatch();
            _collection = coll;
        }

                /*Question 1  */
        public void CalculateUniqueUN(){
            System.Console.WriteLine("Question 1) How many Twitter users are in the database?");
            System.Console.WriteLine("Calculating unique number of usernames in Db - standby");
            //var collection = mdb.GetCollection<TwitterData>(CollectionName);

            var UserNameCounter = _collection.Distinct(x => x.UserName, _=> true).ToList().Count();

            System.Console.WriteLine($"Number of unique username found in Db : {UserNameCounter}");
            System.Console.WriteLine();
        }

        /*Question 2  */
        public void CalculateUserLinkToOther(){
            System.Console.WriteLine("Question 2) Which Twitter users link the most to other Twitter users? (Provide the top ten.)");            
            System.Console.WriteLine("Calculating linking between users - standby");
            //var collection = mdb.GetCollection<TwitterData>(CollectionName);

            var res = _collection.AsQueryable()
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

            //var collection = mdb.GetCollection<TwitterData>(CollectionName);
            var res = _collection.AsQueryable().Where(x=>x.Text.Contains("@")).ToList();
            
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
            //var collection = mdb.GetCollection<TwitterData>(CollectionName);

            var res = _collection.AsQueryable()
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

            //var collection = mdb.GetCollection<TwitterData>(CollectionName);
            var args = new AggregateOptions(){AllowDiskUse=true};
            
            var Full = _collection.Aggregate(args)
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