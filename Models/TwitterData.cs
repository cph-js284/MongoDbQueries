using System;
using MongoDB.Bson;

namespace mongoTwitter1.Models
{
    public class TwitterData
    {
        public ObjectId _id { get; set; }
        public long Standford_id { get; set; }
        public int Polarity { get; set; }
        public string LongDate { get; set; }
        public string Query { get; set; }
        public string UserName { get; set; }
        public string Text { get; set; }
    }
}