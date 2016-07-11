using System;

namespace Acquaint.Abstractions
{
	public interface IObservableEntityData
	{
		string Id { get; set; }

		DateTimeOffset CreatedAt { get; set; }

		DateTimeOffset UpdatedAt { get; set; }

		string Version { get; set; }
	}
}

