using System;
namespace Acquaint.Models
{
	public interface IAcquaintance
	{
		string Id { get; set; }

		string DataPartitionId { get; set; }

		string FirstName { get; set; }

		string LastName { get; set; }

		string Company { get; set; }

		string JobTitle { get; set; }

		string Email { get; set; }

		string Phone { get; set; }

		string Street { get; set; }

		string City { get; set; }

		string PostalCode { get; set; }

		string State { get; set; }

		string PhotoUrl { get; set; }

		string AddressString { get; }

		string DisplayName { get; }

		string DisplayLastNameFirst { get; }

		string StatePostal { get; }

		string ToString();


	}
}

