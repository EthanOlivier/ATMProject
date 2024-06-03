namespace ATMProject.System
{
    public static class AttributeExtensions
    {
        public static bool HasAttribute<TAttributeType>(this Type type) where TAttributeType : Attribute
        {
            var attributes = type.GetCustomAttributes(typeof(TAttributeType), true);
            return attributes is not null && attributes.Any();
        }
    }
}
