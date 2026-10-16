using HouseholdPlanner.Web.ApiClients;
using HouseholdPlanner.Web.Models;
using HouseholdPlanner.Web.Services;
using HouseholdPlanner.Web.Theme;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using MudBlazor;

namespace HouseholdPlanner.Web.Components;

/// <summary>
/// Base for pages and parts that show the <see cref="HomeStore"/>: re-renders when it changes, and turns
/// failed API calls into an error toast instead of an unhandled error.
/// </summary>
public abstract class StoreComponent : ComponentBase, IDisposable
{
    [Inject] protected HomeStore Store { get; set; } = default!;
    [Inject] protected ThemeState ThemeState { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;
    [Inject] protected IStringLocalizer<Strings> T { get; set; } = default!;
    [Inject] protected DateText Dates { get; set; } = default!;

    protected static DateOnly Today => DueDates.Today;

    protected override void OnInitialized()
    {
        Store.Changed += OnChanged;
        ThemeState.Changed += OnChanged;
    }

    private void OnChanged() => InvokeAsync(StateHasChanged);

    /// <summary>Runs the action; on failure shows why and reloads, since the list may be out of date.</summary>
    protected async Task<bool> TryAsync(Func<Task> action)
    {
        try
        {
            await action();
            return true;
        }
        catch (ApiException ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);
            try { await Store.RefreshAsync(); } catch (ApiException) { /* The error is already shown. */ }
            return false;
        }
    }

    /// <summary>A toast for a change, with an Undo button that runs <paramref name="undo"/>. It replaces the previous toast.</summary>
    protected void ShowUndo(string message, Func<Task> undo)
    {
        Snackbar.Clear();
        Snackbar.Add(message, Severity.Normal, options =>
        {
            // A toast with an action stays until clicked unless told otherwise.
            options.RequireInteraction = false;
            options.Action             = T["Common_Undo"];
            options.ActionColor        = Color.Primary;
            options.ActionVariant      = Variant.Text;
            options.OnClick            = _ => TryAsync(undo);
        });
    }

    /// <summary>
    /// Adds a grocery, with an Undo toast. When the same item is still to buy, its quantity goes up
    /// instead ("Oat milk ×3"), and Undo puts it back.
    /// </summary>
    protected Task AddGroceryAsync(SaveGroceryRequest request) => TryAsync(async () =>
    {
        var added = await Store.AddGroceryAsync(request);
        if (added.Merged)
        {
            var before = added.Item.ToRequest();
            before.Quantity = added.PreviousQuantity;
            ShowUndo($"{added.Item.Name} ×{added.Item.Quantity}", () => Store.UpdateGroceryAsync(added.Item.Id, before));
        }
        else
        {
            ShowUndo(T["Common_Added", added.Item.Name], () => Store.DeleteGroceryAsync(added.Item.Id));
        }
    });

    public virtual void Dispose()
    {
        Store.Changed -= OnChanged;
        ThemeState.Changed -= OnChanged;
        GC.SuppressFinalize(this);
    }
}
