using System;

namespace FluentCeVIOWrapper.Common;

[Serializable]
public static class ServerCommand
{
	public const string CallMethod = "CALL_METHOD";
    public const string GetProperty = "GET_PROPERTY";
    public const string SetProperty = "SET_PROPERTY";

    /// <summary>
	/// debug use only
	/// </summary>
    public const string Echo = "ECHO";
}
