using Nemeris.App.ViewModels;

namespace Nemeris.App.Views;

public partial class ProductListPage : ContentPage
{
    public ProductListPage(ProductListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // First display only; afterwards RefreshView / search drive reloads.
        if (BindingContext is ProductListViewModel { Products.Count: 0 } viewModel)
        {
            _ = viewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
