using System;

namespace Acquaint.Abstractions
{
	/// <summary>
	/// Helper methods for working with <see cref="Guid"/>.
	/// </summary>
	public interface IGuidUtility
	{
		Guid Create(Guid namespaceId, string name);

		Guid Create(Guid namespaceId, string name, int version);
	}
}