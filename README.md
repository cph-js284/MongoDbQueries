# MongoDbQueries
Turn-in exercise2 in PB database soft2019spring

# What is it?
This is C# console program that runs in a docker container

# What does it do?
The program connects to Stanford http://cs.stanford.edu/people/alecmgo/ it downloads the file trainingandtestdata.zip and decompresses it to a local folder placed inside the container in which the program is running.

It uses Bulk-insert, to insert the 1.6 mill lines of data into a Mongo-Database running in a seperate container.
After inserting the data it runs 5 queries corresponding to the 5 questions in the exercise.

A timer is run at the start of each query and printed to the console along with the response from the database.

------------------------------------------------------------------------------------------------------------------

# How to make it work
1) Vagrant ssh into your Ubuntu

2) Create a new folder and clone the repo to this newly created folder

3) Put the following line into your terminal : *$sudo docker inspect bridge | grep "Gateway" > gateway.txt*
  - This will put the docker brigde inet-adr into a file called gateway.txt, the program uses this to connect to the container running      Mongo.

4) Start up a Docker container running MongoDb by typing: *$sudo docker run -d --rm -p=27017:27017 --name dbms mongo*

5) Build a Docker container that will run the console program by typing: *$sudo docker build -t justaname .*
  - You can call the container whatever you want, remember to use the same name when starting the container
  - This will fetch all dependencies and build the .dll file
 
6) Start the container with the console program by typing: *$sudo docker run -it --rm --link dbms:mongo justaname*

-----------------------------------------------------------------------------------------------------------------
# Additional info
- You can stop the Docker container running the MongoDB by typing: *$sudo docker stop dbms*

- Both of the Docker containers are started the with --rm flag meaning they will automaticly be deleted once they are stopped

- This has happened to me once, but since the download method is run asyncromously the decompresser might fail with the follwing error:
*System.IO.InvalidDataException: End of Central Directory record could not be found.*
Should this be the case just start a new container by redoing step 6)

--------------------------------------------------------------------------------------------------------------------
# Program output
The following is output produced by the program:

vagrant@vagrant:/vagrant/DBs/mongoTwitter1$ sudo docker run -it --rm --link dbms:mongo monapp1
Connecting to http://cs.stanford.edu/people/alecmgo/
Retriving file : trainingandtestdata.zip
Download progress :     99%Download done ...
Unzipping file ...
Done unzipping twitterdata.zip
Connecting to Mongodb using connectionstring : mongodb://172.17.0.1:27017
Using database name : TwitterTextDb
Using collection name : TweetDocs
Building documents...
Number of Docs created :          1600000 of 1600000
Inserting documents into Db - standby
This could take several seconds
All documents inserted
Building indexes - standby
Done building indexes
--------------------------------------------------------------------
-------------- Setup done - running queries ------------------------
--------------------------------------------------------------------

Question 1) How many Twitter users are in the database?
Calculating unique number of usernames in Db - standby
Number of unique username found in Db : 659775

Method completed in 4056 ms


Question 2) Which Twitter users link the most to other Twitter users? (Provide the top ten.)
Calculating linking between users - standby
User : lost_dog references other users 549 times
User : tweetpet references other users 310 times
User : VioletsCRUK references other users 251 times
User : what_bugs_u references other users 246 times
User : tsarnick references other users 245 times
User : SallytheShizzle references other users 229 times
User : mcraddictal references other users 217 times
User : Karen230683 references other users 216 times
User : keza34 references other users 211 times
User : TraceyHewins references other users 202 times

Method completed in 2900 ms


Question 3) Who are the most mentioned Twitter users? (Provide the top five.)
Calculating most mentioned users - standby
@mileycyrus - 4310
@tommcfly - 3767
@ddlovato - 3258
@DavidArchie - 1245
@Jonasbrothers - 1237

Method completed in 13269 ms


Question 4) Who are the most active Twitter users (top ten)?
Calculating most active twitter users - standby
User : lost_dog has made 549 tweets
User : webwoke has made 345 tweets
User : tweetpet has made 310 tweets
User : SallytheShizzle has made 281 tweets
User : VioletsCRUK has made 279 tweets
User : mcraddictal has made 276 tweets
User : tsarnick has made 248 tweets
User : what_bugs_u has made 246 tweets
User : Karen230683 has made 238 tweets
User : DarkPiano has made 236 tweets

Method completed in 3110 ms


Question 5) Who are the five most grumpy (most negative tweets) and the most happy (most positive tweets)?
Calculating most grumpy twitter users - standby
5 users with lowest polarity score (most grumpy)
iverissc - - 0
alanarubi - - 0
lizzyswims24 - - 0
lettherestdie - - 0
sarnwardhan - - 0

5 users with highest polarity score (least grumpy)
what_bugs_u - - 984
DarkPiano - - 924
VioletsCRUK - - 872
tsarnick - - 848
keza34 - - 844

Method completed in 9724 ms


Done... Press any key to exit program - This will also stop and remove (--rm) the docker container running this program



