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
1) vagrant ssh into your Ubuntu
2) create a new folder and clone the repo to this newly created folder
3) put the following line into your terminal : sudo docker inspect bridge | grep "Gateway" > gateway.txt
