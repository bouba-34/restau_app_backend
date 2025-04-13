using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using backend.Api.Models.Responses;

namespace backend.Api.Helpers
{
    public static class PaginationHelper
    {
        public static PagedResponse<T> CreatePagedResponse<T>(
            List<T> pagedData, 
            int pageNumber, 
            int pageSize, 
            int totalRecords)
        {
            return new PagedResponse<T>(pagedData, pageNumber, pageSize, totalRecords);
        }
        
        public static IQueryable<T> ApplyPaging<T>(
            this IQueryable<T> query, 
            int pageNumber, 
            int pageSize)
        {
            if (pageNumber <= 0)
                pageNumber = 1;
                
            if (pageSize <= 0)
                pageSize = 10;
                
            return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
        }
        
        public static IQueryable<T> ApplySort<T>(
            this IQueryable<T> query, 
            string sortBy, 
            string sortDirection)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query;
                
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, sortBy);
            var lambda = Expression.Lambda(property, parameter);
            
            var method = sortDirection?.ToLower() == "desc" 
                ? "OrderByDescending" 
                : "OrderBy";
                
            var genericMethod = typeof(Queryable).GetMethods()
                .Where(m => m.Name == method && m.IsGenericMethodDefinition && m.GetParameters().Length == 2)
                .Single();
                
            var methodWithGenerics = genericMethod.MakeGenericMethod(typeof(T), property.Type);
            var result = methodWithGenerics.Invoke(null, new object[] { query, lambda });
            
            return (IQueryable<T>)result;
        }
        
        public static IQueryable<T> ApplyFilter<T>(
            this IQueryable<T> query, 
            Expression<Func<T, bool>> predicate)
        {
            return predicate != null ? query.Where(predicate) : query;
        }
    }
}