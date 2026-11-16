// site.js - Global JS utilities for DiemRenLuyen

// Auto-hide alerts after 5 seconds
document.addEventListener('DOMContentLoaded', () => {
    document.querySelectorAll('.alert.alert-dismissible').forEach(el => {
        setTimeout(() => {
            const bs = bootstrap.Alert.getOrCreateInstance(el);
            bs?.close();
        }, 5000);
    });

    // Hiển thị địa chỉ ví trong navbar nếu MetaMask đã kết nối
    if (typeof window.ethereum !== 'undefined') {
        window.ethereum.request({ method: 'eth_accounts' }).then(accounts => {
            if (accounts.length > 0) {
                const addr = accounts[0];
                const el   = document.getElementById('walletAddr');
                if (el) el.textContent = addr.substring(0,6) + '...' + addr.substring(38);
                const badge = document.getElementById('walletStatus');
                if (badge) badge.className = 'badge bg-success';
            }
        }).catch(() => {});
    }
});

// Utility: Format số tiền
function formatDiem(d) { return parseFloat(d || 0).toFixed(2); }

// Utility: Copy to clipboard
function copyText(text) {
    navigator.clipboard.writeText(text).then(() => {
        alert('Đã sao chép: ' + text);
    });
}
