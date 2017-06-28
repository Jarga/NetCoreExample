using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Shared.Models
{
    [BsonIgnoreExtraElements]
    public class Entity
    {
        public Guid Key { get; set; }

        public object Data { get; set; }
    }
}