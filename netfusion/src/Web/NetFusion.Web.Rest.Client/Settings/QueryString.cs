using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NetFusion.Web.Rest.Client.Settings;

/// <summary>
/// Used to specify query string parameters used when making a request.
/// </summary>
public class QueryString
{
	/// <summary>
	/// The collection of parameters.
	/// </summary>
	public IReadOnlyDictionary<string, string> Params { get; } 

	private readonly IDictionary<string, string> _params;

	public QueryString()
	{
		_params = new Dictionary<string, string>();
		Params = new ReadOnlyDictionary<string, string>(_params);
	}

	/// <summary>
	/// Adds a query string parameter name and value to be added to query string.
	/// </summary>
	/// <returns>Reference to query string instance.</returns>
	/// <param name="name">Name of the query string parameter.</param>
	/// <param name="value">Value of the query string parameter.</param>
	public QueryString AddParam(string name, string value)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("Query string parameter name not specified.", nameof(name));

		if (string.IsNullOrWhiteSpace(value))
			throw new ArgumentException("Query string parameter value not specified.", nameof(value));

		_params[name] = value;
		return this;
	}

	internal QueryString GetMerged(QueryString queryString)
	{
		var mergedQuery = new QueryString();

		foreach (var (key, value) in queryString.Params)
		{
			mergedQuery._params[key] = value;
		}
            
		// If the settings that are being merged into has a corresponding
		// query string value, it has precedence.
		foreach (var (key, value) in Params)
		{
			mergedQuery._params[key] = value;
		}

		return mergedQuery;
	}
}