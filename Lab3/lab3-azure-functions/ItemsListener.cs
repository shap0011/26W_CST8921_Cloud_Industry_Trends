using System;
using System.Collections.Generic;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Company.Function;

public class ItemsListener
{
    private readonly ILogger<ItemsListener> _logger;

    public ItemsListener(ILogger<ItemsListener> logger)
    {
        _logger = logger;
    }

    [Function("ItemsListener")]
    public void Run(
    [CosmosDBTrigger(
        databaseName: "cosmicworks",
        containerName: "products",
        Connection = "cst8921cosmoslab3shap0011_DOCUMENTDB",
        LeaseContainerName = "productslease",
        CreateLeaseContainerIfNotExists = false)]
    IReadOnlyList<MyDocument> input)
    {
        _logger.LogInformation($"# Modified Items:\t{input?.Count ?? 0}");

        if (input is null) return;

        foreach (var item in input)
        {
            _logger.LogInformation($"Detected Operation:\t{item.id ?? "<null id>"}");
        }
    }

}

public class MyDocument
{
    public string? id { get; set; }
    public string? name { get; set; }
    public string? categoryId { get; set; }
}

