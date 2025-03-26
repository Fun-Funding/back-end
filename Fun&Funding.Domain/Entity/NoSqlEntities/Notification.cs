using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Domain.Entity.NoSqlEntities
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, bool> UserReadStatus { get; set; } = new Dictionary<string, bool>();
        public Object Actor { get; set; }
        public Guid? ObjectId { get; set; }
        public NotificationType? NotificationType { get; set; }
    }
}
