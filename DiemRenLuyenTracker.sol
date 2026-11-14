// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

/**
 * @title DiemRenLuyenTracker
 * @author Senior Dev - Hệ thống Quản lý Điểm Rèn Luyện Sinh Viên
 * @notice Smart Contract lưu trữ Mã Hash điểm rèn luyện sinh viên
 *         trên Sepolia Testnet. Chỉ Admin ví (địa chỉ deploy) mới
 *         được phép ghi. Dữ liệu chi tiết lưu off-chain (SQL Server).
 * @dev Deploy qua Remix IDE tại https://remix.ethereum.org
 *      Network: Sepolia Testnet (ChainId: 11155111)
 *      Compiler: Solidity 0.8.20, Optimization: 200 runs
 */
contract DiemRenLuyenTracker {

    // ================================================================
    // STATE VARIABLES
    // ================================================================

    /// @notice Địa chỉ ví Admin - chỉ được thiết lập 1 lần lúc deploy
    address public immutable admin;

    /// @notice Tên hợp đồng để nhận diện
    string public constant CONTRACT_NAME = "DiemRenLuyenTracker";

    /// @notice Phiên bản hợp đồng
    string public constant VERSION = "1.0.0";

    // ================================================================
    // STRUCTS
    // ================================================================

    /**
     * @notice Lưu thông tin mã hash đã chốt cho từng sinh viên/học kỳ
     * @param mssv       Mã số sinh viên (off-chain key)
     * @param hocKy      Học kỳ, VD: "HK1_2024-2025"
     * @param dataHash   SHA-256 hash (64 ký tự hex) của: MSSV+HocKy+TongDiem+TenFile
     * @param timestamp  Thời điểm Unix (block.timestamp) lúc ghi
     * @param adminAddr  Địa chỉ ví Admin đã ký giao dịch ghi
     */
    struct PhieuChot {
        string   mssv;
        string   hocKy;
        string   dataHash;
        uint256  timestamp;
        address  adminAddr;
    }

    // ================================================================
    // MAPPINGS & STORAGE
    // ================================================================

    /**
     * @dev Lưu trữ theo key = keccak256(mssv, hocKy)
     *      Mỗi cặp MSSV+HocKy chỉ có 1 record (immutable sau khi chốt)
     */
    mapping(bytes32 => PhieuChot) private _phieuChot;

    /**
     * @dev Danh sách tất cả key đã được lưu (để hỗ trợ enumerate nếu cần)
     */
    bytes32[] private _danhSachKey;

    /**
     * @dev Kiểm tra key đã tồn tại chưa
     */
    mapping(bytes32 => bool) private _keyExists;

    // ================================================================
    // EVENTS
    // ================================================================

    /**
     * @notice Phát ra khi Admin chốt điểm thành công
     */
    event DaLuuMaHash(
        string  indexed mssv,
        string          hocKy,
        string          dataHash,
        uint256         timestamp,
        address indexed adminAddr
    );

    /**
     * @notice Phát ra khi truy vấn xác minh được thực hiện
     */
    event DaXacMinh(
        string  indexed mssv,
        string          hocKy,
        bool            ketQua,
        uint256         timestamp
    );

    // ================================================================
    // ERRORS (Solidity 0.8.4+ custom errors - tiết kiệm gas hơn revert string)
    // ================================================================

    error KhongPhai_Admin(address caller, address admin_);
    error DaChot_KhongTheGhiLai(string mssv, string hocKy);
    error ThongTin_KhongHopLe(string message);
    error KhongTimThay_PhieuChot(string mssv, string hocKy);

    // ================================================================
    // MODIFIERS
    // ================================================================

    /**
     * @dev Chỉ cho phép địa chỉ ví Admin (đã deploy contract) thực hiện
     */
    modifier onlyAdmin() {
        if (msg.sender != admin) {
            revert KhongPhai_Admin(msg.sender, admin);
        }
        _;
    }

    // ================================================================
    // CONSTRUCTOR
    // ================================================================

    /**
     * @notice Deploy contract - địa chỉ ví deploy tự động trở thành Admin
     * @dev Sau khi deploy, copy address của contract vào appsettings.json
     */
    constructor() {
        admin = msg.sender;
    }

    // ================================================================
    // WRITE FUNCTIONS (Chỉ Admin)
    // ================================================================

    /**
     * @notice Lưu mã Hash điểm rèn luyện đã chốt lên Blockchain
     * @dev Chỉ Admin mới được gọi. Mỗi cặp MSSV+HocKy chỉ ghi được 1 lần.
     *      Backend C# tính SHA-256 rồi gọi hàm này qua Ethers.js + MetaMask.
     * @param _mssv     Mã số sinh viên
     * @param _hocKy    Học kỳ (VD: "HK1_2024-2025")
     * @param _dataHash SHA-256 hash 64 ký tự hex (không có prefix 0x)
     */
    function luuMaHash(
        string calldata _mssv,
        string calldata _hocKy,
        string calldata _dataHash
    )
        external
        onlyAdmin
    {
        // Validate input
        if (bytes(_mssv).length == 0)     revert ThongTin_KhongHopLe("MSSV khong duoc rong");
        if (bytes(_hocKy).length == 0)    revert ThongTin_KhongHopLe("HocKy khong duoc rong");
        if (bytes(_dataHash).length != 64) revert ThongTin_KhongHopLe("DataHash phai la 64 ky tu SHA-256 hex");

        bytes32 key = _taoKey(_mssv, _hocKy);

        // Không cho ghi đè (bảo đảm tính bất biến)
        if (_keyExists[key]) {
            revert DaChot_KhongTheGhiLai(_mssv, _hocKy);
        }

        // Lưu dữ liệu
        _phieuChot[key] = PhieuChot({
            mssv:      _mssv,
            hocKy:     _hocKy,
            dataHash:  _dataHash,
            timestamp: block.timestamp,
            adminAddr: msg.sender
        });

        _keyExists[key] = true;
        _danhSachKey.push(key);

        emit DaLuuMaHash(_mssv, _hocKy, _dataHash, block.timestamp, msg.sender);
    }

    // ================================================================
    // READ FUNCTIONS (Public - bất kỳ ai cũng xem được)
    // ================================================================

    /**
     * @notice Lấy mã Hash đã lưu của 1 sinh viên trong 1 học kỳ
     * @dev Dùng để đối soát với dữ liệu SQL Server hiện tại
     * @param _mssv  Mã số sinh viên
     * @param _hocKy Học kỳ
     * @return dataHash  Mã SHA-256 đã lưu (64 ký tự)
     * @return timestamp Thời điểm Unix lúc chốt
     * @return adminAddr Địa chỉ ví Admin đã ký
     */
    function layMaHash(
        string calldata _mssv,
        string calldata _hocKy
    )
        external
        view
        returns (
            string  memory dataHash,
            uint256        timestamp,
            address        adminAddr
        )
    {
        bytes32 key = _taoKey(_mssv, _hocKy);

        if (!_keyExists[key]) {
            revert KhongTimThay_PhieuChot(_mssv, _hocKy);
        }

        PhieuChot storage p = _phieuChot[key];
        return (p.dataHash, p.timestamp, p.adminAddr);
    }

    /**
     * @notice Lấy toàn bộ thông tin PhieuChot
     * @param _mssv  Mã số sinh viên
     * @param _hocKy Học kỳ
     * @return Struct PhieuChot đầy đủ
     */
    function layThongTinDayDu(
        string calldata _mssv,
        string calldata _hocKy
    )
        external
        view
        returns (PhieuChot memory)
    {
        bytes32 key = _taoKey(_mssv, _hocKy);
        if (!_keyExists[key]) {
            revert KhongTimThay_PhieuChot(_mssv, _hocKy);
        }
        return _phieuChot[key];
    }

    /**
     * @notice Kiểm tra xem cặp MSSV+HocKy đã được chốt hay chưa
     * @param _mssv  Mã số sinh viên
     * @param _hocKy Học kỳ
     * @return true nếu đã chốt
     */
    function daChotDiem(
        string calldata _mssv,
        string calldata _hocKy
    )
        external
        view
        returns (bool)
    {
        return _keyExists[_taoKey(_mssv, _hocKy)];
    }

    /**
     * @notice Xác minh tính toàn vẹn - so sánh hash mới với hash đã lưu
     * @dev Backend C# băm lại dữ liệu SQL hiện tại, frontend gọi hàm này
     * @param _mssv         Mã số sinh viên
     * @param _hocKy        Học kỳ
     * @param _hashCanSoSanh Hash SHA-256 mới tính từ SQL Server hiện tại
     * @return trungKhop    true nếu dữ liệu còn toàn vẹn
     * @return hashTrenChain Hash đang lưu trên blockchain
     */
    function xacMinhToanVen(
        string calldata _mssv,
        string calldata _hocKy,
        string calldata _hashCanSoSanh
    )
        external
        view
        returns (bool trungKhop, string memory hashTrenChain)
    {
        bytes32 key = _taoKey(_mssv, _hocKy);
        if (!_keyExists[key]) {
            revert KhongTimThay_PhieuChot(_mssv, _hocKy);
        }

        hashTrenChain = _phieuChot[key].dataHash;
        trungKhop = (
            keccak256(bytes(hashTrenChain)) ==
            keccak256(bytes(_hashCanSoSanh))
        );
    }

    /**
     * @notice Lấy tổng số phiếu đã được chốt
     * @return Số lượng phiếu chốt
     */
    function tongSoPhieuChot() external view returns (uint256) {
        return _danhSachKey.length;
    }

    // ================================================================
    // INTERNAL / PRIVATE
    // ================================================================

    /**
     * @dev Tạo key bytes32 duy nhất từ MSSV + HocKy
     */
    function _taoKey(
        string memory _mssv,
        string memory _hocKy
    )
        internal
        pure
        returns (bytes32)
    {
        return keccak256(abi.encodePacked(_mssv, "|", _hocKy));
    }
}
