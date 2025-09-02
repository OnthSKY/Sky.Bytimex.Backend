using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Sky.Template.Backend.WebAPI.Controllers.Base;

public static class HateoasHelper
{
    public static class LinkRelations
    {
        public const string Self = "self";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Create = "create";
        public const string Next = "next";
        public const string Previous = "previous";
        public const string First = "first";
        public const string Last = "last";
    }

    public static List<Link> CreateSupplierLinks(IUrlHelper urlHelper, Guid supplierId, string version)
    {
        var links = new List<Link>
        {
            new Link
            {
                Rel = LinkRelations.Self,
                Href = urlHelper.Action("GetSupplierById", "Supplier", new { id = supplierId, version }, urlHelper.ActionContext.HttpContext.Request.Scheme)
            },
            new Link
            {
                Rel = LinkRelations.Update,
                Href = urlHelper.Action("UpdateSupplier", "Supplier", new { version }, urlHelper.ActionContext.HttpContext.Request.Scheme)
            },
            new Link
            {
                Rel = LinkRelations.Delete,
                Href = urlHelper.Action("DeleteSupplier", "Supplier", new { id = supplierId, version }, urlHelper.ActionContext.HttpContext.Request.Scheme)
            }
        };

        return links;
    }

    public static List<Link> CreateSupplierListLinks(IUrlHelper urlHelper, string version, int page, int pageSize, int totalPages)
    {
        var links = new List<Link>
        {
            new Link
            {
                Rel = LinkRelations.Self,
                Href = urlHelper.Action("GetFilteredPaginatedSuppliers", "Supplier", new { page, pageSize, version }, urlHelper.ActionContext.HttpContext.Request.Scheme)
            },
            new Link
            {
                Rel = LinkRelations.Create,
                Href = urlHelper.Action("CreateSupplier", "Supplier", new { version }, urlHelper.ActionContext.HttpContext.Request.Scheme)
            }
        };

        if (page > 1)
        {
            links.Add(new Link
            {
                Rel = LinkRelations.Previous,
                Href = urlHelper.Action("GetFilteredPaginatedSuppliers", "Supplier", new { page = page - 1, pageSize, version }, urlHelper.ActionContext.HttpContext.Request.Scheme)
            });
        }

        if (page < totalPages)
        {
            links.Add(new Link
            {
                Rel = LinkRelations.Next,
                Href = urlHelper.Action("GetFilteredPaginatedSuppliers", "Supplier", new { page = page + 1, pageSize, version }, urlHelper.ActionContext.HttpContext.Request.Scheme)
            });
        }

        if (totalPages > 1)
        {
            links.Add(new Link
            {
                Rel = LinkRelations.First,
                Href = urlHelper.Action("GetFilteredPaginatedSuppliers", "Supplier", new { page = 1, pageSize, version }, urlHelper.ActionContext.HttpContext.Request.Scheme)
            });

            links.Add(new Link
            {
                Rel = LinkRelations.Last,
                Href = urlHelper.Action("GetFilteredPaginatedSuppliers", "Supplier", new { page = totalPages, pageSize, version }, urlHelper.ActionContext.HttpContext.Request.Scheme)
            });
        }

        return links;
    }
}

public class Link
{
    public string Rel { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public string? Method { get; set; }
} 