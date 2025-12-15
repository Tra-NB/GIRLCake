<template>
  <div class="account-page-wrapper">
    <div class="container py-5">
      <div class="row g-4">
        <!-- PROFILE CARD -->
        <div class="col-lg-4">
          <div class="profile-card">
            <div class="action-buttons">
              <button class="btn-action-float btn-key" @click="openPasswordModal" title="Đổi mật khẩu">
                <i class="bi bi-key-fill"></i>
              </button>
              <button class="btn-action-float btn-edit" @click="openEditModal" title="Chỉnh sửa thông tin">
                <i class="bi bi-pencil-fill"></i>
              </button>
            </div>

            <div class="profile-avatar">
              {{ fullName ? fullName.charAt(0).toUpperCase() : 'U' }}
            </div>

            <h2 class="profile-name">{{ fullName || 'Người dùng' }}</h2>
            <span class="profile-role badge-role">{{ role || 'Member' }}</span>

            <div class="profile-details">
              <div class="detail-item">
                <i class="bi bi-envelope"></i>
                <span>{{ email }}</span>
              </div>
              <div class="detail-item">
                <i class="bi bi-telephone"></i>
                <span>{{ phone || 'Chưa cập nhật SĐT' }}</span>
              </div>
              <div class="detail-item">
                <i class="bi bi-geo-alt"></i>
                <span>{{ address || 'Chưa cập nhật địa chỉ' }}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- DASHBOARD GRID -->
        <div class="col-lg-8">
          <div class="dashboard-grid">
            <router-link v-if="role === 'Admin'" to="/ManageFoods" class="dash-card">
              <div class="icon-box"><i class="bi bi-basket2-fill"></i></div>
              <h3>Quản lý món ăn</h3>
              <p>Thêm, sửa, xóa thực đơn</p>
            </router-link>

            <router-link v-if="role === 'Admin'" to="/ManageCombos" class="dash-card">
              <div class="icon-box"><i class="bi bi-collection-fill"></i></div>
              <h3>Quản lý Combo</h3>
              <p>Thiết lập các gói combo</p>
            </router-link>

            <router-link v-if="role === 'Admin'" to="/ManageVouchers" class="dash-card">
              <div class="icon-box"><i class="bi bi-ticket-perforated-fill"></i></div>
              <h3>Voucher</h3>
              <p>Mã giảm giá & KM</p>
            </router-link>

            <router-link v-if="role === 'Admin'" to="/ManageUsers" class="dash-card">
              <div class="icon-box"><i class="bi bi-people-fill"></i></div>
              <h3>Người dùng</h3>
              <p>Quản lý tài khoản</p>
            </router-link>

            <router-link to="/ManageOrders" class="dash-card">
              <div class="icon-box"><i class="bi bi-receipt-cutoff"></i></div>
              <h3>Hóa đơn</h3>
              <p>Lịch sử đơn hàng</p>
            </router-link>

            <router-link to="/ManageReviews" class="dash-card">
              <div class="icon-box"><i class="bi bi-chat-left-text-fill"></i></div>
              <h3>Đánh giá</h3>
              <p>Phản hồi của bạn</p>
            </router-link>
          </div>
        </div>
      </div>
    </div>

    <!-- EDIT PROFILE MODAL -->
    <div v-if="showModal" class="modal-overlay">
      <div class="modal-box animate-pop">
        <div class="modal-header">
          <h2 class="modal-title">Cập nhật hồ sơ</h2>
          <button class="btn-close-modal" @click="closeModal"><i class="bi bi-x-lg"></i></button>
        </div>
        <div class="form-body">
          <div class="form-group">
            <label>Họ và tên</label>
            <div class="input-with-icon">
              <i class="bi bi-person"></i>
              <input v-model="editUser.fullName" placeholder="Nhập họ tên của bạn" />
            </div>
          </div>

          <div class="form-group">
            <label>Email (Không thể thay đổi)</label>
            <div class="input-with-icon disabled">
              <i class="bi bi-envelope"></i>
              <input v-model="editUser.email" disabled />
            </div>
          </div>

          <div class="form-group">
            <label>Địa chỉ</label>
            <div class="input-with-icon">
              <i class="bi bi-geo-alt"></i>
              <input v-model="editUser.address" placeholder="Nhập địa chỉ giao hàng" />
            </div>
          </div>

          <div class="form-group">
            <label>Số điện thoại</label>
            <div class="input-with-icon">
              <i class="bi bi-phone"></i>
              <input v-model="editUser.phoneNumber" placeholder="Nhập số điện thoại" />
            </div>
          </div>
        </div>
        <div class="modal-actions">
          <button class="btn-cancel" @click="closeModal">Hủy bỏ</button>
          <button class="btn-save" @click="saveUser">Lưu thay đổi</button>
        </div>
      </div>
    </div>

    <!-- CHANGE PASSWORD MODAL -->
    <div v-if="showPasswordModal" class="modal-overlay">
      <div class="modal-box animate-pop">
        <div class="modal-header">
          <h2 class="modal-title">Đổi mật khẩu</h2>
          <button class="btn-close-modal" @click="closePasswordModal"><i class="bi bi-x-lg"></i></button>
        </div>
        <div class="form-body">
          <div class="form-group">
            <label>Mật khẩu hiện tại</label>
            <div class="input-with-icon">
              <i class="bi bi-shield-lock"></i>
              <input type="password" v-model="passwordForm.currentPassword" placeholder="Nhập mật khẩu cũ..." />
            </div>
          </div>
          <div class="form-group">
            <label>Mật khẩu mới</label>
            <div class="input-with-icon">
              <i class="bi bi-key"></i>
              <input type="password" v-model="passwordForm.newPassword" placeholder="Nhập mật khẩu mới..." />
            </div>
          </div>
          <div class="form-group">
            <label>Xác nhận mật khẩu mới</label>
            <div class="input-with-icon">
              <i class="bi bi-check-circle"></i>
              <input type="password" v-model="passwordForm.confirmPassword" placeholder="Nhập lại mật khẩu mới..." />
            </div>
          </div>
        </div>
        <div class="modal-actions">
          <button class="btn-cancel" @click="closePasswordModal">Hủy bỏ</button>
          <button class="btn-save" @click="changePassword">Đổi mật khẩu</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import api from "@/api";
import { useRouter } from "vue-router";

const router = useRouter();

// --- 1. BIẾN DỮ LIỆU ---
const email = ref("");
const fullName = ref("");
const address = ref("");
const role = ref("");
const phone = ref(""); // Biến hiển thị ra màn hình chính

// Modal Edit
const showModal = ref(false);
const editUser = ref({
  fullName: "",
  email: "",
  address: "",
  phoneNumber: "" // Biến dùng trong form sửa
});

// Modal Password
const showPasswordModal = ref(false);
const passwordForm = ref({
  currentPassword: "",
  newPassword: "",
  confirmPassword: ""
});

// --- 2. LOAD PROFILE (ĐÃ SỬA ĐỂ BẮT CẢ CHỮ HOA/THƯỜNG) ---
// Copy đè hàm này vào code của bạn
const loadProfile = async () => {
  try {
    const token = localStorage.getItem("token");
    if (!token) return;

    const res = await api.get("/Users/me", {
      headers: { Authorization: `Bearer ${token}` }
    });
    
    const user = res.data;
    
    // 1. In ra để kiểm tra (Nhìn vào Console xem nó viết Hoa hay thường)
    console.log("🔥 DATA TỪ SERVER:", user); 

    // 2. Map dữ liệu cơ bản
    email.value = user.email || user.Email || "";
    fullName.value = user.fullName || user.FullName || "";
    address.value = user.address || user.Address || "";
    role.value = user.role || user.Role || "";
    
    // 3. --- SỬA LỖI PHONE Ở ĐÂY ---
    // Logic: Nếu không thấy 'phoneNumber' thì tìm 'PhoneNumber', không thấy nữa thì tìm 'Phone'...
    const serverPhone = user.phoneNumber || user.PhoneNumber || user.phone || user.Phone || user.mobile || "";
    
    // Gán vào biến hiển thị
    phone.value = serverPhone; 

    // Gán luôn vào biến dùng cho Form Sửa (Để khi bấm nút Sửa nó hiện sẵn số cũ)
    editUser.value = {
      fullName: fullName.value,
      email: email.value,
      address: address.value,
      phoneNumber: serverPhone // <--- Quan trọng: Gán đúng biến vừa tìm được
    };

    if (role.value) localStorage.setItem("role", role.value);

  } catch (err) {
    console.error("Lỗi load profile:", err);
  }
};

// --- 3. CÁC HÀM ĐÓNG MỞ MODAL ---
const openEditModal = () => {
    // Trước khi mở modal, gán lại giá trị mới nhất từ màn hình vào form
    editUser.value.fullName = fullName.value;
    editUser.value.address = address.value;
    editUser.value.phoneNumber = phone.value;
    showModal.value = true;
};
const closeModal = () => showModal.value = false;

const openPasswordModal = () => {
  passwordForm.value = { currentPassword: "", newPassword: "", confirmPassword: "" };
  showPasswordModal.value = true;
};
const closePasswordModal = () => showPasswordModal.value = false;

// --- 4. CẬP NHẬT HỒ SƠ (ĐÃ FIX ĐỂ LƯU SĐT) ---
const saveUser = async () => {
  try {
    const token = localStorage.getItem("token");
    
    // Payload gửi đi (Gửi kèm số điện thoại)
    const payload = {
      fullName: editUser.value.fullName,
      address: editUser.value.address,
      phoneNumber: editUser.value.phoneNumber // Đảm bảo trường này có dữ liệu
    };
    
    console.log("Đang lưu profile:", payload);

    await api.patch("/Users/me", payload, {
      headers: { Authorization: `Bearer ${token}` }
    });

    // Cập nhật lại giao diện ngay lập tức
    fullName.value = payload.fullName;
    address.value = payload.address;
    phone.value = payload.phoneNumber;

    showModal.value = false;
    alert("Cập nhật thành công!");
    
    // Load lại lần nữa cho chắc chắn
    loadProfile(); 

  } catch (err) {
    console.error("Lỗi cập nhật:", err);
    alert("Cập nhật thất bại! Kiểm tra console để xem chi tiết.");
  }
};

// --- 5. ĐỔI MẬT KHẨU (GIỮ NGUYÊN CODE ĐÃ CHẠY ĐƯỢC) ---
const changePassword = async () => {
  if (!passwordForm.value.currentPassword || !passwordForm.value.newPassword) {
    alert("Vui lòng nhập đầy đủ thông tin!");
    return;
  }
  if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
    alert("Mật khẩu xác nhận không khớp!");
    return;
  }

  try {
    const token = localStorage.getItem("token");
    
    // Gửi kèm thông tin cá nhân để tránh lỗi validation
    const payload = {
      passwordOld: passwordForm.value.currentPassword,
      passwordNew: passwordForm.value.newPassword,
      fullName: fullName.value,
      address: address.value,
      phoneNumber: phone.value // Gửi lại số điện thoại hiện tại
    };

    await api.patch("/Users/me", payload, {
      headers: { Authorization: `Bearer ${token}` }
    });

    alert("Đổi mật khẩu thành công!");
    closePasswordModal();
  } catch (err) {
    console.error("Lỗi đổi pass:", err);
    let msg = "Đổi mật khẩu thất bại!";
    if(err.response?.data?.message) msg = err.response.data.message;
    else if(Array.isArray(err.response?.data)) msg = err.response.data.map(e=>e.description).join("\n");
    else if(err.response?.data?.errors) msg = JSON.stringify(err.response.data.errors);
    
    alert(msg);
  }
};

onMounted(loadProfile);
</script>


<style scoped>
/* --- 1. GLOBAL LAYOUT --- */
.account-page-wrapper {
  background-color: #fdf2f7; 
  background-image: radial-gradient(circle at 10% 20%, rgb(255, 226, 237) 0%, rgb(253, 242, 242) 90%);
  min-height: 100vh;
  font-family: 'Segoe UI', sans-serif;
  color: #444;
  border-radius: 20px;
}

/* --- 2. PROFILE CARD --- */
.profile-card {
  background: white;
  border-radius: 24px;
  padding: 40px 20px;
  text-align: center;
  position: relative;
  box-shadow: 0 15px 35px rgba(255, 79, 142, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.8);
  transition: transform 0.3s ease;
}
.profile-card:hover { transform: translateY(-5px); }

/* Buttons Action Floating (Góc phải) */
.action-buttons {
  position: absolute;
  top: 15px;
  right: 15px;
  display: flex;
  gap: 8px; /* Khoảng cách giữa 2 nút */
}

.btn-action-float {
  width: 36px; height: 36px;
  border-radius: 50%; border: none;
  cursor: pointer; transition: all 0.2s;
  display: flex; align-items: center; justify-content: center;
}

/* Nút Edit (Cũ) */
.btn-edit { background: #fff0f5; color: #ff4f8e; }
.btn-edit:hover { background: #ff4f8e; color: white; transform: rotate(15deg); }

/* Nút Password (Mới) */
.btn-key { background: #e3f2fd; color: #1976d2; }
.btn-key:hover { background: #1976d2; color: white; transform: scale(1.1); }


/* Avatar Circle */
.profile-avatar {
  width: 100px; height: 100px;
  background: linear-gradient(135deg, #ff9bb5, #ff4f8e);
  color: white; font-size: 40px; font-weight: bold;
  border-radius: 50%;
  display: flex; align-items: center; justify-content: center;
  margin: 0 auto 15px;
  box-shadow: 0 8px 20px rgba(255, 79, 142, 0.3);
  border: 4px solid white;
}

.profile-name { font-size: 22px; font-weight: 700; color: #333; margin-bottom: 5px; }

.badge-role {
  display: inline-block; background: #ffe2ed; color: #ff2f70;
  padding: 5px 15px; border-radius: 20px; font-size: 12px;
  font-weight: 700; text-transform: uppercase; margin-bottom: 25px;
  letter-spacing: 0.5px;
}

.profile-details { text-align: left; padding: 0 10px; }
.detail-item {
  display: flex; align-items: center; margin-bottom: 15px;
  color: #666; font-size: 15px; padding: 10px;
  background: #fffcfd; border-radius: 12px; border: 1px solid #fff0f6;
}
.detail-item i { font-size: 18px; color: #ff8eb0; margin-right: 12px; width: 20px; text-align: center; }

/* --- 3. DASHBOARD GRID --- */
.dashboard-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(220px, 1fr)); gap: 20px; }

.dash-card {
  background: white; border-radius: 20px; padding: 25px 20px;
  text-decoration: none; display: flex; flex-direction: column;
  align-items: center; justify-content: center; text-align: center;
  box-shadow: 0 10px 25px rgba(230, 230, 230, 0.5);
  transition: all 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  border: 1px solid transparent; height: 100%;
}
.dash-card .icon-box {
  width: 60px; height: 60px; background: #fff0f5;
  border-radius: 50%; display: flex; align-items: center; justify-content: center;
  margin-bottom: 15px; transition: 0.3s;
}
.dash-card i { font-size: 28px; color: #ff4f8e; transition: 0.3s; }
.dash-card h3 { font-size: 16px; font-weight: 700; color: #444; margin-bottom: 5px; }
.dash-card p { font-size: 13px; color: #999; margin: 0; }

.dash-card:hover {
  transform: translateY(-8px);
  background: linear-gradient(135deg, #ff6f99, #ff3b7e);
  box-shadow: 0 15px 30px rgba(255, 59, 126, 0.3);
}
.dash-card:hover .icon-box { background: rgba(255, 255, 255, 0.2); }
.dash-card:hover i, .dash-card:hover h3, .dash-card:hover p { color: white; }

/* --- 4. MODAL STYLES --- */
.modal-overlay { 
  position: fixed; inset: 0; 
  background: rgba(0,0,0,0.4); backdrop-filter: blur(4px);
  display: flex; justify-content: center; align-items: center; z-index: 1000; 
}
.modal-box { 
  background: white; width: 450px; border-radius: 24px; 
  overflow: hidden; box-shadow: 0 25px 50px rgba(0,0,0,0.2);
}
.animate-pop { animation: popUp 0.3s cubic-bezier(0.175, 0.885, 0.32, 1.275); }
@keyframes popUp { from { transform: scale(0.8); opacity: 0; } to { transform: scale(1); opacity: 1; } }

.modal-header {
  padding: 20px 24px; border-bottom: 1px solid #f0f0f0;
  display: flex; justify-content: space-between; align-items: center;
}
.modal-title { font-size: 18px; color: #ff4f8e; margin: 0; }
.btn-close-modal { background: none; border: none; font-size: 20px; cursor: pointer; color: #999; }

.form-body { padding: 24px; }
.form-group { margin-bottom: 16px; }
.form-group label { font-size: 13px; font-weight: 600; color: #666; margin-bottom: 6px; display: block; }

.input-with-icon { position: relative; }
.input-with-icon i {
  position: absolute; left: 12px; top: 50%; transform: translateY(-50%); color: #ff9bb5;
}
.input-with-icon input {
  width: 100%; padding: 10px 10px 10px 38px;
  border: 1px solid #eee; border-radius: 10px;
  font-size: 14px; outline: none; transition: 0.2s; background: #fdfdfd;
}
.input-with-icon input:focus {
  border-color: #ff4f8e; background: white;
  box-shadow: 0 0 0 3px rgba(255, 79, 142, 0.1);
}
.input-with-icon.disabled input { background: #f5f5f5; color: #999; }

.modal-actions {
  padding: 15px 24px; background: #fdfdfd; display: flex; justify-content: flex-end; gap: 10px;
}
.btn-cancel {
  background: #fff0f5; color: #ff4f8e; padding: 10px 20px;
  border-radius: 10px; border: none; font-weight: 600; cursor: pointer;
}
.btn-save {
  background: #ff4f8e; color: white; padding: 10px 24px;
  border-radius: 10px; border: none; font-weight: 600; cursor: pointer;
  box-shadow: 0 4px 15px rgba(255, 79, 142, 0.3);
}
.btn-save:hover { background: #e63e77; }
</style>