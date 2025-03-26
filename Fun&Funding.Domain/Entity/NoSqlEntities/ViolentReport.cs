using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Fun_Funding.Domain.Enum;

namespace Fun_Funding.Domain.Entity.NoSqlEntities
{
    public class ViolentReport
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonElement("ReporterId")]
        public Guid ReporterId { get; set; }

        [BsonElement("ViolatorId")]
        public Guid ViolatorId { get; set; }
        [BsonElement("Type")]
        public ReportType Type { get; set; }

        [BsonElement("Content")]
        public string Content { get; set; }
        [BsonElement("IsHandle")]
        public bool IsHandle { get; set; } 
        [BsonElement("ReportDate")]
        public DateTime Date { get; set; }

        [BsonElement("FileUrls")]
        public List<string>? FileUrls { get; set; }
        [BsonElement("FaultCauses")]
        public List<string>? FaultCauses { get; set; }

    }
}
