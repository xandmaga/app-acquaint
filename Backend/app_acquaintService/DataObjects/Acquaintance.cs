using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Tables;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace Acquaint.Service.DataObjects
{
    public class Acquaintance : EntityData
    {
        public string DataPartitionId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Company { get; set; }

        public string JobTitle { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Street { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        public string State { get; set; }

        public string PhotoUrl { get; set; }
    }
}