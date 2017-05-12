using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Library.API.Services;
using System.Linq.Dynamic.Core;

namespace Library.API.Helpers
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> source, string orderBy, 
            Dictionary<string, PropertyMappingValue> mappingDictionary)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (mappingDictionary == null)
            {
                throw new ArgumentNullException("mappingDictionary");
            }
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                return source;
            }
            var orderByAfterSplit = orderBy.Split(',');

            //apply each orderby clause in reverse order, otherwise, the
            //IQueryable will be ordered in wrong direction
            foreach (var orderbyClause in orderByAfterSplit.Reverse())
            {
                var orderByClauseTrimmed = orderbyClause.Trim();

                var orderDescending = orderByClauseTrimmed.EndsWith(" desc");

                var indexOfFirstSpace = orderByClauseTrimmed.IndexOf(" ");

                //remove "asc" or "desc" in orderByClauseTrimmed, so
                //we get the property name to look for in the mapping dictionary
                var propertyName = indexOfFirstSpace == -1 ? orderByClauseTrimmed : orderByClauseTrimmed.Remove(indexOfFirstSpace);

                //find the matching property
                if (!mappingDictionary.ContainsKey(propertyName))
                {
                    throw new ArgumentException($"Key mapping for {propertyName} is missing");
                }

                var propertyMappingValue = mappingDictionary[propertyName];
                if (propertyMappingValue == null)
                {
                    throw new ArgumentNullException("propertyMappingValue");
                }

                foreach(var destinationProperty in propertyMappingValue.DestinationProperties.Reverse())
                {
                    if (propertyMappingValue.Revert)
                    {
                        orderDescending = !orderDescending;
                    }
                    source = source.OrderBy(destinationProperty + (orderDescending ? " descending" : " ascending"));
                }
            }
            return source;
        }
    }
}
