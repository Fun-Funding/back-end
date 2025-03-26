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
    public class CreatorContract
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        public string Policies { get; set; }

        public Guid? UserId { get; set; }

        public ContractType ContractType { get; set; }
    }
}
