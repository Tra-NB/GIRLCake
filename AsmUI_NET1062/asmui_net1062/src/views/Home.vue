<template>
  <div class="home-container">
    <div class="bg-decor-blob"></div>

    <div class="hero-section mb-5">
      <div class="hero-content text-center">
        <h1 class="hero-title">Đến với GIRLCake</h1>
        <p class="hero-subtitle">Thưởng thức những món ăn và thức uống dễ thương nhất – ngọt ngào như chính bạn</p>
      </div>
    </div>

    <div class="container main-wrapper">
      <div class="row align-items-center g-5 mb-5">
        
        <div class="col-lg-7">
          <div id="cakeCarousel" class="carousel slide carousel-fade glass-frame shadow-lg" data-bs-ride="carousel">
            <div class="carousel-inner">
              <div class="carousel-item active" data-bs-interval="3000">
                <img src="https://i.pinimg.com/736x/2b/53/8a/2b538ab70e903cbcb13762cf0152d5e3.jpg" class="d-block w-100 carousel-img" alt="cake 1" />
              </div>
              <div class="carousel-item" data-bs-interval="3000">
                <img src="https://i.pinimg.com/1200x/66/d0/d0/66d0d09edaa68136eaa5007c5f99b18e.jpg" class="d-block w-100 carousel-img" alt="cake 2" />
              </div>
              <div class="carousel-item" data-bs-interval="3000">
                <img src="https://i.pinimg.com/1200x/c0/e4/35/c0e435bde618483742259a9ee82ee422.jpg" class="d-block w-100 carousel-img" alt="cake 3" />
              </div>
            </div>
            <button class="carousel-control-prev" type="button" data-bs-target="#cakeCarousel" data-bs-slide="prev">
              <span class="carousel-btn"><i class="bi bi-chevron-left"></i></span>
            </button>
            <button class="carousel-control-next" type="button" data-bs-target="#cakeCarousel" data-bs-slide="next">
              <span class="carousel-btn"><i class="bi bi-chevron-right"></i></span>
            </button>
          </div>
        </div>

        <div class="col-lg-5">
          <div class="intro-box">
            <h2 class="intro-title">Ngọt ngào từng chi tiết <i class="bi bi-balloon-heart-fill text-danger"></i></h2>
            <p class="intro-text">
              Chào mừng bạn đến với <strong>GIRLCake</strong>! Chúng tôi mang đến những chiếc bánh được làm thủ công tỉ mỉ,
              nguyên liệu premium và hương vị độc bản. 
            </p>
            <p class="intro-text">Hãy để chúng tôi tô điểm cho khoảnh khắc của bạn thêm phần rực rỡ!</p>
            <router-link to="/combo" class="btn btn-gradient mt-3">
              Xem combo ưu đãi <i class="bi bi-arrow-right-short"></i>
            </router-link>
          </div>
        </div>
        
        <div class="col-12 mt-5">
           <div class="voucher-wrapper">
               <div class="d-flex align-items-center mb-3">
                  <h4 class="section-badge"><i class="bi bi-ticket-perforated-fill"></i> Mã Ưu Đãi</h4>
                  <span class="ms-2 text-muted small">Lướt để xem thêm</span>
               </div>
               
               <div class="voucher-scroll-container">
                 <div class="voucher-track" v-if="vouchers.length">
                    <div class="voucher-ticket" v-for="v in vouchers" :key="v.id">
                        <div class="ticket-left">
                            <span class="ticket-icon"><i class="bi bi-gift-fill"></i></span>
                        </div>
                        <div class="ticket-right">
                            <h5 class="ticket-code">{{ v.code }}</h5>
                            <p class="ticket-desc">{{ v.description }}</p>
                            <button class="btn-copy">Sao chép</button>
                        </div>
                        <div class="ticket-holes-top"></div>
                        <div class="ticket-holes-bottom"></div>
                    </div>
                 </div>
                 <div v-else-if="!isLoadingVouchers" class="empty-voucher">
                    <i class="bi bi-emoji-frown"></i> Chưa có mã giảm giá nào hôm nay.
                 </div>
               </div>
           </div>
        </div>
      </div>
    </div>

    <div class="container pb-5" v-for="cat in categoriesWithFoods" :key="cat.categoryId">
      <div class="category-header">
         <h2 class="category-title">{{ cat.name }}</h2>
         <div class="category-line"></div>
      </div>

      <div v-if="isLoading" class="loading-spinner">
          <div class="spinner-border text-pink" role="status"></div>
          <span>Đang nướng bánh...</span>
      </div>
      
      <div v-else class="row g-4">
        <div class="col-xl-3 col-lg-4 col-md-6" v-for="food in cat.foods" :key="food.id">
          
          <div class="card food-card" 
               @click="goToDetail(food.id)"
               :class="{'unavailable': !food.isAvailable}">
            
            <div class="badge-wrapper">
                <span v-if="!food.isAvailable" class="badge-sold-out">HẾT HÀNG</span>
                <span v-else class="badge-new">BEST SELLER</span>
            </div>

            <div class="img-wrapper">
                <img 
                  :src="resolveImage(food.imageUrl)" 
                  :alt="food.name" 
                  class="food-img" 
                  @error="onImgError($event)" 
                />
            </div>

            <div class="card-body">
              <h5 class="food-title" :title="food.name">{{ food.name }}</h5>
              
              <div class="rating-row">
                <div class="stars">
                   <i class="bi bi-star-fill text-warning" v-for="n in Math.round(food.avgRating)" :key="'fill'+n"></i>
                   <i class="bi bi-star text-muted" v-for="n in (5 - Math.round(food.avgRating))" :key="'empty'+n"></i>
                </div>
                <span class="review-count">({{ food.reviewCount }} đánh giá)</span>
              </div>

              <p class="food-desc">{{ food.description }}</p>
              
              <div class="card-footer-action">
                  <span class="price-tag">{{ formatPrice(food.price) }}</span>
                  
                  <button 
                    @click.stop="addToCart(food.id, food.isAvailable)" 
                    class="btn-cart-icon"
                    :disabled="!food.isAvailable"
                    title="Thêm vào giỏ"
                  >
                    <i class="bi bi-basket2-fill"></i>
                  </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { useRouter } from "vue-router";
import api from '@/api';

const router = useRouter();
const allFoods = ref([]);
const isLoading = ref(false);

// Voucher
const vouchers = ref([]);
const isLoadingVouchers = ref(false);

// Actions
const goToDetail = (id) => router.push(`/food/${id}`);

/* -----------------------------------------------------------
   FIX: HÀM ĐỊNH DẠNG TIỀN VNĐ (MẠNH MẼ HƠN)
   ----------------------------------------------------------- */
const formatPrice = (value) => {
    // 1. Chuyển thành số
    const number = Number(value);

    // 2. Nếu lỗi hoặc bằng 0
    if (isNaN(number) || number === 0) return '0 VND';

    // 3. Format số có dấu chấm (20.000) + nối thêm chữ " VND"
    return number.toLocaleString('vi-VN') + ' VND';
};

// Add to Cart
const addToCart = async (foodId, isAvailable) => {
  if (!isAvailable) return;
  const user = JSON.parse(localStorage.getItem("user"));
  const token = localStorage.getItem("token");

  if (!user || !token) return alert("Bạn cần đăng nhập để mua hàng!");
  if (user.role === "Admin") return alert("Admin không thể mua hàng!");

  try {
    const { data: cartData } = await api.get("/Cart", { headers: { Authorization: `Bearer ${token}` } });
    const existing = cartData.items?.find(i => i.productId === foodId);
    const quantityToAdd = existing ? existing.quantity + 1 : 1;
    await api.post("/Cart/product", { productId: foodId, quantity: quantityToAdd }, { headers: { Authorization: `Bearer ${token}` } });
    alert("Đã thêm món ngon vào giỏ!");
  } catch (err) {
    console.error("Cart error:", err);
    alert("Có lỗi xảy ra khi thêm giỏ hàng.");
  }
};

const resolveImage = (imageUrl) => {
  if (!imageUrl || imageUrl.trim() === "") return "/img/default-cake.jpg"; 
  if (imageUrl.startsWith("http")) return imageUrl;
  return `https://localhost:7119${imageUrl}`; 
};
const onImgError = (event) => event.target.src = "https://placehold.co/400x300?text=No+Image";

const fetchReviewsForFood = async (foodId) => {
  try {
    const { data } = await api.get(`/Reviews/product/${foodId}`);
    if (!data || !data.length) return { avgRating: 5, reviewCount: 0 }; 
    const total = data.reduce((sum, r) => sum + r.rating, 0);
    return { avgRating: total / data.length, reviewCount: data.length };
  } catch (err) { return { avgRating: 5, reviewCount: 0 }; }
};

const fetchAllFoods = async () => {
  isLoading.value = true;
  try {
    const { data } = await api.get('/Food/active');
    allFoods.value = Array.isArray(data) ? data : [];
    
    // Debug: Kiểm tra xem dữ liệu giá có đúng không
    // console.log("Data loaded:", allFoods.value);

    await Promise.all(allFoods.value.map(async f => {
       const stats = await fetchReviewsForFood(f.id);
       Object.assign(f, stats);
    }));
  } catch (err) {
    console.error(err);
    allFoods.value = []; 
  } finally { isLoading.value = false; }
};

const fetchActiveVouchers = async () => {
  isLoadingVouchers.value = true;
  try {
    const { data } = await api.get("/Voucher/active");
    vouchers.value = data || [];
  } catch (err) { vouchers.value = []; } 
  finally { isLoadingVouchers.value = false; }
};

const categoriesWithFoods = computed(() => {
  const map = new Map();
  allFoods.value.forEach(f => {
    if (!map.has(f.categoryId)) map.set(f.categoryId, { categoryId: f.categoryId, foods: [], name: f.categoryName || 'Menu' });
    map.get(f.categoryId).foods.push(f);
  });
  return Array.from(map.values());
});

onMounted(() => {
  fetchAllFoods();
  fetchActiveVouchers();
});
</script>

<style scoped>
@import url('https://fonts.googleapis.com/css2?family=Quicksand:wght@400;500;600;700&display=swap');

/* =========================================
   1. BẢNG MÀU (CỐ ĐỊNH)
   ========================================= */
:root {
  --primary: #ff3366;        /* Màu hồng đỏ chủ đạo */
  --primary-hover: #e6004c;
  --bg-page: #fff0f5;        /* Nền trang hồng rất nhạt */
  --bg-card: #ffffff;        /* Nền card trắng */
  --shadow-card: 0 8px 20px rgba(255, 51, 102, 0.1);
  --shadow-hover: 0 15px 30px rgba(255, 51, 102, 0.25);
  --font-main: 'Quicksand', sans-serif;
}

.home-container {
  font-family: var(--font-main);
  background-color: var(--bg-page);
  min-height: 100vh;
  padding-bottom: 80px;
  color: #4a4a4a;
}
.bg-decor-blob {
  position: absolute; top: -100px; right: -100px; width: 600px; height: 600px;
  background: radial-gradient(circle, #ffcce0 0%, transparent 70%); opacity: 0.5; pointer-events: none;
}
.main-wrapper { position: relative; z-index: 2; }

/* =========================================
   2. CÁC PHẦN MÀU SẮC QUAN TRỌNG (FIXED)
   ========================================= */

/* --- 2.1 BADGE (BEST SELLER) --- */
.badge-new {
  background-color: #ff3366 !important; /* Ép buộc màu hồng */
  color: #ffffff !important;           /* Chữ trắng */
  padding: 5px 12px;
  border-radius: 8px;
  font-weight: 700;
  font-size: 0.75rem;
  box-shadow: 0 4px 10px rgba(255, 51, 102, 0.3);
  display: inline-block;
}

/* Badge Hết hàng */
.badge-sold-out {
  background-color: #333333 !important;
  color: #ffffff !important;
  padding: 5px 12px;
  border-radius: 8px;
  font-weight: 700;
  font-size: 0.75rem;
}
.badge-wrapper { position: absolute; top: 12px; left: 12px; z-index: 5; display: flex; flex-direction: column; gap: 5px; }

/* --- 2.2 GIÁ TIỀN (PRICE) --- */
.price-tag {
  color: #ff3366 !important; /* Ép buộc màu hồng */
  font-weight: 800;
  font-size: 1.25rem;
}

/* --- 2.3 NÚT GIỎ HÀNG (CART BUTTON) --- */
.btn-cart-icon {
  width: 45px; height: 45px;
  border-radius: 12px;
  border: none;
  background-color: #ffe6ee !important; /* Nền hồng nhạt */
  color: #ff3366 !important;           /* Icon màu hồng đậm */
  font-size: 1.3rem;
  display: flex; align-items: center; justify-content: center;
  transition: all 0.3s;
  cursor: pointer;
}
.btn-cart-icon:hover:not(:disabled) {
  background-color: #ff3366 !important; /* Hover chuyển nền đỏ */
  color: #ffffff !important;            /* Hover chuyển icon trắng */
  transform: scale(1.15) rotate(10deg);
  box-shadow: 0 4px 12px rgba(255, 51, 102, 0.4);
}
.btn-cart-icon:disabled { background: #eee !important; color: #bbb !important; cursor: not-allowed; }

/* --- 2.4 VOUCHER (PHẦN TRÁI MÀU HỒNG) --- */
.ticket-left {
  width: 80px;
  background: linear-gradient(180deg, #ff3366, #ff5c8d) !important; /* Màu nền cột trái */
  display: flex; align-items: center; justify-content: center;
  color: white !important;
  font-size: 1.8rem;
  border-right: 2px dashed white;
}
.ticket-icon { color: white !important; }

.ticket-code {
  font-weight: 800;
  color: #ff3366 !important; /* Mã code màu hồng */
  margin: 0; font-size: 1.1rem;
}

/* Nút sao chép voucher */
.btn-copy {
  font-size: 0.75rem; font-weight: 600;
  background: white;
  color: #ff3366 !important;
  border: 1px solid #ff3366 !important;
  border-radius: 4px; padding: 3px 10px;
  align-self: flex-start; cursor: pointer; transition: 0.2s;
}
.btn-copy:hover { background: #ff3366 !important; color: white !important; }

/* =========================================
   3. LAYOUT CƠ BẢN
   ========================================= */

/* Hero & Carousel */
.hero-title {
  font-weight: 800; font-size: 2.8rem;
  background: linear-gradient(45deg, #ff3366, #ff99ac);
  -webkit-background-clip: text; -webkit-text-fill-color: transparent;
  margin-bottom: 10px;
}
.hero-subtitle { color: #888; font-size: 1.1rem; }
.glass-frame { border-radius: 20px; overflow: hidden; border: 5px solid #fff; box-shadow: 0 10px 30px rgba(0,0,0,0.1); }
.carousel-img { height: 420px; object-fit: cover; }
.carousel-btn { width: 40px; height: 40px; background: rgba(255,255,255,0.9); color: var(--primary); border-radius: 50%; display: flex; align-items: center; justify-content: center; }
.intro-box { background: rgba(255,255,255,0.8); backdrop-filter: blur(10px); padding: 30px; border-radius: 20px; border: 1px solid #fff; box-shadow: var(--shadow-card); }
.btn-gradient { background: linear-gradient(135deg, #ff3366, #ff6685); color: white; border: none; padding: 10px 25px; border-radius: 30px; font-weight: 600; text-decoration: none; display: inline-block; box-shadow: 0 5px 15px rgba(255,51,102,0.3); transition: 0.3s; }
.btn-gradient:hover { transform: translateY(-3px); box-shadow: 0 8px 20px rgba(255,51,102,0.5); color: white; }

/* Voucher Container */
.voucher-wrapper { background: white; padding: 20px; border-radius: 20px; box-shadow: var(--shadow-card); border: 1px solid #ffdde5; }
.section-badge { color: var(--primary); font-weight: 700; margin: 0; }
.voucher-scroll-container { overflow-x: auto; padding: 10px 0; }
.voucher-scroll-container::-webkit-scrollbar { height: 6px; }
.voucher-scroll-container::-webkit-scrollbar-thumb { background: #ffb3c6; border-radius: 10px; }
.voucher-track { display: flex; gap: 20px; }
.voucher-ticket {
  flex: 0 0 290px; display: flex; background: #fff; border-radius: 12px;
  position: relative; overflow: hidden; transition: 0.3s;
}
.voucher-ticket:hover { transform: translateY(-5px); box-shadow: 0 8px 20px rgba(255,51,102,0.15); border-color: var(--primary); }
.ticket-holes-top, .ticket-holes-bottom { position: absolute; left: 68px; width: 24px; height: 24px; background-color: white; border: 1px solid #ffb3c6; border-radius: 50%; z-index: 2; }
.ticket-holes-top { top: -14px; border-bottom-color: transparent; }
.ticket-holes-bottom { bottom: -14px; border-top-color: transparent; }
.ticket-right {background-color: #ffd5d5b3; flex: 1; padding: 15px; display: flex; flex-direction: column; justify-content: center; }
.ticket-desc { font-size: 0.85rem; color: #000000; margin: 4px 0 10px; line-height: 1.2; }

/* Food Card Layout */
.category-header { display: flex; align-items: center; gap: 15px; margin: 40px 0 25px; }
.category-title { font-weight: 800; font-size: 1.8rem; color: #333; margin: 0; }
.category-line { flex: 1; height: 3px; background: linear-gradient(90deg, #ffd1dc, transparent); border-radius: 2px; }

.food-card {
  background: white; border-radius: 20px;
  border: 1px solid rgba(255, 180, 200, 0.4);
  box-shadow: var(--shadow-card);
  transition: all 0.35s ease;
  height: 100%; display: flex; flex-direction: column; position: relative; overflow: hidden;
}
.food-card:hover { transform: translateY(-10px); box-shadow: var(--shadow-hover); border-color: var(--primary); }
.img-wrapper { height: 220px; overflow: hidden; position: relative; border-bottom: 1px solid #fff0f5; }
.food-img { width: 100%; height: 100%; object-fit: cover; transition: 0.5s; }
.food-card:hover .food-img { transform: scale(1.1); }
.unavailable .food-img { filter: grayscale(100%); opacity: 0.7; }
.unavailable .card-body { opacity: 0.6; }
.unavailable:hover { transform: none; box-shadow: none; border-color: transparent; }
.card-body { padding: 18px; display: flex; flex-direction: column; flex: 1; }
.food-title { font-weight: 700; font-size: 1.1rem; color: #222; margin-bottom: 8px; display: -webkit-box; -webkit-line-clamp: 1; -webkit-box-orient: vertical; overflow: hidden; }
.food-card:hover .food-title { color: var(--primary); }
.rating-row { display: flex; font-size: 0.85rem; margin-bottom: 10px; color: #ffbb00; }
.review-count { color: #999; margin-left: 5px; font-size: 0.75rem; }
.food-desc { font-size: 0.9rem; color: #666; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; margin-bottom: 15px; }
.card-footer-action { margin-top: auto; padding-top: 15px; border-top: 1px dashed #ffe0e9; display: flex; justify-content: space-between; align-items: center; }
.loading-spinner { padding: 40px; text-align: center; color: var(--primary); }
</style>