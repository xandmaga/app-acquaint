using System;

namespace Acquaint.Abstractions
{
	/// <summary>
	/// A utlity that creates dterministic GUIDs for given strings.
	/// </summary>
	public interface IGuidUtility
	{
		/// <summary>
		/// Create a deterministic GUID for a given string.
		/// </summary>
		/// <param name="value">Any string value.</param>
		Guid Create(string value);

		/// <summary>
		/// The namespace for fully-qualified domain names (from RFC 4122, Appendix C).
		/// </summary>
		Guid DnsNamespace { get; }

		/// <summary>
		/// The namespace for URLs (from RFC 4122, Appendix C).
		/// </summary>
		Guid UrlNamespace { get; }

		/// <summary>
		/// The namespace for ISO OIDs (from RFC 4122, Appendix C).
		/// </summary>
		Guid IsoOidNamespace { get; }
	}
}