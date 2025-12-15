<template>
  <div class="login-wrapper">
    
    <div class="login-card shadow-lg">
      
      <div class="login-image">
        <img src="https://i.pinimg.com/736x/35/c8/b7/35c8b74b5062171c86a892dd8b3cf750.jpg" alt="Sweet Cake" />
        <div class="image-overlay">
          <h3>Ngọt ngào từng khoảnh khắc</h3>
        </div>
      </div>

      <div class="login-form-container">
        <div class="form-header">
          <h2 class="brand-title">GIRLCake</h2>
          <p class="welcome-text">Đăng nhập để thưởng thức bánh ngon nhé!</p>
        </div>

        <form @submit.prevent="handleLogin" class="main-form">
          <div class="form-group mb-3">
            <label class="form-label">Email</label>
            <input 
              v-model="email" 
              type="email" 
              class="form-control modern-input" 
              placeholder="name@example.com" 
              required 
            />
          </div>

          <div class="form-group mb-4">
            <label class="form-label">Mật khẩu</label>
            <input 
              v-model="password" 
              type="password" 
              class="form-control modern-input" 
              placeholder="••••••••" 
              required 
            />
          </div>

          <button class="btn btn-gradient w-100">Đăng nhập</button>
        </form>

        <div class="separator">
          <span>Hoặc đăng nhập với</span>
        </div>

        <div class="google-btn-wrapper">
          <GoogleLogin :callback="onGoogle" />
        </div>

        <p class="register-text mt-4">
          Bạn chưa có tài khoản? 
          <router-link to="/register" class="link-pink">Đăng ký ngay</router-link>
        </p>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref } from "vue";
import { useRouter } from "vue-router";
import api from '@/api';
import { GoogleLogin } from "vue3-google-login";

const router = useRouter(); 
const email = ref("");
const password = ref("");

// --- XỬ LÝ ĐĂNG NHẬP THƯỜNG ---
const handleLogin = async () => {
  try {
    const res = await api.post("/Auth/login", {
      email: email.value,
      password: password.value
    });
    
    // Lưu thông tin
    localStorage.setItem("token", res.data.token);
    localStorage.setItem("user", JSON.stringify(res.data.user));
    
    // Chuyển hướng trang chủ
    alert("Đăng nhập thành công!");
    router.push("/"); 
  } catch (error) {
    alert(error.response?.data?.message || "Email hoặc mật khẩu chưa đúng!");
  }
};

// --- XỬ LÝ GOOGLE LOGIN ---
const onGoogle = async (response) => {
  try {
    const idToken = response.credential;
    const res = await api.post('/Auth/google-login', { idToken });
    
    localStorage.setItem('token', res.data.token);
    localStorage.setItem('user', JSON.stringify(res.data.user));
    
    alert('Chào mừng bạn đến với GIRLCake!');    
    router.push("/");
  } catch (error) {
    console.error("Lỗi Google Login:", error);
    alert("Có lỗi xảy ra khi đăng nhập Google.");
  }
}
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@400;600;700&display=swap');

/* --- TỔNG THỂ --- */
.login-wrapper {
  font-family: 'Quicksand', sans-serif;
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  /* Background gradient hồng phấn nhẹ nhàng */
  background: linear-gradient(135deg, #fff0f5 0%, #ffe3ec 100%);
  padding: 20px;
  overflow-y: auto;
}

/* --- CARD CONTAINER --- */
.login-card {
  display: flex;
  background: #ffffff;
  border-radius: 24px;
  overflow: hidden;
  width: 100%;
  max-width: 900px;
  min-height: 550px;
  /* Đổ bóng mềm mại tạo độ nổi */
  box-shadow: 0 20px 40px rgba(255, 79, 142, 0.15);
}

/* --- CỘT TRÁI (ẢNH) --- */
.login-image {
  flex: 1.2; /* Chiếm 1.2 phần */
  position: relative;
  display: none; /* Mobile ẩn đi */
}

/* Hiển thị ảnh trên màn hình lớn */
@media (min-width: 768px) {
  .login-image {
    display: block;
  }
}

.login-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

/* Lớp phủ mờ trên ảnh để chữ nổi bật nếu cần */
.image-overlay {
  position: absolute;
  bottom: 0;
  left: 0;
  width: 100%;
  padding: 40px;
  background: linear-gradient(to top, rgba(0, 0, 0, 0.5), transparent);
  color: white;
  
}

.image-overlay h3 {
  font-weight: 900;
  font-size: 1.5rem;
  text-shadow: 0 9px 20px rgb(255, 0, 132);
}

/* --- CỘT PHẢI (FORM) --- */
.login-form-container {
  flex: 1; /* Chiếm 1 phần */
  padding: 50px 40px;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.brand-title {
  color: #ff4f8e; /* Màu hồng chủ đạo */
  font-weight: 800;
  font-size: 2rem;
  margin-bottom: 5px;
}

.welcome-text {
  color: #888;
  font-size: 0.95rem;
  margin-bottom: 30px;
}

/* --- INPUT STYLE MỚI --- */
.form-label {
  font-weight: 600;
  color: #555;
  font-size: 0.9rem;
}

.modern-input {
  border: 2px solid #f0f0f0;
  border-radius: 12px;
  padding: 12px 15px;
  background: #fdfdfd;
  transition: all 0.3s ease;
  font-size: 0.95rem;
}

.modern-input:focus {
  background: #fff;
  border-color: #ffb3c6; /* Viền hồng nhạt khi click */
  box-shadow: 0 0 0 4px rgba(255, 79, 142, 0.1);
  outline: none;
}

/* --- BUTTON GRADIENT --- */
.btn-gradient {
  background: linear-gradient(45deg, #ff4f8e, #ff8fae);
  color: white;
  border: none;
  padding: 12px;
  border-radius: 12px;
  font-weight: 700;
  font-size: 1rem;
  box-shadow: 0 4px 15px rgba(255, 79, 142, 0.4);
  transition: transform 0.2s, box-shadow 0.2s;
}

.btn-gradient:hover {
  transform: translateY(-2px);
  box-shadow: 0 6px 20px rgba(255, 79, 142, 0.5);
  background: linear-gradient(45deg, #ff2f70, #ff6f91);
}

/* --- SEPARATOR --- */
.separator {
  display: flex;
  align-items: center;
  text-align: center;
  margin: 25px 0;
  color: #aaa;
  font-size: 0.85rem;
}

.separator::before,
.separator::after {
  content: '';
  flex: 1;
  border-bottom: 1px solid #eee;
}

.separator span {
  padding: 0 10px;
}

/* --- GOOGLE BUTTON --- */
.google-btn-wrapper {
  display: flex;
  justify-content: center;
}

/* --- FOOTER LINK --- */
.register-text {
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