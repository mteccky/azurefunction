#r "Newtonsoft.Json"

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
//using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

public static async Task<HttpResponseMessage> Run(HttpRequest req, ILogger log)
{    
   
    log.LogInformation("Sort HTTP trigger function processed a request.");

    var sortedProducts = new List<Product>();

    string sortOption = req.Query["sortOption"];

    var productList = GetProductList(log).Result;

    switch (sortOption.ToLower())
    {
        case "low":
            sortedProducts = productList.SortProductsByLow();
            break;
        case "high":
            sortedProducts = productList.SortProductsByHigh();
            break;
        case "ascending":
            sortedProducts = productList.SortProductsByAscending();
            break;
        case "descending":
            sortedProducts = productList.SortProductsByDescending();
            break;
        case "recommended":
            sortedProducts = await SortProductsByPopularity(productList, log);
            break;
    }

    var response = new HttpResponseMessage()
    {
        Content = new StringContent(JsonConvert.SerializeObject(sortedProducts), System.Text.Encoding.UTF8, "application/json"),
        StatusCode = HttpStatusCode.OK,
    };

    return response;

} 

static async Task<List<Product>> SortProductsByPopularity(ProductList productList, ILogger log)
{
    string shopperHistoryItemsJson = await GetShopperHistory(productList);
    log.LogInformation(shopperHistoryItemsJson);

    List<ShopperHistoryItem> shopperHistoryItems = JsonConvert.DeserializeObject<List<ShopperHistoryItem>>(shopperHistoryItemsJson);
    return productList.SortProductsByPopularity(shopperHistoryItems);
}

static async Task<ProductList> GetProductList(ILogger log)
{
    string productsJson = await GetProductsAsync();
    log.LogInformation(productsJson);
    var products = JsonConvert.DeserializeObject<List<Product>>(productsJson);

    ProductList productList = new ProductList();
    productList.products = products;

    return productList;
}

public static async Task<string> GetProductsAsync()
{
    using (var client = new HttpClient())
    {        
        var url = $"{Environment.GetEnvironmentVariable("ResourceApiBaseUrl")}products?token={Environment.GetEnvironmentVariable("ResourceApiToken")}";
        var httpResponse = await client.GetAsync(url);
        if (httpResponse.StatusCode == HttpStatusCode.OK)
        {
            return await httpResponse.Content.ReadAsStringAsync();
        }
    }
    return null;
}
 
public static async Task<string> GetShopperHistory(ProductList productList)
{
    using (var client = new HttpClient())
    {        
        var url = $"{Environment.GetEnvironmentVariable("ResourceApiBaseUrl")}shopperHistory?token={Environment.GetEnvironmentVariable("ResourceApiToken")}";
        var httpResponse = await client.GetAsync(url);
        if (httpResponse.StatusCode == HttpStatusCode.OK)
        {
            return await httpResponse.Content.ReadAsStringAsync();
        }
    }
    return null;
}

public class ProductList
{
    public List<Product> products;
    public List<Product> SortProductsByLow()
    {
        return products.OrderBy(p => p.price).ToList();
    }
    public List<Product> SortProductsByHigh()
    {
        return products.OrderByDescending(p => p.price).ToList();
    }

    public List<Product> SortProductsByAscending()
    {
        return products.OrderBy(p => p.name).ToList();
    }

    public List<Product> SortProductsByDescending()
    {
        return products.OrderByDescending(p => p.name).ToList();
    }

    public List<Product> SortProductsByPopularity(List<ShopperHistoryItem> shopperHistoryItems)
    {

        var productsWithTotalQty = shopperHistoryItems.SelectMany(p => p.products)
                                                            .GroupBy(p => p.name)
                                                            .Select(g => new ProductWithTotalQuantity
                                                            {
                                                                name = g.Key,
                                                                totalQuantity = g.Sum(x => x.quantity)
                                                            }).OrderByDescending(o => o.totalQuantity);



        var orderedProducts = this.products
                                                            .Select(c => new
                                                            {
                                                                c.name,
                                                                c.price,
                                                                c.quantity,
                                                                totalQuantity =
                                                                    productsWithTotalQty != null && productsWithTotalQty.Any(x => x.name == c.name) ?
                                                                        productsWithTotalQty.FirstOrDefault(x => x.name == c.name).totalQuantity
                                                                        : 0
                                                            })
                                                            .OrderByDescending(x => x.totalQuantity)
                                                            .Select(x => new Product()
                                                            {
                                                                name = x.name,
                                                                price = x.price,
                                                                quantity = x.quantity
                                                            }).ToList();

        return orderedProducts;
    }
}

public class ProductWithTotalQuantity : Product
{
    public double totalQuantity;
}

public class Product
{
    public string name { get; set; }
    public double price { get; set; }
    public double quantity { get; set; }
}

public class ShopperHistoryItem
{
    public int customerId { get; set; }
    public List<Product> products { get; set; }
}
