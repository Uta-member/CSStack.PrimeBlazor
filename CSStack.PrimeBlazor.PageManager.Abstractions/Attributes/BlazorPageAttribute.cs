namespace CSStack.PrimeBlazor.PageManager
{
    using System;

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class BlazorPageAttribute<T> : Attribute
    {
        public BlazorPageAttribute()
        {
        }
    }
}