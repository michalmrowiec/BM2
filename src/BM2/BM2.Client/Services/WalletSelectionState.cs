using BM2.Shared.DTOs;

namespace BM2.Client.Services;

public interface IWalletSelectionState
{
    void Subscribe(Action<WalletDTO> changeAction);
    void SetWallet(WalletDTO walletDto);
    void SetWallets(List<WalletDTO> walletDtos, WalletDTO? selectedWallet = null);
    WalletDTO? SelectedWallet { get; }
    List<WalletDTO> Wallets { get; }
}

public class WalletSelectionState : IWalletSelectionState
{
    private Action<WalletDTO>? _onChange;
    public WalletDTO? SelectedWallet { get; private set; }
    public List<WalletDTO> Wallets { get; private set; } = [];

    public void Subscribe(Action<WalletDTO> changeAction)
    {
        _onChange = changeAction;
    }

    public void SetWallet(WalletDTO? walletDto)
    {
        if (walletDto is null)
            return;

        SelectedWallet = walletDto;
        _onChange?.Invoke(walletDto);
    }

    public void SetWallets(List<WalletDTO> walletDtos, WalletDTO? selectedWallet = null)
    {
        Wallets = walletDtos;
        SetWallet(selectedWallet ?? Wallets.FirstOrDefault());
    }
}