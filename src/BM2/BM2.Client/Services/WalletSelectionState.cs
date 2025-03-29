using BM2.Shared.DTOs;

namespace BM2.Client.Services;

public interface IWalletSelectionState
{
    event Func<Task>? OnWalletChanged;
    Task SetWallet(WalletDTO? walletDto);
    void SetWallets(List<WalletDTO> walletDtos, WalletDTO? selectedWallet = null);
    WalletDTO? SelectedWallet { get; }
    List<WalletDTO> Wallets { get; }
}

public class WalletSelectionState : IWalletSelectionState
{
    public event Func<Task>? OnWalletChanged;

    private WalletDTO? _selectedWallet;

    public WalletDTO? SelectedWallet
    {
        get => _selectedWallet;
        private set
        {
            if (Equals(_selectedWallet, value)) return;
            _selectedWallet = value;
            _ = NotifyWalletChangedAsync();
        }
    }

    public List<WalletDTO> Wallets { get; private set; } = [];

    public async Task SetWallet(WalletDTO? walletDto)
    {
        SelectedWallet = walletDto;
        await NotifyWalletChangedAsync();
    }

    public void SetWallets(List<WalletDTO> walletDtos, WalletDTO? selectedWallet = null)
    {
        Wallets = walletDtos ?? [];

        _ = SetWallet(selectedWallet ?? Wallets.FirstOrDefault());
    }

    private async Task NotifyWalletChangedAsync()
    {
        if (OnWalletChanged != null)
        {
            await OnWalletChanged.Invoke();
        }
    }
}