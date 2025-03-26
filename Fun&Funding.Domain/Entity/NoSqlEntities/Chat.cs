using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Fun_Funding.Domain.Entity.NoSqlEntities
{
    public class Chat
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}
