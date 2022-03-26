namespace SharpItch;

using System;

[AttributeUsage(AttributeTargets.Method)]
public class ScopeAttribute : Attribute
{
	public ScopeAttribute (string scope)
	{
		Scope = scope;
	}
	
	public string Scope;
}