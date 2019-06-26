using System;

namespace MagmaConverse.Persistence
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
	public class IndexAttribute : Attribute
	{
		public bool Descending { get; set; }
	}
}
