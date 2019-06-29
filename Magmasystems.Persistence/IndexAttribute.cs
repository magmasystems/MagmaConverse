using System;

namespace Magmasystems.Persistence
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class IndexAttribute : Attribute
	{
		public bool Descending { get; set; }
	}
}
