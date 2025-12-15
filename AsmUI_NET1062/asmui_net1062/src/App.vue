<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';

// Router
const route = useRoute();
const router = useRouter();

// State login
const user = ref(null);
const cartCount = ref(2); // Giả lập số lượng

// Logic kiểm tra trạng thái
const isLoggedIn = computed(() => !!user.value);
const isAdmin = computed(() => user.value?.role === 'Admin');

// Lấy tên hiển thị
const userName = computed(() => {
  const userData = JSON.parse(localStorage.getItem('user') || '{}');
  return userData.fullName || userData.email || 'Khách hàng';
});

// Load user từ localStorage khi vào trang
onMounted(() => {
  const userData = localStorage.getItem('user');
  if (userData) {
    user.value = JSON.parse(userData); 
  }
});

// Logout
const logout = () => {
  localStorage.removeItem('user');
  localStorage.removeItem('token');
  user.value = null;
  router.push('/');
};

// Footer hiển thị nếu không phải login/register
const showFooter = computed(() => !['/login', '/register'].includes(route.path));
</script>

<template>
  <div class="app-container">
    
    <div class="bg-blob blob-1"></div>
    <div class="bg-blob blob-2"></div>

    <header class="header-wrapper">
      <nav class="Girlcake-nav">
        <div class="nav-content d-flex align-items-center justify-content-between">

          <router-link to="/" class="brand-logo">
            GIRLCake
          </router-link>

          <ul class="nav-menu d-flex gap-1 list-unstyled mb-0">
            <li><router-link to="/home" class="nav-item-link">Trang chủ</router-link></li>
            <li><router-link to="/combo" class="nav-item-link">Combo</router-link></li>
            <li><router-link to="/about" class="nav-item-link">Giới thiệu</router-link></li>
            <li><router-link to="/contact" class="nav-item-link">Liên hệ</router-link></li>
          </ul>

          <div class="action-area d-flex align-items-center gap-3">
            
            <template v-if="isLoggedIn">
              <router-link to="/account" class="user-pill">
                <div class="avatar-circle">
                   {{ userName.charAt(0).toUpperCase() }}
                </div>
                <span class="user-name">{{ userName }}</span>
              </router-link>
              
              <button class="btn-logout" @click="logout" title="Đăng xuất">
                <i class="bi bi-box-arrow-right"></i>
              </button>
            </template>

            <template v-else>
              <router-link to="/login" class="btn-login-pill">Đăng nhập</router-link>
            </template>

            <router-link 
              to="/cart" 
              class="btn-cart-floating" 
              :class="{ 'cart-disabled': !isLoggedIn || isAdmin }"
              @click.prevent="(!isLoggedIn || isAdmin) && $event.preventDefault()"
            >
              <i class="bi bi-bag-heart-fill"></i>
            </router-link>

          </div>
        </div>
      </nav>
    </header>

    <main class="main-content">
      <router-view />
    </main>

    <footer v-if="showFooter" class="footer-section mt-5 pt-5 pb-4">
      <div class="container">
        <div class="row">
          <div class="col-lg-4 col-md-6 mb-4">
            <router-link to="/" class="navbar-brand d-flex align-items-center gap-2 me-auto brand-text">
              <h3>GIRLCake</h3>
            </router-link>
            <p class="footer-text">
              GIRLCake mang đến trải nghiệm đặt bánh ngọt nhanh chóng – tiện lợi – tinh tế.
            </p>
          </div>
          <div class="col-lg-4 col-md-6 mb-4">
            <h5 class="footer-title">Liên kết nhanh</h5>
            <ul class="footer-links list-unstyled">
              <li><a href="#">Trang chủ</a></li>
              <li><a href="#">Sản phẩm</a></li>
              <li><a href="#">Giới thiệu</a></li>
              <li><a href="#">Liên hệ</a></li>
            </ul>
          </div>
          <div class="col-lg-4 col-md-6 mb-4">
            <h5 class="footer-title">Thông tin liên hệ</h5>
            <ul class="footer-info list-unstyled">
              <li><i class="bi bi-geo-alt-fill"></i> 123 Nguyễn Trãi, Hà Nội</li>
              <li><i class="bi bi-envelope-fill"></i> support@girlcake.com</li>
              <li><i class="bi bi-telephone-fill"></i> 0123 456 789</li>
            </ul>
          </div>
        </div>
        <hr class="footer-line" />
        <p class="footer-copy text-center mt-3">© 2025 GirlCake. All rights reserved.</p>
      </div>
    </footer>

  </div>
</template>

<style scoped>
/* ========= GLOBAL & FONTS ========== */
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@400;500;600;700&display=swap');

:root {
  --primary-pink: #ff4f8e;       
  --primary-hover: #ff2f70;      
  --soft-pink: #fff0f5;          
  --glass-border: rgba(255, 255, 255, 0.6);
  --glass-bg: rgba(255, 255, 255, 0.7);
  --text-main: #4a4a4a;
  --shadow-pink: 0 8px 32px rgba(255, 79, 142, 0.15);
}

.app-container {
  font-family: 'Quicksand', sans-serif;
  color: var(--text-main);
  min-height: 100vh;
  position: relative;
  overflow-x: hidden;
}

/* ========= BACKGROUND BLOBS ========== */
.bg-blob {
  position: fixed;
  border-radius: 50%;
  filter: blur(80px);
  z-index: 0;
  opacity: 0.6;
}
/* ========= HEADER (GLASSMORPHISM) ========== */
.header-wrapper {
  position: fixed;
  top: 20px;
  left: 0;
  right: 0;
  display: flex;
  justify-content: center;
  z-index: 1000;
  pointer-events: none;
}

.Girlcake-nav {
  pointer-events: auto;
  width: 90%;
  max-width: 1200px;
  background: var(--glass-bg);
  backdrop-filter: blur(16px);
  -webkit-backdrop-filter: blur(16px);
  border-radius: 50px;
  padding: 10px 25px;
  border: 1px solid var(--glass-border);
  box-shadow: var(--shadow-pink);
  transition: all 0.3s ease;
}

/* Logo & Menu Style */
.brand-logo {
  font-size: 1.5rem;
  font-weight: 800;
  color: var(--primary-pink);
  text-decoration: none;
  display: flex;
  align-items: center;
}
.nav-item-link {
  color: #666;
  text-decoration: none;
  font-weight: 600;
  padding: 8px 18px;
  border-radius: 20px;
  transition: all 0.3s;
  font-size: 0.95rem;
}
.nav-item-link:hover, .nav-item-link.router-link-active {
  background-color: rgba(255, 79, 142, 0.1);
  color: var(--primary-pink);
}

/* Buttons */
.btn-login-pill {
  padding: 8px 24px;
  border-radius: 30px;
  background: linear-gradient(135deg, #ff8fae, #ff4f8e);
  color: white;
  text-decoration: none;
  font-weight: 700;
  box-shadow: 0 4px 15px rgba(255, 79, 142, 0.3);
  border: none;
  transition: 0.2s;
}
.btn-login-pill:hover { transform: translateY(-2px); color: white; }

/* --- USER PILL (Đã sửa thành màu HỒNG) --- */
.user-pill {
  display: flex; 
  align-items: center; 
  gap: 10px; 
  background: #ffe6ee; /* Nền hồng nhạt */
  padding: 5px 15px 5px 5px; 
  border-radius: 30px; 
  text-decoration: none;
  border: 1px solid #ffb3c6; /* Viền hồng */
  transition: 0.2s;
  height: 42px;
}
.user-pill:hover {
  background: #ffccd5; /* Hover đậm hơn chút */
}

.avatar-circle { 
  width: 32px; 
  height: 32px; 
  background: var(--primary-pink); /* Avatar nền hồng */
  color: white; 
  border-radius: 50%; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  font-weight: 700; 
  flex-shrink: 0;
}
.user-name { 
  font-weight: 700; 
  color: #d6336c; /* Tên chữ hồng đậm */
  font-size: 0.95rem; 
  white-space: nowrap;
}

/* Logout Button */
.btn-logout { 
  width: 42px; 
  height: 42px; 
  border-radius: 50%; 
  border: none; 
  background: white; 
  color: #888; 
  display: flex; 
  align-items: center; 
  justify-content: center; 
  transition: 0.2s; 
  font-size: 1.2rem; 
  box-shadow: 0 2px 8px rgba(0,0,0,0.05);
}
.btn-logout:hover { background: #ffebee; color: #d32f2f; }

/* --- CART BUTTON (MÀU HỒNG & LOGIC KHÓA) --- */
.btn-cart-floating {
  position: relative; 
  width: 42px; 
  height: 42px; 
  background: rgb(253, 172, 185); /* Nền MÀU HỒNG */
  color: white; /* Icon trắng */
  border-radius: 50%;
  display: flex; 
  align-items: center; 
  justify-content: center; 
  font-size: 1.2rem; 
  box-shadow: 0 4px 10px rgba(255, 79, 142, 0.4); 
  transition: 0.3s;
}

.btn-cart-floating:hover { 
  background: rgb(255, 84, 113); 
  transform: translateY(-2px); 
  color: white;
}

/* Style khi bị khóa (Admin hoặc chưa Login) */
.cart-disabled { 
  background: #e0e0e0 !important; /* Màu xám */
  color: #999 !important; /* Icon xám đậm */
  cursor: not-allowed; 
  box-shadow: none;
  pointer-events: none; /* Không cho click */
  opacity: 0.7;
}



/* ========= MAIN CONTENT ========== */
.main-content {
  padding-top: 100px;
  position: relative;
  z-index: 1;
}

/* ========= FOOTER ========== */
.footer-section {
  background: #ffe6ee;
  color: #444;
  border-top-left-radius: 35px;
  border-top-right-radius: 35px;
  position: relative;
}

.footer-logo h3 { font-size: 28px; font-weight: 800; color: var(--primary-pink); }
.brand-text h3 { color: var(--primary-pink); font-weight: 800; }
.footer-title { font-weight: 700; color: var(--primary-pink); margin-bottom: 15px; }
.footer-text { line-height: 1.6; font-size: 15px; }
.footer-links li, .footer-info li { margin-bottom: 10px; font-size: 15px; }
.footer-links a { color: #444; text-decoration: none; transition: 0.2s; }
.footer-links a:hover { color: var(--primary-pink); text-decoration: underline; }
.footer-info i { color: var(--primary-pink); margin-right: 8px; }
.footer-line { border-color: #ffb6d1; opacity: 1; }
.footer-copy { color: #666; font-size: 14px; }
</style>