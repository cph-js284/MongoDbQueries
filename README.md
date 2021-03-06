# MongoDbQueries
Turn-in assignment 2 in PB database soft2019spring

# What is it?
This is C# console program that runs in a docker container

# What does it do?
The program connects to Stanford http://cs.stanford.edu/people/alecmgo/ it downloads the file trainingandtestdata.zip and decompresses it to a local folder placed inside the container in which the program is running.

It uses Bulk-insert, to insert the 1.6 mill lines of data into a Mongo-Database running in a separate container.
After inserting the data it runs 5 queries corresponding to the 5 questions in the assignment.

A timer is run at the start of each query and printed to the console along with the response from the database.

------------------------------------------------------------------------------------------------------------------

# How to make it work
1) Vagrant ssh into your Ubuntu

2) Create a new folder and clone the repo to this newly created folder

3) Put the following line into your terminal : *$sudo ifconfig docker0 | grep "inet addr" > gateway.txt*
  - This will put the docker brigde inet-adr into a file called gateway.txt, the program uses this to connect to the container running      Mongo.

4) Start up a Docker container running MongoDb by typing: *$sudo docker run -d --rm -p=27017:27017 --name dbms mongo*

5) Build a Docker container that will run the console program by typing: *$sudo docker build -t justaname .*
  - *(Including the "." in the end)*
  - You can call the container whatever you want, remember to use the same name when starting the container
  - This will fetch all dependencies and build the .dll file
 
6) Start the container with the console program by typing: *$sudo docker run -it --rm --link dbms:mongo justaname*

-----------------------------------------------------------------------------------------------------------------
# Additional info
- You can stop the Docker container running the MongoDB by typing: *$sudo docker stop dbms*

- Both of the Docker containers are started the with --rm flag meaning they will automaticly be deleted once they are stopped

- The timing results that you see in the programOutput.txt file may vary depending on your rig (on my 3 year old laptop the execution of each query take about 25 secs)

- This has happened to me once, but since the download method is run asyncromously the decompresser might fail with the follwing error:
*System.IO.InvalidDataException: End of Central Directory record could not be found.*
Should this be the case just start a new container by redoing step 6)

--------------------------------------------------------------------------------------------------------------------
# Program output
Please see file programOutput.txt for output.
