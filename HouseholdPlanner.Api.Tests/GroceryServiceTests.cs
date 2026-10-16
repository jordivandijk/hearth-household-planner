using HouseholdPlanner.Api.Models.DTOs;
using HouseholdPlanner.Api.Models.Entities;
using HouseholdPlanner.Api.Models.Results;
using HouseholdPlanner.Api.Services.Implementations;
using static HouseholdPlanner.Api.Tests.TestData;

namespace HouseholdPlanner.Api.Tests;

public class GroceryServiceTests
{
    private readonly FakeGroceryRepository _groceries = new();
    private readonly GroceryService _service;

    public GroceryServiceTests() => _service = new GroceryService(_groceries, new FixedTime(Now));

    private GroceryItem Add(GroceryItem item)
    {
        _groceries.Items.Add(item);
        return item;
    }

    private static SaveGroceryRequest Request(string name, int quantity = 1, bool inCart = false)
        => new() { Name = name, Quantity = quantity, Category = GroceryCategory.DairyEggs, InCart = inCart };

    [Fact]
    public async Task Adding_a_name_that_is_still_to_buy_raises_its_quantity()
    {
        var milk = Add(Grocery("Oat milk", 2));

        var result = await _service.AddAsync(HouseholdId, Request(" oat MILK ", 1));

        Assert.True(result.Data.Merged);
        Assert.Equal(2, result.Data.PreviousQuantity);
        Assert.Equal(3, milk.Quantity);
        Assert.Single(_groceries.Items);
    }

    [Fact]
    public async Task Adding_a_name_that_is_in_the_cart_or_cleared_adds_a_new_line()
    {
        Add(Grocery("Eggs", inCart: true));
        Add(Grocery("Bread", clearedAt: Now.UtcDateTime));

        var eggs = await _service.AddAsync(HouseholdId, Request("Eggs"));
        var bread = await _service.AddAsync(HouseholdId, Request("Bread"));

        Assert.False(eggs.Data.Merged);
        Assert.False(bread.Data.Merged);
        Assert.Equal(4, _groceries.Items.Count);
    }

    [Fact]
    public async Task Another_households_item_is_never_merged_into()
    {
        var theirs = Add(Grocery("Eggs", householdId: OtherHouseholdId));

        var result = await _service.AddAsync(HouseholdId, Request("Eggs"));

        Assert.False(result.Data.Merged);
        Assert.Equal(1, theirs.Quantity);
    }

    [Fact]
    public async Task Merging_never_goes_over_the_maximum_quantity()
    {
        var rice = Add(Grocery("Rice", 998));

        await _service.AddAsync(HouseholdId, Request("Rice", 5));

        Assert.Equal(GroceryItem.MaxQuantity, rice.Quantity);
    }

    [Fact]
    public async Task Clearing_the_cart_takes_bought_items_off_the_list_and_into_recents()
    {
        var bananas = Add(Grocery("Bananas", inCart: true));
        Add(Grocery("Eggs"));

        var cleared = await _service.ClearCartAsync(HouseholdId);
        var list = await _service.GetListAsync(HouseholdId);

        Assert.Equal([bananas.Id], cleared.Data);
        Assert.Equal(["Eggs"], list.Data.Items.Select(i => i.Name));
        Assert.Equal(["Bananas"], list.Data.Recents);
    }

    [Fact]
    public async Task Clearing_the_cart_forgets_items_bought_more_than_90_days_ago()
    {
        Add(Grocery("Old", clearedAt: Now.UtcDateTime.AddDays(-91)));
        Add(Grocery("Recent", clearedAt: Now.UtcDateTime.AddDays(-89)));
        Add(Grocery("Theirs", clearedAt: Now.UtcDateTime.AddDays(-91), householdId: OtherHouseholdId));

        await _service.ClearCartAsync(HouseholdId);

        Assert.Equal(["Recent", "Theirs"], _groceries.Items.Select(i => i.Name).Order());
    }

    [Fact]
    public async Task Restoring_puts_cleared_items_back_in_the_cart()
    {
        var bananas = Add(Grocery("Bananas", inCart: true));
        var cleared = await _service.ClearCartAsync(HouseholdId);

        await _service.RestoreAsync(HouseholdId, new GroceryIdsRequest { Ids = cleared.Data });

        Assert.Null(bananas.ClearedAt);
        Assert.True(bananas.InCart);
    }

    [Fact]
    public async Task Another_households_items_cannot_be_restored()
    {
        var theirs = Add(Grocery("Bananas", inCart: true, clearedAt: Now.UtcDateTime, householdId: OtherHouseholdId));

        await _service.RestoreAsync(HouseholdId, new GroceryIdsRequest { Ids = [theirs.Id] });

        Assert.NotNull(theirs.ClearedAt);
    }

    [Fact]
    public async Task A_cleared_item_can_no_longer_be_edited_or_deleted()
    {
        var bread = Add(Grocery("Bread", clearedAt: Now.UtcDateTime));

        var update = await _service.UpdateAsync(HouseholdId, bread.Id, Request("Rye bread"));
        var delete = await _service.DeleteAsync(HouseholdId, bread.Id);

        Assert.Equal(ErrorCode.NotFound, update.ErrorCode);
        Assert.Equal(ErrorCode.NotFound, delete.ErrorCode);
        Assert.Equal("Bread", bread.Name);
    }

    [Fact]
    public async Task Updating_an_item_moves_it_in_and_out_of_the_cart()
    {
        var eggs = Add(Grocery("Eggs"));

        await _service.UpdateAsync(HouseholdId, eggs.Id, Request("Eggs", 6, inCart: true));

        Assert.True(eggs.InCart);
        Assert.Equal(6, eggs.Quantity);
    }
}
