<template>
  <div class="register-wrapper">
    
    <div class="register-card shadow-lg">
      
      <div class="register-image">
        <img src="https://i.pinimg.com/736x/51/dc/99/51dc99fe21c1e10c211da34357f68388.jpg" alt="Sweet Bakery" />
        <div class="image-overlay">
          <h3>Thành viên GIRLCake</h3>
          <p>Nhận ngàn ưu đãi ngọt ngào mỗi ngày</p>
        </div>
      </div>

      <div class="register-form-container">
        <div class="form-header">
          <h2 class="brand-title">Đăng Ký</h2>
          <p class="subtitle">Chào mừng bạn đến với gia đình GIRLCake</p>
        </div>

        <form @submit.prevent="handleRegister" class="main-form">
          
          <div class="form-group mb-2">
            <label class="form-label">Họ tên</label>
            <input 
              v-model="fullName" 
              type="text" 
              class="form-control modern-input" 
              placeholder="Ví dụ: Nguyễn Văn A" 
              required 
            />
          </div>

          <div class="form-group mb-2">
            <label class="form-label">Email</label>
            <input 
              v-model="email" 
              type="email" 
              class="form-control modern-input" 
              placeholder="name@example.com" 
              required 
            />
          </div>

          <div class="form-group mb-2">
            <label class="form-label">Địa chỉ</label>
            <input 
              v-model="address" 
              type="text" 
              class="form-control modern-input" 
              placeholder="Số nhà, đường, quận..." 
              required 
            />
          </div>

          <div class="row">
            <div class="col-md-6 mb-3">
              <label class="form-label">Mật khẩu</label>
              <input 
                v-model="password" 
                type="password" 
                class="form-control modern-input" 
                placeholder="••••••••" 
                required 
              />
            </div>
            <div class="col-md-6 mb-3">
              <label class="form-label">Nhập lại</label>
              <input 
                v-model="confirmPassword" 
                type="password" 
                class="form-control modern-input" 
                placeholder="••••••••" 
                required 
              />
            </div>
          </div>

          <button class="btn btn-gradient w-100 mt-1">Đăng ký thành viên</button>
        </form>

        <p class="login-text mt-3">
          Bạn đã có tài khoản? 
          <router-link to="/login" class="link-pink">Đăng nhập ngay</router-link>
        </p>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import api from '@/api';

const router = useRouter();

// State Variables
const fullName = ref('');
const email = ref('');
const password = ref('');
const address = ref('');
const confirmPassword = ref('');

// Xử lý Đăng ký
const handleRegister = async () => {
 
  if (password.value !== confirmPassword.value) {
    alert("Mật khẩu nhập lại không khớp! Vui lòng kiểm tra lại.");
    return;
  }

  try {
    const body = {
      fullName: fullName.value,
      email: email.value,
      password: password.value,
      address: address.value 
    };

    console.log("Đang gửi đăng ký:", body);
    
    await api.post("/Auth/register", body);

    alert("Đăng ký thành công! Hãy đăng nhập ngay");
    router.push("/login"); 

  } catch (error) {
    console.error("Lỗi đăng ký:", error);
    const apiError = error.response?.data;

    // Hiển thị lỗi chi tiết từ Backend trả về
    if (apiError?.errors) {
      // Lấy lỗi đầu tiên trong danh sách lỗi
      const firstKey = Object.keys(apiError.errors)[0];
      alert(apiError.errors[firstKey][0]);
    } else if (typeof apiError === "string") {
      alert(apiError);
    } else {
      alert("Đăng ký thất bại. Vui lòng kiểm tra kết nối mạng!");
    }
  }
};
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@400;600;700;800&display=swap');

/* --- WRAPPER: FULL MÀN HÌNH --- */
.register-wrapper {
  position: fixed; /* Cố định để đè lên mọi thứ */
  top: 0;
  left: 0;
  width: 100%;
  height: 100vh;
  z-index: 9999; /* Đè lên Navbar (thường là 1000) */
  
  font-family: 'Quicksand', sans-serif;
  display: flex;
  align-items: center; /* Căn giữa dọc */
  justify-content: center; /* Căn giữa ngang */
  
  /* Gradient hồng phấn nhẹ nhàng */
  background: linear-gradient(135deg, #fff0f5 0%, #ffe3ec 100%);
  padding: 20px;
  
  /* QUAN TRỌNG: Cho phép cuộn nếu màn hình quá nhỏ/ngắn */
  overflow-y: auto; 
}

/* --- CARD: FORM CHÍNH --- */
.register-card {
  display: flex;
  background: #ffffff;
  border-radius: 20px;
  overflow: hidden;
  width: 100%;
  
  /* Kích thước gọn gàng để không bị tràn */
  max-width: 850px; 
  margin: auto; /* Căn giữa khi cuộn */
  
  box-shadow: 0 15px 35px rgba(255, 79, 142, 0.15);
  position: relative;
}

/* --- CỘT TRÁI (ẢNH) --- */
.register-image {
  flex: 0.9; /* Tỷ lệ ảnh nhỏ hơn form một chút */
  position: relative;
  display: none; /* Mobile ẩn ảnh đi cho gọn */
}
@media (min-width: 992px) {
  .register-image { display: block; } /* PC hiện ảnh */
}

.register-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.image-overlay {
  position: absolute;
  bottom: 0; left: 0; width: 100%;
  padding: 30px;
  background: linear-gradient(to top, rgba(0,0,0,0.7), transparent);
  color: white;
}
.image-overlay h3 { font-weight: 800; font-size: 1.6rem; margin-bottom: 5px; text-shadow: 0 2px 4px rgba(0,0,0,0.3); }
.image-overlay p { font-size: 0.95rem; opacity: 0.95; }

/* --- CỘT PHẢI (FORM) --- */
.register-form-container {
  flex: 1.1;
  padding: 30px 35px; /* Padding vừa phải */
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.brand-title {
  color: #ff4f8e;
  font-weight: 800;
  font-size: 1.8rem;
  margin-bottom: 5px;
}

.subtitle {
  color: #888;
  font-size: 0.9rem;
  margin-bottom: 20px;
}

/* --- INPUT STYLE --- */
.form-label {
  font-weight: 600;
  color: #555;
  font-size: 0.85rem;
  margin-bottom: 4px;
}

.modern-input {
  border: 2px solid #f0f0f0;
  border-radius: 10px;
  padding: 10px 15px;
  background: #fdfdfd;
  transition: all 0.3s ease;
  font-size: 0.9rem;
  color: #333;
}

.modern-input:focus {
  background: #fff;
  border-color: #ffb3c6; /* Viền hồng khi focus */
  box-shadow: 0 0 0 3px rgba(255, 79, 142, 0.1);
  outline: none;
}

/* --- BUTTON GRADIENT --- */
.btn-gradient {
  background: linear-gradient(45deg, #ff4f8e, #ff8fae);
  color: white;
  border: none;
  padding: 12px;
  border-radius: 10px;
  font-weight: 700;
  font-size: 1rem;
  box-shadow: 0 4px 15px rgba(255, 79, 142, 0.4);
  transition: transform 0.2s, box-shadow 0.2s;
  cursor: pointer;
}

.btn-gradient:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(255, 79, 142, 0.5);
  background: linear-gradient(45deg, #ff2f70, #ff6f91);
}

/* --- FOOTER LINK --- */
.login-text {
  text-align: center;
  font-size: 0.9rem;
  color: #666;
}

.link-pink {
  color: #ff4f8e;
  font-weight: 700;
  text-decoration: none;
  transition: color 0.2s;
}

.link-pink:hover {
  color: #d6336c;
  text-decoration: underline;
}
</style>