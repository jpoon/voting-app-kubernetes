using Microsoft.WindowsAzure.Storage.Table;

namespace worker
{
    public class VoteEntity : TableEntity
    {
        public const string PK = "vote";

        public VoteEntity() { }

        public VoteEntity(string id, string vote)
        {
            PartitionKey = PK;
            RowKey = id;
            Vote = vote;
        }

        public string Vote { get; set; }
    }

    public class VoteCountEntity : TableEntity
    {
        public const string PK = "votecount";

        public VoteCountEntity() { }

        public VoteCountEntity(string vote, int count)
        {
            PartitionKey = PK;
            RowKey = vote;
            Count = count;
        }

        public int Count { get; set; }
    }
}
