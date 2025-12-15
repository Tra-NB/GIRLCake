<template>
  <div class="dashboard-container">
    <header class="page-header">
      <div class="header-content">
        <h1 class="page-title">
          <i class="bi bi-people-fill"></i> Quản lý người dùng
        </h1>
        <p class="sub-title">Danh sách tài khoản và phân quyền hệ thống</p>
      </div>

      <div class="header-actions">
        <div class="search-box">
          <i class="bi bi-search"></i>
          <input v-model="searchTerm" type="text" placeholder="Tìm tên, email..." />
        </div>
        <button class="btn-primary" @click="openModal()">
          <i class="bi bi-person-plus-fill"></i> Thêm Admin
        </button>
      </div>
    </header>

    <div class="content-card">
      <div class="table-responsive">
        <table class="modern-table">
          <thead>
            <tr>
              <th>Người dùng</th>
              <th>Liên hệ</th>
              <th>Địa chỉ</th>
              <th>Vai trò</th>
              <th>Ngày tham gia</th>
              <th>Trạng thái</th>
              <th class="text-right">Hành động</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in filteredUsers" :key="user.id" :class="getRowClass(user)">
              <td>
                <div class="user-profile">
                  <div class="user-info">
                    <span class="user-name">
                      {{ user.fullName }}
                      <small v-if="user.status === STATUS.INACTIVE" class="text-warning">(Tạm ngưng)</small>
                      <small v-else-if="user.status === STATUS.BANNED" class="text-danger">(Đã cấm)</small>
                    </span>
                    <span class="user-id">ID: {{ user.id }}</span>
                  </div>
                </div>
              </td>
              <td>
                <div class="contact-info">
                  <div class="c-row"><i class="bi bi-envelope"></i> {{ user.email }}</div>
                  <div class="c-row" v-if="user.phone || user.phoneNumber">
                    <i class="bi bi-telephone"></i> {{ user.phone || user.phoneNumber }}
                  </div>
                </div>
              </td>
              <td class="text-muted">{{ user.address || 'Chưa cập nhật' }}</td>
              <td>
                <span :class="['role-badge', getRoleClass(user.role)]">
                  {{ user.role || 'User' }}
                </span>
              </td>
              <td>{{ formatDate(user.createdAt) }}</td>
              <td>
                <span :class="['status-dot', getStatusDotClass(user.status)]">
                  {{ getStatusLabel(user.status) }}
                </span>
              </td>
              <td class="text-right">
                <button class="action-btn btn-edit" @click="editUser(user)">
                  <i class="bi bi-pencil-square"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <transition name="fade">
      <div v-if="showModal" class="modal-backdrop" @click.self="closeModal">
        <div class="modal-panel">
          <div class="modal-header">
            <h3>{{ modalTitle }}</h3>
            <button class="btn-close" @click="closeModal"><i class="bi bi-x-lg"></i></button>
          </div>

          <div class="modal-body">
            <div class="split-layout">

              <div class="layout-left">
                <h4 class="section-title">Thông tin cá nhân</h4>
                <div class="form-group">
                  <label>Họ và tên</label>
                  <div class="input-icon">
                    <i class="bi bi-person"></i>
                    <input v-model="currentUser.fullName" placeholder="VD: Nguyễn Văn A" />
                  </div>
                </div>

                <div class="form-group">
                  <label>Email đăng nhập</label>
                  <div class="input-icon">
                    <i class="bi bi-envelope"></i>
                    <input type="email" v-model="currentUser.email" placeholder="example@gmail.com"  />
                  </div>
                </div>

                <div class="form-row">
                  <div class="form-group">
                    <label>Số điện thoại</label>
                    <input v-model="currentUser.phone" placeholder="Số điện thoại"
                                     maxlength="10"
                                     @input="currentUser.phone = currentUser.phone.replace(/\D/g,'')" />
                  </div>
                  <div class="form-group">
                    <label>Ngày tham gia</label>
                    <input type="date" :value="formatDateInput(currentUser.createdAt)" disabled style="background: #e9ecef;" />
                  </div>
                </div>

                <div class="form-group">
                  <label>Địa chỉ</label>
                  <input v-model="currentUser.address" placeholder="Nhập địa chỉ..." />
                </div>
                
                <div class="form-group" v-if="!currentUser.id">
                  <label>Mật khẩu</label>
                  <input type="password" v-model="currentUser.password" placeholder="Mật khẩu" />
                </div>
                
              </div>

              <div class="layout-right">
                <h4 class="section-title">Cài đặt tài khoản</h4>

                <div class="avatar-preview">
                  <div class="big-avatar">{{ getInitials(currentUser.fullName || 'U') }}</div>
                </div>

                <div class="form-group">
                  <label>Phân quyền (Role)</label>
                  <select v-model="currentUser.role" disabled style="background: #e9ecef; cursor: not-allowed;">
                    <option value="Admin">Admin (Quản trị)</option>
                    <option value="User">User (Khách hàng)</option>
                  </select>
                  <small class="hint-text">Cố định quyền Admin (Không thể thay đổi)</small>
                </div>

                <div class="form-group">
                  <label>Trạng thái Tài khoản (Status)</label>
                  <select v-model="currentUser.status" 
                          :disabled="currentUser.id === currentAdminId">
                    
                    <option :value="STATUS.ACTIVE">
                      {{ getStatusLabel(STATUS.ACTIVE) }} (Hoạt động)
                    </option>
                    <option :value="STATUS.INACTIVE">
                      {{ getStatusLabel(STATUS.INACTIVE) }} (Tạm ngưng/Xóa mềm)
                    </option>
                    <option :value="STATUS.BANNED">
                      {{ getStatusLabel(STATUS.BANNED) }} (Cấm/Vi phạm)
                    </option>
                    <option :value="STATUS.PENDING" v-if="currentUser.status === STATUS.PENDING">
                      {{ getStatusLabel(STATUS.PENDING) }} (Chờ xác thực)
                    </option>
                  </select>
                  <small class="hint-text">
                    <span v-if="currentUser.id === currentAdminId">Không thể sửa trạng thái của chính mình.</span>
                    <span v-else>Chọn trạng thái tài khoản. Lưu thay đổi sẽ áp dụng trạng thái mới.</span>
                  </small>
                </div>
                
                <div v-if="currentUser.status === STATUS.BANNED" class="alert-danger">
                    <i class="bi bi-exclamation-triangle-fill"></i> Tài khoản đã bị CẤM VĨNH VIỄN. Thay đổi trạng thái khác để khôi phục.
                </div>

              </div>

            </div>
          </div>

          <div class="modal-footer">
            <button class="btn-text" @click="closeModal">Hủy bỏ</button>
            <button class="btn-primary" @click="saveUser">
              <i class="bi bi-check2-circle"></i> Lưu người dùng
            </button>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
// 🔥 Cần đảm bảo file này tồn tại và đã cấu hình Axios với Base URL, Authorization header.
import api from "@/api"; 

// --- ENUM TRẠNG THÁI ---
const STATUS = {
  ACTIVE: 1, 
  INACTIVE: 2, 
  BANNED: 3, 
  PENDING: 4 
};

const users = ref([]);
const currentAdminId = ref(null);
const searchTerm = ref("");
const showModal = ref(false);
const modalTitle = ref("");

// Model mặc định
const defaultUser = {
  id: null,
  fullName: "",
  email: "",
  password: "",
  phone: "",
  address: "",
  role: "Admin",
  status: STATUS.ACTIVE, 
  createdAt: null,
};

const currentUser = ref({ ...defaultUser });

// --- UTILS ---
const getInitials = (name) => {
  if (!name) return "";
  const parts = name.trim().split(" ");
  if (parts.length === 1) return parts[0][0].toUpperCase();
  return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
};

const getRoleClass = (role) => {
  switch (role) {
    case "Admin": return "role-admin";
    case "Staff": return "role-editor";
    default: return "role-user";
  }
};

const formatDate = (date) => {
  if (!date) return "";
  const d = new Date(date);
  return d.toLocaleDateString("vi-VN");
};

const formatDateInput = (date) => {
  if (!date) return "";
  return new Date(date).toISOString().split("T")[0];
};

const getStatusLabel = (status) => {
    switch (status) {
        case STATUS.ACTIVE: return 'Hoạt động';
        case STATUS.INACTIVE: return 'Tạm ngưng';
        case STATUS.BANNED: return 'Cấm / Vi phạm';
        case STATUS.PENDING: return 'Chờ xác thực';
        default: return 'Không xác định';
    }
};

const getStatusDotClass = (status) => {
    switch (status) {
        case STATUS.ACTIVE: return 'active'; 
        case STATUS.INACTIVE: return 'warning'; 
        case STATUS.BANNED: return 'danger'; 
        case STATUS.PENDING: return 'secondary'; 
        default: return 'secondary';
    }
};

const getRowClass = (user) => {
    if (user.status === STATUS.BANNED) return 'row-banned';
    if (user.status === STATUS.INACTIVE) return 'row-inactive';
    return '';
};

// --- API ACTIONS ---

const fetchProfile = async () => {
  try {
      // Gọi API để lấy ID của Admin đang đăng nhập
      const res = await api.get("/Users/me");
      currentAdminId.value = res.data.id;
  } catch (e) { console.error("Lỗi lấy thông tin Admin:", e) }
};

const fetchUsers = async () => {
  try {
      const res = await api.get("/Users");
      // Quan trọng: Đảm bảo status luôn có giá trị (fallback về ACTIVE)
      users.value = res.data.map(u => ({ ...u, status: u.status || STATUS.ACTIVE }));
  } catch (e) { console.error("Lỗi lấy danh sách User:", e) }
};


// --- LOGIC LƯU (Bao gồm việc lưu trạng thái mới từ Modal) ---
const saveUser = async () => {
  if (!currentUser.value.fullName || !currentUser.value.email) {
    alert("Thiếu thông tin bắt buộc (Họ tên, Email)");
    return;
  }

  if (!currentUser.value.id && !currentUser.value.password) {
    alert("Cần có Mật khẩu khi tạo người dùng mới");
    return;
  }

  // Bắt lỗi nếu Admin tự khóa tài khoản của mình qua Modal
  if (currentUser.value.id === currentAdminId.value && currentUser.value.status !== STATUS.ACTIVE) {
    alert("Bạn không thể tự khóa/cấm tài khoản đang hoạt động của mình.");
    // Đặt lại status về trạng thái cũ để tránh gửi request sai
    const oldStatus = users.value.find(u => u.id === currentAdminId.value)?.status || STATUS.ACTIVE;
    currentUser.value.status = oldStatus;
    return;
  }
  
  const payload = {
    FullName: currentUser.value.fullName,
    Email: currentUser.value.email,
    PhoneNumber: currentUser.value.phone,
    Address: currentUser.value.address,
    Role: currentUser.value.role,
    Status: currentUser.value.status // Lấy trạng thái mới từ select trong Modal
  };

  try {
    if (currentUser.value.id) {
      // PATCH update user
      await api.patch(`/Users/${currentUser.value.id}`, payload);
      alert("Cập nhật thành công");
    } else {
      // Create new Admin
      await api.post("/Users", {
        ...payload,
        Password: currentUser.value.password,
        Status: STATUS.ACTIVE
      });
      alert("Tạo Admin thành công");
    }
    closeModal();
    await fetchUsers(); // Cập nhật lại danh sách
  } catch (err) {
    console.error(err.response?.data || err);
    const msg = err.response?.data?.title || "Lỗi không xác định";
    alert("Thao tác thất bại: " + msg);
  }
};

// --- MODAL CONTROLS ---

const openModal = () => {
  modalTitle.value = "Thêm Admin Mới";
  currentUser.value = { ...defaultUser, createdAt: new Date().toISOString() }; 
  showModal.value = true;
};

const editUser = (user) => {
  modalTitle.value = "Cập nhật tài khoản";
  currentUser.value = {
    id: user.id,
    fullName: user.fullName,
    email: user.email,
    phone: user.phoneNumber || user.phone || "",
    address: user.address,
    role: user.role,
    status: user.status || STATUS.ACTIVE, // Lấy trạng thái hiện tại
    createdAt: user.createdAt
  };
  showModal.value = true;
};

const closeModal = () => {
  showModal.value = false;
};

const filteredUsers = computed(() => {
  if (!searchTerm.value) return users.value;
  const key = searchTerm.value.toLowerCase();
  return users.value.filter(
    u => u.fullName?.toLowerCase().includes(key) || u.email?.toLowerCase().includes(key)
  );
});

onMounted(async () => {
  await fetchProfile();
  await fetchUsers();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@500;600;700&display=swap');

/* --- CẬP NHẬT CSS TRẠNG THÁI MỚI VÀ HÀNH ĐỘNG --- */
.status-dot { display: inline-flex; align-items: center; gap: 6px; font-weight: 600; font-size: 13px; }
.status-dot::before { content: ""; width: 8px; height: 8px; border-radius: 50%; background: currentColor; }
.status-dot.active { color: #2e7d32; } 
.status-dot.warning { color: #ef6c00; } 
.status-dot.danger { color: #c62828; } 
.status-dot.secondary { color: #6c757d; } 

.row-banned { background-color: #fce4ec; opacity: 0.8; } 
.row-inactive { background-color: #fff3e0; }

.alert-danger {
    padding: 10px; border: 1px solid #c62828; background: #ffebee; color: #c62828;
    border-radius: 8px; font-size: 13px; margin-top: 10px; display: flex; align-items: center; gap: 8px;
}

/* --- HÀNH ĐỘNG (Chỉ còn nút Edit ở ngoài bảng) --- */
.action-btn { width: 32px; height: 32px; border: none; background: transparent; color: #ccc; cursor: pointer; border-radius: 6px; transition: 0.2s;}
.action-btn:disabled { opacity: 0.5; cursor: not-allowed; }
.btn-edit { color: #d63384; }
.btn-edit:hover { background: #fff0f6; color: #d63384; }

/* --- LAYOUT CHUNG (Giữ nguyên) --- */
.dashboard-container { padding: 20px; background-color: #fff5f7; min-height: 10vh; font-family: 'Quicksand', sans-serif; color: #444; }
.page-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; flex-wrap: wrap; gap: 15px; }
.page-title { font-size: 24px; color: #d63384; margin: 0; font-weight: 700; }
.sub-title { font-size: 13px; color: #888; margin: 4px 0 0 0; }
.header-actions { display: flex; gap: 12px; }
.search-box { position: relative; }
.search-box input { padding: 10px 10px 10px 36px; border: 1px solid #f8bbd0; border-radius: 8px; outline: none; color: #d63384; width: 220px; }
.search-box i { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #d63384; }
.btn-primary { background: linear-gradient(135deg, #d63384, #ff4081); color: white; border: none; padding: 10px 20px; border-radius: 8px; font-weight: 600; cursor: pointer; display: flex; align-items: center; gap: 8px; box-shadow: 0 4px 10px rgba(214, 51, 132, 0.3); transition: 0.2s; }
.btn-primary:hover { transform: translateY(-2px); box-shadow: 0 6px 15px rgba(214, 51, 132, 0.4); }
.content-card { background: white; border-radius: 16px; box-shadow: 0 4px 20px rgba(0,0,0,0.03); border: 1px solid #ffeef4; overflow: hidden; }
.modern-table { width: 100%; border-collapse: collapse; font-size: 14px; }
.modern-table th { background: #fff0f6; color: #d63384; padding: 14px 20px; text-align: left; font-weight: 700; text-transform: uppercase; font-size: 12px; }
.modern-table td { padding: 14px 20px; border-bottom: 1px solid #fff5f7; vertical-align: middle; }
.modern-table tr:hover { background: #fffbfd; }
.user-profile { display: flex; align-items: center; gap: 12px; }
.user-info { display: flex; flex-direction: column; }
.user-name { font-weight: 700; color: #333; }
.user-id { font-size: 11px; color: #999; }
.contact-info .c-row { font-size: 13px; color: #666; display: flex; align-items: center; gap: 6px; margin-bottom: 2px;}
.contact-info i { color: #f48fb1; font-size: 12px; }
.role-badge { padding: 4px 10px; border-radius: 20px; font-size: 11px; font-weight: 700; text-transform: uppercase; }
.role-admin { background: #e3f2fd; color: #1976d2; }
.role-editor { background: #fff3e0; color: #f57c00; }
.role-user { background: #f3e5f5; color: #7b1fa2; }
.text-right { text-align: right; }
.modal-backdrop { position: fixed; inset: 0; background: rgba(0,0,0,0.5); backdrop-filter: blur(3px); z-index: 9999; display: flex; justify-content: center; align-items: center; }
.modal-panel { background: white; width: 750px; max-width: 95%; max-height: 90vh; border-radius: 20px; display: flex; flex-direction: column; box-shadow: 0 20px 60px rgba(0,0,0,0.2); animation: slideUp 0.3s cubic-bezier(0.16, 1, 0.3, 1); }
@keyframes slideUp { from { transform: translateY(30px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
.modal-header { padding: 16px 24px; background: #fff0f6; border-bottom: 1px solid #fce4ec; display: flex; justify-content: space-between; align-items: center; border-radius: 20px 20px 0 0; }
.modal-header h3 { margin: 0; color: #d63384; font-size: 18px; font-weight: 700; }
.btn-close { border: none; background: none; font-size: 20px; color: #f48fb1; cursor: pointer; }
.modal-body { padding: 24px; overflow-y: auto; }
.split-layout { display: grid; grid-template-columns: 1.6fr 1fr; gap: 30px; }
.layout-left { border-right: 1px dashed #eee; padding-right: 30px; }
.layout-right { padding-top: 10px; }
.section-title { font-size: 14px; color: #888; text-transform: uppercase; letter-spacing: 1px; margin-bottom: 15px; border-bottom: 2px solid #fce4ec; display: inline-block; padding-bottom: 4px; }
.form-group { margin-bottom: 15px; }
.form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 15px; }
.form-group label { display: block; font-size: 13px; font-weight: 600; color: #555; margin-bottom: 6px; }
.form-group input, .form-group select { width: 100%; padding: 10px 12px; border: 1px solid #eee; border-radius: 8px; font-size: 14px; box-sizing: border-box; background: #fafafa; }
.form-group input:focus, .form-group select:focus { border-color: #d63384; background: white; outline: none; box-shadow: 0 0 0 3px rgba(214, 51, 132, 0.1); }
.input-icon { position: relative; }
.input-icon i { position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #f48fb1; }
.input-icon input { padding-left: 36px; }
.avatar-preview { display: flex; flex-direction: column; align-items: center; margin-bottom: 20px; }
.big-avatar { width: 80px; height: 80px; background: linear-gradient(135deg, #fce4ec, #f8bbd0); border-radius: 50%; color: #d63384; font-size: 30px; font-weight: bold; display: flex; align-items: center; justify-content: center; box-shadow: 0 5px 15px rgba(214, 51, 132, 0.15); border: 2px solid white; }
.hint-text { font-size: 11px; color: #999; font-style: italic; }
.modal-footer { padding: 16px 24px; border-top: 1px solid #fce4ec; display: flex; justify-content: flex-end; gap: 10px; }
.btn-text { background: none; border: none; color: #777; font-weight: 600; cursor: pointer; padding: 10px 15px;}
.btn-text:hover { color: #333; background: #f5f5f5; border-radius: 8px;}
@media (max-width: 700px) {
  .split-layout { grid-template-columns: 1fr; gap: 0; }
  .layout-left { border-right: none; padding-right: 0; padding-bottom: 20px; border-bottom: 1px solid #eee; margin-bottom: 20px; }
}
</style>