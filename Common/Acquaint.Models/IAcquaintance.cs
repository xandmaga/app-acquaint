namespace Acquaint.Models
{
	// The choice was made to use an interface as the model so that it can be 
	// shared between the mobile app and backend, without having a separate 
	// DTO class. There are different implemetnations of IAcquaintance in 
	// the app and backend because they each need to inherit from different base 
	// classes; specifically ObservableObject in the app, and EntityData in 
	// the backend.
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
	}
}

