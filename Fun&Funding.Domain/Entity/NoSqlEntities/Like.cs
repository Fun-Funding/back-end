using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fun_Funding.Domain.Entity.NoSqlEntities
{
    public class Like
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public bool IsLike { get; set; }
        public DateTime CreateDate { get; set; }

        public Guid ProjectId { get; set; }
        public Guid UserId { get; set; }
        public bool IsDelete { get; set; }

    }
}
