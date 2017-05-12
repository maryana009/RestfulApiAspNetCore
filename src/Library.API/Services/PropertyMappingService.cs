using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Models;
using Library.API.Entities;

namespace Library.API.Services
{
    public class PropertyMappingService: IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _authorPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>() {"Id" }) },
                { "Genre", new PropertyMappingValue(new List<string>() {"Genre" }) },
                { "Age", new PropertyMappingValue(new List<string>() {"DateOfBirth" }, true) },
                { "Name", new PropertyMappingValue(new List<string>() {"FirstName", "LastName" }) }
            };

        //private IList<PropertyMapping<TSource, TDestination>> propertyMappings;
        private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService(){
            propertyMappings.Add(new PropertyMapping<AuthorDto, Author>(_authorPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)}, {typeof(TDestination)}>");
        }

        public bool ValidateMappingPropertyExistsFor<TSourse, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSourse, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            var fieldsAfterSplit = fields.Split(',');

            //apply each orderby clause in reverse order, otherwise, the
            //IQueryable will be ordered in wrong direction
            foreach (var field in fieldsAfterSplit.Reverse())
            {
                var trimmedField = field.Trim();

                var indexOfFirstSpace = trimmedField.IndexOf(" ");

                var propertyName = indexOfFirstSpace == -1 ? trimmedField : trimmedField.Remove(indexOfFirstSpace);

                //find the matching property
                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
